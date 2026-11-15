using GeneticAlgorithms.Individuals;

namespace GeneticAlgorithms.Repair
{
    public interface IRepair<TGene>
    {
        Individual<TGene> Repair(Individual<TGene> individual);
    }
}
