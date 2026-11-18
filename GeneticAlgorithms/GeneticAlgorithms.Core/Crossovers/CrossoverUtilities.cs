using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers
{
    internal static class CrossoverUtilities
    {
        public static void ValidateArgumentsNotNull<TGene>(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            if (firstParent == null)
                throw new ArgumentNullException(nameof(firstParent));
            if (secondParent == null)
                throw new ArgumentNullException(nameof(secondParent));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
        }

        public static void ValidateParentsHaveEqualLength<TGene>(Individual<TGene> firstParent, Individual<TGene> secondParent)
        {
            if (firstParent.Count != secondParent.Count)
                throw new ArgumentException($"Fixed-length crossover requires parents of equal length, but the parents have {firstParent.Count} and {secondParent.Count} genes.");
        }

        public static void ValidateFiniteGenes(Individual<double> firstParent, Individual<double> secondParent)
        {
            if (!HasOnlyFiniteValuesGenes(firstParent))
                throw new ArgumentException("Real-valued crossover requires finite values genes.", nameof(firstParent));
            if (!HasOnlyFiniteValuesGenes(secondParent))
                throw new ArgumentException("Real-valued crossover requires finite values genes.", nameof(secondParent));
        }

        public static void ValidateGenesWithinBounds(Individual<double> firstParent, Individual<double> secondParent, double minGeneValue, double maxGeneValue)
        {
            if (!HasOnlyGenesWithinBounds(firstParent, minGeneValue, maxGeneValue))
                throw new ArgumentException("Bounded real-valued crossover requires parent genes values between the min and max gene values.", nameof(firstParent));
            if (!HasOnlyGenesWithinBounds(secondParent, minGeneValue, maxGeneValue))
                throw new ArgumentException("Bounded real-valued crossover requires parent genes values between the min and max gene values.", nameof(secondParent));
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

        // Turns the second parent into numbers that permutation crossovers can work with:
        // each gene is replaced by the position of the same gene in the first parent.
        // If a gene repeats, its k-th occurrence in the second parent gets the position of its k-th occurrence in the first parent,
        // so all numbers are different. The first parent itself becomes 0, 1, ..., n - 1.
        // Example: first [A, B, A, C], second [C, A, A, B] -> [3, 0, 2, 1].
        public static int[] FindPositionsOfSecondParentGenesInFirstParent<TGene>(Individual<TGene> firstParent, Individual<TGene> secondParent)
        {
            var positionsByGeneInFirstParent = new Dictionary<TGene, Queue<int>>();
            for (int i = 0; i < firstParent.Count; i++)
            {
                if (!positionsByGeneInFirstParent.TryGetValue(firstParent.Genes[i], out var positions))
                {
                    positions = new Queue<int>();
                    positionsByGeneInFirstParent.Add(firstParent.Genes[i], positions);
                }
                positions.Enqueue(i);
            }

            var secondParentPermutation = new int[secondParent.Count];
            for (int i = 0; i < secondParent.Count; i++)
            {
                if (!positionsByGeneInFirstParent.TryGetValue(secondParent.Genes[i], out var positions) || positions.Count == 0)
                    throw new ArgumentException("Permutation crossover requires parents that are permutations of the same genes.");
                secondParentPermutation[i] = positions.Dequeue();
            }
            return secondParentPermutation;
        }

        // Returns the genes of the first parent at the given positions; turns a child built by a permutation crossover back into genes.
        // Example: first [A, B, A, C], positions for permutations [0, 3, 1, 2] -> [A, C, B, A].
        public static List<TGene> GetFirstParentGenesAtPositions<TGene>(Individual<TGene> firstParent, int[] positionsForPermutations)
        {
            var genes = new List<TGene>(positionsForPermutations.Length);
            foreach (int position in positionsForPermutations)
                genes.Add(firstParent.Genes[position]);
            return genes;
        }

        // Returns 0, 1, ..., count - 1: the first parent in the numbers of FindPositionsOfSecondParentGenesInFirstParent, or the positions in their original order.
        public static int[] CreateSequentialIndices(int count)
        {
            var permutation = new int[count];
            for (int i = 0; i < count; i++)
                permutation[i] = i;
            return permutation;
        }

        // Creates indexCount different random indices from 0 to maxIndexExclusive - 1 and returns them in ascending order.
        // If indexCount is larger than maxIndexExclusive, all indices from 0 to maxIndexExclusive - 1 are returned.
        // Floyd's algorithm: calls random only indexCount times, without shuffling all indices; every set of indices is equally likely.
        // Example: maxIndexExclusive 10, indexCount 3 -> [2, 5, 8].
        public static int[] CreateRandomSortedIndices(int maxIndexExclusive, int indexCount, IRandom random)
        {
            indexCount = Math.Min(indexCount, maxIndexExclusive);
            var randomIndices = new HashSet<int>();
            for (int largestAllowedIndex = maxIndexExclusive - indexCount; largestAllowedIndex < maxIndexExclusive; largestAllowedIndex++)
            {
                int index = random.Next(largestAllowedIndex + 1);
                randomIndices.Add(randomIndices.Contains(index) ? largestAllowedIndex : index);
            }

            var sortedIndices = new int[randomIndices.Count];
            randomIndices.CopyTo(sortedIndices);
            Array.Sort(sortedIndices);
            return sortedIndices;
        }

        // Creates different random cut points for parents of the same length, in ascending order.
        // Cut point c means the cut goes right before the gene at position c, so only 1 ... length - 1 really split the parent.
        // If the parent is too short for cutPointCount cuts, there are only length - 1 of them.
        // Example: length 6, cutPointCount 2 -> [2, 5], so the segments are [0, 2), [2, 5), [5, 6).
        public static int[] CreateRandomDistinctCutPoints(int length, int cutPointCount, IRandom random)
        {
            var cutPoints = CreateRandomSortedIndices(Math.Max(length - 1, 0), cutPointCount, random);
            for (int i = 0; i < cutPoints.Length; i++)
                cutPoints[i]++;
            return cutPoints;
        }

        // Creates random cut points for one parent of a variable-length crossover, in ascending order.
        // The points may repeat and may be 0 or length, so a segment between them may be empty or the whole parent;
        // this is what lets the child be shorter or longer than its parents.
        // Example: length 5, cutPointCount 2 -> [0, 3], so the segments are [0, 0) empty, [0, 3), [3, 5).
        public static int[] CreateRandomCutPoints(int length, int cutPointCount, IRandom random)
        {
            var cutPoints = new int[cutPointCount];
            for (int i = 0; i < cutPointCount; i++)
                cutPoints[i] = random.Next(length + 1);
            Array.Sort(cutPoints);
            return cutPoints;
        }

        // Appends to the child the segments between the cut points, taking them in turn from the first and the second parent:
        // segment 0 from the first parent, segment 1 from the second, segment 2 from the first, and so on.
        // Each parent is cut at its own points: segment i of the first parent ends at firstCutPoints[i],
        // segment i of the second parent ends at secondCutPoints[i]. Equal points for both parents give k-point crossover.
        // Example: first [A, B, C, D] cut at [1], second [E, F, G] cut at [2] -> [A] + [G] = [A, G].
        public static void AppendSegmentsAlternatingParents<TGene>(
            Individual<TGene> child,
            Individual<TGene> firstParent,
            Individual<TGene> secondParent,
            int[] firstCutPoints,
            int[] secondCutPoints)
        {
            for (int segment = 0; segment <= firstCutPoints.Length; segment++)
            {
                bool fromFirst = segment % 2 == 0;
                var currentParent = fromFirst ? firstParent : secondParent;
                var cutPoints = fromFirst ? firstCutPoints : secondCutPoints;

                int segmentStart = segment == 0 ? 0 : cutPoints[segment - 1];
                int segmentEnd = segment == cutPoints.Length ? currentParent.Count : cutPoints[segment];

                for (int i = segmentStart; i < segmentEnd; i++)
                    child.Append(currentParent.Genes[i]);
            }
        }

        public static void Shuffle<T>(IList<T> items, IRandom random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }

        private static bool HasOnlyFiniteValuesGenes(Individual<double> parent)
        {
            foreach (double gene in parent.Genes)
                if (double.IsNaN(gene) || double.IsInfinity(gene))
                    return false;
            return true;
        }

        private static bool HasOnlyGenesWithinBounds(Individual<double> parent, double minGeneValue, double maxGeneValue)
        {
            foreach (double gene in parent.Genes)
                if (gene < minGeneValue || gene > maxGeneValue)
                    return false;
            return true;
        }
    }
}
