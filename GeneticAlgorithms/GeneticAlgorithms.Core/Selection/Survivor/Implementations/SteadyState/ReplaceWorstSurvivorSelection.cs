using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.SteadyState
{
    // Steady-state replacement of the worst individual: each child replaces the worst individual of the population
    // if the child is better. Fast convergence, but diversity is lost quickly.
    // With RejectDuplicates, a child with the same genes as an existing individual is discarded.
    //
    // Complexity: time O(lambda * n)
    //             memory O(1)
    // n - the population size;
    // lambda - the number of offspring.
    public sealed class ReplaceWorstSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Whether a child is discarded when an individual with the same genes is already in the population.
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }


        public ReplaceWorstSurvivorSelection(bool rejectDuplicates = true, IIndividualComparer<TGene> comparer = null)
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
                int indexWorstIndividual = SurvivorSelectionUtilities.IndexOfWorst(population, Comparer);
                if (Comparer.IsNotBetter(child, population[indexWorstIndividual]))
                    continue;
                if (RejectDuplicates && SurvivorSelectionUtilities.PopulationContainsSameIndividual(population, child))
                    continue;

                population[indexWorstIndividual] = child;
                accepted++;
            }
            return accepted;
        }
    }
}
