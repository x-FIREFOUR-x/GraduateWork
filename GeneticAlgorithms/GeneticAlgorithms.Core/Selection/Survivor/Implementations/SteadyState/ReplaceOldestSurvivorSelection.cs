using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.SteadyState
{
    // Steady-state replacement of the oldest individual (FIFO): each child replaces the individual that has been
    // in the population for the longest time, regardless of fitness. No individual lives forever, so the population keeps renewing.
    // The age order is reset by Initialize. With RejectDuplicates, a child with the same genes as an existing individual is discarded.
    //
    // Complexity: time O(lambda * n) with RejectDuplicates, otherwise O(lambda)
    //             memory O(n) for the age order
    // n - the population size;
    // lambda - the number of offspring.
    public sealed class ReplaceOldestSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        private readonly Queue<int> _populationIndicesOldestFirst = new Queue<int>();

        // Whether a child is discarded when an individual with the same genes is already in the population.
        public bool RejectDuplicates { get; }

        public IIndividualComparer<TGene> Comparer { get; }


        public ReplaceOldestSurvivorSelection(bool rejectDuplicates = true, IIndividualComparer<TGene> comparer = null)
        {
            RejectDuplicates = rejectDuplicates;
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public void Initialize(List<Individual<TGene>> population)
        {
            if (population == null)
                throw new ArgumentNullException(nameof(population));
            _populationIndicesOldestFirst.Clear();
            for (int i = 0; i < population.Count; i++)
                _populationIndicesOldestFirst.Enqueue(i);
        }

        public int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            SurvivorSelectionUtilities.Validate(population, offspring, random);
            if (_populationIndicesOldestFirst.Count != population.Count)
                throw new InvalidOperationException("Initialize method must be called with the same population before Replace.");

            int accepted = 0;
            foreach (var offspringItem in offspring)
            {
                var child = offspringItem.Child;
                if (RejectDuplicates && SurvivorSelectionUtilities.PopulationContainsSameIndividual(population, child))
                    continue;

                int oldestIndex = _populationIndicesOldestFirst.Dequeue();
                population[oldestIndex] = child;
                _populationIndicesOldestFirst.Enqueue(oldestIndex);
                accepted++;
            }
            return accepted;
        }
    }
}
