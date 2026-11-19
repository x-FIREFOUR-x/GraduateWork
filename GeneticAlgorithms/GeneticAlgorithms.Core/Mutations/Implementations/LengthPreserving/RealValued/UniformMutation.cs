using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.RealValued
{
    // Uniform mutation (Michalewicz, 1992): every gene, with probability GeneMutationProbability,
    // is replaced by a value drawn uniformly from [MinGeneValue; MaxGeneValue), independent of its old value.
    // The real-valued analogue of random resetting: steps may be as large as the whole range,
    // so it keeps diversity but does not refine good solutions.
    // Genes of the individual must be finite and lie within the bounds. If no gene changes, the individual itself is returned.
    //
    // Example: bounds [0; 10], individual [2.0, 7.5, 4.0], only the first gene is chosen -> [8.3, 7.5, 4.0].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class UniformMutation : IMutation<double>
    {
        // Smallest value a mutated gene can take; finite.
        public double MinGeneValue { get; }

        // Upper bound of the values a mutated gene can take; finite.
        public double MaxGeneValue { get; }

        // Probability that a gene is mutated, in [0; 1]; null means 1 / n, so on average one gene per individual.
        public double? GeneMutationProbability { get; }


        public UniformMutation(double minGeneValue, double maxGeneValue, double? geneMutationProbability = null)
        {
            MutationUtilities.ValidateFiniteGeneBounds(minGeneValue, maxGeneValue);
            MinGeneValue = minGeneValue;
            MaxGeneValue = maxGeneValue;

            MutationUtilities.ValidateGeneMutationProbability(geneMutationProbability);
            GeneMutationProbability = geneMutationProbability;
        }

        public Individual<double> Mutate(Individual<double> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);
            MutationUtilities.ValidateGenesWithinBounds(individual, MinGeneValue, MaxGeneValue);
            return MutationUtilities.MutateEachGene(
                individual,
                GeneMutationProbability,
                random,
                gene => MinGeneValue + random.NextDouble() * (MaxGeneValue - MinGeneValue));
        }
    }
}
