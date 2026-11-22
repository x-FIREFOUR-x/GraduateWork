using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor.Implementations.SingleObjective.Niching
{
    // Deterministic crowding (Mahfoud, 1995): each child competes with the more similar of its two parents and replaces it
    // if the child is better. Similar individuals compete with each other, so different niches of the search space survive.
    // The distance between individuals defaults to the Jaccard distance between the gene multisets.
    // A parent that has already been replaced by an earlier child of the same generation no longer competes.
    //
    // Complexity: time O(lambda * (n + D))
    //             memory O(1)
    // n - the population size;
    // lambda - the number of offspring;
    // D - the cost of one distance computation (O(g) for the default Jaccard distance, g is the number of genes).
    public sealed class DeterministicCrowdingSurvivorSelection<TGene> : ISurvivorSelection<TGene>
    {
        // Measures how different two individuals are; defaults to the Jaccard distance between gene multisets.
        private readonly Func<Individual<TGene>, Individual<TGene>, double> _getIndividualDistance;

        public IIndividualComparer<TGene> Comparer { get; }

        public bool RequiresTotalOrder => false;


        public DeterministicCrowdingSurvivorSelection(Func<Individual<TGene>, Individual<TGene>, double> getIndividualDistance = null, IIndividualComparer<TGene> comparer = null)
        {
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
                int closerParentIndex = FindCloserParentIndex(population, child, offspringItem.Parents.First, offspringItem.Parents.Second);
                if (closerParentIndex < 0 || Comparer.IsNotBetter(child, population[closerParentIndex]))
                    continue;

                population[closerParentIndex] = child;
                accepted++;
            }
            return accepted;
        }

        private int FindCloserParentIndex(List<Individual<TGene>> population, Individual<TGene> child, Individual<TGene> firstParent, Individual<TGene> secondParent)
        {
            int firstParentIndex = SurvivorSelectionUtilities.IndexOf(population, firstParent);
            int secondParentIndex = SurvivorSelectionUtilities.IndexOf(population, secondParent);
            if (firstParentIndex < 0)
                return secondParentIndex;
            if (secondParentIndex < 0)
                return firstParentIndex;
            return _getIndividualDistance(child, secondParent) < _getIndividualDistance(child, firstParent) ? secondParentIndex : firstParentIndex;
        }
    }
}
