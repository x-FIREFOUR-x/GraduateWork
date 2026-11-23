using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;

namespace GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.SteadyState
{
    public sealed class BestChildReplacesWorstSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }

        public bool RequiresTotalOrder => false;


        public BestChildReplacesWorstSurvivorSelection(bool rejectDuplicates = false, IIndividualComparer<TGene> comparer = null)
        {
            RejectDuplicates = rejectDuplicates;
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);
            if (offspring.Count == 0)
                return 0;

            var bestChild = offspring[0].Child;
            for (int i = 1; i < offspring.Count; i++)
                if (Comparer.IsBetter(offspring[i].Child, bestChild))
                    bestChild = offspring[i].Child;

            int indexWorstIndividual = SurvivorSelectionUtilities.IndexOfWorst(population, Comparer);
            if (Comparer.IsNotBetter(bestChild, population[indexWorstIndividual]))
                return 0;
            if (RejectDuplicates && SurvivorSelectionUtilities.PopulationContainsSameIndividual(population, bestChild))
                return 0;

            population[indexWorstIndividual] = bestChild;
            return 1;
        }
    }
}
