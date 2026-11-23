namespace GeneticAlgorithms.Constraints
{
    public interface IConstraint<TGene>
    {
        IConstraintTracker<TGene> CreateTracker();
    }
}
