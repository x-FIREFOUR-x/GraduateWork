using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Permutation
{
    // Partially mapped crossover, PMX (Goldberg, Lingle, 1985): the child takes a random segment from the first parent
    // and the other positions from the second parent, so genes keep their absolute positions from both parents.
    // 1. The segment is copied from the first parent to the same positions.
    // 2. A gene of the second parent's segment that is not in the child yet has been pushed out by the segment.
    //    It goes to the position where the second parent holds the gene that pushed it out (the first parent gene
    //    at its position); if that position is inside the segment too, the same step is repeated from there.
    // 3. All other positions are copied from the second parent.
    // Repeated genes are told apart by their occurrence number.
    // Example: first [A, B, C, D, E, F, G, H, I], second [I, C, G, H, B, F, E, A, D], segment positions 3 .. 6:
    // child [_, _, _, D, E, F, G, _, _]; H was pushed out by D, D is at 8 in the second parent, so H goes to 8;
    // B was pushed out by E, E is at 6 (inside the segment), there G, G is at 2, so B goes to 2;
    // the rest from the second parent -> [I, C, B, D, E, F, G, A, H].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class PartiallyMappedCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;


        public PartiallyMappedCrossover(IConstraint<TGene> constraint)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            int childSize = firstParent.Count;

            var positionsSecondParentGenesInFirstParent = CrossoverUtilities.FindPositionsOfSecondParentGenesInFirstParent(firstParent, secondParent);
            var positionsFirstParentGenesInSecondParent = new int[childSize];
            for (int i = 0; i < childSize; i++)
                positionsFirstParentGenesInSecondParent[positionsSecondParentGenesInFirstParent[i]] = i;

            int segmentStart = random.Next(childSize + 1);
            int segmentEnd = random.Next(childSize + 1);
            if (segmentStart > segmentEnd)
                (segmentStart, segmentEnd) = (segmentEnd, segmentStart);

            var child = new int[childSize];
            var isFilled = new bool[childSize];
            for (int i = segmentStart; i < segmentEnd; i++)
            {
                child[i] = i;
                isFilled[i] = true;
            }

            for (int i = segmentStart; i < segmentEnd; i++)
            {
                int gene = positionsSecondParentGenesInFirstParent[i];
                if (gene >= segmentStart && gene < segmentEnd)
                    continue;

                int position = i;
                do
                    position = positionsFirstParentGenesInSecondParent[position];
                while (position >= segmentStart && position < segmentEnd);

                child[position] = gene;
                isFilled[position] = true;
            }

            for (int i = 0; i < childSize; i++)
                if (!isFilled[i])
                    child[i] = positionsSecondParentGenesInFirstParent[i];

            return Individual<TGene>.FromGenes(_constraint, CrossoverUtilities.GetFirstParentGenesAtPositions(firstParent, child));
        }
    }
}
