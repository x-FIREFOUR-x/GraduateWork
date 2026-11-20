using System;
using System.Collections.Generic;
using System.Linq;


namespace GeneticAlgorithms.Randomness
{
    internal static class ProbabilityUtilities
    {
        private const double RoundingErrorTolerance = 1e-9;


        // Absolute probabilities of choosing each of count options, for roulette wheel selectors whose remaining share means "choose none".
        // Each probability must be in [0; 1] and their sum at most 1; null gives every option the same probability, summing to 1.
        // Example: [0.2, 0.3], count 2 -> [0.2, 0.3], and 0.5 is left for choosing none; null, count 4 -> [0.25, 0.25, 0.25, 0.25].
        public static double[] CreateAbsoluteProbabilities(IReadOnlyList<double> probabilities, int count, string parameterName)
        {
            if (probabilities == null)
                return Enumerable.Repeat(1.0 / count, count).ToArray();

            if (probabilities.Count != count)
                throw new ArgumentException($"Probabilities count {probabilities.Count} must match the count of options {count}.", parameterName);

            if (probabilities.Any(p => !(p >= 0 && p <= 1)))
                throw new ArgumentException("Every probability must be between 0 and 1.", parameterName);

            double sum = probabilities.Sum();
            if (sum > 1 + RoundingErrorTolerance)
                throw new ArgumentException($"Probabilities must sum to at most 1, but they sum to {sum}.", parameterName);

            return probabilities.ToArray();
        }

        // Sum of the probabilities, rounded to exactly 1 when it differs from 1 only by a rounding error,
        // so that probabilities meant to sum to 1, such as [0.1, 0.2, 0.7], never leave a tiny share for choosing none.
        public static double SumProbabilities(IReadOnlyList<double> probabilities)
        {
            double sum = probabilities.Sum();
            return sum >= 1 - RoundingErrorTolerance ? 1 : sum;
        }

        // Roulette wheel: returns the index i whose segment [p_0 + ... + p_(i-1); p_0 + ... + p_i) contains the roll,
        // so for a roll uniform in [0; 1) index i is returned with probability probabilities[i].
        // The roll must lie below the sum of the probabilities; it is a parameter, so the caller can use the same roll for other decisions.
        // If rounding leaves the roll above the last cumulative sum, the last index with a positive probability is returned,
        // so an option with zero probability is never chosen.
        // Example: probabilities [0.2, 0.8], roll 0.5 -> 1.
        public static int ChooseIndexByProbability(IReadOnlyList<double> probabilities, double roll)
        {
            double cumulative = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                cumulative += probabilities[i];
                if (roll < cumulative)
                    return i;
            }
            for (int i = probabilities.Count - 1; i > 0; i--)
                if (probabilities[i] > 0)
                    return i;
            return 0;
        }
    }
}
