using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Permutation
{
    // Order crossover, OX (Davis, 1985): a random segment is copied from the first parent to the same positions.
    // The remaining positions, starting right after the segment and wrapping around, are filled with the genes
    // that are not in the child yet, in the order they appear in the second parent starting after the segment.
    // Keeps the relative order of genes rather than their positions. Repeated genes are told apart by their occurrence number.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class OrderCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;


        public OrderCrossover(IConstraint<TGene> constraint)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            int childSize = firstParent.Count;
            if (childSize == 0)
                return new Individual<TGene>(_constraint.CreateTracker());

            var positionsSecondParentGenesInFirstParent = CrossoverUtilities.FindPositionsOfSecondParentGenesInFirstParent(firstParent, secondParent);
           
            int segmentStart = random.Next(childSize + 1);
            int segmentEnd = random.Next(childSize + 1);
            if (segmentStart > segmentEnd)
                (segmentStart, segmentEnd) = (segmentEnd, segmentStart);

            //Set segment with first parent gene 
            var child = new int[childSize];
            for (int i = segmentStart; i < segmentEnd; i++)
                child[i] = i;

            //Set empty child gene using unused genes with second parent in order 
            int childPosition = segmentEnd == childSize ? 0 : segmentEnd;
            for (int offset = 0; offset < childSize; offset++)
            {
                int gene = positionsSecondParentGenesInFirstParent[(segmentEnd + offset) % childSize];
                if (gene >= segmentStart && gene < segmentEnd)
                    continue;
                child[childPosition] = gene;
                childPosition = (childPosition + 1) % childSize;
            }

            return Individual<TGene>.FromGenes(_constraint, CrossoverUtilities.GetFirstParentGenesAtPositions(firstParent, child));
        }
    }
}
