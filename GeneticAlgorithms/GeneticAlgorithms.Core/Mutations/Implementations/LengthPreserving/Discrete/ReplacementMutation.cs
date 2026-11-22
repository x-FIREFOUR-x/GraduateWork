using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Discrete
{
    // Replacement mutation, a point mutation: the gene at a random position is replaced by a random different gene
    // from AvailableGenes; the length is kept.
    // Unlike random resetting it changes exactly one gene, whatever the length, so its step does not grow with the individual.
    // The new gene is not checked against the constraint; an individual that breaks it is fixed by the repair of the genetic algorithm.
    // An empty individual, or one whose gene is the only available gene, is returned unchanged.
    //
    // Example: available genes [A, B, C], individual [A, B, A], position 1 -> [A, C, A].
    //
    // Complexity: time O(n + g)
    //             memory O(n)
    // n - the number of genes of the individual;
    // g - the number of distinct available genes.
    public sealed class ReplacementMutation<TGene> : IMutation<TGene>
    {
        public bool PreservesIndividualLength => true;

        // Distinct genes a replaced gene can become; at least one.
        public IReadOnlyList<TGene> AvailableGenes { get; }


        public ReplacementMutation(IReadOnlyList<TGene> availableGenes)
        {
            AvailableGenes = MutationUtilities.CreateDistinctGenes(availableGenes, nameof(availableGenes));
        }

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            if (individual.Count == 0)
                return individual;

            int index = random.Next(individual.Count);
            var newGene = MutationUtilities.ChooseDifferentGene(AvailableGenes, individual.Genes[index], random);
            if (EqualityComparer<TGene>.Default.Equals(newGene, individual.Genes[index]))
                return individual;

            var mutant = individual.Clone();
            mutant.Replace(index, newGene);
            return mutant;
        }
    }
}
