using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers
{
    public interface ICrossover<TGene>
    {
        // Whether the crossover needs parents of equal length, as crossovers that cross genes position by position do;
        // then every mutation and repair must keep the length, otherwise individuals of different lengths meet as parents.
        bool RequiresParentsOfEqualLength => true;

        Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random);

    }
}
