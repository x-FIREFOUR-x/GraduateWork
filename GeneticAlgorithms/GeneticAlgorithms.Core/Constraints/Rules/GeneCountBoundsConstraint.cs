using System;

namespace GeneticAlgorithms.Constraints.Rules
{
    public sealed class GeneCountBoundsConstraint<TGene> : IConstraint<TGene>
    {
        public int? MinGeneCount { get; }
        public int? MaxGeneCount { get; }


        public GeneCountBoundsConstraint(int? minGeneCount = null, int? maxGeneCount = null)
        {
            if (minGeneCount == null && maxGeneCount == null)
                throw new ArgumentException("At least one bound is required: min gene count or max gene count.");
            if (minGeneCount < 1)
                throw new ArgumentOutOfRangeException(nameof(minGeneCount), minGeneCount, "Min gene count must be at least 1.");
            if (maxGeneCount < 1)
                throw new ArgumentOutOfRangeException(nameof(maxGeneCount), maxGeneCount, "Max gene count must be at least 1.");
            if (maxGeneCount < minGeneCount)
                throw new ArgumentOutOfRangeException(nameof(maxGeneCount), maxGeneCount, "Max gene count must be greater than or equal to min gene count.");

            MinGeneCount = minGeneCount;
            MaxGeneCount = maxGeneCount;
        }

        public IConstraintTracker<TGene> CreateTracker() => new Tracker(this, 0);

        private sealed class Tracker : IConstraintTracker<TGene>
        {
            private readonly GeneCountBoundsConstraint<TGene> _constraint;
            private int _geneCount;

            public Tracker(GeneCountBoundsConstraint<TGene> constraint, int geneCount)
            {
                _constraint = constraint;
                _geneCount = geneCount;
            }

            public bool IsSatisfied => _geneCount >= (_constraint.MinGeneCount ?? 0) && _geneCount <= (_constraint.MaxGeneCount ?? int.MaxValue);

            public bool CanAppend(TGene gene) => _geneCount < (_constraint.MaxGeneCount ?? int.MaxValue);

            public void Append(TGene gene) => _geneCount++;

            public void Remove(TGene gene) => _geneCount--;

            public IConstraintTracker<TGene> Clone() => new Tracker(_constraint, _geneCount);
        }
    }
}
