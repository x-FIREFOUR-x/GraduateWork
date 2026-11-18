using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers
{
    public interface ICrossover<TGene>
    {
        Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random);
    }
}
