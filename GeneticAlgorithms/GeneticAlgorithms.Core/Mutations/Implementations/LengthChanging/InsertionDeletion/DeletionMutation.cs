using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthChanging.InsertionDeletion
{
    // Deletion mutation: the gene at a random position is removed, so the individual becomes one gene shorter.
    // Together with addition mutation it lets the search change the length of variable-length individuals.
    // Removing a gene may break a lower bound of the constraint, for example a minimal number of genes;
    // such an individual is fixed by the repair of the genetic algorithm. An empty individual is returned unchanged.
    //
    // Example: [A, B, C, D], position 2 -> [A, B, D].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class DeletionMutation<TGene> : IMutation<TGene>
    {
        public bool PreservesIndividualLength => false;

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            if (individual.Count == 0)
                return individual;

            var mutant = individual.Clone();
            mutant.RemoveAt(random.Next(individual.Count));

            return mutant;
        }
    }
}
