using System;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthChanging.Constrained
{
    // Range regeneration mutation: a random range of genes is removed, and the free place is filled again
    // with random genes that still satisfy the constraint, as the individual builder creates a random individual.
    // 1. A random start is chosen and a length from 1 to ceil(MaxShare * n), cut so that the range does not go past the last gene.
    // 2. The range is removed, which frees part of the budget or other limits of the constraint.
    // 3. At the place of the range, a random gene among those that can still be appended is inserted, again and again,
    //    until no gene fits the constraint.
    // So the mutation keeps the rest of the individual and builds a new feasible part in place of the old one;
    // the new part may be shorter or longer than the removed range, since it fills all the limits that are left.
    // Upper bounds of the constraint are never exceeded. An empty individual is filled from scratch, like a new random individual.
    // The filling stops only when the constraint forbids every gene, so every gene must use up some of a limit, for example cost more than 0.
    //
    // Example: budget 10, gene cost = gene, available genes [1, 2, 3], individual [3, 3, 2, 2], range [3, 2] removed ->
    // [3, 2] with 5 of the budget left, then 2 and 3 are inserted -> [3, 2, 3, 2].
    //
    // Complexity: time O(n + k * (g + n))
    //             memory O(n + g)
    // n - the number of genes of the individual;
    // k - the number of inserted genes;
    // g - the number of distinct available genes of the individual builder.
    public sealed class RangeRegenerationMutation<TGene> : IMutation<TGene>
    {
        private readonly IndividualBuilder<TGene> _individualBuilder;

        // Largest share of the genes the removed range may take, in (0; 1]; the range always has at least one gene.
        public double MaxShare { get; }

        public RangeRegenerationMutation(IndividualBuilder<TGene> individualBuilder, double maxShare = 0.5)
        {
            _individualBuilder = individualBuilder ?? throw new ArgumentNullException(nameof(individualBuilder));

            MutationUtilities.ValidateMaxShare(maxShare);
            MaxShare = maxShare;
        }

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            var mutant = individual.Clone();
            if (mutant.Count == 0)
            {
                _individualBuilder.FillRandom(mutant, 0, random);
                return mutant;
            }

            var (startRangeIndex, rangeLength) = MutationUtilities.ChooseRandomRange(mutant.Count, MaxShare, random);

            mutant.RemoveRange(startRangeIndex, rangeLength);
            _individualBuilder.FillRandom(mutant, startRangeIndex, random);

            return mutant;
        }
    }
}
