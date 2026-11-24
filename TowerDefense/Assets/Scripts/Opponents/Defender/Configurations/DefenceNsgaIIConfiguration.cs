using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.InitialPopulation;
using GeneticAlgorithms.Selection.Parent.Implementations.MultiObjective;
using GeneticAlgorithms.Selection.Survivor.Implementations.MultiObjective;

using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Configurations
{
    public static class DefenceNsgaIIConfiguration
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
                InitialPopulationGreedyShare = 0.2,
                InitialPopulationWarmStartShare = 0,
            };
            config.CountOffspring = config.PopulationSize;
            return config;
        }

        public static GeneticAlgorithm<DefenderGene> Create(IFitnessFunction<DefenderGene> fitness, CostLimitConstraint<DefenderGene> costConstraint, IConstraint<DefenderGene> constraint, IReadOnlyList<DefenderGene> candidateGenes, GeneticAlgorithmConfig config)
        {
            config.Validate();
            var fitnessPerCostCalculator = new FitnessPerCostCalculator<DefenderGene>(fitness, costConstraint);
            var repair = DefenceOriginalGaConfiguration.CreateRepair(candidateGenes, fitnessPerCostCalculator);
            var individualBuilder = new IndividualBuilder<DefenderGene>(constraint, candidateGenes, fitnessPerCostCalculator);
            var comparer = new ConstrainedFitnessComparer<DefenderGene>(new ParetoDominanceComparer<DefenderGene>());

            return new GeneticAlgorithm<DefenderGene>(
                config,
                new PopulationInitializer<DefenderGene>(individualBuilder, repair),
                fitness,
                new CrowdedTournamentParentSelection<DefenderGene>(config.TournamentSize, comparer),
                new HalfAndFeasibleFillCrossover<DefenderGene>(constraint),
                DefenceOriginalGaConfiguration.CreateMutation(costConstraint, candidateGenes, individualBuilder, config),
                new NondominatedSortingSurvivorSelection<DefenderGene>(comparer: comparer),
                OriginalGeneticAlgorithm.CreateStopCriterion(config),
                DefenceOriginalGaConfiguration.CreateRandom(config),
                repair);
        }
    }
}
