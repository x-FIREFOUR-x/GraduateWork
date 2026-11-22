using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained
{
    // Half and feasible fill crossover, the child takes the first half of the first parent, then goes through the second parent
    // from its end and adds every gene that still satisfies the constraints. The genes of the second parent keep their order in the child.
    // Deterministic: the same parents always give the same child. Upper bounds of the constraints are never exceeded, lower bounds are left to the repair.
    //
    // Complexity: time O(n + m * (n + m))
    //             memory O(n + m)
    // n - the number of genes of the first parent;
    // m - the number of genes of the second parent.
    public sealed class HalfAndFeasibleFillCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        public bool RequiresParentsOfEqualLength => false;


        public HalfAndFeasibleFillCrossover(IConstraint<TGene> constraint)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);

            var child = new Individual<TGene>(_constraint.CreateTracker());

            int halfSizeFirstParent = firstParent.Count / 2;
            for (int i = 0; i < halfSizeFirstParent; i++)
                child.TryAppend(firstParent.Genes[i]);

            int startIndexForSecondParentGenes = child.Count;
            for (int j = secondParent.Count - 1; j >= 0; j--)
                child.TryInsert(startIndexForSecondParentGenes, secondParent.Genes[j]);

            return child;
        }
    }
}
