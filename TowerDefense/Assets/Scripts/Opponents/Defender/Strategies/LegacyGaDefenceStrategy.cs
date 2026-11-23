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

        public override string StrategyName => "GeneticAlgorithm-legacy";


        public LegacyGaDefenceStrategy(TimeSpan timeLimit, int? seed, double balanceWeight) : base(timeLimit, seed)
        {
            _balanceWeight = balanceWeight;
        }

        protected override IFitnessFunction<DefenderGene> CreateFitness(DefenderSnapshot snapshot) => new DefenceHeuristicFitness(snapshot, _balanceWeight);

        protected override GeneticAlgorithmConfig CreateConfig() => DefenceLegacyGaConfiguration.CreateConfig();

        protected override GeneticAlgorithm<DefenderGene> CreateAlgorithm(IFitnessFunction<DefenderGene> fitness, CostLimitConstraint<DefenderGene> costConstraint, IConstraint<DefenderGene> constraint, IReadOnlyList<DefenderGene> candidateGenes, GeneticAlgorithmConfig config) =>
            DefenceLegacyGaConfiguration.Create(fitness, costConstraint, constraint, candidateGenes, config);

        protected override IReadOnlyList<DefenderGene> SelectTowers(GeneticAlgorithmResult<DefenderGene> result) => result.BestIndividual.Genes;
    }
}
