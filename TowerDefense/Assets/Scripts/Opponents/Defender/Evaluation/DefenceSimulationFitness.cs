using System;
using System.Collections.Generic;
using System.Linq;

using GeneticAlgorithms.Fitness;

using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Opponents.Defender.State;
using TowerDefense.Opponents.SimulationBridge;
using Simulation;
using Simulation.Geometry;


namespace TowerDefense.Opponents.Defender.Evaluation
{
    public sealed class DefenceSimulationFitness : IFitnessFunction<DefenderGene>, INamedFitnessValues
    {
        public const int DefenceObjectiveIndex = 0;

        private static readonly string[] ValueNames = { "defence", "balance", "remainingBudget", "robustness" };

        private readonly DefenderSnapshot _snapshot;
        private readonly SimulationEvaluator _evaluator;
        private readonly DefenceHeuristicFitness _heuristicFitness;
        private readonly IReadOnlyList<PlacedTower> _builtTowers;
        private readonly IReadOnlyList<UnitType> _expectedWave;
        private readonly IReadOnlyList<UnitType> _threatUnits;
        private readonly double _attackScoreWithoutNewTowers;

        public int FitnessValueCount => ValueNames.Length;

        public IReadOnlyList<string> FitnessValueNames => ValueNames;

        public bool PrefersFeasibleIndividuals => false;


        public DefenceSimulationFitness(DefenderSnapshot snapshot, double balanceWeight, double fireDensityWeight)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            _evaluator = new SimulationEvaluator(snapshot.Simulation);
            _heuristicFitness = new DefenceHeuristicFitness(snapshot, balanceWeight, fireDensityWeight);
            _builtTowers = snapshot.Simulation.BuiltTowers;
            _expectedWave = snapshot.ExpectedWave;
            _threatUnits = snapshot.UnitTypesUnlockedByAttacker.Count > 0
                ? snapshot.UnitTypesUnlockedByAttacker
                : Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToList();
            _attackScoreWithoutNewTowers = _evaluator.AttackScore(_builtTowers, _expectedWave);
        }

        public double EvaluateGene(DefenderGene gene) => _heuristicFitness.Value(new[] { gene });

        public double[] Evaluate(IReadOnlyList<DefenderGene> genes)
        {
            double defence = Defence(genes);
            double balance = _heuristicFitness.Balance(genes);
            double remainingBudget = _snapshot.Budget - _snapshot.Cost(genes);
            return new[] { defence, balance, remainingBudget, Robustness(genes) };
        }

        private double Defence(IReadOnlyList<DefenderGene> genes)
        {
            var occupiedCells = new HashSet<Cell>(_builtTowers.Select(tower => tower.Cell));
            var towers = new List<PlacedTower>(_builtTowers);
            foreach (DefenderGene gene in genes)
            {
                PlacedTower tower = SimulationTowerPlacementConverter.Of(gene);
                if (occupiedCells.Add(tower.Cell))
                    towers.Add(tower);
            }

            return _attackScoreWithoutNewTowers - _evaluator.AttackScore(towers, _expectedWave);
        }

        private double Robustness(IReadOnlyList<DefenderGene> genes)
        {
            int[] countByType = _heuristicFitness.CountByTypeAfter(genes);

            double weakestResistance = double.MaxValue;
            foreach (UnitType unit in _threatUnits)
            {
                double resistance = 0;
                for (int type = 0; type < countByType.Length; type++)
                    if (countByType[type] > 0)
                        resistance += countByType[type] * (AdvantageMatrix.MaxAdvantagePoints - AdvantageMatrix.GetAdvantagePointsUnitOverTower(unit, (TowerType)type));
                weakestResistance = Math.Min(weakestResistance, resistance);
            }
            return weakestResistance == double.MaxValue ? 0 : weakestResistance;
        }
    }
}
