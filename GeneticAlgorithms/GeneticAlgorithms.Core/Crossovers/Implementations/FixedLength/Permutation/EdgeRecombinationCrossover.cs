using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Permutation
{
    // Edge recombination crossover, ERX (Whitley, Starkweather, Fuquay, 1989): keeps which genes stand next to each other.
    // For every gene the set of its neighbours in both parents is built (the sequence is not closed into a cycle).
    // The child starts with the first gene of a random parent; each next gene is the unused neighbour of the current gene
    // that has the fewest unused neighbours itself (ties are broken at random), or a random unused gene when there is none.
    // Repeated genes are told apart by their occurrence number.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class EdgeRecombinationCrossover<TGene> : ICrossover<TGene>
    {
        // Every gene, a repeated one counted separately for each occurrence, has at most 2 neighbours in each parent
        // (left and right), so at most 4 in both parents together; a neighbour shared by both parents is stored once.
        private const int MaxNeighbourCountForGene = 4;

        private readonly IConstraint<TGene> _constraint;


        public EdgeRecombinationCrossover(IConstraint<TGene> constraint)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            int sizeChild = firstParent.Count;
            if (sizeChild == 0)
                return new Individual<TGene>(_constraint.CreateTracker());

            // Genes as numbers: the position of each gene in the first parent.
            var positionsFirstParentGenes = CrossoverUtilities.CreateSequentialIndices(sizeChild);
            var positionsSecondParentGenesInFirstParent = CrossoverUtilities.FindPositionsOfSecondParentGenesInFirstParent(firstParent, secondParent);

            // Neighbour table: geneNeighbours[gene, 0 .. neighbourCountsOfGene[gene] - 1] are the neighbours of the gene in both parents.
            var geneNeighbours = new int[sizeChild, MaxNeighbourCountForGene];
            var neighbourCountsOfGene = new int[sizeChild];
            AddNeighbours(positionsFirstParentGenes, geneNeighbours, neighbourCountsOfGene);
            AddNeighbours(positionsSecondParentGenesInFirstParent, geneNeighbours, neighbourCountsOfGene);

            // Genes not yet in the child: unusedGenes[0 .. unusedGeneCount - 1], and unusedGeneIndices[gene] is where the gene lies there.
            var unusedGenes = CrossoverUtilities.CreateSequentialIndices(sizeChild);
            var unusedGeneIndices = CrossoverUtilities.CreateSequentialIndices(sizeChild);
            int unusedGeneCount = sizeChild;

            // The child starts with the first gene of a random parent.
            var child = new int[sizeChild];
            int gene = random.Next(2) == 0 ? positionsFirstParentGenes[0] : positionsSecondParentGenesInFirstParent[0];
            for (int i = 0; i < sizeChild; i++)
            {
                child[i] = gene;

                // Remove the gene from the unused genes: the last unused gene takes its place.
                int unusedGeneIndex = unusedGeneIndices[gene];
                int lastUnusedGene = unusedGenes[--unusedGeneCount];
                unusedGenes[unusedGeneIndex] = lastUnusedGene;
                unusedGeneIndices[lastUnusedGene] = unusedGeneIndex;

                // Cross the gene out of the lists of its neighbours, so that only unused genes stay in the table.
                for (int j = 0; j < neighbourCountsOfGene[gene]; j++)
                    RemoveNeighbour(geneNeighbours[gene, j], gene, geneNeighbours, neighbourCountsOfGene);

                if (unusedGeneCount == 0)
                    break;

                // Next gene: the neighbour with the fewest neighbours left; among equal ones each is equally likely.
                int nextGene = -1;
                int tieCount = 0;
                for (int j = 0; j < neighbourCountsOfGene[gene]; j++)
                {
                    int neighbour = geneNeighbours[gene, j];
                    if (nextGene == -1 || neighbourCountsOfGene[neighbour] < neighbourCountsOfGene[nextGene])
                    {
                        nextGene = neighbour;
                        tieCount = 1;
                    }
                    else if (neighbourCountsOfGene[neighbour] == neighbourCountsOfGene[nextGene] && random.Next(++tieCount) == 0)
                    {
                        nextGene = neighbour;
                    }
                }

                // Dead end, no unused neighbours: a random unused gene.
                gene = nextGene != -1 ? nextGene : unusedGenes[random.Next(unusedGeneCount)];
            }

            // Numbers back to the genes of the first parent.
            return Individual<TGene>.FromGenes(_constraint, CrossoverUtilities.GetFirstParentGenesAtPositions(firstParent, child));
        }

        private static void AddNeighbours(int[] parent, int[,] neighbours, int[] neighbourCounts)
        {
            for (int i = 0; i < parent.Length; i++)
            {
                if (i > 0)   //Add left neigbour
                    AddNeighbour(parent[i], parent[i - 1], neighbours, neighbourCounts);
                if (i < parent.Length - 1)   //Add right neigbour
                    AddNeighbour(parent[i], parent[i + 1], neighbours, neighbourCounts);
            }
        }

        private static void AddNeighbour(int gene, int neighbour, int[,] neighbours, int[] neighbourCounts)
        {
            for (int j = 0; j < neighbourCounts[gene]; j++)
                if (neighbours[gene, j] == neighbour)
                    return;
            neighbours[gene, neighbourCounts[gene]++] = neighbour;
        }

        private static void RemoveNeighbour(int gene, int neighbour, int[,] neighbours, int[] neighbourCounts)
        {
            for (int j = 0; j < neighbourCounts[gene]; j++)
            {
                if (neighbours[gene, j] != neighbour)
                    continue;
                neighbours[gene, j] = neighbours[gene, --neighbourCounts[gene]];
                return;
            }
        }
    }
}
