using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repair.Implementations
{
    // Downgrade repair: a gene that breaks an upper bound is replaced by a cheaper gene of the same kind instead of being removed,
    // for example a tower is replaced by a weaker tower of the same type on the same place,
    // so the positions and the structure of the individual are kept and only what is needed is made cheaper.
    // The constraint decides what is fixed (see RepairUtilities.RepairConstraint):
    // 1. While upper bounds such as a budget are exceeded, the last gene whose removal helps is chosen and replaced
    //    by the most expensive available gene of the same kind that is cheaper and lowers the upper bound violation, so it loses as little as possible;
    //    the same gene may be downgraded again later. Only if no such gene exists, the gene is removed;
    //    removed genes that fit again are put back, so no gene is removed needlessly.
    // 2. While lower bounds such as a min count are not reached, the cheapest available gene that fits and helps is appended.
    // 3. Only if FillRemainder is set, the cheapest available gene that fits is appended while any fits, spending unused budget.
    // Two genes are of the same kind when checkGenesSameKind returns true for them; without it every gene is of the same kind,
    // so any cheaper available gene may replace any gene.
    // If RemoveUnavailableGenes is set, genes that are not among the available genes are removed first.
    // The earlier genes are kept first, so the later ones are downgraded first. If nothing changes, the individual itself is returned.
    // The filling stops only when the constraint forbids every gene, so every gene must use up some of a limit, for example cost more than 0.
    //
    // Example: budget 10, gene cost = gene, available genes [2, 3, 5, 6], every gene of the same kind, genes [6, 5, 2] ->
    // 2 has no cheaper gene and is removed, 5 is downgraded to 3 -> [6, 3]; 2 does not fit back.
    //
    // Complexity: time O(g * log g + r * (n + g) * c + (a + f) * g * c)
    //             memory O(n + g)
    // n - the number of genes of the individual;
    // r - the number of downgraded and removed genes;
    // a - the number of genes appended for the lower bounds;
    // f - the number of genes appended by the filling;
    // g - the number of distinct available genes;
    // c - the time of one check or update of the constraint tracker.
    public sealed class DowngradeRepair<TGene> : IRepair<TGene>
    {
        private readonly ICostModel<TGene> _costModel;
        private readonly Func<TGene, TGene, bool> _checkGenesSameKind;
        private readonly List<TGene> _availableGenesByCostDescending;

        // Distinct genes that may replace or be appended; at least one.
        public IReadOnlyList<TGene> AvailableGenes { get; }

        // Whether free limits, such as unused budget, are filled after the constraint is satisfied.
        public bool FillRemainder { get; }

        // Whether genes that are not among the available genes are removed first.
        public bool RemoveUnavailableGenes { get; }


        public DowngradeRepair(
            ICostModel<TGene> costModel,
            IReadOnlyList<TGene> availableGenes,
            Func<TGene, TGene, bool> checkGenesSameKind = null,
            bool fillRemainder = false,
            bool removeUnavailableGenes = false)
        {
            _costModel = costModel ?? throw new ArgumentNullException(nameof(costModel));

            AvailableGenes = RepairUtilities.CreateDistinctGenes(availableGenes, nameof(availableGenes));
            _availableGenesByCostDescending = AvailableGenes.OrderByDescending(costModel.Cost).ToList();

            _checkGenesSameKind = checkGenesSameKind ?? ((first, second) => true);

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
                (candidateGenes, _) => RepairUtilities.ChooseGeneWithHighestPriority(candidateGenes, gene => -_costModel.Cost(gene)),
                ChooseCheaperGene);
        }

        private (bool Found, TGene Gene) ChooseCheaperGene(IReadOnlyList<TGene> genes, int index, IConstraintTracker<TGene> tracker)
        {
            var gene = genes[index];
            double geneCost = _costModel.Cost(gene);
            double upperBoundViolation = tracker.UpperBoundViolation;
            foreach (var candidate in _availableGenesByCostDescending)
            {
                if (_costModel.Cost(candidate) >= geneCost || !_checkGenesSameKind(candidate, gene))
                    continue;

                tracker.Remove(gene);
                tracker.Append(candidate);
                bool lowersUpperBoundViolation = tracker.UpperBoundViolation < upperBoundViolation;
                tracker.Remove(candidate);
                tracker.Append(gene);
                if (lowersUpperBoundViolation)
                    return (true, candidate);
            }
            return (false, default);
        }
    }
}
