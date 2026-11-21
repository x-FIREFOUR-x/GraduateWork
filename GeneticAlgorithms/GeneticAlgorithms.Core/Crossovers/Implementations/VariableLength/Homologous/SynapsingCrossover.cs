using System;
using System.Collections.Generic;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.VariableLength.Homologous
{
    // Synapsing crossover (Hutt, Warwick, 2007), a homologous crossover: the parents are aligned by their longest common
    // subsequence, and each pair of matched genes is a synapse, a place where the parents are similar. The cut points are
    // chosen only among the synapses, so both parents are cut at corresponding places and the child takes the segments
    // alternately from the first and the second parent. Similar parts of the parents meet at the cuts, so the crossover
    // breaks less of what both parents have in common. If the parents have fewer synapses than CutPointCount, all of them are used;
    // if they have none, the cut points are drawn independently as in cut-and-splice crossover.
    //
    // Complexity: time O(n * m)
    //             memory O(n * m)
    // n - the number of genes of the first parent;
    // m - the number of genes of the second parent.
    public sealed class SynapsingCrossover<TGene> : ICrossover<TGene>
    {
        private static readonly EqualityComparer<TGene> GeneComparer = EqualityComparer<TGene>.Default;

        private readonly IConstraint<TGene> _constraint;

        public bool RequiresParentsOfEqualLength => false;

        // How many synapses are used as cut points; at least 1.
        public int CutPointCount { get; }


        public SynapsingCrossover(IConstraint<TGene> constraint, int cutPointCount = 1)
        {
            if (cutPointCount < 1)
                throw new ArgumentOutOfRangeException(nameof(cutPointCount), cutPointCount, "Cut point count must be at least 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            CutPointCount = cutPointCount;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);

            var synapses = FindSynapses(firstParent, secondParent);
            int[] firstCutPoints;
            int[] secondCutPoints;
            if (synapses.Count == 0)
            {
                firstCutPoints = CrossoverUtilities.CreateRandomCutPoints(firstParent.Count, CutPointCount, random);
                secondCutPoints = CrossoverUtilities.CreateRandomCutPoints(secondParent.Count, CutPointCount, random);
            }
            else
            {
                var chosenSynapseIndices = CrossoverUtilities.CreateRandomSortedIndices(synapses.Count, CutPointCount, random);
                firstCutPoints = new int[chosenSynapseIndices.Length];
                secondCutPoints = new int[chosenSynapseIndices.Length];
                for (int i = 0; i < chosenSynapseIndices.Length; i++)
                    (firstCutPoints[i], secondCutPoints[i]) = synapses[chosenSynapseIndices[i]];
            }

            var child = new Individual<TGene>(_constraint.CreateTracker());
            CrossoverUtilities.AppendSegmentsAlternatingParents(child, firstParent, secondParent, firstCutPoints, secondCutPoints);
            return child;
        }

        // Positions (in the first parent, in the second parent) of the genes of the longest common subsequence, in increasing order.
        private static List<(int FirstParentPosition, int SecondParentPosition)> FindSynapses(Individual<TGene> firstParent, Individual<TGene> secondParent)
        {
            int firstParentSize = firstParent.Count;
            int secondParentSize = secondParent.Count;
            var commonSuffixLengths = new int[firstParentSize + 1, secondParentSize + 1];
            for (int i = firstParentSize - 1; i >= 0; i--)
                for (int j = secondParentSize - 1; j >= 0; j--)
                    commonSuffixLengths[i, j] = GeneComparer.Equals(firstParent.Genes[i], secondParent.Genes[j])
                        ? commonSuffixLengths[i + 1, j + 1] + 1
                        : Math.Max(commonSuffixLengths[i + 1, j], commonSuffixLengths[i, j + 1]);

            var synapses = new List<(int FirstParentPosition, int SecondParentPosition)>(commonSuffixLengths[0, 0]);
            int firstPosition = 0;
            int secondPosition = 0;
            while (firstPosition < firstParentSize && secondPosition < secondParentSize)
            {
                if (GeneComparer.Equals(firstParent.Genes[firstPosition], secondParent.Genes[secondPosition]))
                {
                    synapses.Add((firstPosition, secondPosition));
                    firstPosition++;
                    secondPosition++;
                }
                else if (commonSuffixLengths[firstPosition + 1, secondPosition] >= commonSuffixLengths[firstPosition, secondPosition + 1])
                {
                    firstPosition++;
                }
                else
                {
                    secondPosition++;
                }
            }
            return synapses;
        }
    }
}
