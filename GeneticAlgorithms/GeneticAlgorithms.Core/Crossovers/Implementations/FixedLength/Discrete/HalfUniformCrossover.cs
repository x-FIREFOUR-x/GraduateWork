using System;
using System.Collections.Generic;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Discrete
{
    // Half uniform crossover, HUX (Eshelman, 1991, CHC algorithm): among the positions where the parents have different genes,
    // exactly half (rounded down), chosen at random, take the gene of the second parent; all other genes come from the first parent.
    // So the child is equally far from both parents, which keeps the search from collapsing onto one of them.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class HalfUniformCrossover<TGene> : ICrossover<TGene>
    {
        private static readonly EqualityComparer<TGene> GeneComparer = EqualityComparer<TGene>.Default;

        private readonly IConstraint<TGene> _constraint;

        public bool RequiresParentsOfEqualLength => true;


        public HalfUniformCrossover(IConstraint<TGene> constraint)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            var positionsOfDifferentGenes = new List<int>();
            for (int i = 0; i < firstParent.Count; i++)
                if (!GeneComparer.Equals(firstParent.Genes[i], secondParent.Genes[i]))
                    positionsOfDifferentGenes.Add(i);
            CrossoverUtilities.Shuffle(positionsOfDifferentGenes, random);

            var takeFromSecond = new bool[firstParent.Count];
            for (int i = 0; i < positionsOfDifferentGenes.Count / 2; i++)
                takeFromSecond[positionsOfDifferentGenes[i]] = true;

            var child = new Individual<TGene>(_constraint.CreateTracker());
            for (int i = 0; i < firstParent.Count; i++)
                child.Append(takeFromSecond[i] ? secondParent.Genes[i] : firstParent.Genes[i]);
            return child;
        }
    }
}
