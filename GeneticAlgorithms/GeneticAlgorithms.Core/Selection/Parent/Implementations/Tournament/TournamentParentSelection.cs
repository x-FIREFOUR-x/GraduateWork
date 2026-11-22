using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Tournament
{
    // Tournament selection: TournamentSize individuals are drawn at random (with replacement)
    // and the best of them becomes a parent. Selection pressure grows with the tournament size.
    //
    // Complexity: time O(m * k), memory O(m).
    // m - the number of selected parents (2 * parentPairCount);
    // k - the tournament size.
    public sealed class TournamentParentSelection<TGene> : IParentSelection<TGene>
    {
        public IIndividualComparer<TGene> Comparer { get; }

        public bool UsesFitnessValues => false;

        // How many randomly drawn candidates compete in one tournament; at least 1.
        public int TournamentSize { get; }


        public TournamentParentSelection(int tournamentSize = 3, IIndividualComparer<TGene> comparer = null)
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

            int parentCount = 2 * parentPairCount;
            var parents = new Individual<TGene>[parentCount];

            for (int i = 0; i < parentCount; i++)
                parents[i] = SelectParent(population, Comparer, random);

            return ParentSelectionUtilities.ToPairs(parents);
        }

        private Individual<TGene> SelectParent(IReadOnlyList<Individual<TGene>> population, IIndividualComparer<TGene> comparer, IRandom random)
        {
            var bestCandidate = population[random.Next(population.Count)];
            for (int i = 1; i < TournamentSize; i++)
            {
                var candidate = population[random.Next(population.Count)];
                if (comparer.IsBetter(candidate, bestCandidate))
                    bestCandidate = candidate;
            }
            return bestCandidate;
        }
    }
}
