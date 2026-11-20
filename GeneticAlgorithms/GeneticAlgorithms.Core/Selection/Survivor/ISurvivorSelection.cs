using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor
{
    // Survivor selection: decides which individuals of the current population and the new offspring
    // form the next population. 
    public interface ISurvivorSelection<TGene>
    {
        // Decides which individual is better.
        IIndividualComparer<TGene> Comparer { get; }


        // Called once per run with the initial population.
        void Initialize(List<Individual<TGene>> population);

        // Replace changes the population in place and returns how many offspring entered it. offspring are the children together with their parents.
        int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random);
    }
}
