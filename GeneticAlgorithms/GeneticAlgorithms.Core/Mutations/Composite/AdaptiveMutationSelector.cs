using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;

namespace GeneticAlgorithms.Mutations.Composite
{
    // Adaptive mutation selector, probability matching of adaptive operator selection (Goldberg, 1990; Thierens, 2005):
    // on every call one of the mutations is chosen by roulette wheel, and the probabilities move towards the mutations
    // that improve children more often, so the search uses the mutations that work best on the current problem and stage.
    // After every report about the result of the last chosen mutation:
    // 1. The report is added to the window of the last Window reports; the oldest one leaves it.
    // 2. Every mutation gets the credit c_i, the number of its improvements in the window.
    // 3. p_i = (1 - Smoothing) * p_i + Smoothing * c_i / sum(c), so the probabilities move towards the shares of improvements.
    // 4. Every probability is raised to at least MinProbability, and the probabilities are normalised to sum to 1,
    //    so no mutation is ever dropped and it can come back when it starts to help again.
    // While the window has no improvements, the probabilities stay the same.
    // The genetic algorithm reports only when the mutant differs from the child, so calls that change nothing are not counted.
    // The credit is the number of improvements, not their share of the calls, so a mutation chosen more often gets more credit;
    // the probabilities still move towards the mutation with the higher rate of improvements, only slower when it starts low.
    // The iteration is passed to the chosen mutation and the report to it as well, so nested adaptive selectors keep working.
    //
    // Example: mutations [Swap, Inversion], probabilities [0.5, 0.5], Smoothing 0.2, the window holds 1 improvement by Swap
    // and 3 by Inversion -> shares [0.25, 0.75] -> probabilities [0.45, 0.55].
    //
    // Complexity: time O(m) per call and per report, plus the time of the chosen mutation
    //             memory O(m + w)
    // m - the number of mutations;
    // w - the window size.
    public sealed class AdaptiveMutationSelector<TGene> : IMutation<TGene>, IMutationFeedback
    {
        private readonly IReadOnlyList<IMutation<TGene>> _mutations;
        private readonly double[] _probabilities;

        private readonly int[] _improvements;
        private readonly Queue<(int Mutation, bool Improved)> _history;
        private int _indexLastMutation = -1;


        // How fast the probabilities follow the shares of improvements, in (0; 1): small values change them slowly and smoothly,
        // large values follow the latest reports.
        public double Smoothing { get; }

        // Number of the last reports the credit is counted over; at least 1. Older reports are forgotten,
        // so the selector follows the stage of the search.
        public int Window { get; }

        // Smallest probability of a mutation, so that every mutation is still tried; at most 1 divided by the number of mutations.
        public double MinProbability { get; }

        // Current probability of choosing each mutation, in the order of the mutations; they sum to 1.
        public IReadOnlyList<double> Probabilities => _probabilities;


        // initialProbabilities - starting point of Probabilities, in the order of the mutations; null gives every mutation the same probability.
        // They may be given either as probabilities or as weights: any non-negative finite numbers with a positive sum,
        // divided by their sum, so [0.25, 0.75] and [1, 3] give the same start. A sum below 1 does not mean "no mutation":
        // [0.2, 0.3] becomes [0.4, 0.6], and some mutation is always applied.
        public AdaptiveMutationSelector(
            IReadOnlyList<IMutation<TGene>> mutations,
            double smoothing = 0.2,
            int window = 50,
            double minProbability = 0.02,
            IReadOnlyList<double> initialProbabilities = null)
        {
            MutationUtilities.ValidateMutations(mutations, nameof(mutations));
            _mutations = mutations.ToArray();

            if (smoothing <= 0 || smoothing >= 1)
                throw new ArgumentOutOfRangeException(nameof(smoothing), smoothing, "Smoothing must be greater than 0 and less than 1.");
            Smoothing = smoothing;

            if (window < 1)
                throw new ArgumentOutOfRangeException(nameof(window), window, "Window must be at least 1.");
            Window = window;

            if (minProbability < 0 || minProbability * _mutations.Count > 1)
                throw new ArgumentOutOfRangeException(nameof(minProbability), minProbability, "Min probability must be non-negative and at most 1 divided by the number of mutations.");
            MinProbability = minProbability;

            _probabilities = CreateProbabilities(initialProbabilities, _mutations.Count, nameof(initialProbabilities));
            _improvements = new int[_mutations.Count];
            _history = new Queue<(int, bool)>(Window + 1);
        }

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            _indexLastMutation = MutationUtilities.ChooseIndexByProbability(_probabilities, random.NextDouble());
            return _mutations[_indexLastMutation].Mutate(individual, random, currentIterationNumber);
        }

        public void Report(bool improved)
        {
            if (_indexLastMutation < 0)
                return;

            _history.Enqueue((_indexLastMutation, improved));
            if (improved)
                _improvements[_indexLastMutation]++;

            if (_history.Count > Window)
            {
                var (mutation, wasImproved) = _history.Dequeue();
                if (wasImproved)
                    _improvements[mutation]--;
            }

            if (_mutations[_indexLastMutation] is IMutationFeedback nested)
                nested.Report(improved);

            UpdateProbabilities();
            _indexLastMutation = -1;
        }

        private void UpdateProbabilities()
        {
            int total = _improvements.Sum();
            if (total == 0)
                return;

            for (int i = 0; i < _probabilities.Length; i++)
                _probabilities[i] = (1 - Smoothing) * _probabilities[i] + Smoothing * _improvements[i] / total;

            for (int i = 0; i < _probabilities.Length; i++)
                _probabilities[i] = Math.Max(_probabilities[i], MinProbability);
            Normalize();
        }

        private void Normalize()
        {
            double sum = _probabilities.Sum();
            if (sum <= 0)
                throw new InvalidOperationException("Mutation probabilities must have a positive sum.");
            for (int i = 0; i < _probabilities.Length; i++)
                _probabilities[i] /= sum;
        }

        // Turns the given probabilities or weights into probabilities that sum to 1 by dividing each by their sum;
        // probabilities that already sum to 1 stay the same, null gives every mutation the same probability.
        // Example: [0.25, 0.75] -> [0.25, 0.75]; weights [1, 3] -> [0.25, 0.75]; [0.2, 0.3] -> [0.4, 0.6];
        // null, count 4 -> [0.25, 0.25, 0.25, 0.25].
        private static double[] CreateProbabilities(IReadOnlyList<double> probabilities, int mutationCount, string parameterName)
        {
            if (probabilities == null)
                return Enumerable.Repeat(1.0 / mutationCount, mutationCount).ToArray();
            if (probabilities.Count != mutationCount)
                throw new ArgumentException("Probabilities count must match mutations count.", parameterName);

            double sum = probabilities.Sum();
            if (probabilities.Any(p => !(p >= 0) || double.IsInfinity(p)) || sum <= 0)
                throw new ArgumentException($"Probabilities must be non-negative and finite, and their sum must be positive, but they sum to {sum}.", parameterName);

            return probabilities.Select(p => p / sum).ToArray();
        }
    }
}
