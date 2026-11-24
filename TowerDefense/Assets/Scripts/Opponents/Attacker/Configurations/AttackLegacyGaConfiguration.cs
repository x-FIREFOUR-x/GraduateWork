using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.InitialPopulation;
using GeneticAlgorithms.Mutations.Composite;
using GeneticAlgorithms.Mutations.Implementations.LengthChanging.Constrained;
using GeneticAlgorithms.Selection.Parent.Implementations.SingleObjective.Rank;
using GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.SteadyState;

using TowerDefense.Main.Units;


namespace TowerDefense.Opponents.Attacker.Configurations
{
    public static class AttackLegacyGaConfiguration
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

        public static GeneticAlgorithm<UnitType> Create(IFitnessFunction<UnitType> fitness, CostLimitConstraint<UnitType> constraint, IReadOnlyList<UnitType> availableUnits, GeneticAlgorithmConfig config)
        {
            config.Validate();
            var fitnessPerCostCalculator = new FitnessPerCostCalculator<UnitType>(fitness, constraint);
            var individualBuilder = new IndividualBuilder<UnitType>(constraint, availableUnits, fitnessPerCostCalculator);

            return new GeneticAlgorithm<UnitType>(
                config,
                new PopulationInitializer<UnitType>(individualBuilder),
                fitness,
                new AdjacentPairsParentSelection<UnitType>(),
                new HalfAndFeasibleFillCrossover<UnitType>(constraint),
                new ProbabilisticMutation<UnitType>(new RangeRegenerationMutation<UnitType>(individualBuilder, config.MaxMutationShare), MutationProbability),
                new BestChildReplacesWorstSurvivorSelection<UnitType>(),
                OriginalGeneticAlgorithm.CreateStopCriterion(config),
                AttackOriginalGaConfiguration.CreateRandom(config));
        }
    }
}
