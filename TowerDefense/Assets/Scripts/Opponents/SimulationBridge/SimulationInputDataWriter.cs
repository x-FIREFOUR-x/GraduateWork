using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using TowerDefense.Main.Managers.WaveSpawners;
using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Projectiles;
using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Main.Units.ForkPathChoice;
using TowerDefense.Main.Units.Squad;
using TowerDefense.Storage;
using Simulation;
using Simulation.InputData.Game;
using Simulation.InputData.Game.Towers;
using Simulation.InputData.Game.Units;

using GamePathGraph = TowerDefense.Main.Map.Path.PathGraph;
using GamePathGraphNode = TowerDefense.Main.Map.Path.PathGraphNode;
using SimCell = Simulation.Geometry.Cell;
using SimMapData = Simulation.InputData.Map.MapData;
using SimPathNodeData = Simulation.InputData.Map.PathNodeData;
using SimTowerTileData = Simulation.InputData.Map.TowerTileData;
using SimVec3 = Simulation.Geometry.Vec3;


namespace TowerDefense.Opponents.SimulationBridge
{
    public static class SimulationInputDataWriter
    {
        public static string Write(TilesMap tilesMap, GamePathGraph pathGraph, Vector3 spawnPosition, string fileName)
        {
            string path = Path.Combine(Application.temporaryCachePath, fileName);
            CreateInputData(tilesMap, pathGraph, spawnPosition).Save(path);
            return path;
        }

        private static SimulationInputData CreateInputData(TilesMap tilesMap, GamePathGraph pathGraph, Vector3 spawnPosition)
        {
            return new SimulationInputData
            {
                Data = CreateGameData(),
                Map = CreateMapData(tilesMap, pathGraph, spawnPosition)
            };
        }

        private static GameData CreateGameData()
        {
            var data = new GameData { Settings = CreateBattleSettings() };

            List<Unit> units = LoadComponents<Unit>(Resources.Load<UnitsStorage>(nameof(UnitsStorage)).Units).ToList();
            foreach (Unit unit in units)
                data.Units.Add(CreateUnitSpec(unit));

            foreach (WraithDragonRider rider in units.OfType<WraithDragonRider>())
            {
                if (rider.WraithPrefab != null && data.Units.All(unit => unit.Type != rider.WraithPrefab.UnitType.ToString()))
                    data.Units.Add(CreateUnitSpec(rider.WraithPrefab));
            }

            foreach (Tower tower in LoadComponents<Tower>(Resources.Load<TowersStorage>(nameof(TowersStorage)).Towers))
                data.Towers.Add(CreateTowerSpec(tower));

            return data;
        }

        private static SimMapData CreateMapData(TilesMap tilesMap, GamePathGraph pathGraph, Vector3 spawnPosition)
        {
            if (tilesMap == null)
                throw new ArgumentNullException(nameof(tilesMap));
            if (pathGraph == null)
                throw new ArgumentNullException(nameof(pathGraph));

            MapSizeParamsStorage mapSizeParams = Resources.Load<MapSizeParamsStorage>(nameof(MapSizeParamsStorage));
            var map = new SimMapData
            {
                GroundHeight = mapSizeParams.GroundHeight,
                SpawnPosition = ToVec3(spawnPosition)
            };

            for (int row = 0; row < tilesMap.Size; row++)
            {
                for (int column = 0; column < tilesMap.Size; column++)
                {
                    TowerTile towerTile = tilesMap.GetTowerTileAt(row, column);
                    if (towerTile != null)
                        map.TowerTiles.Add(new SimTowerTileData { Cell = new SimCell(row, column), BuildPosition = ToVec3(towerTile.GetTowerBuildPosition()) });
                }
            }

            var idByNode = new Dictionary<GamePathGraphNode, int>();
            for (int i = 0; i < pathGraph.Nodes.Count; i++)
                idByNode[pathGraph.Nodes[i]] = i;

            foreach (GamePathGraphNode node in pathGraph.Nodes)
            {
                map.PathNodes.Add(new SimPathNodeData
                {
                    Id = idByNode[node],
                    Position = ToVec3(node.Position),
                    Next = node.Next.Select(next => idByNode[next]).ToList()
                });
            }
            map.StartNodeId = idByNode[pathGraph.Start];

            return map;
        }

        private static BattleSettings CreateBattleSettings()
        {
            return new BattleSettings
            {
                FixedTimeStepSeconds = Time.fixedDeltaTime,
                UnitSpawnIntervalSeconds = AIWaveSpawner.UnitSpawnInterval,
                TowerRetargetIntervalSeconds = Tower.TargetingInterval,
                PathNodeReachDistance = Unit.NodeReachDistance,
                CatapultStoneMinFlightSeconds = CatapultStone.MinFlightTime
            };
        }

