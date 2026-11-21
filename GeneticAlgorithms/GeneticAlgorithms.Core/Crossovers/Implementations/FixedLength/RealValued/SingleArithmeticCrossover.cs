using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.RealValued
{
    // Single arithmetic crossover (Michalewicz, 1992): the child is a copy of the first parent except one random gene,
    // which becomes the weighted average FirstParentWeight * firstParent + (1 - FirstParentWeight) * secondParent. The smallest possible arithmetic change.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class SingleArithmeticCrossover : ICrossover<double>
    {
        private readonly IConstraint<double> _constraint;

        public bool RequiresParentsOfEqualLength => true;

        // Weight of the first parent in the average of the chosen gene; in [0; 1].
        public double FirstParentWeight { get; }

        // Weight of the second parent in the average of the chosen gene; in [0; 1].
        public double SecondParentWeight { get; }

        public SingleArithmeticCrossover(IConstraint<double> constraint, double firstParentWeight = 0.5)
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

            int averagePosition = firstParent.Count == 0 ? 0 : random.Next(firstParent.Count);
            var child = new Individual<double>(_constraint.CreateTracker());
            for (int i = 0; i < firstParent.Count; i++)
                child.Append(i == averagePosition ? FirstParentWeight * firstParent.Genes[i] + SecondParentWeight * secondParent.Genes[i] : firstParent.Genes[i]);
            return child;
        }
    }
}
