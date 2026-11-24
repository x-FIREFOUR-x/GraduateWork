using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.InitialPopulation;
using GeneticAlgorithms.Mutations.Composite;
using GeneticAlgorithms.Mutations.Implementations.LengthChanging.Constrained;
using GeneticAlgorithms.Selection.Parent.Implementations.SingleObjective.Rank;
using GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.SteadyState;

using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Configurations
{
    public static class DefenceLegacyGaConfiguration
    {
        private const double MutationProbability = 0.5;

        public static GeneticAlgorithmConfig CreateConfig()
        {
            var config = new GeneticAlgorithmConfig
            {
                PopulationSize = 20,
                MaxIterations = 1000,
                StagnationIterations = null,
                MaxMutationShare = 0.5,
                AcceptMutationOnlyIfNotWorse = true,
                InitialPopulationRandomShare = 1,
                InitialPopulationGreedyShare = 0,
                InitialPopulationWarmStartShare = 0,
            };
            config.CountOffspring = config.PopulationSize - 1;
            return config;
        }

        public static GeneticAlgorithm<DefenderGene> Create(IFitnessFunction<DefenderGene> fitness, CostLimitConstraint<DefenderGene> costConstraint, IConstraint<DefenderGene> constraint, IReadOnlyList<DefenderGene> candidateGenes, GeneticAlgorithmConfig config)
        {
            config.Validate();
            var fitnessPerCostCalculator = new FitnessPerCostCalculator<DefenderGene>(fitness, costConstraint);
            var individualBuilder = new IndividualBuilder<DefenderGene>(constraint, candidateGenes, fitnessPerCostCalculator);

            return new GeneticAlgorithm<DefenderGene>(
                config,
                new PopulationInitializer<DefenderGene>(individualBuilder),
                fitness,
                new AdjacentPairsParentSelection<DefenderGene>(),
                new HalfAndFeasibleFillCrossover<DefenderGene>(constraint),
                new ProbabilisticMutation<DefenderGene>(new RangeRegenerationMutation<DefenderGene>(individualBuilder, config.MaxMutationShare), MutationProbability),
                new BestChildReplacesWorstSurvivorSelection<DefenderGene>(),
                OriginalGeneticAlgorithm.CreateStopCriterion(config),
                DefenceOriginalGaConfiguration.CreateRandom(config));
        }
    }
}
