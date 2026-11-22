using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations
{
    // Mutates a copy of the individual and returns it; returns the individual itself when nothing changes,
    // so the genetic algorithm can skip evaluating it again.
    // currentIterationNumber - the number of the current iteration of the search, starting from 0, or null when it is unknown;
    // the genetic algorithm passes it on every call. Most mutations ignore it;
    // mutations whose step depends on the progress of the search, such as non-uniform mutation, require it and throw on null.
    // Composite mutations pass it to the nested ones.
    public interface IMutation<TGene>
    {
        Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null);

        // Whether the mutated individual always has the same number of genes as the original one;
        // false for mutations that insert or remove genes. True by default, as most mutations only change or reorder genes.
        bool PreservesIndividualLength => false;
    }
}
