using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.EvolutionStrategy
{
    // Best of offspring: the current population is discarded and the best children survive, as many as there are individuals
    // in the population. Known as (mu, lambda) selection in evolution strategies (Schwefel, 1977), where mu is the population size
    // and lambda is the number of offspring. Requires at least as many offspring as there are individuals in the population.
    // Not elitist: the best individual may be lost, which helps to leave local optima.
    // With RejectDuplicates, children with the same genes as an already chosen survivor are taken only if places remain.
    //
    // Complexity: time O(lambda * (n + log lambda))
    //             memory O(lambda)
    // n - the population size;
    // lambda - the number of offspring.
    public sealed class BestOfOffspringSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Whether candidates with the same genes as an already chosen survivor are skipped and used only if there are not enough unique candidates.
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }

        public bool RequiresTotalOrder => true;


        public BestOfOffspringSurvivorSelection(bool rejectDuplicates = true, IIndividualComparer<TGene> comparer = null)
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
            if (offspring.Count < population.Count)
                throw new InvalidOperationException("Best-of-offspring survivor selection requires at least as many offspring as there are individuals in the population.");

            var childrenBestToWorst = SurvivorSelectionUtilities.SortBestToWorst(SurvivorSelectionUtilities.GetChildren(offspring), Comparer);
            var survivors = SurvivorSelectionUtilities.SelectSurvivors(childrenBestToWorst, population.Count, RejectDuplicates);
            return SurvivorSelectionUtilities.ReplacePopulationWithSurvivors(population, survivors, offspring);
        }
    }
}
