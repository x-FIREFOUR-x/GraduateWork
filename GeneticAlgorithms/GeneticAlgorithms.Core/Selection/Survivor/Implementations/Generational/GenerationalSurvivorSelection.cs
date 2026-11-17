using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.Generational
{
    // Generational replacement (Holland, 1975): the offspring replace the whole population.
    // Requires exactly as many offspring as there are individuals in the population; the best individual may be lost.
    //
    // Complexity: time O(n)
    //             memory O(1)
    // n - the population size.
    public sealed class GenerationalSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        public IIndividualComparer<TGene> Comparer { get; }


        public GenerationalSurvivorSelection(IIndividualComparer<TGene> comparer = null)
        {
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);
            if (offspring.Count != population.Count)
                throw new InvalidOperationException("Generational survivor selection requires as many offspring as there are individuals in the population.");

            for (int i = 0; i < population.Count; i++)
                population[i] = offspring[i].Child;
            return offspring.Count;
        }
    }
}
