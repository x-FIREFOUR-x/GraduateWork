using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthChanging.InsertionDeletion
{
    // Addition mutation, also called insertion of a new gene: a random gene from AvailableGenes
    // is inserted at a random position from 0 to n, so the individual becomes one gene longer.
    // Together with deletion mutation it lets the search change the length of variable-length individuals.
    // The new gene is not checked against the constraint; an individual that breaks it, for example goes over the budget,
    // is fixed by the repair of the genetic algorithm.
    //
    // Example: available genes [X, Y], individual [A, B, C], gene Y at position 1 -> [A, Y, B, C].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class AdditionMutation<TGene> : IMutation<TGene>
    {
        public bool PreservesIndividualLength => false;

        // Distinct genes that can be inserted, each with the same probability; at least one.
        public IReadOnlyList<TGene> AvailableGenes { get; }


        public AdditionMutation(IReadOnlyList<TGene> availableGenes)
        {
            AvailableGenes = MutationUtilities.CreateDistinctGenes(availableGenes, nameof(availableGenes));
        }

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            var mutant = individual.Clone();
            mutant.Insert(random.Next(individual.Count + 1), AvailableGenes[random.Next(AvailableGenes.Count)]);

            return mutant;
        }
    }
}
