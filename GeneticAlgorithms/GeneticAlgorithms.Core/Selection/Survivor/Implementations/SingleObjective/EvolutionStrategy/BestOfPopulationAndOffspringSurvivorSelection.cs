using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.EvolutionStrategy
{
    // Best of population and offspring: the current population and the offspring are merged and the best individuals survive,
    // as many as there are individuals in the population. Known as (mu + lambda) selection in evolution strategies (Schwefel, 1977),
    // where mu is the population size and lambda is the number of offspring. Elitist: the best individual is never lost.
    // With RejectDuplicates, individuals with the same genes as an already chosen survivor are taken only if places remain.
    //
    // Complexity: time O((n + lambda) * (n + log(n + lambda))) with RejectDuplicates, without O((n + lambda) * log(n + lambda) + n * lambda)
    //             memory O(n + lambda)
    // n - the population size;
    // lambda - the number of offspring.
    public sealed class BestOfPopulationAndOffspringSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Whether candidates with the same genes as an already chosen survivor are skipped and used only if there are not enough unique candidates.
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }

        public bool RequiresTotalOrder => true;


        public BestOfPopulationAndOffspringSurvivorSelection(bool rejectDuplicates = true, IIndividualComparer<TGene> comparer = null)
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

            var survivorCandidates = SurvivorSelectionUtilities.CombinePopulationAndChildren(population, offspring);
            var bestToWorstSurvivorCandidates = SurvivorSelectionUtilities.SortBestToWorst(survivorCandidates, Comparer);
            var survivors = SurvivorSelectionUtilities.SelectSurvivors(bestToWorstSurvivorCandidates, population.Count, RejectDuplicates);
            return SurvivorSelectionUtilities.ReplacePopulationWithSurvivors(population, survivors, offspring);
        }
    }
}
