using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Permutation
{
    // Position-based crossover (Syswerda, 1991): each position takes the gene of the first parent with probability FirstParentGeneProbability.
    // The other positions are filled, from left to right, with the genes that are not in the child yet,
    // in the order they appear in the second parent. Like Order Crossover (OX), but the genes taken from the first parent
    // are scattered instead of forming one segment, and filling starts from the first position.
    // Repeated genes are told apart by their occurrence number.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class PositionBasedCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        // Probability that a position of the child takes the gene of the first parent at the same position; in [0; 1].
        public double FirstParentGeneProbability { get; }


        public PositionBasedCrossover(IConstraint<TGene> constraint, double firstParentGeneProbability = 0.5)
        {
            if (!(firstParentGeneProbability >= 0 && firstParentGeneProbability <= 1))
                throw new ArgumentOutOfRangeException(nameof(firstParentGeneProbability), firstParentGeneProbability, "First parent gene probability must be between 0 and 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            FirstParentGeneProbability = firstParentGeneProbability;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            var positionsSecondParentGenesInFirstParent = CrossoverUtilities.FindPositionsOfSecondParentGenesInFirstParent(firstParent, secondParent);

            int childSize = firstParent.Count;
            var child = new int[childSize];
            var isTakenFromFirstParent = new bool[childSize];
            for (int i = 0; i < childSize; i++)
            {
                if (random.NextDouble() >= FirstParentGeneProbability)
                    continue;
                child[i] = i;
                isTakenFromFirstParent[i] = true;
            }

            int secondParentPosition = 0;
            for (int i = 0; i < childSize; i++)
            {
                if (isTakenFromFirstParent[i])
                    continue;
                while (isTakenFromFirstParent[positionsSecondParentGenesInFirstParent[secondParentPosition]])
                    secondParentPosition++;
                child[i] = positionsSecondParentGenesInFirstParent[secondParentPosition++];
            }

            return Individual<TGene>.FromGenes(_constraint, CrossoverUtilities.GetFirstParentGenesAtPositions(firstParent, child));
        }
    }
}
