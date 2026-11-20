using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Permutation
{
    // Simple inversion mutation (Holland, 1975; see the survey of Larrañaga et al., 1999):
    // the genes of a random segment of at least two genes are put in reverse order.
    // Keeps the set of genes and all adjacencies inside the segment, breaking only the two at its ends,
    // which makes it the permutation analogue of the 2-opt move for routes.
    // Individuals with fewer than two genes are returned unchanged.
    //
    // Example: [A, B, C, D, E, F, G], segment [1; 5] -> [A, F, E, D, C, B, G].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class InversionMutation<TGene> : IMutation<TGene>
    {
        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            if (individual.Count < 2)
                return individual;

            var (startIndex, endIndex) = MutationUtilities.ChooseRandomTwoDistinctIndices(individual.Count, random);

            var mutant = individual.Clone();

            for (int left = startIndex, right = endIndex; left < right; left++, right--)
                mutant.Swap(left, right);

            return mutant;
        }
    }
}
