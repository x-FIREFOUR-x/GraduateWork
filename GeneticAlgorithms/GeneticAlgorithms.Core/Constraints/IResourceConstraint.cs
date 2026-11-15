namespace GeneticAlgorithms.Constraints
{
    public interface IResourceConstraint<TGene> : IConstraint<TGene>
    {
        double MaxTotalCost { get; }

        double Cost(TGene gene);
    }
}
