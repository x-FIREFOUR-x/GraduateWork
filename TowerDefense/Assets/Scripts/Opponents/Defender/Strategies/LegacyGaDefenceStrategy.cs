using System;
using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Fitness;

using TowerDefense.Opponents.Defender.Configurations;
using TowerDefense.Opponents.Defender.Evaluation;
using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Strategies
{
    public sealed class LegacyGaDefenceStrategy : GeneticDefenceStrategy
    {
        private readonly double _balanceWeight;
        private readonly double _fireDensityWeight;

        public override string StrategyName => "GeneticAlgorithm-legacy";


        public LegacyGaDefenceStrategy(TimeSpan timeLimit, int? seed, double balanceWeight, double fireDensityWeight) : base(timeLimit, seed)
        {
            _balanceWeight = balanceWeight;
            _fireDensityWeight = fireDensityWeight;
        }

        protected override IFitnessFunction<DefenderGene> CreateFitness(DefenderSnapshot snapshot) => new DefenceHeuristicFitness(snapshot, _balanceWeight, _fireDensityWeight);

        protected override GeneticAlgorithmConfig CreateConfig() => DefenceLegacyGaConfiguration.CreateConfig();

        protected override GeneticAlgorithm<DefenderGene> CreateAlgorithm(IFitnessFunction<DefenderGene> fitness, CostLimitConstraint<DefenderGene> costConstraint, IConstraint<DefenderGene> constraint, IReadOnlyList<DefenderGene> candidateGenes, GeneticAlgorithmConfig config) =>
            DefenceLegacyGaConfiguration.Create(fitness, costConstraint, constraint, candidateGenes, config);

        protected override IReadOnlyList<DefenderGene> SelectTowers(GeneticAlgorithmResult<DefenderGene> result) => result.BestIndividual.Genes;
    }
}
