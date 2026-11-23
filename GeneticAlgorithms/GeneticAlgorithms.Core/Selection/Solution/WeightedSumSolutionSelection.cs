using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Selection.Solution
{
    // Weighted sum solution selection: each objective is normalized on the front to [0; 1] and the individual with the largest
    // weighted sum of the normalized objectives is chosen; of equal sums the first one. Without weights all objectives weigh the same.
    // For two objectives with equal weights it chooses the knee point of the front: the point farthest from the line through
    // the extreme points, where improving one objective costs the most of the other.
    // An objective with the same value on the whole front gets 0.5 for every individual and does not affect the choice.
    //
    // Example: front (0, 10), (6, 8), (9, 3), (10, 0), equal weights -> sums 1, 1.4, 1.2, 1 -> (6, 8).
    //
    // Complexity: time O(m * k)
    //             memory O(m * k)
    // m - the size of the front; k - the number of objectives.
    public sealed class WeightedSumSolutionSelection<TGene> : ISolutionSelection<TGene>
    {
        // Weights of the objectives, finite and non-negative with at least one positive; null when all objectives weigh the same.
        public IReadOnlyList<double> Weights { get; }


        public WeightedSumSolutionSelection(IReadOnlyList<double> weights = null)
        {
            if (weights == null)
                return;

            if (weights.Any(weight => !(weight >= 0) || double.IsInfinity(weight)))
                throw new ArgumentException("Every weight must be a finite non-negative number.", nameof(weights));
            if (!weights.Any(weight => weight > 0))
                throw new ArgumentException("At least one weight must be positive.", nameof(weights));
            Weights = weights.ToArray();
        }

        public Individual<TGene> Select(IReadOnlyList<Individual<TGene>> front)
        {
            Validate(front);

            int objectiveCount = front[0].FitnessValues.Count;
            var normalizedObjectivesOfIndividualsInFront = SolutionSelectionUtilities.Normalize(front, objectiveCount);
            int bestIndex = 0;
            double bestSum = double.NegativeInfinity;
            for (int i = 0; i < front.Count; i++)
            {
                double sum = 0;
                for (int objective = 0; objective < objectiveCount; objective++)
                    sum += (Weights?[objective] ?? 1) * normalizedObjectivesOfIndividualsInFront[i][objective];
                if (sum > bestSum)
                {
                    bestSum = sum;
                    bestIndex = i;
                }
            }
            return front[bestIndex];
        }

        private void Validate(IReadOnlyList<Individual<TGene>> front)
        {
            SolutionSelectionUtilities.Validate(front);

            int objectiveCount = front[0].FitnessValues.Count;
            if (Weights != null && Weights.Count != objectiveCount)
                throw new ArgumentException($"Weight count {Weights.Count} must match the number of objectives {objectiveCount}.", nameof(front));
        }
    }
}
