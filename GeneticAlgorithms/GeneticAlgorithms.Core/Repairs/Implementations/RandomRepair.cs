using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repairs.Implementations
{
    // Random repair: removes and appends random genes, every candidate being equally likely.
    // It prefers no gene, so it keeps the diversity of the population and serves as the baseline for the other repairs.
    // The constraint decides what is fixed (see RepairUtilities.RepairConstraint):
    // 1. While upper bounds such as a budget are exceeded, a random gene whose removal helps is removed;
    //    removed genes that fit again are put back, so no gene is removed needlessly.
    // 2. While lower bounds such as a min count are not reached, a random available gene that fits and helps is appended.
    // 3. Only if FillRemainder is set, a random available gene that fits is appended while any fits, spending unused budget.
    // If RemoveUnavailableGenes is set, genes that are not among the available genes are removed first.
    // If nothing changes, the individual itself is returned. It requires the random number generator, so calling it with null throws.
    // The filling stops only when the constraint forbids every gene, so every gene must use up some of a limit, for example cost more than 0.
    //
    // Example: budget 10, gene cost = gene, genes [3, 6, 5] -> 6 is removed at random, the rest fits -> [3, 5].
    //
    // Complexity: time O(r * n * c + (a + f) * g * c)
    //             memory O(n + g)
    // n - the number of genes of the individual;
    // r - the number of removed genes;
    // a - the number of genes appended for the lower bounds;
    // f - the number of genes appended by the filling;
    // g - the number of distinct available genes;
    // c - the time of one check or update of the constraint tracker.
    public sealed class RandomRepair<TGene> : IRepair<TGene>
    {
        public bool PreservesIndividualLength => false;

        // Distinct genes that may be appended; at least one.
        public IReadOnlyList<TGene> AvailableGenes { get; }

        // Whether free limits, such as unused budget, are filled after the constraint is satisfied.
        public bool FillRemainder { get; }

        // Whether genes that are not among the available genes are removed first.
        public bool RemoveUnavailableGenes { get; }


        public RandomRepair(IReadOnlyList<TGene> availableGenes, bool fillRemainder = false, bool removeUnavailableGenes = false)
        {
            AvailableGenes = RepairUtilities.CreateDistinctGenes(availableGenes, nameof(availableGenes));
            FillRemainder = fillRemainder;
            RemoveUnavailableGenes = removeUnavailableGenes;
        }

        public Individual<TGene> Repair(Individual<TGene> individual, IRandom random = null)
        {
            RepairUtilities.ValidateRandomNotNull(random);

            return RepairUtilities.RepairConstraint(
                individual,
                random,
                AvailableGenes,
                FillRemainder,
                RemoveUnavailableGenes,
                (genes, candidateIndices, rnd) => candidateIndices[rnd.Next(candidateIndices.Count)],
                (candidateGenes, rnd) => candidateGenes[rnd.Next(candidateGenes.Count)]);
        }
    }
}
