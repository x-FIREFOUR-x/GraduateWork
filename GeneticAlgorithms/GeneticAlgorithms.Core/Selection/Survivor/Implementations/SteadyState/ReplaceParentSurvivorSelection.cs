using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;
using GeneticAlgorithms.Selection.Parent;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.SteadyState
{
    // Steady-state replacement of a parent: each child competes only with its own parents and replaces the worse of them
    // if the child is better. Competition stays local, so the population keeps more diversity than with ReplaceWorstSurvivorSelection.
    // A parent that has already been replaced by an earlier child of the same generation no longer competes.
    // With RejectDuplicates, a child with the same genes as an existing individual is discarded.
    //
    // Complexity: time O(lambda * n)
    //             memory O(1)
    // n - the population size;
    // lambda - the number of offspring.
    public sealed class ReplaceParentSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Whether a child is discarded when an individual with the same genes is already in the population.
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }


        public ReplaceParentSurvivorSelection(bool rejectDuplicates = true, IIndividualComparer<TGene> comparer = null)
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
                int worseParentIndex = FindWorseParentIndex(population, offspringItem.Parents);
                if (worseParentIndex < 0 || Comparer.IsNotBetter(child, population[worseParentIndex]))
                    continue;
                if (RejectDuplicates && SurvivorSelectionUtilities.PopulationContainsSameIndividual(population, child))
                    continue;

                population[worseParentIndex] = child;
                accepted++;
            }
            return accepted;
        }

        private int FindWorseParentIndex(List<Individual<TGene>> population, ParentPair<TGene> parents)
        {
            int firstParentIndex = SurvivorSelectionUtilities.IndexOf(population, parents.First);
            int secondParentIndex = SurvivorSelectionUtilities.IndexOf(population, parents.Second);
            if (firstParentIndex < 0)
                return secondParentIndex;
            if (secondParentIndex < 0)
                return firstParentIndex;
            return Comparer.IsWorse(population[secondParentIndex], population[firstParentIndex]) ? secondParentIndex : firstParentIndex;
        }
    }
}
