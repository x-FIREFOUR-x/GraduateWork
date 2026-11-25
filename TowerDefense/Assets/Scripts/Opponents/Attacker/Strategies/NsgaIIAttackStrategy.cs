using System;
using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Selection.Solution;

using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.Configurations;
using TowerDefense.Opponents.Attacker.State;
using TowerDefense.Opponents.Attacker.Evaluation;


namespace TowerDefense.Opponents.Attacker.Strategies
{
    public sealed class NsgaIIAttackStrategy : GeneticAttackStrategy
    {
        private readonly double _minDifficultyLevel;
        private readonly bool _adaptiveDifficulty;
        private readonly double _targetCastleDamage;
        private readonly double _regulatorStep;

        public double DifficultyLevel { get; private set; }

        public override string StrategyName => "NSGA-II-original";

        protected override double? ReportedDifficultyLevel => DifficultyLevel;


        public NsgaIIAttackStrategy(TimeSpan timeLimit, int? seed, double difficultyLevel, double minDifficultyLevel, bool adaptiveDifficulty, double targetCastleDamage, double regulatorStep) : base(timeLimit, seed)
        {
            if (!(difficultyLevel >= 0 && difficultyLevel <= 1))
                throw new ArgumentOutOfRangeException(nameof(difficultyLevel), difficultyLevel, "Difficulty level must be in [0; 1].");
            if (!(minDifficultyLevel >= 0 && minDifficultyLevel <= 1))
                throw new ArgumentOutOfRangeException(nameof(minDifficultyLevel), minDifficultyLevel, "Min difficulty level must be in [0; 1].");
            if (!(targetCastleDamage >= 0))
                throw new ArgumentOutOfRangeException(nameof(targetCastleDamage), targetCastleDamage, "Target castle damage must be non-negative.");
            if (!(regulatorStep >= 0))
                throw new ArgumentOutOfRangeException(nameof(regulatorStep), regulatorStep, "Regulator step must be non-negative.");

            _minDifficultyLevel = minDifficultyLevel;
            DifficultyLevel = Math.Max(minDifficultyLevel, difficultyLevel);
            _adaptiveDifficulty = adaptiveDifficulty;
            _targetCastleDamage = targetCastleDamage;
            _regulatorStep = regulatorStep;
        }

        public override void OnWaveFinished(int castleDamage)
        {
            if (!_adaptiveDifficulty)
                return;

            double relativeError = (_targetCastleDamage - castleDamage) / Math.Max(1, _targetCastleDamage);
            double difficultyLevel = DifficultyLevel + _regulatorStep * Math.Max(-1, Math.Min(1, relativeError));
            DifficultyLevel = Math.Min(1, Math.Max(_minDifficultyLevel, difficultyLevel));
        }

        protected override IFitnessFunction<UnitType> CreateFitness(AttackerSnapshot snapshot) => new AttackSimulationFitness(snapshot);

        protected override GeneticAlgorithmConfig CreateConfig() => AttackNsgaIIConfiguration.CreateConfig();

        protected override GeneticAlgorithm<UnitType> CreateAlgorithm(IFitnessFunction<UnitType> fitness, CostLimitConstraint<UnitType> constraint, IReadOnlyList<UnitType> availableUnits, GeneticAlgorithmConfig config) =>
            AttackNsgaIIConfiguration.Create(fitness, constraint, availableUnits, config);

        protected override IReadOnlyList<UnitType> SelectWave(GeneticAlgorithmResult<UnitType> result) =>
            new QuantileSolutionSelection<UnitType>(AttackSimulationFitness.AttackObjectiveIndex, DifficultyLevel).Select(result.ParetoFront).Genes;
    }
}
