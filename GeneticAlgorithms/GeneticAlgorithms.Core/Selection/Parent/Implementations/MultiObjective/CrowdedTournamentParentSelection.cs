using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.MultiObjective
{
    // Crowded tournament selection of NSGA-II (Deb et al., 2002): TournamentSize individuals are drawn at random (with replacement)
    // and the winner of the crowded comparison becomes a parent: the lower rank of the front wins, and of equal ranks the larger
    // crowding distance, so non-dominated individuals in sparse regions of the front are preferred; of equal ranks and distances
    // the first drawn wins. The ranks and the crowding distances are computed once per call on the current population.
    // For the constrained domination use ConstrainedFitnessComparer with ParetoDominanceComparer as the comparer.
    //
    // Example: candidates (1, 1) of rank 1 and (2, 2) of rank 0 -> (2, 2); boundary (5, 0) and inner (2, 2) of one front -> (5, 0).
    //
    // Complexity: time O(n^2 * c + k * n * log(n) + m * t)
    //             memory O(n^2 + m)
    // n - the population size; m - the number of selected parents (2 * parentPairCount); t - the tournament size;
    // k - the number of objectives; c - the time of one comparison.
    public sealed class CrowdedTournamentParentSelection<TGene> : IParentSelection<TGene>
    {
        public IIndividualComparer<TGene> Comparer { get; }

        public bool UsesFitnessValues => false;

        public bool RequiresTotalOrder => false;

        // How many randomly drawn candidates compete in one tournament; at least 1.
        public int TournamentSize { get; }


        public CrowdedTournamentParentSelection(int tournamentSize = 2, IIndividualComparer<TGene> comparer = null)
        {
            if (tournamentSize < 1)
                throw new ArgumentOutOfRangeException(nameof(tournamentSize), tournamentSize, "Tournament size must be at least 1.");
            TournamentSize = tournamentSize;
            Comparer = comparer ?? new ParetoDominanceComparer<TGene>();
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, parentPairCount, random);

            ParetoUtilities.OrderByRankAndCrowding(population, Comparer, out var ranks, out var crowdingDistances);

            int parentCount = 2 * parentPairCount;
            var parents = new Individual<TGene>[parentCount];
            for (int i = 0; i < parentCount; i++)
                parents[i] = population[SelectParentIndex(population.Count, ranks, crowdingDistances, random)];

            return ParentSelectionUtilities.ToPairs(parents);
        }

        private int SelectParentIndex(int populationSize, int[] ranks, double[] crowdingDistances, IRandom random)
        {
            int bestIndex = random.Next(populationSize);
            for (int i = 1; i < TournamentSize; i++)
            {
                int candidateIndex = random.Next(populationSize);
                if (ranks[candidateIndex] < ranks[bestIndex]
                    || (ranks[candidateIndex] == ranks[bestIndex] && crowdingDistances[candidateIndex] > crowdingDistances[bestIndex]))
                    bestIndex = candidateIndex;
            }
            return bestIndex;
        }
    }
}
