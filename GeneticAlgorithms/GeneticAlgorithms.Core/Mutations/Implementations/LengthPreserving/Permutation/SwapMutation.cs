using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Permutation
{
    // Swap mutation, also called exchange mutation (Banzhaf, 1990; see the survey of Larrañaga et al., 1999):
    // two different random positions exchange their genes.
    // Keeps the set of genes and changes the positions of only two of them, so it is the smallest step for permutations.
    // Individuals with fewer than two genes are returned unchanged.
    //
    // Example: [A, B, C, D, E], positions 1 and 3 -> [A, D, C, B, E].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual; both are spent on cloning it.
    public sealed class SwapMutation<TGene> : IMutation<TGene>
    {
        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);
            if (individual.Count < 2)
                return individual;

            var (firstIndex, secondIndex) = MutationUtilities.ChooseRandomTwoDistinctIndices(individual.Count, random);

            var mutant = individual.Clone();
            mutant.Swap(firstIndex, secondIndex);

            return mutant;
        }
    }
}
