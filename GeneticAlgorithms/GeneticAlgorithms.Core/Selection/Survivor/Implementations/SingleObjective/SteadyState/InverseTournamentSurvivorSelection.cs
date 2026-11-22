using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.SteadyState
{
    // Steady-state inverse (kill) tournament: for each child, TournamentSize random individuals are drawn
    // (with replacement) and the worst of them is replaced if the child is better. Selection pressure grows with
    // the tournament size; a tournament as large as the population behaves like ReplaceWorstSurvivorSelection.
    // With RejectDuplicates, a child with the same genes as an existing individual is discarded.
    //
    // Complexity: time O(lambda * (k + n)) with RejectDuplicates, otherwise O(lambda * k)
    //             memory O(1)
    // n - the population size;
    // lambda - the number of offspring;
    // k - the tournament size.
    public sealed class InverseTournamentSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // How many random individuals are compared to choose the one the child may replace; at least 1.
        public int TournamentSize { get; }

        // Whether a child is discarded when an individual with the same genes is already in the population.
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }

        public bool RequiresTotalOrder => false;


        public InverseTournamentSurvivorSelection(int tournamentSize = 2, bool rejectDuplicates = true, IIndividualComparer<TGene> comparer = null)
        {
            if (tournamentSize < 1)
                throw new ArgumentOutOfRangeException(nameof(tournamentSize), tournamentSize, "Tournament size must be at least 1.");
            TournamentSize = tournamentSize;
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
                int worstCandidateIndex = random.Next(population.Count);
                for (int i = 1; i < TournamentSize; i++)
                {
                    int candidateIndex = random.Next(population.Count);
                    if (Comparer.IsWorse(population[candidateIndex], population[worstCandidateIndex]))
                        worstCandidateIndex = candidateIndex;
                }

                if (Comparer.IsNotBetter(child, population[worstCandidateIndex]))
                    continue;
                if (RejectDuplicates && SurvivorSelectionUtilities.PopulationContainsSameIndividual(population, child))
                    continue;

                population[worstCandidateIndex] = child;
                accepted++;
            }
            return accepted;
        }
    }
}
