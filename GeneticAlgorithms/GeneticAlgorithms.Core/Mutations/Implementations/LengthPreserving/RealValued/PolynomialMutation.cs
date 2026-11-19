using System;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.RealValued
{
    // Bounded polynomial mutation (Deb, Agrawal, 1995; Deb, Goyal, 1996; bounded form as in the NSGA-II code of Deb):
    // the mutation pair of simulated binary crossover. The mutated gene is drawn from a polynomial distribution
    // centred on the old gene: the larger DistributionIndex, the closer it stays to the old gene.
    //
    // For every chosen gene x, with R = MaxGeneValue - MinGeneValue and u uniform in [0; 1):
    // 1. If R = 0, the gene is kept.
    // 2. For u < 0.5 the gene moves towards MinGeneValue:
    //      s = 1 - (x - MinGeneValue) / R,  v = 2u + (1 - 2u) * s^(eta + 1),  shift = v^(1 / (eta + 1)) - 1;
    //    otherwise it moves towards MaxGeneValue:
    //      s = 1 - (MaxGeneValue - x) / R,  v = 2(1 - u) + 2(u - 0.5) * s^(eta + 1),  shift = 1 - v^(1 / (eta + 1)).
    // 3. The mutated gene is x + shift * R.
    // eta - DistributionIndex;
    // s - how far the gene is from the bound it moves to, as a share of the range, subtracted from 1 (lowerShare, upperShare);
    // shift - the step as a share of the range, in [-1; 1].
    //
    // Bounds: the terms with s narrow the distribution near a bound, so mutated genes never leave the bounds
    // and do not pile up on them; the clamp only guards against rounding.
    // Genes of the individual must lie within the bounds. If no gene changes, the individual itself is returned.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class PolynomialMutation : IMutation<double>
    {
        // Smallest allowed gene value; finite, the distribution is narrowed so that genes never go below it.
        public double MinGeneValue { get; }

        // Largest allowed gene value; finite, the distribution is narrowed so that genes never go above it.
        public double MaxGeneValue { get; }

        // How close mutated genes stay to the old ones: large values give small steps; non-negative, usually 20 to 100.
        public double DistributionIndex { get; }

        // Probability that a gene is mutated, in (0; 1]; null means 1 / n, so on average one gene per individual.
        public double? GeneMutationProbability { get; }


        public PolynomialMutation(
            double minGeneValue,
            double maxGeneValue,
            double distributionIndex = 20,
            double? geneMutationProbability = null)
        {
            MutationUtilities.ValidateFiniteGeneBounds(minGeneValue, maxGeneValue);
            MinGeneValue = minGeneValue;
            MaxGeneValue = maxGeneValue;

            if (!(distributionIndex >= 0) || double.IsInfinity(distributionIndex))
                throw new ArgumentOutOfRangeException(nameof(distributionIndex), distributionIndex, "Distribution index must be non-negative and finite.");
            DistributionIndex = distributionIndex;

            MutationUtilities.ValidateGeneMutationProbability(geneMutationProbability);
            GeneMutationProbability = geneMutationProbability;
        }

        public Individual<double> Mutate(Individual<double> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);
            MutationUtilities.ValidateGenesWithinBounds(individual, MinGeneValue, MaxGeneValue);

            return MutationUtilities.MutateEachGene(individual, GeneMutationProbability, random, gene => MutateGene(gene, random));
        }

        private double MutateGene(double gene, IRandom random)
        {
            double range = MaxGeneValue - MinGeneValue;
            if (range == 0)
                return gene;

            double power = DistributionIndex + 1;
            double exponent = 1 / power;
            double u = random.NextDouble();
            double shift;
            if (u < 0.5)
            {
                double lowerShare = 1 - (gene - MinGeneValue) / range;
                double value = 2 * u + (1 - 2 * u) * Math.Pow(lowerShare, power);
                shift = Math.Pow(value, exponent) - 1;
            }
            else
            {
                double upperShare = 1 - (MaxGeneValue - gene) / range;
                double value = 2 * (1 - u) + 2 * (u - 0.5) * Math.Pow(upperShare, power);
                shift = 1 - Math.Pow(value, exponent);
            }
            return Math.Clamp(gene + shift * range, MinGeneValue, MaxGeneValue);
        }
    }
}
