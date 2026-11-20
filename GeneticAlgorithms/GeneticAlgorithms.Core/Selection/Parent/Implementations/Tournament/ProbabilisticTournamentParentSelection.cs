using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Tournament
{
    // Probabilistic tournament: candidates are ordered from best to worst, the best wins with
    // probability p = WinProbability, the second with probability p(1 - p), and so on; the last one gets
    // the remaining probability. Allows the selection pressure to be lowered smoothly.
    //
    // Complexity: time O(m * k * log k), memory O(m + k).
    // m - the number of selected parents (2 * parentPairCount);
    // k - the tournament size.
    public sealed class ProbabilisticTournamentParentSelection<TGene> : IParentSelection<TGene>
    {
        // How many randomly drawn candidates compete in one tournament; at least 2.
        public int TournamentSize { get; }

        // Probability p that the best of the remaining candidates wins. Allowed range is [0.5; 1]:
        // p = 1 means the best candidate always wins, which is a regular TournamentParentSelection;
        // p = 0.5 with two candidates gives each a 50/50 chance, so there is effectively no selection;
        // p < 0.5 would make the worse candidate win more often than the better one, which is anti-selection, so it is rejected.
        public double WinProbability { get; }


        public ProbabilisticTournamentParentSelection(int tournamentSize = 2, double winProbability = 0.75)
        {
            if (tournamentSize < 2)
                throw new ArgumentOutOfRangeException(nameof(tournamentSize), tournamentSize, "Tournament size must be at least 2.");
            if (winProbability < 0.5 || winProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(winProbability), winProbability, "Win probability must be between 0.5 and 1.");
            TournamentSize = tournamentSize;
            WinProbability = winProbability;
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IIndividualComparer<TGene> comparer,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, comparer, parentPairCount, random);

            int parentCount = 2 * parentPairCount;
            var parents = new Individual<TGene>[parentCount];

            var candidates = new Individual<TGene>[TournamentSize];
            for (int i = 0; i < parentCount; i++)
            {
                for (int j = 0; j < TournamentSize; j++)
                    candidates[j] = population[random.Next(population.Count)];
                Array.Sort(candidates, (x, y) => comparer.Compare(y, x));
                parents[i] = PickWinner(candidates, random);
            }

            return ParentSelectionUtilities.ToPairs(parents);
        }

        private Individual<TGene> PickWinner(Individual<TGene>[] candidatesBestFirst, IRandom random)
        {
            for (int i = 0; i < candidatesBestFirst.Length - 1; i++)
                if (random.NextDouble() < WinProbability)
                    return candidatesBestFirst[i];
            return candidatesBestFirst[candidatesBestFirst.Length - 1];
        }
    }
}
