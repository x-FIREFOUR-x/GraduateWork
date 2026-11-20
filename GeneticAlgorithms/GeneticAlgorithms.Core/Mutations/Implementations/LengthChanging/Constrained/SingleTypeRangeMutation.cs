using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthChanging.Constrained
{
    // Single type range mutation: a random range of genes is replaced by a group of genes of one kind,
    // so the individual gets a group of similar genes, for example several towers of the same type.
    // 1. A random start is chosen and a length from 1 to ceil(MaxShare * n), cut so that the range does not go past the last gene;
    //    the range is removed.
    // 2. A random gene among the available genes that can still be appended is inserted at the place of the range;
    //    its kind becomes the kind of the group. The removed range usually holds genes of several kinds,
    //    so replacing it with one kind changes it even when that kind was among them.
    // 3. After it, random genes of that kind that can still be appended are inserted until none of them fits;
    //    two genes are of the same kind when checkGenesSameKind returns true for them, by default when they are equal.
    // 4. Only if FillRemainder is set, the limits that are left are filled with the cheapest genes first: every available gene,
    //    from the cheapest one, is inserted while it still fits. Otherwise the budget left after the group stays unused,
    //    so the mutation changes only the range and its effect is not mixed with greedy filling.
    // Upper bounds of the constraint are never exceeded. If no gene can be appended after the removal,
    // the individual without the range is returned; an empty individual is then returned itself.
    // The filling stops only when the constraint forbids every gene, so every gene must use up some of a limit, for example cost more than 0.
    //
    // Example: budget 10, gene cost = gene, available genes [1, 2, 3], individual [1, 2, 3, 2], range [2, 3] removed ->
    // [1, 2] with 7 of the budget left, kind 3 chosen -> [1, 3, 3, 2] with 1 left;
    // with FillRemainder the cheapest gene 1 fills it -> [1, 3, 3, 1, 2].
    //
    // Complexity: time O(n + g * log g + k * (g + n))
    //             memory O(n + g)
    // n - the number of genes of the individual;
    // k - the number of inserted genes;
    // g - the number of distinct available genes.
    public sealed class SingleTypeRangeMutation<TGene> : IMutation<TGene>
    {
        private readonly List<TGene> _availableGenes;
        private readonly List<TGene> _availableGenesOrderedByCost;
        private readonly Func<TGene, TGene, bool> _checkGenesSameKind;
        private readonly List<TGene> _fittingGenes = new List<TGene>();

        // Largest share of the genes the removed range may take, in (0; 1]; the range always has at least one gene.
        public double MaxShare { get; }

        // Whether the budget left after the group is filled with the cheapest genes; false keeps the mutation to the range only.
        public bool FillRemainder { get; }


        public SingleTypeRangeMutation(
            ICostModel<TGene> costModel,
            IReadOnlyList<TGene> availableGenes,
            double maxShare = 0.5,
            Func<TGene, TGene, bool> checkGenesSameKind = null,
            bool fillRemainder = false)
        {
            MutationUtilities.ValidateMaxShare(maxShare);
            MaxShare = maxShare;

            if (costModel == null)
                throw new ArgumentNullException(nameof(costModel));
            _availableGenes = availableGenes?.Distinct().ToList() ?? throw new ArgumentNullException(nameof(availableGenes));
            _availableGenesOrderedByCost = _availableGenes.OrderBy(costModel.Cost).ToList();

            _checkGenesSameKind = checkGenesSameKind ?? EqualityComparer<TGene>.Default.Equals;
            FillRemainder = fillRemainder;
        }

        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            var mutant = individual.Clone();
            int insertIndex = RemoveRandomRange(mutant, random);

            if (!TryChooseFittingGene(mutant, gene => true, random, out var kindGene))
                return individual.Count == 0 ? individual : mutant;

            mutant.Insert(insertIndex++, kindGene);
            insertIndex = InsertGenesOfKind(mutant, insertIndex, kindGene, random);
            if (FillRemainder)
                FillWithCheapestGenes(mutant, insertIndex);

            return mutant;
        }

        private int RemoveRandomRange(Individual<TGene> mutant, IRandom random)
        {
            if (mutant.Count == 0)
                return 0;

            var (startRangeIndex, rangeLength) = MutationUtilities.ChooseRandomRange(mutant.Count, MaxShare, random);
            mutant.RemoveRange(startRangeIndex, rangeLength);
            return startRangeIndex;
        }

        private int InsertGenesOfKind(Individual<TGene> mutant, int insertIndex, TGene kindGene, IRandom random)
        {
            while (TryChooseFittingGene(mutant, gene => _checkGenesSameKind(gene, kindGene), random, out var gene))
                mutant.Insert(insertIndex++, gene);
            return insertIndex;
        }

        private void FillWithCheapestGenes(Individual<TGene> mutant, int insertIndex)
        {
            foreach (var gene in _availableGenesOrderedByCost)
                while (mutant.TryInsert(insertIndex, gene))
                    insertIndex++;
        }

        private bool TryChooseFittingGene(Individual<TGene> mutant, Func<TGene, bool> filter, IRandom random, out TGene chosenGene)
        {
            _fittingGenes.Clear();
            foreach (var gene in _availableGenes)
                if (filter(gene) && mutant.CanAppend(gene))
                    _fittingGenes.Add(gene);

            if (_fittingGenes.Count == 0)
            {
                chosenGene = default;
                return false;
            }

            chosenGene = _fittingGenes[random.Next(_fittingGenes.Count)];
            return true;
        }
    }
}
