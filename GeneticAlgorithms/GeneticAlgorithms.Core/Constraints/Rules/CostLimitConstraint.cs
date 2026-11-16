using System;

namespace GeneticAlgorithms.Constraints.Rules
{
    public sealed class CostLimitConstraint<TGene> : IConstraint<TGene>, ICostModel<TGene>
    {
        private const double RoundingErrorTolerance = 1e-9;

        private readonly Func<TGene, double> _getGeneCost;

        public double MaxTotalCost { get; }


        public CostLimitConstraint(Func<TGene, double> getGeneCost, double maxTotalCost)
        {
            _getGeneCost = getGeneCost ?? throw new ArgumentNullException(nameof(getGeneCost));
            if (maxTotalCost < 0)
                throw new ArgumentOutOfRangeException(nameof(maxTotalCost), maxTotalCost, "Max total cost must be non-negative.");
            MaxTotalCost = maxTotalCost;
        }

        public double Cost(TGene gene)
        {
            double geneCost = _getGeneCost(gene);
            if (geneCost <= 0)
                throw new InvalidOperationException("Gene cost must be positive.");
            return geneCost;
        }

        public IConstraintTracker<TGene> CreateTracker() => new Tracker(this, 0);

        private sealed class Tracker : IConstraintTracker<TGene>
        {
            private readonly CostLimitConstraint<TGene> _constraint;
            private double _totalCost;

            public Tracker(CostLimitConstraint<TGene> constraint, double totalCost)
            {
                _constraint = constraint;
                _totalCost = totalCost;
            }

            public bool IsSatisfied => _totalCost <= _constraint.MaxTotalCost + RoundingErrorTolerance;

            public bool CanAppend(TGene gene) => _totalCost + _constraint.Cost(gene) <= _constraint.MaxTotalCost + RoundingErrorTolerance;

            public void Append(TGene gene) => _totalCost += _constraint.Cost(gene);

            public void Remove(TGene gene) => _totalCost -= _constraint.Cost(gene);

            public IConstraintTracker<TGene> Clone() => new Tracker(_constraint, _totalCost);
        }
    }
}
