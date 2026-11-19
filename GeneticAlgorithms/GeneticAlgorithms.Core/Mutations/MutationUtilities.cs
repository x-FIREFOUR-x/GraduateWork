using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations
{
    internal static class MutationUtilities
    {
        public static void ValidateArgumentsNotNull<TGene>(Individual<TGene> individual, IRandom random)
        {
            if (individual == null)
                throw new ArgumentNullException(nameof(individual));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
        }

        // Null is allowed and means the classic rate 1 / n, where n is the number of genes of the mutated individual.
        public static void ValidateGeneMutationProbability(double? geneMutationProbability)
        {
            if (geneMutationProbability.HasValue && !(geneMutationProbability.Value >= 0 && geneMutationProbability.Value <= 1))
                throw new ArgumentOutOfRangeException(nameof(geneMutationProbability), geneMutationProbability, "Gene mutation probability must be greater between 0 and 1.");
        }

        public static void ValidateGeneBounds(double minGeneValue, double maxGeneValue)
        {
            if (double.IsNaN(minGeneValue))
                throw new ArgumentOutOfRangeException(nameof(minGeneValue), minGeneValue, "Min gene value must be a number.");
            if (double.IsNaN(maxGeneValue))
                throw new ArgumentOutOfRangeException(nameof(maxGeneValue), maxGeneValue, "Max gene value must be a number.");
            if (minGeneValue > maxGeneValue)
                throw new ArgumentException("Min gene value must not exceed max gene value.", nameof(minGeneValue));
        }

        // For mutations that draw values relative to the whole range [min; max], so the range must be finite.
        public static void ValidateFiniteGeneBounds(double minGeneValue, double maxGeneValue)
        {
            ValidateGeneBounds(minGeneValue, maxGeneValue);
            if (double.IsInfinity(minGeneValue))
                throw new ArgumentOutOfRangeException(nameof(minGeneValue), minGeneValue, "Min gene value must be finite.");
            if (double.IsInfinity(maxGeneValue))
                throw new ArgumentOutOfRangeException(nameof(maxGeneValue), maxGeneValue, "Max gene value must be finite.");
        }

        public static void ValidateGenesWithinBounds(Individual<double> individual, double minGeneValue, double maxGeneValue)
        {
            foreach (double gene in individual.Genes)
            {
                if (double.IsNaN(gene) || double.IsInfinity(gene) || gene < minGeneValue || gene > maxGeneValue)
                    throw new ArgumentException("Real-valued mutation requires finite genes values between the min and max gene values.", nameof(individual));
            }

        }

        public static void ValidateMaxShare(double maxShare)
        {
            if (!(maxShare > 0 && maxShare <= 1))
                throw new ArgumentOutOfRangeException(nameof(maxShare), maxShare, "Max share must be greater than 0 and at most 1.");
        }

        // Returns the available genes without repeats, so that every gene is chosen with the same probability.
        // Example: [A, B, A, C] -> [A, B, C].
        public static List<TGene> CreateDistinctGenes<TGene>(IReadOnlyList<TGene> availableGenes, string parameterName)
        {
            if (availableGenes == null)
                throw new ArgumentNullException(parameterName);

            var genes = availableGenes.Distinct().ToList();
            if (genes.Count == 0)
                throw new ArgumentException("At least one available gene is required.", parameterName);
            return genes;
        }

        // Chooses a random gene that differs from the current one, so that a mutation really changes the gene.
        // If the current gene is not among the genes, any gene may be chosen;
        // if it is the only gene, it is returned and the gene stays the same.
        // Example: genes [A, B, C], current B -> A or C.
        public static TGene ChooseDifferentGene<TGene>(IReadOnlyList<TGene> genesVariants, TGene currentGene, IRandom random)
        {
            var comparer = EqualityComparer<TGene>.Default;
            int currentIndex = -1;
            for (int i = 0; i < genesVariants.Count; i++)
            {
                if (comparer.Equals(genesVariants[i], currentGene))
                {
                    currentIndex = i;
                    break;
                }
            }
            if (currentIndex < 0)
                return genesVariants[random.Next(genesVariants.Count)];
            if (genesVariants.Count == 1)
                return currentGene;

            int index = random.Next(genesVariants.Count - 1);
            if (index >= currentIndex)
                index++;

            return genesVariants[index];
        }

        // Mutates every gene on its own with the given probability (null means 1 / individualSize) and returns the mutant.
        // The individual is cloned only when the first gene really changes; if no gene changes, the individual itself is returned,
        // so the genetic algorithm can skip evaluating a mutant equal to its source.
        // Example: genes [1, 2, 3], probability 0.5, only the second gene is chosen and becomes 5 -> [1, 5, 3].
        public static Individual<TGene> MutateEachGene<TGene>(
            Individual<TGene> individual,
            double? geneMutationProbability,
            IRandom random,
            Func<TGene, TGene> getMutatedGene)
        {
            if (individual.Count == 0)
                return individual;

            double probability = geneMutationProbability ?? 1.0 / individual.Count;

            var comparer = EqualityComparer<TGene>.Default;

            Individual<TGene> mutant = null;
            for (int i = 0; i < individual.Count; i++)
            {
                if (random.NextDouble() >= probability)
                    continue;

                var gene = individual.Genes[i];
                var mutatedGene = getMutatedGene(gene);
                if (comparer.Equals(gene, mutatedGene))
                    continue;

                mutant ??= individual.Clone();
                mutant.Replace(i, mutatedGene);
            }
            return mutant ?? individual;
        }

        // Chooses two different indices from 0 to count - 1 and returns them in ascending order;
        // every pair is equally likely. The count must be at least 2.
        // Example: count 5 -> (1, 4).
        public static (int StartIndex, int EndIndex) ChooseRandomTwoDistinctIndices(int count, IRandom random)
        {
            int startIndex = random.Next(count);
            int endIndex = random.Next(count - 1);
            if (endIndex >= startIndex)
                endIndex++;
            return startIndex < endIndex ? (startIndex, endIndex) : (endIndex, startIndex);
        }

        // Chooses a random non-empty range of genes: a random start and a length from 1 to ceil(maxShare * count),
        // cut so that the range does not go past the last gene. The count must be positive.
        // Example: count 10, maxShare 0.3, start 8 -> length 1 or 2.
        public static (int StartIndex, int RangeLength) ChooseRandomRange(int count, double maxShareRenge, IRandom random)
        {
            int startIndex = random.Next(count);
            int maxRangeLength = Math.Max(1, (int)Math.Ceiling(maxShareRenge * count));
            maxRangeLength = Math.Min(maxRangeLength, count - startIndex);
            int rangeLength = random.Next(1, maxRangeLength + 1);
            return (startIndex, rangeLength);
        }
    }
}
