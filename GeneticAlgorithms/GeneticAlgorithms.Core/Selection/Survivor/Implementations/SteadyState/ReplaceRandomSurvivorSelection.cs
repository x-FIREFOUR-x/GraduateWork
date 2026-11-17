using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.SteadyState
{
    // Steady-state replacement of a random individual: each child replaces a uniformly random individual of the population,
    // regardless of fitness. Keeps diversity, but the best individual may be lost.
    // With RejectDuplicates, a child with the same genes as an existing individual is discarded.
    //
    // Complexity: time O(lambda * n) with RejectDuplicates, otherwise O(lambda)
    //             memory O(1)
    // n - the population size;
    // lambda - the number of offspring.
    public sealed class ReplaceRandomSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Whether a child is discarded when an individual with the same genes is already in the population.
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }


        public ReplaceRandomSurvivorSelection(bool rejectDuplicates = true, IIndividualComparer<TGene> comparer = null)
        {
            RejectDuplicates = rejectDuplicates;
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);

            int accepted = 0;
            foreach (var offspringItem in offspring)
            {
                var child = offspringItem.Child;
                if (RejectDuplicates && SurvivorSelectionUtilities.PopulationContainsSameIndividual(population, child))
                    continue;

                population[random.Next(population.Count)] = child;
                accepted++;
            }
            return accepted;
        }
    }
}
