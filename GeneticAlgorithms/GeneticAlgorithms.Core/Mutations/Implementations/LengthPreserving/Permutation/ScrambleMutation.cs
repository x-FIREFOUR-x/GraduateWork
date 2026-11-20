using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Permutation
{
    // Scramble mutation (Syswerda, 1991; see the survey of Larrañaga et al., 1999):
    // the genes of a random segment of at least two genes are shuffled by Fisher-Yates; genes outside the segment stay in place.
    // Keeps the set of genes; a larger step than swap or insertion, since a whole segment loses its order.
    // The shuffle may give the same order, so the mutant may equal the individual.
    // Individuals with fewer than two genes are returned unchanged.
    //
    // Example: [A, B, C, D, E], segment [1; 3] -> [A, D, B, C, E].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class ScrambleMutation<TGene> : IMutation<TGene>
    {
        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);
            if (individual.Count < 2)
                return individual;

            var (startIndex, endIndex) = MutationUtilities.ChooseRandomTwoDistinctIndices(individual.Count, random);

            var mutant = individual.Clone();
            for (int i = endIndex; i > startIndex; i--)
                mutant.Swap(i, random.Next(startIndex, i + 1));

            return mutant;
        }
    }
}
