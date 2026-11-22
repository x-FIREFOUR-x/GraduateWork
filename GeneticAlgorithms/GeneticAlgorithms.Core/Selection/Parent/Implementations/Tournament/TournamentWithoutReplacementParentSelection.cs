using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Tournament
{
    // Tournament without replacement (Goldberg, Korb, Deb, 1989): the population is shuffled and split
    // into consecutive groups of TournamentSize individuals, and the best of each group becomes a parent.
    // When the permutation is exhausted, the population is shuffled again. Every individual takes part
    // in tournaments evenly, so the selection variance is lower than in a regular tournament.
    //
    // Complexity: time O(n + m * k), memory O(n + m).
    // n - the population size;
    // m - the number of selected parents (2 * parentPairCount);
    // k - the tournament size.
    public sealed class TournamentWithoutReplacementParentSelection<TGene> : IParentSelection<TGene>
    {
        public IIndividualComparer<TGene> Comparer { get; }

        // How many candidates of the shuffled population compete in one group; from 1 to the population size.
        public int TournamentSize { get; }


        public TournamentWithoutReplacementParentSelection(int tournamentSize = 2, IIndividualComparer<TGene> comparer = null)
        {
            if (tournamentSize < 1)
                throw new ArgumentOutOfRangeException(nameof(tournamentSize), tournamentSize, "Tournament size must be at least 1.");
            TournamentSize = tournamentSize;
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, parentPairCount, random);
            if (TournamentSize > population.Count)
                throw new InvalidOperationException("Tournament size must not exceed population size.");

            int parentCount = 2 * parentPairCount;
            var parents = new Individual<TGene>[parentCount];

            var shuffledCandidateIndices = new int[population.Count];
            for (int i = 0; i < shuffledCandidateIndices.Length; i++)
                shuffledCandidateIndices[i] = i;
            
            int groupStart = shuffledCandidateIndices.Length;
            for (int i = 0; i < parentCount; i++)
            {
                if (groupStart + TournamentSize > shuffledCandidateIndices.Length)
                {
                    ParentSelectionUtilities.Shuffle(shuffledCandidateIndices, random);
                    groupStart = 0;
                }

                var bestCandidate = population[shuffledCandidateIndices[groupStart]];
                for (int j = 1; j < TournamentSize; j++)
                {
                    var candidate = population[shuffledCandidateIndices[groupStart + j]];
                    if (Comparer.IsBetter(candidate, bestCandidate))
                        bestCandidate = candidate;
                }
                groupStart += TournamentSize;
                parents[i] = bestCandidate;
            }

            return ParentSelectionUtilities.ToPairs(parents);
        }
    }
}
