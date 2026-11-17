using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.Niching
{
    // Restricted tournament selection (Harik, 1995): for each child, CandidateCount random individuals (the window in the original paper) are drawn (with replacement)
    // and the one most similar to the child is replaced if the child is better. A child competes only within its own niche,
    // so several different solutions can coexist. The distance between individuals defaults to the Jaccard distance
    // between the gene multisets.
    //
    // Complexity: time O(lambda * w * D)
    //             memory O(1)
    // lambda - the number of offspring;
    // w - the candidate count (windows size);
    // D - the cost of one distance computation (O(g) for the default Jaccard distance, g is the number of genes).
    public sealed class RestrictedTournamentSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Measures how different two individuals are; defaults to the Jaccard distance between gene multisets.
        private readonly Func<Individual<TGene>, Individual<TGene>, double> _getIndividualDistance;

        // How many random individuals are checked to find the one most similar to the child; at least 1.
        public int CandidateCount { get; }

        public IIndividualComparer<TGene> Comparer { get; }


        public RestrictedTournamentSurvivorSelection(int candidateCount = 5, Func<Individual<TGene>, Individual<TGene>, double> getIndividualDistance = null, IIndividualComparer<TGene> comparer = null)
        {
            if (candidateCount < 1)
                throw new ArgumentOutOfRangeException(nameof(candidateCount), candidateCount, "Candidate count must be at least 1.");
            CandidateCount = candidateCount;
            _getIndividualDistance = getIndividualDistance ?? SurvivorSelectionUtilities.GeneMultisetJaccardDistance;
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
                int closestCandidateIndex = random.Next(population.Count);
                double closestDistance = _getIndividualDistance(child, population[closestCandidateIndex]);
                for (int i = 1; i < CandidateCount; i++)
                {
                    int candidateIndex = random.Next(population.Count);
                    double distance = _getIndividualDistance(child, population[candidateIndex]);
                    if (distance < closestDistance)
                    {
                        closestCandidateIndex = candidateIndex;
                        closestDistance = distance;
                    }
                }

                if (Comparer.IsNotBetter(child, population[closestCandidateIndex]))
                    continue;

                population[closestCandidateIndex] = child;
                accepted++;
            }
            return accepted;
        }
    }
}
