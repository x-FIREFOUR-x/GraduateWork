using System;
using System.Collections.Generic;
using Simulation.Combat.Paths;
using Simulation.Combat.Units;
using Simulation.Geometry;
using Simulation.InputData.Game.Units;


namespace Simulation.Combat.Engine
{
    internal sealed partial class Battle
    {
        private const float SpawnTimeTolerance = 1e-4f;
        private const float MinMoveDistance = 1e-6f;

        private readonly BattleSimulator simulator;
        private readonly IReadOnlyList<string> wave;
        private readonly SimulationRandom random;
        private readonly List<SimUnit> units = new List<SimUnit>();
        private readonly SimulationOutputData result = new SimulationOutputData();
        private int castleHealth;
        private int spawnedCount;
        private float time;
        private float nextSpawnTime;
        private bool hasRemovedUnits;

        private PathGraph Graph => simulator.Graph;

        public Battle(BattleSimulator simulator, IReadOnlyList<PlacedTower> placedTowers, IReadOnlyList<string> wave,
                      int castleHealth, int seed)
        {
            this.simulator = simulator;
            this.wave = wave;
            this.castleHealth = castleHealth;
            random = new SimulationRandom(seed);

            foreach (string type in wave)
            {
                UnitSpec spec = simulator.UnitSpec(simulator.UnitIndex(type));
                if (spec.HasSuccessor)
                    simulator.UnitIndex(spec.SuccessorType);
            }

            PlaceTowers(placedTowers);
        }

        public SimulationOutputData Play()
        {
            if (wave.Count == 0)
                return result;

            while (time < BattleSimulator.MaxDuration)
            {
                SpawnDue();
                MoveUnits();
                if (castleHealth <= 0)
                {
                    result.CastleDestroyed = true;
                    break;
                }

                ActTowers();
                if (hasRemovedUnits)
                {
                    units.RemoveAll(unit => !unit.IsActive);
                    hasRemovedUnits = false;
                }
                time += simulator.Settings.FixedTimeStepSeconds;

                if (spawnedCount == wave.Count && units.Count == 0)
                    break;
            }

            result.Duration = time;
            return result;
        }

        internal void Damage(SimUnit unit, float amount)
        {
            if (!unit.IsActive)
                return;

            unit.Health -= amount;
            if (unit.Health > 0f)
                return;

            result.KilledCount++;
            OnKilled(unit);
            Remove(unit);
        }

        partial void PlaceTowers(IReadOnlyList<PlacedTower> placedTowers);

        partial void ActTowers();

        partial void ChooseBranch(SimUnit unit, PathNode fork, ref PathNode next);

        partial void OnSpawned(SimUnit unit);

        partial void OnAdvanced(SimUnit unit);

        partial void OnKilled(SimUnit unit);

        partial void OnRemoved(SimUnit unit);

        private void SpawnDue()
        {
            if (spawnedCount >= wave.Count || time + SpawnTimeTolerance < nextSpawnTime)
                return;

            int typeIndex = simulator.UnitIndex(wave[spawnedCount]);
            UnitSpec spec = simulator.UnitSpec(typeIndex);
            spawnedCount++;
            nextSpawnTime += simulator.Settings.UnitSpawnIntervalSeconds;

            PathNode start = Graph.Start;
            Vec3 spawn = simulator.Map.SpawnPosition;
            var unit = new SimUnit(spec, typeIndex, spec.Health, RandomPathOffset(spec.MaxPathOffset), spawn.Y + spec.TravelHeightOffset)
            {
                Previous = start
            };
            unit.Position = new Vec3(spawn.X + unit.PathOffset.X, unit.TravelHeight, spawn.Z + unit.PathOffset.Z);

            OnSpawned(unit);
            unit.Target = ChooseNext(unit, start);
            OnAdvanced(unit);
            units.Add(unit);
        }

        private Vec3 RandomPathOffset(float maxOffset)
        {
            float radius = maxOffset * (float)Math.Sqrt(random.NextFloat());
            float angle = 2f * (float)Math.PI * random.NextFloat();
            return new Vec3(radius * (float)Math.Cos(angle), 0f, radius * (float)Math.Sin(angle));
        }

        private PathNode ChooseNext(SimUnit unit, PathNode node)
        {
            PathNode next = node.Next[0];
            if (node.IsFork)
                ChooseBranch(unit, node, ref next);
            return next;
        }

        private void MoveUnits()
        {
            float timeStep = simulator.Settings.FixedTimeStepSeconds;
            float reachDistance = simulator.Settings.PathNodeReachDistance;
            for (int i = 0; i < units.Count; i++)
            {
                SimUnit unit = units[i];
                if (!unit.IsActive)
                    continue;

                if (unit.RecoveryTimer > 0f)
                {
                    unit.RecoveryTimer -= timeStep;
                    continue;
                }

                float dx = unit.TargetX - unit.Position.X;
                float dz = unit.TargetZ - unit.Position.Z;
                float distance = (float)Math.Sqrt(dx * dx + dz * dz);
                if (distance > MinMoveDistance)
                {
                    float move = unit.Spec.Speed * (1f - unit.SlowPercent) * timeStep;
                    float scale = move / distance;
                    unit.Position.X += dx * scale;
                    unit.Position.Z += dz * scale;
                    unit.DistanceMoved += move;
                }

                if (unit.SlowTimer > 0f)
                {
                    unit.SlowTimer -= timeStep;
                    if (unit.SlowTimer <= 0f)
                        unit.SlowPercent = 0f;
                }

                if (distance <= reachDistance)
                    Advance(unit);
                if (castleHealth <= 0)
                    return;
            }
        }

        private void Advance(SimUnit unit)
        {
            if (unit.Target.IsEnd)
            {
                ReachCastle(unit);
                return;
            }

            unit.Previous = unit.Target;
            unit.Target = ChooseNext(unit, unit.Previous);
            OnAdvanced(unit);
        }

        private void ReachCastle(SimUnit unit)
        {
            int damage = Math.Min(unit.Spec.CastleDamage, castleHealth);
            castleHealth -= damage;
            result.CastleDamage += damage;
            result.ReachedCount++;
            Remove(unit);
        }

        private void Remove(SimUnit unit)
        {
            unit.IsActive = false;
            hasRemovedUnits = true;
            if (!unit.HasSuccessor)
                result.DistanceMoved.Add(unit.DistanceMoved);
            OnRemoved(unit);
        }
    }
}
