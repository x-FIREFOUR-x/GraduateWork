using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.Constraints.Rules
{
    public sealed class GroupWeightBoundsConstraint<TGene, TGroup> : IConstraint<TGene>
    {
        private const double RoundingErrorTolerance = 1e-9;

        private readonly Func<TGene, TGroup> _getGeneGroup;
        private readonly HashSet<TGroup> _groups;

        private readonly Func<TGene, double> _getGeneWeight;
        private readonly Func<TGroup, double?> _getGroupMaxWeight;
        private readonly Func<TGroup, double?> _getGroupMinWeight;
        
        private readonly IEqualityComparer<TGroup> _groupEqualityComparer;

        private readonly int _initialOutOfBoundsGroupCount;


        public GroupWeightBoundsConstraint(
            Func<TGene, TGroup> getGeneGroup,
            Func<TGroup, double?> getGroupMaxWeight = null,
            Func<TGroup, double?> getGroupMinWeight = null,
            IEnumerable<TGroup> groups = null,
            Func<TGene, double> getGeneWeight = null,
            IEqualityComparer<TGroup> groupEqualityComparer = null)
        {
            _getGeneGroup = getGeneGroup ?? throw new ArgumentNullException(nameof(getGeneGroup));
            if (getGroupMaxWeight == null && getGroupMinWeight == null)
                throw new ArgumentException("At least one bound is required: group max weight or group min weight.");
            if (getGroupMinWeight != null && groups == null)
                throw new ArgumentException("Groups are required when group min weight is set.", nameof(groups));
            if (getGroupMinWeight == null && groups != null)
                throw new ArgumentException("Group min weight is required when groups are set.", nameof(getGroupMinWeight));

            _getGeneWeight = getGeneWeight ?? (gene => 1);
            _getGroupMaxWeight = getGroupMaxWeight;
            _getGroupMinWeight = getGroupMinWeight;
            _groupEqualityComparer = groupEqualityComparer ?? EqualityComparer<TGroup>.Default;
            _groups = new HashSet<TGroup>(groups ?? Array.Empty<TGroup>(), _groupEqualityComparer);

            foreach (var group in _groups)
            {
                double? groupMinWeight = GetGroupMinWeight(group);
                double? groupMaxWeight = GetGroupMaxWeight(group);
                if (groupMaxWeight < groupMinWeight)
                    throw new ArgumentException($"Min weight {groupMinWeight} of group '{group}' must not exceed its max weight {groupMaxWeight}.", nameof(getGroupMinWeight));
                if (!IsWithinGroupMinWeight(group, 0))
                    _initialOutOfBoundsGroupCount++;
            }
        }

        public IConstraintTracker<TGene> CreateTracker()
        {
            return new Tracker(this, new Dictionary<TGroup, double>(_groupEqualityComparer), _initialOutOfBoundsGroupCount);
        }

        private double GetGeneWeight(TGene gene)
        {
            double geneWeight = _getGeneWeight(gene);
            if (geneWeight < 0)
                throw new InvalidOperationException("Gene weight must be non-negative.");
            return geneWeight;
        }

        private double? GetGroupMaxWeight(TGroup group)
        {
            double? groupMaxWeight = _getGroupMaxWeight?.Invoke(group);
            if (groupMaxWeight < 0)
                throw new InvalidOperationException($"Max weight of group '{group}' must be non-negative.");
            return groupMaxWeight;
        }

        private double? GetGroupMinWeight(TGroup group)
        {
            if (!_groups.Contains(group))
                return null;
            double? groupMinWeight = _getGroupMinWeight(group);
            if (groupMinWeight < 0)
                throw new InvalidOperationException($"Min weight of group '{group}' must be non-negative.");
            return groupMinWeight;
        }

        private bool IsWithinGroupMaxWeight(TGroup group, double groupWeight)
        {
            double? groupMaxWeight = GetGroupMaxWeight(group);
            return groupMaxWeight == null || groupWeight <= groupMaxWeight.Value + RoundingErrorTolerance;
        }

        private bool IsWithinGroupMinWeight(TGroup group, double groupWeight)
        {
            double? groupMinWeight = GetGroupMinWeight(group);
            return groupMinWeight == null || groupWeight >= groupMinWeight.Value - RoundingErrorTolerance;
        }

        private bool IsWithinGroupBounds(TGroup group, double groupWeight)
        {
            return IsWithinGroupMinWeight(group, groupWeight) && IsWithinGroupMaxWeight(group, groupWeight);
        }

        private sealed class Tracker : IConstraintTracker<TGene>
        {
            private readonly GroupWeightBoundsConstraint<TGene, TGroup> _constraint;
            private readonly Dictionary<TGroup, double> _weightByGroup;
            private int _outOfBoundsGroupCount;

            public Tracker(GroupWeightBoundsConstraint<TGene, TGroup> constraint, Dictionary<TGroup, double> weightByGroup, int outOfBoundsGroupCount)
            {
                _constraint = constraint;
                _weightByGroup = weightByGroup;
                _outOfBoundsGroupCount = outOfBoundsGroupCount;
            }

            public bool IsSatisfied => _outOfBoundsGroupCount == 0;

            public bool CanAppend(TGene gene)
            {
                TGroup group = _constraint._getGeneGroup(gene);
                _weightByGroup.TryGetValue(group, out double groupWeight);
                return _constraint.IsWithinGroupMaxWeight(group, groupWeight + _constraint.GetGeneWeight(gene));
            }

            public void Append(TGene gene) => AddGroupWeight(_constraint._getGeneGroup(gene), _constraint.GetGeneWeight(gene));

            public void Remove(TGene gene) => AddGroupWeight(_constraint._getGeneGroup(gene), -_constraint.GetGeneWeight(gene));

            public IConstraintTracker<TGene> Clone()
            {
                return new Tracker(_constraint, new Dictionary<TGroup, double>(_weightByGroup, _constraint._groupEqualityComparer), _outOfBoundsGroupCount);
            }

            private void AddGroupWeight(TGroup group, double weightDelta)
            {
                _weightByGroup.TryGetValue(group, out double groupWeight);
                bool wasOutOfBounds = !_constraint.IsWithinGroupBounds(group, groupWeight);

                groupWeight += weightDelta;
                bool isOutOfBounds = !_constraint.IsWithinGroupBounds(group, groupWeight);

                if (Math.Abs(groupWeight) <= RoundingErrorTolerance)
                    _weightByGroup.Remove(group);
                else
                    _weightByGroup[group] = groupWeight;

                if (isOutOfBounds && !wasOutOfBounds)
                    _outOfBoundsGroupCount++;
                else if (wasOutOfBounds && !isOutOfBounds)
                    _outOfBoundsGroupCount--;
            }
        }
    }
}
