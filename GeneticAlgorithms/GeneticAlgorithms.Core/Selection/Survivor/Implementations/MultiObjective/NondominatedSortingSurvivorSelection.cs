using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.MultiObjective
{
    // Non-dominated sorting survivor selection of NSGA-II (Deb et al., 2002): the population and the offspring are merged,
    // sorted into fronts by dominance and ordered by front, and within a front by crowding distance from the largest;
    // the first individuals survive, as many as there are individuals in the population. Whole fronts survive while they fit,
    // and the front that does not fit is cut by crowding distance, so individuals in sparse regions of the front are kept.
    // It is a (mu + lambda) selection for several objectives and elitist: non-dominated individuals are lost only when there are more
    // of them than places in the population. With RejectDuplicates, individuals with the same genes as an already chosen survivor
    // are taken only if places remain. For the constrained domination use ConstrainedFitnessComparer with ParetoDominanceComparer.
    //
    // Example: population size 3, candidates (5, 0), (0, 5), (3, 3), (1, 1), (2, 2) -> the first front (5, 0), (0, 5), (3, 3) survives.
    //
    // Complexity: time O(N^2 * c + k * N * log(N)), plus O(N * n) with RejectDuplicates
    //             memory O(N^2)
    // N - the number of candidates, n + lambda; n - the population size; lambda - the number of offspring;
    // k - the number of objectives; c - the time of one comparison.
    public sealed class NondominatedSortingSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Whether candidates with the same genes as an already chosen survivor are skipped and used only if there are not enough unique candidates.
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }

        public bool RequiresTotalOrder => false;


        public NondominatedSortingSurvivorSelection(bool rejectDuplicates = true, IIndividualComparer<TGene> comparer = null)
        {
            RejectDuplicates = rejectDuplicates;
            Comparer = comparer ?? new ParetoDominanceComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);

            var survivorCandidates = SurvivorSelectionUtilities.CombinePopulationAndChildren(population, offspring);
            var priorityOrder = ParetoUtilities.OrderByRankAndCrowding(survivorCandidates, Comparer, out _, out _);
            var survivorCandidatesInPriorityOrder = new Individual<TGene>[priorityOrder.Length];
            for (int i = 0; i < priorityOrder.Length; i++)
                survivorCandidatesInPriorityOrder[i] = survivorCandidates[priorityOrder[i]];

            var survivors = SurvivorSelectionUtilities.SelectSurvivors(survivorCandidatesInPriorityOrder, population.Count, RejectDuplicates);
            return SurvivorSelectionUtilities.ReplacePopulationWithSurvivors(population, survivors, offspring);
        }
    }
}
