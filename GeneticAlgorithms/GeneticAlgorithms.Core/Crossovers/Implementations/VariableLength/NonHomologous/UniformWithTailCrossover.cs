using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.VariableLength.NonHomologous
{
    // Uniform crossover with a tail: the positions both parents have are crossed uniformly, each gene is taken from the second
    // parent with probability SecondParentGeneProbability and from the first parent otherwise. The tail of the longer parent, the genes
    // beyond the length of the shorter one, is appended as a whole with probability TailProbability.
    // So the child is as long as either the shorter or the longer parent.
    //
    // Complexity: time O(n + m)
    //             memory O(n + m)
    // n - the number of genes of the first parent;
    // m - the number of genes of the second parent.
    public sealed class UniformWithTailCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        public bool RequiresParentsOfEqualLength => false;

        // Probability that a gene of the common part is taken from the second parent; in [0; 1].
        public double SecondParentGeneProbability { get; }

        // Probability that the tail of the longer parent is appended to the child; in [0; 1].
        public double TailProbability { get; }


        public UniformWithTailCrossover(IConstraint<TGene> constraint, double secondParentGeneProbability = 0.5, double tailProbability = 0.5)
        {
            if (!(secondParentGeneProbability >= 0 && secondParentGeneProbability <= 1))
                throw new ArgumentOutOfRangeException(nameof(secondParentGeneProbability), secondParentGeneProbability, "Second parent gene probability must be between 0 and 1.");
            if (!(tailProbability >= 0 && tailProbability <= 1))
                throw new ArgumentOutOfRangeException(nameof(tailProbability), tailProbability, "Tail probability must be between 0 and 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            SecondParentGeneProbability = secondParentGeneProbability;
            TailProbability = tailProbability;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);

            int sizeSmallerParent = Math.Min(firstParent.Count, secondParent.Count);
            var child = new Individual<TGene>(_constraint.CreateTracker());
            for (int i = 0; i < sizeSmallerParent; i++)
                child.Append(random.NextDouble() < SecondParentGeneProbability ? secondParent.Genes[i] : firstParent.Genes[i]);

            var longerParent = firstParent.Count > secondParent.Count ? firstParent : secondParent;
            if (longerParent.Count > sizeSmallerParent && random.NextDouble() < TailProbability)
                for (int i = sizeSmallerParent; i < longerParent.Count; i++)
                    child.Append(longerParent.Genes[i]);
            return child;
        }
    }
}
