using System;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;

namespace GeneticAlgorithms.Mutations.Composite
{
    // Probabilistic mutation: applies the nested mutation with probability Probability;
    // otherwise the individual itself is returned, and the genetic algorithm keeps the child without evaluating it again.
    // Gives the classic mutation rate per individual without changing the genetic algorithm,
    // for example mutating only 10% of children instead of every one.
    // The iteration and feedback about the result are passed to the nested mutation.
    //
    // Example: Probability 0.3 -> on average 3 of every 10 children are mutated.
    //
    // Complexity: time O(1) plus the time of the nested mutation when it is applied
    //             memory O(1)
    public sealed class ProbabilisticMutation<TGene> : IMutation<TGene>, IMutationFeedback
    {
        private readonly IMutation<TGene> _mutation;

        public bool PreservesIndividualLength => _mutation.PreservesIndividualLength;

        // Probability that the nested mutation is applied, in [0; 1]; 0 never mutates and 1 always does.
        public double Probability { get; }


        public ProbabilisticMutation(IMutation<TGene> mutation, double probability)
        {
            _mutation = mutation ?? throw new ArgumentNullException(nameof(mutation));

            ValidateProbability(probability);
            Probability = probability;
        }

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            if (random.NextDouble() >= Probability)
                return individual;
            return _mutation.Mutate(individual, random, currentIterationNumber);
        }

        public void Report(bool improved)
        {
            if (_mutation is IMutationFeedback feedback)
                feedback.Report(improved);
        }

        private void ValidateProbability(double probability)
        {
            if (!(probability >= 0 && probability <= 1))
                throw new ArgumentOutOfRangeException(nameof(probability), probability, "Probability must be between 0 and 1.");
        }
    }
}
