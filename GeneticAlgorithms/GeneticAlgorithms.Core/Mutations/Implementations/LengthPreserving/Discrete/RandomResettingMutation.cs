using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Discrete
{
    // Random resetting mutation (see Eiben, Smith, Introduction to Evolutionary Computing, 2003):
    // every gene, with probability GeneMutationProbability, is replaced by a random different gene from AvailableGenes.
    // The discrete analogue of bit-flip mutation: for genes true and false it is bit-flip itself.
    // The length is kept and genes may repeat, so the result is generally not a permutation of the individual.
    // The new gene is not checked against the constraint; an individual that breaks it is fixed by the repair of the genetic algorithm.
    // If no gene changes, the individual itself is returned.
    //
    // Example: genes alphabet [A, B, C], individual [A, A, B], only the second gene is chosen -> [A, C, B].
    //
    // Complexity: time O(n * g) for the search of the current gene among the available ones
    //             memory O(n)
    // n - the number of genes of the individual;
    // g - the number of distinct available genes.
    public sealed class RandomResettingMutation<TGene> : IMutation<TGene>
    {
        // Distinct genes a mutated gene can become; at least one.
        public IReadOnlyList<TGene> AvailableGenes { get; }

        // Probability that a gene is mutated, in (0; 1]; null means 1 / IndividualSize, so on average one gene per individual.
        public double? GeneMutationProbability { get; }


        public RandomResettingMutation(IReadOnlyList<TGene> availableGenes, double? geneMutationProbability = null)
        {
            MutationUtilities.ValidateGeneMutationProbability(geneMutationProbability);
            GeneMutationProbability = geneMutationProbability;

            AvailableGenes = MutationUtilities.CreateDistinctGenes(availableGenes, nameof(availableGenes));
        }

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            return MutationUtilities.MutateEachGene(
                individual,
                GeneMutationProbability,
                random,
                gene => MutationUtilities.ChooseDifferentGene(AvailableGenes, gene, random));
        }
    }
}
