using System;
using System.Collections.Generic;
using GeneticAlgorithms.Constraints;
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
using GeneticAlgorithms.StopCriteria;
using GeneticAlgorithms.StopCriteria.Composite;
using GeneticAlgorithms.StopCriteria.Implementations;

namespace GeneticAlgorithms
{
    public static class OriginalGeneticAlgorithm
    {
        public static GeneticAlgorithm<TGene> Create<TGene>(
            IFitnessFunction<TGene> fitnessFunction,
            CostLimitConstraint<TGene> costLimitConstraint,
            IReadOnlyList<TGene> availableGenes,
            GeneticAlgorithmConfig config = null,
            IRandom random = null,
            Func<TGene, TGene, bool> sameKind = null)
        {
            return Create(fitnessFunction, costLimitConstraint, costLimitConstraint, availableGenes, config, random, sameKind);
        }

        public static GeneticAlgorithm<TGene> Create<TGene>(
            IFitnessFunction<TGene> fitnessFunction,
            IConstraint<TGene> constraint,
            ICostModel<TGene> costModel,
            IReadOnlyList<TGene> availableGenes,
            GeneticAlgorithmConfig config = null,
            IRandom random = null,
            Func<TGene, TGene, bool> sameKind = null)
        {
            config ??= CreateDefaultConfig();
            config.Validate();
            random ??= config.Seed.HasValue ? new SystemRandom(config.Seed.Value) : new SystemRandom();

            var fitnessPerCostCalculator = new FitnessPerCostCalculator<TGene>(fitnessFunction, costModel);
            var repair = new GreedyRepair<TGene>(availableGenes, fitnessPerCostCalculator.Calculate, fillRemainder: true, removeUnavailableGenes: true);
            var individualBuilder = new IndividualBuilder<TGene>(constraint, availableGenes, fitnessPerCostCalculator);
            var populationInitializer = new PopulationInitializer<TGene>(individualBuilder, repair);

            var mutation = new AdaptiveMutationSelector<TGene>(
                new IMutation<TGene>[]
                {
                    new RangeRegenerationMutation<TGene>(individualBuilder,config.MaxMutationShare),
                    new SingleTypeRangeMutation<TGene>(costModel, availableGenes, config.MaxMutationShare, sameKind, fillRemainder: true),
                    new SwapMutation<TGene>(),
                },
                config.MutationSmoothing,
                config.MutationWindow,
                config.MinMutationProbability);

            return new GeneticAlgorithm<TGene>(
                config,
                populationInitializer,
                fitnessFunction,
                new TournamentParentSelection<TGene>(config.TournamentSize),
                new HalfAndFeasibleFillCrossover<TGene>(constraint),
                mutation,
                new ReplaceWorstSurvivorSelection<TGene>(),
                CreateStopCriterion(config),
                random,
                repair);
        }

        // Config close to the genetic algorithm of the Tower Defense project: a population of 20, 1000 iterations without a stagnation stop,
        // PopulationSize - 1 children per iteration, as many as crossings of neighbouring individuals there, ranges of up to half
        // of the individual for the mutations, and a random initial population unless warm start genes are given.
        public static GeneticAlgorithmConfig CreateDefaultConfig()
        {
            var config = new GeneticAlgorithmConfig
            {
                PopulationSize = 20,
                MaxIterations = 1000,
                StagnationIterations = 50,
                MaxMutationShare = 0.5,
                InitialPopulationRandomShare = 0.8,
                InitialPopulationGreedyShare = 0,
                InitialPopulationWarmStartShare = 0.2,
            };
            config.CountOffspring = config.PopulationSize / 2;
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
