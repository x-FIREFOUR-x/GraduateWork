using System;
using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Selection.Solution;

using TowerDefense.Opponents.Defender.Configurations;
using TowerDefense.Opponents.Defender.Evaluation;
using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Strategies
{
    public sealed class NsgaIIDefenceStrategy : GeneticDefenceStrategy
    {
        private readonly double _fireDensityWeight;
        private readonly bool _adaptiveDifficulty;
        private readonly double _targetCastleDamage;
        private readonly double _regulatorStep;

        public double DifficultyLevel { get; private set; }

        public override string StrategyName => "NSGA-II-original";

        protected override double? ReportedDifficultyLevel => DifficultyLevel;


        public NsgaIIDefenceStrategy(TimeSpan timeLimit, int? seed, double fireDensityWeight, double difficultyLevel, bool adaptiveDifficulty, double targetCastleDamage, double regulatorStep) : base(timeLimit, seed)
        {
            if (!(fireDensityWeight >= 0))
                throw new ArgumentOutOfRangeException(nameof(fireDensityWeight), fireDensityWeight, "Fire density weight must be non-negative.");
            if (!(difficultyLevel >= 0 && difficultyLevel <= 1))
                throw new ArgumentOutOfRangeException(nameof(difficultyLevel), difficultyLevel, "Difficulty level must be in [0; 1].");
            if (!(targetCastleDamage >= 0))
                throw new ArgumentOutOfRangeException(nameof(targetCastleDamage), targetCastleDamage, "Target castle damage must be non-negative.");
            if (!(regulatorStep >= 0))
                throw new ArgumentOutOfRangeException(nameof(regulatorStep), regulatorStep, "Regulator step must be non-negative.");

            _fireDensityWeight = fireDensityWeight;
            DifficultyLevel = difficultyLevel;
            _adaptiveDifficulty = adaptiveDifficulty;
            _targetCastleDamage = targetCastleDamage;
            _regulatorStep = regulatorStep;
        }

        public override void OnWaveFinished(int castleDamage)
        {
            if (!_adaptiveDifficulty)
                return;

            double difficultyLevel = DifficultyLevel + _regulatorStep * (castleDamage - _targetCastleDamage);
            DifficultyLevel = Math.Min(1, Math.Max(0, difficultyLevel));
        }

        protected override IFitnessFunction<DefenderGene> CreateFitness(DefenderSnapshot snapshot) => new DefenceMultiObjectiveFitness(snapshot, _fireDensityWeight);

        protected override GeneticAlgorithmConfig CreateConfig() => DefenceNsgaIIConfiguration.CreateConfig();

        protected override GeneticAlgorithm<DefenderGene> CreateAlgorithm(IFitnessFunction<DefenderGene> fitness, CostLimitConstraint<DefenderGene> costConstraint, IConstraint<DefenderGene> constraint, IReadOnlyList<DefenderGene> candidateGenes, GeneticAlgorithmConfig config) =>
            DefenceNsgaIIConfiguration.Create(fitness, costConstraint, constraint, candidateGenes, config);

        protected override IReadOnlyList<DefenderGene> SelectTowers(GeneticAlgorithmResult<DefenderGene> result) =>
            new QuantileSolutionSelection<DefenderGene>(DefenceMultiObjectiveFitness.CoverageObjectiveIndex, DifficultyLevel).Select(result.ParetoFront).Genes;
    }
}
