using System;
using System.Linq;

namespace GeneticAlgorithms.Stopping
{
    public sealed class CompositeStopCriterion : IStopCriterion
    {
        private readonly IStopCriterion[] _criteria;

        public CompositeStopCriterion(params IStopCriterion[] criteria)
        {
            if (criteria == null || criteria.Length == 0)
                throw new ArgumentException("At least one criterion is required.", nameof(criteria));
            _criteria = criteria.ToArray();
        }

        public bool ShouldStop(SearchProgress progress)
        {
            foreach (var criterion in _criteria)
                if (criterion.ShouldStop(progress))
                    return true;
            return false;
        }
    }
}
