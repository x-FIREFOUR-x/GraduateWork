using System;
using System.Collections.Generic;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Permutation
{
    // Order-based crossover (Syswerda, 1991): the child is a copy of the first parent in which some genes
    // are rearranged among themselves to follow their order in the second parent.
    // 1. Each gene of the second parent is chosen with probability SelectionProbability.
    // 2. The positions of the chosen genes in the first parent are freed.
    // 3. The freed positions are filled from left to right with the chosen genes in their second parent order.
    // Not chosen genes keep their positions from the first parent; the chosen ones keep the set of positions
    // from the first parent and take their relative order from the second parent.
    // Repeated genes are told apart by their occurrence number.
    // Example: first [A, B, C, D, E, F, G, A, F], second [D, G, B, F, A, C, E, F, B],
    // chosen G, F, C, F in second parent: freed positions 2, 5, 6, 8 in first perent
    // [A, B,  , D, E,  ,  , A,  ] are filled with G, F, C, F -> [A, B, G, D, E, F, C, A, F]
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class OrderBasedCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        public bool RequiresParentsOfEqualLength => true;

        // Probability that a gene of the second parent passes its relative order to the child; in [0; 1].
        public double SelectionProbability { get; }


        public OrderBasedCrossover(IConstraint<TGene> constraint, double selectionProbability = 0.5)
        {
            if (!(selectionProbability >= 0 && selectionProbability <= 1))
                throw new ArgumentOutOfRangeException(nameof(selectionProbability), selectionProbability, "Selection probability must be between 0 and 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            SelectionProbability = selectionProbability;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            int childSize = firstParent.Count;

            var positionsSecondParentGenesInFirstParent = CrossoverUtilities.FindPositionsOfSecondParentGenesInFirstParent(firstParent, secondParent);
            
            var isSelected = new bool[childSize];
            var selectedGenesInSecondParentOrder = new List<int>();
            foreach (int gene in positionsSecondParentGenesInFirstParent)
            {
                if (random.NextDouble() >= SelectionProbability)
                    continue;
                isSelected[gene] = true;
                selectedGenesInSecondParentOrder.Add(gene);
            }

            var child = CrossoverUtilities.CreateSequentialIndices(childSize);
            int selectedGeneIndex = 0;
            for (int i = 0; i < childSize; i++)
                if (isSelected[i])
                    child[i] = selectedGenesInSecondParentOrder[selectedGeneIndex++];

            return Individual<TGene>.FromGenes(_constraint, CrossoverUtilities.GetFirstParentGenesAtPositions(firstParent, child));
        }
    }
}
