using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Discrete
{
    // Uniform crossover (Syswerda, 1989): each gene of the child is taken from the second parent with probability SecondParentGeneProbability
    // and from the first parent otherwise, independently of the other genes. Unlike k-point crossover,
    // it does not keep neighbouring genes together.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class UniformCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        // Probability that a gene is taken from the second parent (the swap probability of the classic two-child form); in [0; 1].
        public double SecondParentGeneProbability { get; }


        public UniformCrossover(IConstraint<TGene> constraint, double secondParentGeneProbability = 0.5)
        {
            if (!(secondParentGeneProbability >= 0 && secondParentGeneProbability <= 1))
                throw new ArgumentOutOfRangeException(nameof(secondParentGeneProbability), secondParentGeneProbability, "Second parent gene probability must be between 0 and 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            SecondParentGeneProbability = secondParentGeneProbability;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            var child = new Individual<TGene>(_constraint.CreateTracker());
            for (int i = 0; i < firstParent.Count; i++)
                child.Append(random.NextDouble() < SecondParentGeneProbability ? secondParent.Genes[i] : firstParent.Genes[i]);
            return child;
        }
    }
}
