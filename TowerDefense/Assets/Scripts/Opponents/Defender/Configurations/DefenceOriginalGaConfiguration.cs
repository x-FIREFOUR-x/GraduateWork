using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.InitialPopulation;
using GeneticAlgorithms.Mutations;
using GeneticAlgorithms.Mutations.Composite;
using GeneticAlgorithms.Mutations.Implementations.LengthChanging.Constrained;
using GeneticAlgorithms.Randomness;
using GeneticAlgorithms.Repairs.Implementations;
using GeneticAlgorithms.Selection.Parent.Implementations.Common.Tournament;
using GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.SteadyState;

using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Configurations
{
    public static class DefenceOriginalGaConfiguration
    {
        public static GeneticAlgorithmConfig CreateConfig()
        {
            var config = new GeneticAlgorithmConfig
            {
                PopulationSize = 20,
                MaxIterations = 1000,
                StagnationIterations = 50,
                TournamentSize = 3,
                MaxMutationShare = 0.5,
                AcceptMutationOnlyIfNotWorse = true,
                InitialPopulationRandomShare = 0.8,
                InitialPopulationGreedyShare = 0.2,
                InitialPopulationWarmStartShare = 0,
            };
            config.CountOffspring = config.PopulationSize / 2;
            return config;
        }

        public static GeneticAlgorithm<DefenderGene> Create(IFitnessFunction<DefenderGene> fitness, CostLimitConstraint<DefenderGene> costConstraint, IConstraint<DefenderGene> constraint, IReadOnlyList<DefenderGene> candidateGenes, GeneticAlgorithmConfig config)
        {
            config.Validate();
            var fitnessPerCostCalculator = new FitnessPerCostCalculator<DefenderGene>(fitness, costConstraint);
            var repair = CreateRepair(candidateGenes, fitnessPerCostCalculator);
            var individualBuilder = new IndividualBuilder<DefenderGene>(constraint, candidateGenes, fitnessPerCostCalculator);

            return new GeneticAlgorithm<DefenderGene>(
                config,
                new PopulationInitializer<DefenderGene>(individualBuilder, repair),
                fitness,
                new TournamentParentSelection<DefenderGene>(config.TournamentSize),
                new HalfAndFeasibleFillCrossover<DefenderGene>(constraint),
                CreateMutation(costConstraint, candidateGenes, individualBuilder, config),
                new ReplaceWorstSurvivorSelection<DefenderGene>(),
                OriginalGeneticAlgorithm.CreateStopCriterion(config),
                CreateRandom(config),
                repair);
        }

        internal static GreedyRepair<DefenderGene> CreateRepair(IReadOnlyList<DefenderGene> candidateGenes, FitnessPerCostCalculator<DefenderGene> fitnessPerCostCalculator) =>
            new GreedyRepair<DefenderGene>(candidateGenes, fitnessPerCostCalculator.Calculate, fillRemainder: true, removeUnavailableGenes: true);

        internal static IMutation<DefenderGene> CreateMutation(CostLimitConstraint<DefenderGene> costConstraint, IReadOnlyList<DefenderGene> candidateGenes, IndividualBuilder<DefenderGene> individualBuilder, GeneticAlgorithmConfig config) =>
            new AdaptiveMutationSelector<DefenderGene>(
                new IMutation<DefenderGene>[]
                {
                    new RangeRegenerationMutation<DefenderGene>(individualBuilder, config.MaxMutationShare),
                    new SingleTypeRangeMutation<DefenderGene>(costConstraint, candidateGenes, config.MaxMutationShare, (first, second) => first.Type == second.Type, fillRemainder: true),
                },
                config.MutationSmoothing,
                config.MutationWindow,
                config.MinMutationProbability);

        internal static IRandom CreateRandom(GeneticAlgorithmConfig config) =>
            config.Seed.HasValue ? new SystemRandom(config.Seed.Value) : new SystemRandom();
    }
}
