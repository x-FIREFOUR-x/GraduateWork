using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Crossovers.Implementations.VariableLength.NonHomologous;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.InitialPopulation;
using GeneticAlgorithms.Mutations;
using GeneticAlgorithms.Mutations.Composite;
using GeneticAlgorithms.Mutations.Implementations.LengthChanging.InsertionDeletion;
using GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Discrete;
using GeneticAlgorithms.Randomness;
using GeneticAlgorithms.Repairs.Implementations;
using GeneticAlgorithms.Selection.Parent.Implementations.MultiObjective;
using GeneticAlgorithms.Selection.Survivor.Implementations.MultiObjective;
using GeneticAlgorithms.StopCriteria;
using GeneticAlgorithms.StopCriteria.Composite;
using GeneticAlgorithms.StopCriteria.Implementations;

namespace GeneticAlgorithms
{
    public static class NsgaII
    {
        public static GeneticAlgorithm<TGene> Create<TGene>(
            IFitnessFunction<TGene> fitnessFunction,
            CostLimitConstraint<TGene> costLimitConstraint,
            IReadOnlyList<TGene> availableGenes,
            GeneticAlgorithmConfig config = null,
            IRandom random = null)
        {
            return Create(fitnessFunction, costLimitConstraint, costLimitConstraint, availableGenes, config, random);
        }

        public static GeneticAlgorithm<TGene> Create<TGene>(
            IFitnessFunction<TGene> fitnessFunction,
            IConstraint<TGene> constraint,
            ICostModel<TGene> costModel,
            IReadOnlyList<TGene> availableGenes,
            GeneticAlgorithmConfig config = null,
            IRandom random = null)
        {
            config ??= CreateDefaultConfig();
            config.Validate();
            random ??= config.Seed.HasValue ? new SystemRandom(config.Seed.Value) : new SystemRandom();

            var fitnessPerCostCalculator = new FitnessPerCostCalculator<TGene>(fitnessFunction, costModel);
            var repair = new GreedyRepair<TGene>(availableGenes, fitnessPerCostCalculator.Calculate);
            var individualBuilder = new IndividualBuilder<TGene>(constraint, availableGenes, fitnessPerCostCalculator);
            var populationInitializer = new PopulationInitializer<TGene>(individualBuilder, repair);

            var mutation = new FixedProbabilityMutationSelector<TGene>(
                new IMutation<TGene>[]
                {
                    new AdditionMutation<TGene>(availableGenes),
                    new DeletionMutation<TGene>(),
                    new ReplacementMutation<TGene>(availableGenes),
                });

            var comparer = new ConstrainedFitnessComparer<TGene>(new ParetoDominanceComparer<TGene>());
            return new GeneticAlgorithm<TGene>(
                config,
                populationInitializer,
                fitnessFunction,
                new CrowdedTournamentParentSelection<TGene>(config.TournamentSize, comparer),
                new CutAndSpliceCrossover<TGene>(constraint),
                mutation,
                new NondominatedSortingSurvivorSelection<TGene>(comparer: comparer),
                CreateStopCriterion(config),
                random,
                repair);
        }

        public static GeneticAlgorithmConfig CreateDefaultConfig()
        {
            var config = new GeneticAlgorithmConfig
            {
                PopulationSize = 20,
                MaxIterations = 1000,
                StagnationIterations = null,
                TournamentSize = 2,
                AcceptMutationOnlyIfNotWorse = false,
                InitialPopulationRandomShare = 0.8,
                InitialPopulationGreedyShare = 0,
                InitialPopulationWarmStartShare = 0.2,
            };
            config.CountOffspring = config.PopulationSize;
            return config;
        }

        public static IStopCriterion CreateStopCriterion(GeneticAlgorithmConfig config)
        {
            var criteria = new List<IStopCriterion>();
            if (config.MaxIterations.HasValue)
                criteria.Add(new MaxIterationsStopCriterion(config.MaxIterations.Value));
            if (config.TimeLimit.HasValue)
                criteria.Add(new TimeLimitStopCriterion(config.TimeLimit.Value));
            if (config.StagnationIterations.HasValue)
                criteria.Add(new StagnationStopCriterion(config.StagnationIterations.Value));
            return criteria.Count == 1 ? criteria[0] : new CompositeStopCriterion(criteria.ToArray());
        }
    }
}
