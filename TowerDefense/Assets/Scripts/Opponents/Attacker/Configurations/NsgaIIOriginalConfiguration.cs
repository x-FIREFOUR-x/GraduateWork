using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.InitialPopulation;
using GeneticAlgorithms.Mutations;
using GeneticAlgorithms.Mutations.Composite;
using GeneticAlgorithms.Mutations.Implementations.LengthChanging.Constrained;
using GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Permutation;
using GeneticAlgorithms.Repairs.Implementations;
using GeneticAlgorithms.Selection.Parent.Implementations.MultiObjective;
using GeneticAlgorithms.Selection.Survivor.Implementations.MultiObjective;

using TowerDefense.Main.Units;


namespace TowerDefense.Opponents.Attacker.Configurations
{
    public static class NsgaIIOriginalConfiguration
    {
        public static GeneticAlgorithmConfig CreateConfig()
        {
            var config = new GeneticAlgorithmConfig
            {
                PopulationSize = 20,
                MaxIterations = 1000,
                StagnationIterations = null,
                TournamentSize = 2,
                MaxMutationShare = 0.5,
                AcceptMutationOnlyIfNotWorse = true,
                InitialPopulationRandomShare = 0.8,
                InitialPopulationGreedyShare = 0,
                InitialPopulationWarmStartShare = 0.2,
            };
            config.CountOffspring = config.PopulationSize;
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
            var comparer = new ConstrainedFitnessComparer<UnitType>(new ParetoDominanceComparer<UnitType>());

            return new GeneticAlgorithm<UnitType>(
                config,
                new PopulationInitializer<UnitType>(individualBuilder, repair),
                fitness,
                new CrowdedTournamentParentSelection<UnitType>(config.TournamentSize, comparer),
                new HalfAndFeasibleFillCrossover<UnitType>(constraint),
                mutation,
                new NondominatedSortingSurvivorSelection<UnitType>(comparer: comparer),
                OriginalGeneticAlgorithm.CreateStopCriterion(config),
                OriginalGaConfiguration.CreateRandom(config),
                repair);
        }
    }
}
