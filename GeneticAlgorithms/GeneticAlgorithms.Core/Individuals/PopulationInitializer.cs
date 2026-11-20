using System;
using System.Collections.Generic;
using GeneticAlgorithms.Randomness;
using GeneticAlgorithms.Repair;

namespace GeneticAlgorithms.Individuals
{
    public sealed class PopulationInitializer<TGene>
    {
        private readonly IndividualBuilder<TGene> _individualBuilder;
        private readonly IRepair<TGene> _repair;

        public int AttemptsPerIndividual { get; }


        public PopulationInitializer(IndividualBuilder<TGene> individualBuilder, IRepair<TGene> repair, int attemptsPerIndividual = 10)
        {
            _individualBuilder = individualBuilder ?? throw new ArgumentNullException(nameof(individualBuilder));
            _repair = repair ?? throw new ArgumentNullException(nameof(repair));
            if (attemptsPerIndividual < 1)
                throw new ArgumentOutOfRangeException(nameof(attemptsPerIndividual), attemptsPerIndividual, "Attempts per individual must be at least 1.");
            AttemptsPerIndividual = attemptsPerIndividual;
        }

        public List<Individual<TGene>> CreatePopulation(
            GeneticAlgorithmConfig config,
            IEnumerable<TGene> warmStartGenes,
            IRandom random)
        {
            int size = config.PopulationSize;
            double totalShare = config.InitialPopulationRandomShare + config.InitialPopulationGreedyShare + config.InitialPopulationWarmStartShare;

            int warmCount = warmStartGenes == null ? 0 : ShareToCount(config.InitialPopulationWarmStartShare, totalShare, size);
            int greedyCount = Math.Min(size - warmCount, ShareToCount(config.InitialPopulationGreedyShare, totalShare, size));

            var population = new List<Individual<TGene>>(size);

            if (warmCount > 0)
            {
                var warmStartIndividual = CreateIndividualUsingWarmStart(warmStartGenes, random);
                TryAddUniqueIndividualInPopulation(population, warmStartIndividual);
                FillPopulationUniqueIndividuals(population, warmCount, () => CreateIndividualVarianUsingWarmStart(warmStartIndividual, random));
            }

            if (greedyCount > 0)
            {
                TryAddUniqueIndividualInPopulation(population, _individualBuilder.CreateGreedy(1, random));
                FillPopulationUniqueIndividuals(population, warmCount + greedyCount, () => _individualBuilder.CreateGreedy(config.InitialPopulationGreedyCandidatePool, random));
            }

            FillPopulationUniqueIndividuals(population, size, () => _individualBuilder.CreateRandom(random));

            while (population.Count < size)
                population.Add(_individualBuilder.CreateRandom(random));

            return population;
        }

        private Individual<TGene> CreateIndividualUsingWarmStart(IEnumerable<TGene> genes, IRandom random)
        {
            return _repair.Repair(_individualBuilder.CreateFromGenes(genes), random);
        }

        private Individual<TGene> CreateIndividualVarianUsingWarmStart(Individual<TGene> warmStartIndividual, IRandom random)
        {
            Individual<TGene> individual = warmStartIndividual.Clone();
            if (individual.Count > 0)
            {
                int replacementStartIndex = random.Next(individual.Count / 2, individual.Count);
                individual.RemoveRange(replacementStartIndex, individual.Count - replacementStartIndex);
            }
            _individualBuilder.FillRandom(individual, individual.Count, random);
            return individual;
        }

        private static int ShareToCount(double share, double totalShare, int size)
        {
            if (share <= 0 || totalShare <= 0)
                return 0;
            return Math.Max(1, (int)Math.Round(size * share / totalShare));
        }

        private void FillPopulationUniqueIndividuals(List<Individual<TGene>> population, int targetPopulationSize, Func<Individual<TGene>> createIndividualFunc)
        {
            int attemptsRemaining = (targetPopulationSize - population.Count) * AttemptsPerIndividual;
            while (population.Count < targetPopulationSize && attemptsRemaining-- > 0)
                TryAddUniqueIndividualInPopulation(population, createIndividualFunc());
        }

        private static bool TryAddUniqueIndividualInPopulation(List<Individual<TGene>> population, Individual<TGene> individual)
        {
            foreach (var existing in population)
                if (existing.HasSameGenes(individual))
                    return false;
            population.Add(individual);
            return true;
        }
    }
}
