using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor
{
    // Shared helpers for survivor selections: argument validation, searching the population, combining the population with
    // the offspring, selecting survivors from sorted survivor candidates, replacing the population with the survivors
    // and measuring the distance between individuals.
    internal static class SurvivorSelectionUtilities
    {
        public static void Validate<TGene>(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random)
        {
            if (population == null)
                throw new ArgumentNullException(nameof(population));
            if (population.Count == 0)
                throw new ArgumentException("Population must not be empty.", nameof(population));
            if (offspring == null)
                throw new ArgumentNullException(nameof(offspring));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
        }

        public static int IndexOfWorst<TGene>(IReadOnlyList<Individual<TGene>> population, IIndividualComparer<TGene> comparer)
        {
            int worstIndex = 0;
            for (int i = 1; i < population.Count; i++)
                if (comparer.IsWorse(population[i], population[worstIndex]))
                    worstIndex = i;
            return worstIndex;
        }

        public static int IndexOf<TGene>(IReadOnlyList<Individual<TGene>> population, Individual<TGene> individual)
        {
            for (int i = 0; i < population.Count; i++)
                if (ReferenceEquals(population[i], individual))
                    return i;
            return -1;
        }

        public static bool PopulationContainsSameIndividual<TGene>(IReadOnlyList<Individual<TGene>> population, Individual<TGene> individual)
        {
            foreach (var existingIndividual in population)
                if (existingIndividual.HasSameGenes(individual))
                    return true;
            return false;
        }

        public static List<Individual<TGene>> CombinePopulationAndChildren<TGene>(IReadOnlyList<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring)
        {
            var survivorCandidates = new List<Individual<TGene>>(population.Count + offspring.Count);
            survivorCandidates.AddRange(population);
            survivorCandidates.AddRange(GetChildren(offspring));
            return survivorCandidates;
        }

        public static List<Individual<TGene>> GetChildren<TGene>(IReadOnlyList<Offspring<TGene>> offspring)
        {
            var children = new List<Individual<TGene>>(offspring.Count);
            foreach (var offspringItem in offspring)
                children.Add(offspringItem.Child);
            return children;
        }

        public static Individual<TGene>[] SortBestToWorst<TGene>(IEnumerable<Individual<TGene>> individuals, IIndividualComparer<TGene> comparer)
        {
            return individuals.OrderByDescending(individual => individual, comparer).ToArray();
        }

        public static List<Individual<TGene>> SelectSurvivors<TGene>(IReadOnlyList<Individual<TGene>> individualsInPriorityOrder, int survivorCount, bool rejectDuplicates)
        {
            var survivors = new List<Individual<TGene>>(survivorCount);
            var skipped = new List<Individual<TGene>>();
            foreach (var individual in individualsInPriorityOrder)
            {
                if (survivors.Count == survivorCount)
                    break;
                if (rejectDuplicates && PopulationContainsSameIndividual(survivors, individual))
                    skipped.Add(individual);
                else
                    survivors.Add(individual);
            }

            foreach (var individual in skipped)
            {
                if (survivors.Count == survivorCount)
                    break;
                survivors.Add(individual);
            }
            return survivors;
        }

        public static int ReplacePopulationWithSurvivors<TGene>(List<Individual<TGene>> population, IReadOnlyList<Individual<TGene>> survivors, IReadOnlyList<Offspring<TGene>> offspring)
        {
            population.Clear();
            population.AddRange(survivors);

            int acceptedChildCount = 0;
            foreach (var survivor in survivors)
                foreach (var offspringItem in offspring)
                    if (ReferenceEquals(survivor, offspringItem.Child))
                    {
                        acceptedChildCount++;
                        break;
                    }
            return acceptedChildCount;
        }

        public static double GeneMultisetJaccardDistance<TGene>(Individual<TGene> first, Individual<TGene> second)
        {
            if (first.Count == 0 && second.Count == 0)
                return 0;

            var remainingGeneCounts = new Dictionary<TGene, int>();
            foreach (var gene in first.Genes)
                remainingGeneCounts[gene] = remainingGeneCounts.TryGetValue(gene, out int geneCount) ? geneCount + 1 : 1;

            int commonGeneCount = 0;
            foreach (var gene in second.Genes)
                if (remainingGeneCounts.TryGetValue(gene, out int geneCount) && geneCount > 0)
                {
                    remainingGeneCounts[gene] = geneCount - 1;
                    commonGeneCount++;
                }

            int unionGeneCount = first.Count + second.Count - commonGeneCount;
            return 1 - (double)commonGeneCount / unionGeneCount;
        }
    }
}
