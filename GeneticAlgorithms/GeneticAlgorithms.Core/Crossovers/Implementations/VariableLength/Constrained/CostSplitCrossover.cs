using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;
using GeneticAlgorithms.Repairs;


namespace GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained
{
    // Cost split crossover: the budget MaxTotalCost is split between the parents in a share s and 1 - s,
    // where s is FirstParentMaxTotalCostShare if it is set, otherwise drawn at random from [0; 1) for every child.
    // The child takes the longest prefix of the first parent that costs at most s * MaxTotalCost, then the longest suffix
    // of the second parent that costs at most (1 - s) * MaxTotalCost, in its order; every gene must also satisfy the constraints.
    // So, like one-point crossover, the child gets the beginning from one parent and the end from the other,
    // but the parents are cut by cost, not by gene count. The child is then repaired, which may fill the budget that is left.
    // Example: MaxTotalCost 10, gene cost = gene, s = 0.5, first [2, 3, 4, 5], second [1, 1, 6, 2]:
    // prefix of the first within 5 is [2, 3], suffix of the second within 5 is [2] -> [2, 3, 2].
    //
    // Complexity: time O(n + m + R)
    //             memory O(n + m)
    // n - the number of genes of the first parent;
    // m - the number of genes of the second parent;
    // R - the cost of one repair.
    public sealed class CostSplitCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;
        private readonly ICostModel<TGene> _costModel;
        private readonly IRepair<TGene> _repair;

        public bool RequiresParentsOfEqualLength => false;

        // Share of MaxTotalCost allocated for acquiring genes for the offspring from the first parent, the rest of share goes to the second parent; in [0; 1].
        // Null means a new random share for every child.
        public double? FirstParentMaxTotalCostShare { get; }

        // Share of MaxTotalCost allocated for acquiring genes for the offspring from the second parent, 1 - FirstParentMaxTotalCostShare; in [0; 1].
        // Null when the share is random.
        public double? SecondParentMaxTotalCostShare { get; }


        public CostSplitCrossover(IConstraint<TGene> constraint, ICostModel<TGene> costModel, IRepair<TGene> repair, double? firstParentMaxTotalCostShare = null)
        {
            if (firstParentMaxTotalCostShare.HasValue && !(firstParentMaxTotalCostShare.Value >= 0 && firstParentMaxTotalCostShare.Value <= 1))
                throw new ArgumentOutOfRangeException(nameof(firstParentMaxTotalCostShare), firstParentMaxTotalCostShare, "First parent max total cost share must be between 0 and 1.");

            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _costModel = costModel ?? throw new ArgumentNullException(nameof(costModel));
            _repair = repair ?? throw new ArgumentNullException(nameof(repair));
            FirstParentMaxTotalCostShare = firstParentMaxTotalCostShare;
            SecondParentMaxTotalCostShare = FirstParentMaxTotalCostShare != null ? 1 - FirstParentMaxTotalCostShare : null;

        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);

            double firstParentMaxTotalCostShare = FirstParentMaxTotalCostShare ?? random.NextDouble();
            double secondParentMaxTotalCostShare = 1 - firstParentMaxTotalCostShare;

            var child = new Individual<TGene>(_constraint.CreateTracker());

            AppendParentPrefixToChildWithinCostLimit(firstParent, child, firstParentMaxTotalCostShare * _costModel.MaxTotalCost);
            AppendParentSuffixToChildWithinCostLimit(secondParent, child, secondParentMaxTotalCostShare * _costModel.MaxTotalCost);

            return _repair.Repair(child, random);
        }

        private void AppendParentPrefixToChildWithinCostLimit(Individual<TGene> parent, Individual<TGene> child, double costLimit)
        {
            double spentCostLimit = 0;
            foreach (var gene in parent.Genes)
            {
                double costGene = _costModel.Cost(gene);
                //a continuous sequence is taken
                if (spentCostLimit + costGene > costLimit)
                    break;
                if (child.TryAppend(gene))
                    spentCostLimit += costGene;
            }
        }

        private void AppendParentSuffixToChildWithinCostLimit(Individual<TGene> parent, Individual<TGene> child, double costLimit)
        {
            double spentCostLimit = 0;
            int suffixStart = parent.Count;
            //a continuous sequence is taken
            while (suffixStart > 0 && spentCostLimit + _costModel.Cost(parent.Genes[suffixStart - 1]) <= costLimit)
            {
                suffixStart--;
                spentCostLimit += _costModel.Cost(parent.Genes[suffixStart]);
            }

            for (int i = suffixStart; i < parent.Count; i++)
                child.TryAppend(parent.Genes[i]);
        }
    }
}
