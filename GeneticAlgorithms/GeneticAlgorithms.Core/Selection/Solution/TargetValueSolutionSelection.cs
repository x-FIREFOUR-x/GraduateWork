using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Selection.Solution
{
    // Target value solution selection: chooses the individual whose objective ObjectiveIndex is closest to TargetValue,
    // for example a wave whose difficulty matches the level of the player; of equally close individuals the one with the largest sum
    // of the other objectives normalized on the front to [0; 1], and then the first one.
    //
    // Example: front (4, 9), (6, 1), objective 0, target 5 -> both are 1 away, (4, 9) has the larger second objective -> (4, 9).
    //
    // Complexity: time O(m * k)
    //             memory O(m * k)
    // m - the size of the front; k - the number of objectives.
    public sealed class TargetValueSolutionSelection<TGene> : ISolutionSelection<TGene>
    {
        // Index of the objective among the fitness values.
        public int ObjectiveIndex { get; }

        public double TargetValue { get; }


        public TargetValueSolutionSelection(int objectiveIndex, double targetValue)
        {
            if (objectiveIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(objectiveIndex), objectiveIndex, "Objective index must be non-negative.");
            ObjectiveIndex = objectiveIndex;

            if (double.IsNaN(targetValue) || double.IsInfinity(targetValue))
                throw new ArgumentOutOfRangeException(nameof(targetValue), targetValue, "Target value must be a finite number.");
            TargetValue = targetValue;
        }

        public Individual<TGene> Select(IReadOnlyList<Individual<TGene>> front)
        {
            Validate(front);

            int objectiveCount = front[0].FitnessValues.Count;

            var normalizedObjectivesOfIndividualsInFront = 
                SolutionSelectionUtilities.Normalize(front, objectiveCount);
            
            int bestIndex = 0;
            double bestDelta = double.PositiveInfinity;
            double bestOtherObjectivesSum = double.NegativeInfinity;
            for (int i = 0; i < front.Count; i++)
            {
                double delta = Math.Abs(front[i].FitnessValues[ObjectiveIndex] - TargetValue);
                double otherObjectivesSum = 0;
                for (int objective = 0; objective < objectiveCount; objective++)
                    if (objective != ObjectiveIndex)
                        otherObjectivesSum += normalizedObjectivesOfIndividualsInFront[i][objective];

                if (delta < bestDelta || (delta == bestDelta && otherObjectivesSum > bestOtherObjectivesSum))
                {
                    bestDelta = delta;
                    bestOtherObjectivesSum = otherObjectivesSum;
                    bestIndex = i;
                }
            }

            return front[bestIndex];
        }

        private void Validate(IReadOnlyList<Individual<TGene>> front)
        {
            SolutionSelectionUtilities.Validate(front);

            int objectiveCount = front[0].FitnessValues.Count;
            if (ObjectiveIndex >= objectiveCount)
                throw new ArgumentOutOfRangeException(nameof(front), ObjectiveIndex, $"Objective index {ObjectiveIndex} is out of range for {objectiveCount} objectives.");
        }
    }
}
