using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers
{
    public interface ICrossover<TGene>
    {
        Individual<TGene> Cross(Individual<TGene> first, Individual<TGene> second, IRandom random);
    }
}
