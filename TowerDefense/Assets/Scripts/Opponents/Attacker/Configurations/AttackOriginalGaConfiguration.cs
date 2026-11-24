using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.InitialPopulation;
using GeneticAlgorithms.Mutations;
using GeneticAlgorithms.Mutations.Composite;
using GeneticAlgorithms.Mutations.Implementations.LengthChanging.Constrained;
using GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Permutation;
using GeneticAlgorithms.Randomness;
using GeneticAlgorithms.Repairs.Implementations;
using GeneticAlgorithms.Selection.Parent.Implementations.Common.Tournament;
using GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.SteadyState;

using TowerDefense.Main.Units;


namespace TowerDefense.Opponents.Attacker.Configurations
{
    public static class AttackOriginalGaConfiguration
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
                InitialPopulationGreedyShare = 0,
                InitialPopulationWarmStartShare = 0.2,
            };
            config.CountOffspring = config.PopulationSize / 2;
            return config;
        }

        public static GeneticAlgorithm<UnitType> Create(IFitnessFunction<UnitType> fitness, CostLimitConstraint<UnitType> constraint, IReadOnlyList<UnitType> availableUnits, GeneticAlgorithmConfig config)
        {
            config.Validate();
            var fitnessPerCostCalculator = new FitnessPerCostCalculator<UnitType>(fitness, constraint);
            var repair = new GreedyRepair<UnitType>(availableUnits, fitnessPerCostCalculator.Calculate, fillRemainder: true, removeUnavailableGenes: true);
            var individualBuilder = new IndividualBuilder<UnitType>(constraint, availableUnits, fitnessPerCostCalculator);
            var mutation = new AdaptiveMutationSelector<UnitType>(
                new IMutation<UnitType>[]
                {
                    new RangeRegenerationMutation<UnitType>(individualBuilder, config.MaxMutationShare),
                    new SingleTypeRangeMutation<UnitType>(constraint, availableUnits, config.MaxMutationShare, (first, second) => first == second, fillRemainder: true),
                    new SwapMutation<UnitType>(),
                },
                config.MutationSmoothing,
                config.MutationWindow,
                config.MinMutationProbability);

            return new GeneticAlgorithm<UnitType>(
                config,
                new PopulationInitializer<UnitType>(individualBuilder, repair),
                fitness,
                new TournamentParentSelection<UnitType>(config.TournamentSize),
                new HalfAndFeasibleFillCrossover<UnitType>(constraint),
                mutation,
                new ReplaceWorstSurvivorSelection<UnitType>(),
                OriginalGeneticAlgorithm.CreateStopCriterion(config),
                CreateRandom(config),
                repair);
        }

        internal static IRandom CreateRandom(GeneticAlgorithmConfig config) =>
            config.Seed.HasValue ? new SystemRandom(config.Seed.Value) : new SystemRandom();
    }
}
