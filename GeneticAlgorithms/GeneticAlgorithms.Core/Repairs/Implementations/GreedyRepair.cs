using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repairs.Implementations
{
    // Greedy repair: removes the genes with the lowest priority and appends the genes with the highest priority,
    // so the repaired individual keeps the genes that matter most, as a greedy solution of the knapsack problem.
    // The priority of a gene is given by getGenePriority: genes with a higher priority are kept and appended first,
    // those with a lower one are removed first. For example, fitness per cost keeps the genes that give the most for their cost;
    // -cost(gene) removes the most expensive genes and appends the cheapest ones, so the fewest genes are removed
    // and the most are appended, ignoring how useful the genes are.
    // The constraint decides what is fixed (see RepairUtilities.RepairConstraint):
    // 1. While upper bounds such as a budget are exceeded, the gene with the lowest priority whose removal helps is removed;
    //    removed genes that fit again are put back, so no gene is removed needlessly.
    // 2. While lower bounds such as a min count are not reached, the available gene with the highest priority that fits and helps is appended.
    // 3. Only if FillRemainder is set, the available gene with the highest priority that fits is appended while any fits, spending unused budget.
    // If RemoveUnavailableGenes is set, genes that are not among the available genes are removed first.
    // Among genes of equal priority the later gene is removed and the first available gene is appended.
    // If nothing changes, the individual itself is returned.
    // The filling stops only when the constraint forbids every gene, so every gene must use up some of a limit, for example cost more than 0.
    //
    // Example: priority = gene, budget 10, gene cost = gene, at least 3 genes, available genes [1, 2, 4], genes [6, 5, 3] ->
    // 3 is removed, then 5, so [6] fits; 3 is put back -> [6, 3]; one gene is missing, the only one that fits is 1 -> [6, 3, 1].
    //
    // Complexity: time O(r * n * c + (a + f) * g * c)
    //             memory O(n + g)
    // n - the number of genes of the individual;
    // r - the number of removed genes;
    // a - the number of genes appended for the lower bounds;
    // f - the number of genes appended by the filling;
    // g - the number of distinct available genes;
    // c - the time of one check or update of the constraint tracker.
    public sealed class GreedyRepair<TGene> : IRepair<TGene>
    {
        private readonly Func<TGene, double> _getGenePriority;

        // Distinct genes that may be appended; at least one.
        public IReadOnlyList<TGene> AvailableGenes { get; }

        // Whether free limits, such as unused budget, are filled after the constraint is satisfied.
        public bool FillRemainder { get; }

        // Whether genes that are not among the available genes are removed first.
        public bool RemoveUnavailableGenes { get; }


        public GreedyRepair(
            IReadOnlyList<TGene> availableGenes,
            Func<TGene, double> getGenePriority,
            bool fillRemainder = false,
            bool removeUnavailableGenes = false)
        {
            AvailableGenes = RepairUtilities.CreateDistinctGenes(availableGenes, nameof(availableGenes));

            _getGenePriority = getGenePriority ?? throw new ArgumentNullException(nameof(getGenePriority));

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
                (genes, candidateIndices, _) => RepairUtilities.ChooseIndexWithLowestPriority(genes, candidateIndices, _getGenePriority),
                (candidateGenes, _) => RepairUtilities.ChooseGeneWithHighestPriority(candidateGenes, _getGenePriority));
        }
    }
}
