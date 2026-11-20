using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;

namespace GeneticAlgorithms.Mutations.Composite
{
    // Fixed-probability mutation selector: on every call one of the mutations is chosen by roulette wheel
    // with probabilities that never change, and the individual is mutated by it; the rest of the wheel means no mutation,
    // and then the individual itself is returned, so the genetic algorithm keeps the child without evaluating it again.
    // The baseline for AdaptiveMutationSelector: with the same mutations it shows how much the adaptation of probabilities gives.
    // The probabilities are absolute: each is in [0; 1] and their sum is at most 1; NoMutationProbability is 1 minus their sum.
    // Without probabilities every mutation has the same probability and some mutation is always applied.
    // The iteration is passed to the chosen mutation and feedback about the result to the last chosen one,
    // so nested adaptive selectors and iteration-dependent mutations keep working.
    //
    // Example: mutations [Swap, Inversion], probabilities [0.2, 0.3] -> Swap is applied in 20% of calls, Inversion in 30%,
    // and in the other 50% the individual is returned unchanged.
    //
    // Complexity: time O(m) plus the time of the chosen mutation
    //             memory O(m)
    // m - the number of mutations.
    public sealed class FixedProbabilityMutationSelector<TGene> : IMutation<TGene>, IMutationFeedback
    {
        private readonly double _sumMutationsProbabilities;
        private int _indexLastMutation = -1;

        // Mutations to choose from; at least one, none of them null.
        public IReadOnlyList<IMutation<TGene>> Mutations { get; }

        // Probability of choosing each mutation, in the order of the mutations; each in [0; 1], their sum at most 1.
        public IReadOnlyList<double> Probabilities { get; }

        // Probability that no mutation is applied and the individual is returned unchanged: 1 minus the sum of Probabilities.
        public double NoMutationProbability { get; }


        public FixedProbabilityMutationSelector(IReadOnlyList<IMutation<TGene>> mutations, IReadOnlyList<double> probabilities = null)
        {
            MutationUtilities.ValidateMutations(mutations, nameof(mutations));
            Mutations = mutations.ToArray();
            Probabilities = ProbabilityUtilities.CreateAbsoluteProbabilities(probabilities, Mutations.Count, nameof(probabilities));

            _sumMutationsProbabilities = ProbabilityUtilities.SumProbabilities(Probabilities);
            NoMutationProbability = 1 - _sumMutationsProbabilities;
        }

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            _indexLastMutation = -1;
            double roll = random.NextDouble();
            // The roll is beyond the segments of all mutations, so no mutation is applied.
            if (roll >= _sumMutationsProbabilities)
                return individual;

            _indexLastMutation = ProbabilityUtilities.ChooseIndexByProbability(Probabilities, roll);
            return Mutations[_indexLastMutation].Mutate(individual, random, currentIterationNumber);
        }

        public void Report(bool improved)
        {
            if (_indexLastMutation < 0)
                return;

            if (Mutations[_indexLastMutation] is IMutationFeedback nested)
                nested.Report(improved);

            _indexLastMutation = -1;
        }
    }
}
