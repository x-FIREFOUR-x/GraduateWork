using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.Generational
{
    // Generational replacement with elitism (De Jong, 1975): the offspring replace the population, but the EliteCount
    // best individuals of the current population survive unchanged and take the places of the worst offspring.
    // Requires exactly as many offspring as there are individuals in the population; the best individual is never lost.
    //
    // Complexity: time O(n * log n)
    //             memory O(n)
    // n - the population size.
    public sealed class ElitistGenerationalSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // How many best individuals of the current population move to the next one unchanged; from 1 to the population size minus 1.
        public int EliteCount { get; }

        public IIndividualComparer<TGene> Comparer { get; }


        public ElitistGenerationalSurvivorSelection(int eliteCount = 1, IIndividualComparer<TGene> comparer = null)
        {
            if (eliteCount < 1)
                throw new ArgumentOutOfRangeException(nameof(eliteCount), eliteCount, "Elite count must be at least 1.");
            EliteCount = eliteCount;
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);
            if (offspring.Count != population.Count)
                throw new InvalidOperationException("Elitist generational survivor selection requires as many offspring as there are individuals in the population.");
            if (EliteCount >= population.Count)
                throw new InvalidOperationException("Elite count must be less than population size.");

            var eliteCurrentPopulation = SurvivorSelectionUtilities.SortBestToWorst(population, Comparer).Take(EliteCount);
            var bestChildren = SurvivorSelectionUtilities.SortBestToWorst(SurvivorSelectionUtilities.GetChildren(offspring), Comparer)
                .Take(population.Count - EliteCount);

            var survivors = eliteCurrentPopulation.Concat(bestChildren).ToList();
            return SurvivorSelectionUtilities.ReplacePopulationWithSurvivors(population, survivors, offspring);
        }
    }
}
