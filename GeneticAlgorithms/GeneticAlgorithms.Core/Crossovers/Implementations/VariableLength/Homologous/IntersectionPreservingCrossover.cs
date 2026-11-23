using System;
using System.Collections.Generic;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.VariableLength.Homologous
{
    // Intersection-preserving crossover: the parents are treated as multisets of genes. Genes both parents have
    // (their multiset intersection, counted with repetitions) always pass to the child, since both parents agree on them.
    // Every other gene, one that only one of the parents has, passes with probability InclusionProbability.
    // The child keeps the order of the first parent for its genes, and the genes only the second parent has are appended
    // in the order of the second parent.
    //
    // Complexity: time O(n + m)
    //             memory O(n + m)
    // n - the number of genes of the first parent;
    // m - the number of genes of the second parent.
    public sealed class IntersectionPreservingCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        public bool RequiresParentsOfEqualLength => false;

        // Probability that a gene only one of the parents has passes to the child; in [0; 1].
        public double InclusionProbability { get; }


        public IntersectionPreservingCrossover(IConstraint<TGene> constraint, double inclusionProbability = 0.5)
        {
            if (!(inclusionProbability >= 0 && inclusionProbability <= 1))
                throw new ArgumentOutOfRangeException(nameof(inclusionProbability), inclusionProbability, "Inclusion probability must be between 0 and 1.");

            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            InclusionProbability = inclusionProbability;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);

            var unmatchedSecondParentGeneCounts = new Dictionary<TGene, int>();
            foreach (var gene in secondParent.Genes)
            {
                unmatchedSecondParentGeneCounts.TryGetValue(gene, out int count);
                unmatchedSecondParentGeneCounts[gene] = count + 1;
            }

            var child = new Individual<TGene>(_constraint.CreateTracker());
            foreach (var gene in firstParent.Genes)
                if (TryMatchWithUnmatchedGene(unmatchedSecondParentGeneCounts, gene) || random.NextDouble() < InclusionProbability)
                    child.Append(gene);

            foreach (var gene in secondParent.Genes)
                if (TryMatchWithUnmatchedGene(unmatchedSecondParentGeneCounts, gene) && random.NextDouble() < InclusionProbability)
                    child.Append(gene);

            return child;
        }

        private static bool TryMatchWithUnmatchedGene(Dictionary<TGene, int> unmatchedGeneCounts, TGene gene)
        {
            if (!unmatchedGeneCounts.TryGetValue(gene, out int count) || count == 0)
                return false;
            unmatchedGeneCounts[gene] = count - 1;
            return true;
        }
    }
}
