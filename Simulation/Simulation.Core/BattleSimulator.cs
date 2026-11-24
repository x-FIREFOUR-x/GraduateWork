using System;
using System.Collections.Generic;
using Simulation.Combat.Engine;
using Simulation.Combat.Paths;
using Simulation.Geometry;
using Simulation.InputData.Game;
using Simulation.InputData.Game.Towers;
using Simulation.InputData.Game.Units;
using Simulation.InputData.Map;


namespace Simulation
{
    public sealed class BattleSimulator
    {
        internal const float MaxDuration = 600f;

        private readonly Dictionary<Cell, TowerTileData> towerTiles = new Dictionary<Cell, TowerTileData>();
        private readonly Dictionary<string, int> unitIndexByType = new Dictionary<string, int>();
        private readonly List<UnitSpec> unitSpecs = new List<UnitSpec>();
        private readonly Dictionary<string, TowerSpec> towerByType = new Dictionary<string, TowerSpec>();
        private readonly Dictionary<TowerSpec, bool[]> targetableUnitsByTower = new Dictionary<TowerSpec, bool[]>();

        public float RouteLength => Graph.ShortestRouteLength;

        internal MapData Map { get; }

        internal GameData Data { get; }

        internal PathGraph Graph { get; }

        internal BattleSettings Settings => Data.Settings;

        public BattleSimulator(SimulationInputData input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            Map = input.Map ?? throw new ArgumentException("Simulation input has no map.", nameof(input));
            Data = input.Data ?? throw new ArgumentException("Simulation input has no game data.", nameof(input));
            Graph = PathGraph.Build(Map);
            ValidateSettings(Data.Settings);

            foreach (TowerTileData towerTile in Map.TowerTiles)
            {
                if (!towerTiles.TryAdd(towerTile.Cell, towerTile))
                    throw new ArgumentException($"Tower tile {towerTile.Cell} is duplicated.", nameof(input));
            }

            IndexData(Data);
        }

        public SimulationOutputData Run(IReadOnlyList<PlacedTower> towers, IReadOnlyList<string> wave, int castleHealth, int seed)
        {
            if (towers == null)
                throw new ArgumentNullException(nameof(towers));
            if (wave == null)
                throw new ArgumentNullException(nameof(wave));
            if (castleHealth <= 0)
                throw new ArgumentOutOfRangeException(nameof(castleHealth), "Castle health must be positive.");

            return new Battle(this, towers, wave, castleHealth, seed).Play();
        }

        private static void ValidateSettings(BattleSettings settings)
        {
            if (settings == null)
                throw new ArgumentException("Game data has no battle settings.", nameof(settings));
            RequirePositive(settings.FixedTimeStepSeconds, nameof(settings.FixedTimeStepSeconds));
            RequirePositive(settings.UnitSpawnIntervalSeconds, nameof(settings.UnitSpawnIntervalSeconds));
            RequirePositive(settings.TowerRetargetIntervalSeconds, nameof(settings.TowerRetargetIntervalSeconds));
            RequirePositive(settings.PathNodeReachDistance, nameof(settings.PathNodeReachDistance));
            if (settings.CatapultStoneMinFlightSeconds < 0f)
                throw new ArgumentException($"Battle setting {nameof(settings.CatapultStoneMinFlightSeconds)} must not be negative.", nameof(settings));
        }

        private static void RequirePositive(float value, string name)
        {
            if (!(value > 0f))
                throw new ArgumentException($"Battle setting {name} must be positive.", nameof(BattleSettings));
        }

        internal int UnitIndex(string type)
        {
            if (type == null || !unitIndexByType.TryGetValue(type, out int index))
                throw new ArgumentException($"Game data has no unit of type {type}.", nameof(type));
            return index;
        }

        internal UnitSpec UnitSpec(int index) => unitSpecs[index];

        internal TowerSpec TowerSpec(string type)
        {
            if (type == null || !towerByType.TryGetValue(type, out TowerSpec spec))
                throw new ArgumentException($"Game data has no tower of type {type}.", nameof(type));
            return spec;
        }

        internal bool[] TargetableUnits(TowerSpec spec) => targetableUnitsByTower[spec];

        private void IndexData(GameData data)
        {
            foreach (UnitSpec unit in data.Units)
            {
                if (unit.Type != null && unitIndexByType.TryAdd(unit.Type, unitSpecs.Count))
                    unitSpecs.Add(unit);
            }

            foreach (TowerSpec tower in data.Towers)
            {
                if (tower.Type == null || !towerByType.TryAdd(tower.Type, tower))
                    continue;

                var targetableUnits = new bool[unitSpecs.Count];
                foreach (string unitType in tower.TargetableUnitTypes)
                {
                    if (unitType != null && unitIndexByType.TryGetValue(unitType, out int index))
                        targetableUnits[index] = true;
                }
                targetableUnitsByTower[tower] = targetableUnits;
            }
        }

        internal Vec3 TowerPosition(PlacedTower tower, TowerSpec spec)
        {
            if (!towerTiles.TryGetValue(tower.Cell, out TowerTileData towerTile))
                throw new ArgumentException($"Cell {tower.Cell} is not a tower tile.", nameof(tower));
            return towerTile.BuildPosition + spec.BuildPositionOffset;
        }
    }
}
