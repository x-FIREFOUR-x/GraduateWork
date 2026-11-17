using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.EvolutionStrategy
{
    // Round-robin tournament from evolutionary programming (Fogel, 1995): the current population and the offspring are merged,
    // every individual meets OpponentCount random opponents among the survivor candidates and scores a win for each opponent it is not worse than.
    // The individuals with the most wins survive; ties are broken by fitness. Softer selection pressure than BestOfPopulationAndOffspringSurvivorSelection:
    // a weaker individual can survive by meeting weak opponents.
    //
    // Complexity: time O((n + lambda) * (q + log(n + lambda)) + n * lambda)
    //             memory O(n + lambda)
    // n - the population size;
    // lambda - the number of offspring;
    // q - the opponent count.
    public sealed class RoundRobinTournamentSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // How many random opponents each candidate is compared with; at least 1.
        public int OpponentCount { get; }

        public IIndividualComparer<TGene> Comparer { get; }


        public RoundRobinTournamentSurvivorSelection(int opponentCount = 10, IIndividualComparer<TGene> comparer = null)
        {
            if (opponentCount < 1)
                throw new ArgumentOutOfRangeException(nameof(opponentCount), opponentCount, "Opponent count must be at least 1.");
            OpponentCount = opponentCount;
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);
            var survivorCandidates = SurvivorSelectionUtilities.CombinePopulationAndChildren(population, offspring);
            if (survivorCandidates.Count == 1)
                return 0;

            var winCounts = new int[survivorCandidates.Count];
            for (int i = 0; i < survivorCandidates.Count; i++)
                for (int match = 0; match < OpponentCount; match++)
                {
                    int opponentIndex = random.Next(survivorCandidates.Count - 1);
                    if (opponentIndex >= i)
                        opponentIndex++;
                    if (Comparer.IsNotWorse(survivorCandidates[i], survivorCandidates[opponentIndex]))
                        winCounts[i]++;
                }

            var survivors = Enumerable.Range(0, survivorCandidates.Count)
                .OrderByDescending(index => winCounts[index])
                .ThenByDescending(index => survivorCandidates[index], Comparer)
                .Take(population.Count)
                .Select(index => survivorCandidates[index])
                .ToList();
            return SurvivorSelectionUtilities.ReplacePopulationWithSurvivors(population, survivors, offspring);
        }
    }
}
