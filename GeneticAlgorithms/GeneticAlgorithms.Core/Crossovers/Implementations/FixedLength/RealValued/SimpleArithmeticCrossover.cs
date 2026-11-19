using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.RealValued
{
    // Simple arithmetic crossover (Michalewicz, 1992): a random position is drawn; the genes before it are copied
    // from the first parent, and from it on each gene is the weighted average FirstParentWeight * firstParent + (1 - FirstParentWeight) * secondParent.
    // A mix of one-point and whole arithmetic crossover.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class SimpleArithmeticCrossover : ICrossover<double>
    {
        private readonly IConstraint<double> _constraint;

        // Weight of the first parent in the average of the genes after the drawn position; in [0; 1].
        public double FirstParentWeight { get; }

        // Weight of the second parent in the average of the genes after the drawn position; in [0; 1].
        public double SecondParentWeight { get; }


        public SimpleArithmeticCrossover(IConstraint<double> constraint, double firstParentWeight = 0.5)
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

            int averageStart = firstParent.Count == 0 ? 0 : random.Next(firstParent.Count);
            var child = new Individual<double>(_constraint.CreateTracker());
            for (int i = 0; i < firstParent.Count; i++)
                child.Append(i < averageStart ? firstParent.Genes[i] : FirstParentWeight * firstParent.Genes[i] + SecondParentWeight * secondParent.Genes[i]);
            return child;
        }
    }
}
