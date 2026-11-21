using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.RealValued
{
    // Whole arithmetic crossover (Michalewicz, 1992): every gene of the child is the weighted average of the parent genes,
    // child = FirstParentWeight * firstParent + (1 - FirstParentWeight) * secondParent. The child lies on the segment between the parents,
    // so it never leaves their range; FirstParentWeight = 0.5 gives the midpoint.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class WholeArithmeticCrossover : ICrossover<double>
    {
        private readonly IConstraint<double> _constraint;

        public bool RequiresParentsOfEqualLength => true;

        // Weight of the first parent in the average; in [0; 1].
        public double FirstParentWeight { get; }

        // Weight of the second parent in the average; in [0; 1].
        public double SecondParentWeight { get; }


        public WholeArithmeticCrossover(IConstraint<double> constraint, double firstParentWeight = 0.5)
        {
            if (!(firstParentWeight >= 0 && firstParentWeight <= 1))
                throw new ArgumentOutOfRangeException(nameof(firstParentWeight), firstParentWeight, "First parent weight must be between 0 and 1.");

            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            FirstParentWeight = firstParentWeight;
            SecondParentWeight = 1 - FirstParentWeight;
        }

        public Individual<double> Cross(Individual<double> firstParent, Individual<double> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);
            CrossoverUtilities.ValidateFiniteGenes(firstParent, secondParent);

            var child = new Individual<double>(_constraint.CreateTracker());
            for (int i = 0; i < firstParent.Count; i++)
                child.Append(FirstParentWeight * firstParent.Genes[i] + SecondParentWeight * secondParent.Genes[i]);
            return child;
        }
    }
}
