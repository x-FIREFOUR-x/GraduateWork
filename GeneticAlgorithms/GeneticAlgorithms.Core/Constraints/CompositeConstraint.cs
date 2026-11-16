using System;
using System.Linq;

namespace GeneticAlgorithms.Constraints
{
    public sealed class CompositeConstraint<TGene> : IConstraint<TGene>
    {
        private readonly IConstraint<TGene>[] _constraints;

        public CompositeConstraint(params IConstraint<TGene>[] constraints)
        {
            if (constraints == null || constraints.Length == 0)
                throw new ArgumentException("At least one constraint is required.", nameof(constraints));
            _constraints = constraints.ToArray();
        }

        public IConstraintTracker<TGene> CreateTracker()
        {
            return new Tracker(_constraints.Select(c => c.CreateTracker()).ToArray());
        }

        private sealed class Tracker : IConstraintTracker<TGene>
        {
            private readonly IConstraintTracker<TGene>[] _trackers;

            public Tracker(IConstraintTracker<TGene>[] trackers)
            {
                _trackers = trackers;
            }

            public bool IsSatisfied
            {
                get
                {
                    foreach (var tracker in _trackers)
                        if (!tracker.IsSatisfied)
                            return false;
                    return true;
                }
            }

            public bool CanAppend(TGene gene)
            {
                foreach (var tracker in _trackers)
                    if (!tracker.CanAppend(gene))
                        return false;
                return true;
            }

            public void Append(TGene gene)
            {
                foreach (var tracker in _trackers)
                    tracker.Append(gene);
            }

            public void Remove(TGene gene)
            {
                foreach (var tracker in _trackers)
                    tracker.Remove(gene);
            }

            public IConstraintTracker<TGene> Clone()
            {
                var copies = new IConstraintTracker<TGene>[_trackers.Length];
                for (int i = 0; i < _trackers.Length; i++)
                    copies[i] = _trackers[i].Clone();
                return new Tracker(copies);
            }
        }
    }
}
