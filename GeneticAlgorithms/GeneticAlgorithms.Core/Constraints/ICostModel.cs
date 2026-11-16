namespace GeneticAlgorithms.Constraints
{
    public interface ICostModel<TGene>
    {
        double MaxTotalCost { get; }

        double Cost(TGene gene);
    }
}
