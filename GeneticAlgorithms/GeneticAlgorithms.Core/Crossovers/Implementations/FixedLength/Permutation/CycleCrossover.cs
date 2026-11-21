using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Permutation
{
    // Cycle crossover, CX (Oliver, Smith, Holland, 1987): the positions are split into cycles. A cycle starts at a free position,
    // takes the gene of the second parent there, moves to the position where the first parent holds that gene,
    // and repeats until it comes back. The first cycle keeps the genes of the first parent, the second cycle those of the second
    // parent, and so on alternately. Every gene keeps the position it has in one of the parents.
    // Repeated genes are told apart by their occurrence number.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class CycleCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        public bool RequiresParentsOfEqualLength => true;


        public CycleCrossover(IConstraint<TGene> constraint)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            var positionsSecondParentGenesInFirstParent = CrossoverUtilities.FindPositionsOfSecondParentGenesInFirstParent(firstParent, secondParent);

            var child = new int[firstParent.Count];
            var isVisited = new bool[firstParent.Count];
            bool fromFirst = true;
            for (int cycleStart = 0; cycleStart < firstParent.Count; cycleStart++)
            {
                if (isVisited[cycleStart])
                    continue;

                int position = cycleStart;
                do
                {
                    isVisited[position] = true;
                    child[position] = fromFirst ? position : positionsSecondParentGenesInFirstParent[position];
                    position = positionsSecondParentGenesInFirstParent[position];
                }
                while (!isVisited[position]);

                fromFirst = !fromFirst;
            }

            return Individual<TGene>.FromGenes(_constraint, CrossoverUtilities.GetFirstParentGenesAtPositions(firstParent, child));
        }
    }
}
