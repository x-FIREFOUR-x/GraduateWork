using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Permutation
{
    // Insertion mutation (Fogel, 1988; see the survey of Larrañaga et al., 1999):
    // a gene at a random position is taken out and put back at another random position; the genes in between shift by one.
    // Keeps the set of genes and the relative order of all other genes, so it changes adjacency less than swap mutation.
    // The new position is counted after the gene is taken out and always differs from the old one.
    // Individuals with fewer than two genes are returned unchanged.
    //
    // Example: [A, B, C, D, E], gene at 1 moved to 3 -> [A, C, D, B, E].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class InsertMutation<TGene> : IMutation<TGene>
    {
        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            if (individual.Count < 2)
                return individual;

            int index = random.Next(individual.Count);
            int newIndex = random.Next(individual.Count - 1);
            if (newIndex >= index)
                newIndex++;

            var mutant = individual.Clone();
            mutant.Move(index, newIndex);
            return mutant;
        }
    }
}
