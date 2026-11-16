using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Proportional
{
    // Boltzmann selection: the selection probability is proportional to exp(f / T), where f is the fitness
    // at FitnessIndex and T = Temperature. A high temperature makes selection close to uniform,
    // a low one makes it close to always picking the best individual. Allows negative fitness values.
    public sealed class BoltzmannParentSelection<TGene> : IParentSelection<TGene>
    {
        public double Temperature { get; }
        public int FitnessIndex { get; }


        public BoltzmannParentSelection(double temperature = 1, int fitnessIndex = 0)
        {
            if (!(temperature > 0) || double.IsInfinity(temperature))
                throw new ArgumentOutOfRangeException(nameof(temperature), temperature, "Temperature must be positive and finite.");
            ParentSelectionUtilities.ValidateFitnessIndex(fitnessIndex);
            Temperature = temperature;
            FitnessIndex = fitnessIndex;
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IComparer<Individual<TGene>> comparer,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, comparer, parentPairCount, random);

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
