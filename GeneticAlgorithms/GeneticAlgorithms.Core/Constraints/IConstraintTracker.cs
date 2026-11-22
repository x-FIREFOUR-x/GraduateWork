namespace GeneticAlgorithms.Constraints
{
    public interface IConstraintTracker<TGene>
    {
        bool IsSatisfied { get; }

        // How far the upper bounds, such as a budget or a max count, are violated; 0 when none is violated.
        // Each violation is relative to its bound, for example 0.2 for a cost 20% over the budget, so violations of different bounds can be summed.
        // Removing genes can only lower it.
        double UpperBoundViolation { get; }

        // How far the lower bounds, such as a min count or a min weight of a group, are violated; 0 when none is violated.
        // Each violation is relative to its bound, for example 0.5 for half of a min count, so violations of different bounds can be summed.
        // Appending genes can only lower it.
        double LowerBoundViolation { get; }

        bool CanAppend(TGene gene);

        void Append(TGene gene);

        void Remove(TGene gene);

        IConstraintTracker<TGene> Clone();
    }
}
