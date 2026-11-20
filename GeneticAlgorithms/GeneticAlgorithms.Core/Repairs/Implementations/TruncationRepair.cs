using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repairs.Implementations
{
    // Truncation repair: removes the last genes of the individual and appends the available genes in their given order,
    // so mostly the end of the individual is cut off.
    // Suits representations where the order of genes is a priority, for example the order of building or spawning:
    // the first genes are the most important and are never removed in favour of later ones,
    // and the available genes are given from the most to the least preferred.
    // The constraint decides what is fixed (see RepairUtilities.RepairConstraint):
    // 1. While upper bounds such as a budget are exceeded, the last gene whose removal helps is removed;
    //    removed genes that fit again are put back, so no gene is removed needlessly.
    // 2. While lower bounds such as a min count are not reached, the first available gene that fits and helps is appended.
    // 3. Only if FillRemainder is set, the first available gene that fits is appended while any fits, spending unused budget.
    // If RemoveUnavailableGenes is set, genes that are not among the available genes are removed first.
    // If nothing changes, the individual itself is returned.
    // The filling stops only when the constraint forbids every gene, so every gene must use up some of a limit, for example cost more than 0.
    //
    // Example: budget 10, gene cost = gene, genes [3, 6, 5, 1] -> 1 and 5 are removed, so [3, 6] fits; 1 is put back -> [3, 6, 1].
    //
    // Complexity: time O(r * n * c + (a + f) * g * c)
    //             memory O(n + g)
    // n - the number of genes of the individual;
    // r - the number of removed genes;
    // a - the number of genes appended for the lower bounds;
    // f - the number of genes appended by the filling;
    // g - the number of distinct available genes;
    // c - the time of one check or update of the constraint tracker.
    public sealed class TruncationRepair<TGene> : IRepair<TGene>
    {
        // Distinct genes that may be appended, from the most to the least preferred; at least one.
        public IReadOnlyList<TGene> AvailableGenes { get; }

        // Whether free limits, such as unused budget, are filled after the constraint is satisfied.
        public bool FillRemainder { get; }

        // Whether genes that are not among the available genes are removed first.
        public bool RemoveUnavailableGenes { get; }


        public TruncationRepair(IReadOnlyList<TGene> availableGenes, bool fillRemainder = false, bool removeUnavailableGenes = false)
        {
            AvailableGenes = RepairUtilities.CreateDistinctGenes(availableGenes, nameof(availableGenes));
            FillRemainder = fillRemainder;
            RemoveUnavailableGenes = removeUnavailableGenes;
        }

        public Individual<TGene> Repair(Individual<TGene> individual, IRandom random = null)
        {
            return RepairUtilities.RepairConstraint(
                individual,
                random,
                AvailableGenes,
                FillRemainder,
                RemoveUnavailableGenes,
                (genes, candidateIndices, _) => RepairUtilities.ChooseLastIndex(candidateIndices),
                (candidateGenes, _) => candidateGenes[0]);
        }
    }
}
