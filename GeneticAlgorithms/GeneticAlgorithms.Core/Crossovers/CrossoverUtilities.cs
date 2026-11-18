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
    }
}
