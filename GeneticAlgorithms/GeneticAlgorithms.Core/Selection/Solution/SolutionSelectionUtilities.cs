using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Selection.Solution
{
    // Shared helpers for solution selections: validation of the front and normalization of the objectives on the front.
    internal static class SolutionSelectionUtilities
    {
        // The front must not be empty or contain null individuals, and every individual must have the same number of fitness values.
        public static void Validate<TGene>(IReadOnlyList<Individual<TGene>> front)
        {
            if (front == null)
                throw new ArgumentNullException(nameof(front));
            if (front.Count == 0)
                throw new ArgumentException("Front must not be empty.", nameof(front));

            foreach (var individual in front)
                if (individual == null)
                    throw new ArgumentException("Front must not contain null individuals.", nameof(front));

            int objectiveCount = front[0].FitnessValues.Count;
            foreach (var individual in front)
                if (individual.FitnessValues.Count != objectiveCount)
                    throw new ArgumentException("All individuals of the front must have the same number of fitness values.", nameof(front));
        }

        // Each objective normalized on the front to [0; 1] by (f - min) / (max - min); an objective with max = min gets 0.5.
        public static double[][] Normalize<TGene>(IReadOnlyList<Individual<TGene>> front, int objectiveCount)
        {
            double[][] normalizedObjectivesOfIndividualsInFront = new double[front.Count][];
            for (int i = 0; i < front.Count; i++)
                normalizedObjectivesOfIndividualsInFront[i] = new double[objectiveCount];

            for (int objectiveIndx = 0; objectiveIndx < objectiveCount; objectiveIndx++)
            {
                double minObjective = double.PositiveInfinity;
                double maxObjective = double.NegativeInfinity;
                foreach (var individual in front)
                {
                    minObjective = Math.Min(minObjective, individual.FitnessValues[objectiveIndx]);
                    maxObjective = Math.Max(maxObjective, individual.FitnessValues[objectiveIndx]);
                }

                for (int i = 0; i < front.Count; i++)
                    normalizedObjectivesOfIndividualsInFront[i][objectiveIndx] = 
                        maxObjective == minObjective 
                        ? 0.5 
                        : (front[i].FitnessValues[objectiveIndx] - minObjective) / (maxObjective - minObjective);
            }

            return normalizedObjectivesOfIndividualsInFront;
        }
    }
}
