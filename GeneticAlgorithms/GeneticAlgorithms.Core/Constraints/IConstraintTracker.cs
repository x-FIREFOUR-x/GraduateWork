namespace GeneticAlgorithms.Constraints
{
    public interface IConstraintTracker<TGene>
    {
        bool IsSatisfied { get; }

        bool CanAppend(TGene gene);

        void Append(TGene gene);

        void Remove(TGene gene);

        IConstraintTracker<TGene> Clone();
    }
}
