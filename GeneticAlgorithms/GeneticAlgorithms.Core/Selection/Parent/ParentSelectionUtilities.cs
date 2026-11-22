using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent
{
    // Shared helpers for parent selections: argument validation, rank sorting, fitness value extraction,
    // weighted sampling (roulette wheel and stochastic universal sampling), grouping parents into pairs and shuffling.
    internal static class ParentSelectionUtilities
    {
        public static void Validate<TGene>(IReadOnlyList<Individual<TGene>> population, int parentPairCount, IRandom random)
        {
            if (population == null)
                throw new ArgumentNullException(nameof(population));
            if (population.Count == 0)
                throw new ArgumentException("Population must not be empty.", nameof(population));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (parentPairCount < 1)
                throw new ArgumentOutOfRangeException(nameof(parentPairCount), parentPairCount, "Parent pair count must be at least 1.");
        }

        public static ParentPair<TGene>[] ToPairs<TGene>(Individual<TGene>[] parents)
        {
            var pairs = new ParentPair<TGene>[parents.Length / 2];
            for (int i = 0; i < pairs.Length; i++)
                pairs[i] = new ParentPair<TGene>(parents[2 * i], parents[2 * i + 1]);
            return pairs;
        }

        public static Individual<TGene>[] SortWorstToBest<TGene>(IReadOnlyList<Individual<TGene>> population, IIndividualComparer<TGene> comparer)
        {
            return population.OrderBy(individual => individual, comparer).ToArray();
        }

        public static double[] GetFitnessValues<TGene>(IReadOnlyList<Individual<TGene>> population, int fitnessIndex)
        {
            var fitnessValues = new double[population.Count];
            for (int i = 0; i < population.Count; i++)
                fitnessValues[i] = population[i].FitnessValues[fitnessIndex];
            return fitnessValues;
        }

        public static Individual<TGene>[] SelectParentsByProbabilityWeights<TGene>(IReadOnlyList<Individual<TGene>> candidates, double[] selectionProbabilityWeights, int parentCount, IRandom random)
        {
            var cumulativeProbabilityWeights = CalculateCumulativeProbabilityWeights(selectionProbabilityWeights);
            double totalProbabilityWeight = cumulativeProbabilityWeights[cumulativeProbabilityWeights.Length - 1];
            var selectedCandidates = new Individual<TGene>[parentCount];
            for (int i = 0; i < parentCount; i++)
                selectedCandidates[i] = candidates[FindCandidateIndex(cumulativeProbabilityWeights, random.NextDouble() * totalProbabilityWeight)];
            return selectedCandidates;
        }

        public static Individual<TGene>[] SelectParentsByStochasticUniversalSampling<TGene>(IReadOnlyList<Individual<TGene>> candidates, double[] selectionProbabilityWeights, int parentCount, IRandom random)
        {
            var selectedCandidates = new Individual<TGene>[parentCount];

            var cumulativeProbabilityWeights = CalculateCumulativeProbabilityWeights(selectionProbabilityWeights);
            double totalProbabilityWeight = cumulativeProbabilityWeights[cumulativeProbabilityWeights.Length - 1];

            double randomValueStep = totalProbabilityWeight / parentCount;
            double randomValue = random.NextDouble() * randomValueStep;

            int index = 0;
            for (int i = 0; i < parentCount; i++)
            {
                while (index < cumulativeProbabilityWeights.Length - 1 && cumulativeProbabilityWeights[index] <= randomValue)
                    index++;
                selectedCandidates[i] = candidates[index];
                randomValue += randomValueStep;
            }

            Shuffle(selectedCandidates, random);
            return selectedCandidates;
        }

        public static void Shuffle<T>(T[] items, IRandom random)
        {
            for (int i = items.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }

        private static double[] CalculateCumulativeProbabilityWeights(double[] selectionProbabilityWeights)
        {
            var cumulativeProbabilityWeights = new double[selectionProbabilityWeights.Length];
            double totalProbabilityWeight = 0;
            for (int i = 0; i < selectionProbabilityWeights.Length; i++)
            {
                if (double.IsNaN(selectionProbabilityWeights[i]) || double.IsInfinity(selectionProbabilityWeights[i]) || selectionProbabilityWeights[i] < 0)
                    throw new InvalidOperationException("Fitness-proportional selection requires finite non-negative selection probability weights.");
                totalProbabilityWeight += selectionProbabilityWeights[i];
                cumulativeProbabilityWeights[i] = totalProbabilityWeight;
            }

            if (totalProbabilityWeight > 0)
                return cumulativeProbabilityWeights;

                //All candidates have probability weight 0, then we set probability weight 1 for all candidates
            for (int i = 0; i < selectionProbabilityWeights.Length; i++)
                cumulativeProbabilityWeights[i] = i + 1;
            return cumulativeProbabilityWeights;
        }

        private static int FindCandidateIndex(double[] cumulativeProbabilityWeights, double randomValue)
        {
            int startIndex = 0;
            int endIndex = cumulativeProbabilityWeights.Length - 1;
            while (startIndex < endIndex)
            {
                int middleIndex = (startIndex + endIndex) / 2;
                if (cumulativeProbabilityWeights[middleIndex] > randomValue)
                    endIndex = middleIndex;
                else
                    startIndex = middleIndex + 1;
            }
            return startIndex;
        }
    }
}
