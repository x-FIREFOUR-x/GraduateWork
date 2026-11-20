using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations
{
    // Random survivor selection: the current population and the offspring are merged and a uniformly random subset
    // of population size survives, regardless of fitness. No selection pressure; used as a baseline in experiments.
    //
    // Complexity: time O(n * lambda)
    //             memory O(n + lambda)
    // n - the population size;
    // lambda - the number of offspring.
    public sealed class RandomSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        public IIndividualComparer<TGene> Comparer { get; }


        public RandomSurvivorSelection(IIndividualComparer<TGene> comparer = null)
        {
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);

            var survivorCandidates = SurvivorSelectionUtilities.CombinePopulationAndChildren(population, offspring);
            for (int i = 0; i < population.Count; i++)
            {
                int j = random.Next(i, survivorCandidates.Count);
                (survivorCandidates[i], survivorCandidates[j]) = (survivorCandidates[j], survivorCandidates[i]);
            }

            var survivors = survivorCandidates.GetRange(0, population.Count);
            return SurvivorSelectionUtilities.ReplacePopulationWithSurvivors(population, survivors, offspring);
        }
    }
}
