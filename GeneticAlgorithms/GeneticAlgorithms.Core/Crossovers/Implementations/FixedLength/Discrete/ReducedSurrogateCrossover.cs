using System;
using System.Collections.Generic;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Discrete
{
    // Reduced surrogate crossover (Booker, 1987): k-point crossover whose cut points are placed only between positions
    // where the parents have different genes. Cuts between equal genes would change nothing, so whenever the parents differ
    // in at least two positions the child differs from both of them, and no crossover is wasted on producing a parent copy.
    //
    // Complexity: time O(n + k * log k)
    //             memory O(n + k)
    // n - the number of genes of a parent;
    // k - the cut point count.
    public sealed class ReducedSurrogateCrossover<TGene> : ICrossover<TGene>
    {
        private static readonly EqualityComparer<TGene> GeneComparer = EqualityComparer<TGene>.Default;

        private readonly IConstraint<TGene> _constraint;

        public bool RequiresParentsOfEqualLength => true;

        // How many cut points split the sequence of differing positions; at least 1.
        public int CutPointCount { get; }


        public ReducedSurrogateCrossover(IConstraint<TGene> constraint, int cutPointCount = 1)
        {
            if (cutPointCount < 1)
                throw new ArgumentOutOfRangeException(nameof(cutPointCount), cutPointCount, "Cut point count must be at least 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            CutPointCount = cutPointCount;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            var positionsOfDifferentGenes = new List<int>();
            for (int position = 0; position < firstParent.Count; position++)
                if (!GeneComparer.Equals(firstParent.Genes[position], secondParent.Genes[position]))
                    positionsOfDifferentGenes.Add(position);
            var cutPointsIndeces = CrossoverUtilities.CreateRandomDistinctCutPoints(positionsOfDifferentGenes.Count, CutPointCount, random);

            var takeFromSecondParentAtPosition = new bool[firstParent.Count];
            for (int segment = 1; segment <= cutPointsIndeces.Length; segment += 2)
            {
                int segmentStart = cutPointsIndeces[segment - 1];
                int segmentEnd = segment < cutPointsIndeces.Length ? cutPointsIndeces[segment] : positionsOfDifferentGenes.Count;
                for (int positionIndex = segmentStart; positionIndex < segmentEnd; positionIndex++)
                    takeFromSecondParentAtPosition[positionsOfDifferentGenes[positionIndex]] = true;
            }

            var child = new Individual<TGene>(_constraint.CreateTracker());
            for (int position = 0; position < firstParent.Count; position++)
                child.Append(takeFromSecondParentAtPosition[position] ? secondParent.Genes[position] : firstParent.Genes[position]);
            return child;
        }
    }
}
