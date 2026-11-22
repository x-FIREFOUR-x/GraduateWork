using System;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.RealValued
{
    // Gaussian mutation, the mutation of evolution strategies (Rechenberg, 1973; Schwefel, 1981):
    // every gene, with probability GeneMutationProbability, gets a random addition N(0, StandardDeviation^2).
    // Small steps are frequent and large ones rare, so it refines good solutions; the step does not depend on the bounds.
    // Bounds: a mutated gene outside [MinGeneValue; MaxGeneValue] is clamped to the nearest bound;
    // without bounds this is the original unbounded mutation.
    // The normal value is drawn by the Box-Muller transform.
    // Genes of the individual must be finite and lie within the bounds. If no gene changes, the individual itself is returned.
    //
    // Example: standard deviation 0.5, individual [2.0, 7.5], only the second gene is chosen, addition -0.3 -> [2.0, 7.2].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class GaussianMutation : IMutation<double>
    {
        private const double TwoPi = 2 * Math.PI;

        public bool PreservesIndividualLength => true;


        // Smallest allowed gene value; mutated genes below it are clamped to it.
        public double MinGeneValue { get; }

        // Largest allowed gene value; mutated genes above it are clamped to it.
        public double MaxGeneValue { get; }

        // Probability that a gene is mutated, in (0; 1]; null means 1 / n, so on average one gene per individual.
        public double? GeneMutationProbability { get; }

        // Typical size of a step: about 68% of the additions lie within one standard deviation; positive and finite.
        public double StandardDeviation { get; }


        public GaussianMutation(
            double minGeneValue = double.NegativeInfinity,
            double maxGeneValue = double.PositiveInfinity,
            double standardDeviation = 1,
            double? geneMutationProbability = null
        )
        {
            MutationUtilities.ValidateGeneBounds(minGeneValue, maxGeneValue);
            MinGeneValue = minGeneValue;
            MaxGeneValue = maxGeneValue;

            MutationUtilities.ValidateGeneMutationProbability(geneMutationProbability);
            GeneMutationProbability = geneMutationProbability;

            if (!(standardDeviation > 0) || double.IsInfinity(standardDeviation))
                throw new ArgumentOutOfRangeException(nameof(standardDeviation), standardDeviation, "Standard deviation must be positive and finite.");
            StandardDeviation = standardDeviation;
        }

        public Individual<double> Mutate(Individual<double> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);
            MutationUtilities.ValidateGenesWithinBounds(individual, MinGeneValue, MaxGeneValue);

            return MutationUtilities.MutateEachGene(
                individual,
                GeneMutationProbability,
                random,
                gene => Math.Clamp(gene + StandardDeviation * NextGaussian(random), MinGeneValue, MaxGeneValue));
        }

        // Standard normal value N(0, 1) by the Box-Muller transform (Box, Muller, 1958).
        // 1 - NextDouble() lies in (0; 1], so the logarithm is always finite.
        // radius between [0; ≈ 8.57].
        private double NextGaussian(IRandom random)
        {
            double radius = Math.Sqrt(-2 * Math.Log(1 - random.NextDouble()));
            return radius * Math.Cos(TwoPi * random.NextDouble());
        }
    }
}
