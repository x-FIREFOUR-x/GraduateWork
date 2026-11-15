using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;

namespace GeneticAlgorithms.Mutations
{
    public interface IMutation<TGene>
    {
        Individual<TGene> Mutate(Individual<TGene> individual, IRandom random);
    }
}
