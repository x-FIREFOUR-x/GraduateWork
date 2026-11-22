using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Selection
{
    // Shared helpers of NSGA-II (Deb et al., 2002): the fast non-dominated sorting into fronts, the crowding distance,
    // the order by rank and crowding distance used by the selections, and the search of non-dominated individuals.
    // x dominates y when comparer.Compare(x, y) > 0, so with ConstrainedFitnessComparer infeasible individuals fall
    // into worse fronts by their violation of the constraint.
    internal static class ParetoUtilities
    {
        // Indices of all individuals in the order of priority: by front from the non-dominated one, and within a front by crowding distance
        // from the largest; of equal distances the input order is kept. ranks and crowdingDistances are indexed by the individual, ranks from 0.
        public static int[] OrderByRankAndCrowding<TGene>(IReadOnlyList<Individual<TGene>> individuals, IIndividualComparer<TGene> comparer, out int[] ranks, out double[] crowdingDistances)
        {
            var individualRanks = new int[individuals.Count];
            var individualCrowdingDistances = new double[individuals.Count];
            var order = new List<int>(individuals.Count);

            var fronts = SortIntoFronts(individuals, comparer);
            for (int rank = 0; rank < fronts.Count; rank++)
            {
                var front = fronts[rank];
                var distances = CrowdingDistances(individuals, front);
                for (int i = 0; i < front.Count; i++)
                {
                    individualRanks[front[i]] = rank;
                    individualCrowdingDistances[front[i]] = distances[i];
                }
                order.AddRange(Enumerable.Range(0, front.Count).OrderByDescending(i => distances[i]).Select(i => front[i]));
            }

            ranks = individualRanks;
            crowdingDistances = individualCrowdingDistances;
            return order.ToArray();
        }

        // Fronts of indices into individuals, from the non-dominated one; the indices of each front in ascending order.
        // Time O(n^2 * c), memory O(n^2); n - the number of individuals, c - the time of one comparison.
        private static List<List<int>> SortIntoFronts<TGene>(IReadOnlyList<Individual<TGene>> individuals, IIndividualComparer<TGene> comparer)
        {
            List<int>[] individualsDominatedBy = new List<int>[individuals.Count];
            for (int i = 0; i < individuals.Count; i++)
                individualsDominatedBy[i] = new List<int>();

            int[] dominatorCounts = new int[individuals.Count];

            for (int i = 0; i < individuals.Count; i++)
                for (int j = i + 1; j < individuals.Count; j++)
                {
                    int comparison = comparer.Compare(individuals[i], individuals[j]);
                    if (comparison > 0)
                    {
                        individualsDominatedBy[i].Add(j);
                        dominatorCounts[j]++;
                    }
                    else if (comparison < 0)
                    {
                        individualsDominatedBy[j].Add(i);
                        dominatorCounts[i]++;
                    }
                }

            var fronts = new List<List<int>>();
            var front = new List<int>();
            for (int i = 0; i < individuals.Count; i++)
                if (dominatorCounts[i] == 0)
                    front.Add(i);

            int sortedCount = 0;
            while (front.Count > 0)
            {
                fronts.Add(front);
                sortedCount += front.Count;
                var nextFront = new List<int>();
                foreach (int i in front)
                    foreach (int j in individualsDominatedBy[i])
                        if (--dominatorCounts[j] == 0)
                            nextFront.Add(j);
                nextFront.Sort();
                front = nextFront;
            }
            if (sortedCount != individuals.Count)
                throw new InvalidOperationException($"{comparer.GetType().Name.Split('`')[0]} is not transitive: individuals dominate each other in a cycle, " +
                    $"so {individuals.Count - sortedCount} of {individuals.Count} individuals fall into no front.");

            return fronts;
        }

        // Crowding distance of every individual of the front, in the order of front: for each objective the boundary individuals
        // get +infinity and the others the distance between their neighbours divided by the range of the objective on the front.
        // An objective with the same value on the whole front adds nothing; in a front of one or two individuals all distances are +infinity.
        // Time O(k * m * log(m)), memory O(m); k - the number of objectives, m - the size of the front.
        private static double[] CrowdingDistances<TGene>(IReadOnlyList<Individual<TGene>> individuals, IReadOnlyList<int> front)
        {
            double[] distances = new double[front.Count];
            if (front.Count <= 2)
            {
                for (int i = 0; i < distances.Length; i++)
                    distances[i] = double.PositiveInfinity;
                return distances;
            }

            int objectiveCount = individuals[front[0]].FitnessValues.Count;
            int[] positionsInObjectiveOrder = new int[front.Count];
            for (int objective = 0; objective < objectiveCount; objective++)
            {
                for (int i = 0; i < positionsInObjectiveOrder.Length; i++)
                    positionsInObjectiveOrder[i] = i;
                Array.Sort(positionsInObjectiveOrder, (first, second) =>
                {
                    int comparison = ObjectiveValue(individuals, front, first, objective).CompareTo(ObjectiveValue(individuals, front, second, objective));
                    return comparison != 0 ? comparison : first.CompareTo(second);
                });

                int lastIndex = positionsInObjectiveOrder.Length - 1;
                double minObjectiveValue = ObjectiveValue(individuals, front, positionsInObjectiveOrder[0], objective);
                double maxObjectiveValue = ObjectiveValue(individuals, front, positionsInObjectiveOrder[lastIndex], objective);
                if (maxObjectiveValue == minObjectiveValue)
                    continue;

                distances[positionsInObjectiveOrder[0]] = double.PositiveInfinity;
                distances[positionsInObjectiveOrder[lastIndex]] = double.PositiveInfinity;
                for (int i = 1; i < lastIndex; i++)
                    distances[positionsInObjectiveOrder[i]] += (ObjectiveValue(individuals, front, positionsInObjectiveOrder[i + 1], objective) - ObjectiveValue(individuals, front, positionsInObjectiveOrder[i - 1], objective)) / (maxObjectiveValue - minObjectiveValue);
            }

            return distances;
        }

        private static double ObjectiveValue<TGene>(IReadOnlyList<Individual<TGene>> individuals, IReadOnlyList<int> front, int position, int objective)
            => individuals[front[position]].FitnessValues[objective];

        // Individuals that no other individual dominates, in the input order.
        public static List<Individual<TGene>> FindNondominated<TGene>(IReadOnlyList<Individual<TGene>> individuals, IIndividualComparer<TGene> comparer)
        {
            var nondominated = new List<Individual<TGene>>();
            for (int i = 0; i < individuals.Count; i++)
            {
                bool isDominated = false;
                for (int j = 0; j < individuals.Count && !isDominated; j++)
                    isDominated = j != i && comparer.IsBetter(individuals[j], individuals[i]);
                if (!isDominated)
                    nondominated.Add(individuals[i]);
            }
            return nondominated;
        }
    }
}
