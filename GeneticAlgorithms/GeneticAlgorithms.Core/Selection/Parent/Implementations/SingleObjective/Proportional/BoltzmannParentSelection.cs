using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.SingleObjective.Proportional
{
    // Boltzmann selection: the selection probability is proportional to exp(f / T), where f is the fitness
    // value with FitnessIndex and T = Temperature. A high temperature makes selection close to uniform,
    // a low one makes it close to always picking the best individual. Allows negative fitness values.
    //
    // Complexity: time O(n + m * log n), memory O(n + m).
    // n - the population size;
    // m - the number of selected parents (2 * parentPairCount).
    public sealed class BoltzmannParentSelection<TGene> : IParentSelection<TGene>
    {
        // Null: the selection reads the fitness value with FitnessIndex directly instead of comparing individuals.
        public IIndividualComparer<TGene> Comparer => null;

        public bool UsesFitnessValues => true;

        public bool RequiresTotalOrder => true;

        // Index of the fitness value the selection probabilities are computed from; non-negative.
        public int FitnessIndex { get; }

        public double Temperature { get; }


        public BoltzmannParentSelection(double temperature = 1, int fitnessIndex = 0)
        {
            if (!(temperature > 0) || double.IsInfinity(temperature))
                throw new ArgumentOutOfRangeException(nameof(temperature), temperature, "Temperature must be positive and finite.");
            Temperature = temperature;
            if (fitnessIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(fitnessIndex), fitnessIndex, "Fitness index must be non-negative.");
            FitnessIndex = fitnessIndex;
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, parentPairCount, random);

            int parentCount = 2 * parentPairCount;

            var fitnessValues = ParentSelectionUtilities.GetFitnessValues(population, FitnessIndex);
            double maxFitness = double.NegativeInfinity;
            foreach (double fitness in fitnessValues)
                maxFitness = Math.Max(maxFitness, fitness);

            var selectionProbabilityWeights = new double[fitnessValues.Length];
            for (int i = 0; i < fitnessValues.Length; i++)
            {
                    // The weights used are exp((f - max) / T): subtracting the maximum fitness keeps exp from overflowing
                    // and does not change the probabilities, since all weights are scaled by the same factor.
                selectionProbabilityWeights[i] = Math.Exp((fitnessValues[i] - maxFitness) / Temperature);
            }

            return ParentSelectionUtilities.ToPairs(ParentSelectionUtilities.SelectParentsByProbabilityWeights(population, selectionProbabilityWeights, parentCount, random));
        }
    }
}