        private static UnitSpec CreateUnitSpec(Unit unit)
        {
            var spec = new UnitSpec
            {
                Type = unit.UnitType.ToString(),
                Health = unit.StartHealth,
                Speed = unit.StartSpeed,
                CastleDamage = unit.Damage,
                MaxPathOffset = unit.MaxRandomPathOffset,
                TravelHeightOffset = unit.OffsetStartPosY,
                SpeedRecoveryTime = unit.TimeReturnSpeed,
                ForkStrategy = ForkStrategyOf(unit.UnitType)
            };

            if (unit is SquadLeader leader)
            {
                spec.MaxSquadMembers = leader.MaxSquadMembersCount;
                spec.SquadMemberTypes = leader.AllowedSquadMemberUnitTypes.Select(type => type.ToString()).ToList();
            }

            if (unit is WraithDragonRider rider && rider.WraithPrefab != null)
            {
                Wraith wraith = rider.WraithPrefab;
                float dropHeight = Mathf.Max(0f, rider.OffsetStartPosY + rider.RiderStandHeight - wraith.OffsetStartPosY);
                spec.HasSuccessor = true;
                spec.SuccessorType = wraith.UnitType.ToString();
                spec.SuccessorRecoveryTime = Mathf.Sqrt(2f * dropHeight / wraith.Gravity) + wraith.RecoveryTime;
            }

            return spec;
        }

        private static TowerSpec CreateTowerSpec(Tower tower)
        {
            var spec = new TowerSpec
            {
                Type = tower.Type.ToString(),
                ShootRange = tower.ShootRange,
                TimeBetweenShoots = tower.TimeBetweenShoots,
                BuildPositionOffset = ToVec3(tower.OffsetTower),
                FirePointHeight = tower.FirePointHeight,
                AimAtGround = tower.AimAtGround,
                TargetableUnitTypes = tower.TargetableUnitTypes.Select(type => type.ToString()).ToList()
            };

            switch (tower)
            {
                case IceMageTower mage:
                    spec.AttackKind = AttackKind.Beam;
                    spec.BeamDamagePerSecond = mage.DamageInSecond();
                    spec.SlowPercent = mage.PercentSlowing;
                    break;
                case CatapultTower catapult:
                    CatapultStone stone = catapult.ProjectilePrefab.GetComponent<CatapultStone>();
                    spec.AttackKind = AttackKind.Arc;
                    spec.ProjectileCount = 1;
                    spec.ProjectileSpeed = stone.Speed;
                    spec.ProjectileDamage = stone.Damage;
                    spec.ExplosionRadius = stone.ExplosionRadius;
                    spec.SwingTime = catapult.SwingTime;
                    break;
                default:
                    Projectile projectile = tower.ProjectilePrefab.GetComponent<Projectile>();
                    spec.AttackKind = AttackKind.Homing;
                    spec.ProjectileCount = tower.CountProjectileEntitys;
                    spec.ProjectileSpeed = projectile.Speed;
                    spec.ProjectileDamage = projectile.Damage;
                    break;
            }

            return spec;
        }

        private static ForkStrategyKind ForkStrategyOf(UnitType type)
        {
            string name = ForkPathChoiceStrategies.GetByUnitType(type).Name;
            switch (name)
            {
                case "Random":
                    return ForkStrategyKind.Random;
                case "Shortest":
                    return ForkStrategyKind.Shortest;
                case "Longest":
                    return ForkStrategyKind.Longest;
                case "Safest":
                    return ForkStrategyKind.Safest;
                case "Most dangerous":
                    return ForkStrategyKind.MostDangerous;
                case "Least crowded":
                    return ForkStrategyKind.LeastCrowded;
                case "Most crowded":
                    return ForkStrategyKind.MostCrowded;
                default:
                    throw new InvalidOperationException($"Fork strategy '{name}' of {type} has no simulation counterpart.");
            }
        }

        private static IEnumerable<T> LoadComponents<T>(IEnumerable<GameObject> prefabs) where T : Component
        {
            return prefabs.Where(prefab => prefab != null)
                          .Select(prefab => prefab.GetComponent<T>())
                          .Where(component => component != null);
        }

        private static SimVec3 ToVec3(Vector3 vector) => new SimVec3(vector.x, vector.y, vector.z);
    }
}
