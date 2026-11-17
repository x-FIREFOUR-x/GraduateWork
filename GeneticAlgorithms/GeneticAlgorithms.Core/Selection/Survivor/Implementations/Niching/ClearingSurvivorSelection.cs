using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.Niching
{
    // Clearing (Petrowski, 1996) applied to the merged population and offspring: the current population and the offspring are merged and sorted
    // from best to worst. Going down the list, each individual that is not cleared yet opens a niche: within ClearingRadius
    // of it only NicheCapacity best individuals keep their place, the rest are cleared. Niche winners survive first,
    // cleared individuals fill the remaining places from best to worst. Near-duplicates are pushed out, so diversity is kept.
    // The distance between individuals defaults to the Jaccard distance between the gene multisets, which lies in [0; 1].
    //
    // Complexity: time O((n + lambda)^2 * D)
    //             memory O(n + lambda)
    // n - the population size;
    // lambda - the number of offspring;
    // D - the cost of one distance computation (O(g) for the default Jaccard distance, g - is the number of genes).
    public sealed class ClearingSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Measures how different two individuals are; defaults to the Jaccard distance between gene multisets.
        private readonly Func<Individual<TGene>, Individual<TGene>, double> _getIndividualDistance;

        // Largest distance from a niche leader at which an individual still belongs to its niche; positive.
        public double ClearingRadius { get; }

        // How many best individuals of each niche keep priority; at least 1.
        public int NicheCapacity { get; }

        public IIndividualComparer<TGene> Comparer { get; }


        public ClearingSurvivorSelection(double clearingRadius, int nicheCapacity = 1, Func<Individual<TGene>, Individual<TGene>, double> getIndividualDistance = null, IIndividualComparer<TGene> comparer = null)
        {
            if (!(clearingRadius > 0) || double.IsInfinity(clearingRadius))
                throw new ArgumentOutOfRangeException(nameof(clearingRadius), clearingRadius, "Clearing radius must be positive and finite.");
            if (nicheCapacity < 1)
                throw new ArgumentOutOfRangeException(nameof(nicheCapacity), nicheCapacity, "Niche capacity must be at least 1.");
            ClearingRadius = clearingRadius;
            NicheCapacity = nicheCapacity;
            _getIndividualDistance = getIndividualDistance ?? SurvivorSelectionUtilities.GeneMultisetJaccardDistance;
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);

            var survivorCandidates = SurvivorSelectionUtilities.CombinePopulationAndChildren(population, offspring);
            var sortedSurvivorCandidates = SurvivorSelectionUtilities.SortBestToWorst(survivorCandidates, Comparer);

            var isCleared = new bool[sortedSurvivorCandidates.Length];
            for (int i = 0; i < sortedSurvivorCandidates.Length; i++)
            {
                if (isCleared[i])
                    continue;

                int nicheWinnerCount = 1;
                for (int j = i + 1; j < sortedSurvivorCandidates.Length; j++)
                {
                    if (isCleared[j] || _getIndividualDistance(sortedSurvivorCandidates[i], sortedSurvivorCandidates[j]) >= ClearingRadius)
                        continue;
                    if (nicheWinnerCount < NicheCapacity)
                        nicheWinnerCount++;
                    else
                        isCleared[j] = true;
                }
            }

            var survivors = new List<Individual<TGene>>(population.Count);
            for (int i = 0; i < sortedSurvivorCandidates.Length && survivors.Count < population.Count; i++)
                if (!isCleared[i])
                    survivors.Add(sortedSurvivorCandidates[i]);
            for (int i = 0; i < sortedSurvivorCandidates.Length && survivors.Count < population.Count; i++)
                if (isCleared[i])
                    survivors.Add(sortedSurvivorCandidates[i]);

            return SurvivorSelectionUtilities.ReplacePopulationWithSurvivors(population, survivors, offspring);
        }
    }
}
