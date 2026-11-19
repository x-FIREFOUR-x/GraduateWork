using System;
using System.Collections.Generic;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.VariableLength.Constrained
{
    // Tournament merge crossover: the genes of both parents are merged into one pool. While the pool has genes that still satisfy
    // the constraints, a tournament of TournamentSize genes drawn at random from them is held, and the winner, the gene with
    // the highest fitness per cost, is moved from the pool to the end of the child. So the child is built from the genes
    // of the parents, preferring the most cost-effective ones, and never exceeds upper bounds of the constraints.
    // TournamentSize sets how greedy the choice is: 1 gives a random gene, a large size almost always the best one.
    //
    // Complexity: time O((n + m) * (n + m + t))
    //             memory O(n + m)
    // n - the number of genes of the first parent;
    // m - the number of genes of the second parent;
    // t - the tournament size.
    public sealed class TournamentMergeCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;
        private readonly FitnessPerCostCalculator<TGene> _fitnessPerCostCalculator;
        private readonly List<int> _indecesGenesCandidates = new List<int>();

        // How many fitting genes of the pool are compared to choose the next gene of the child; at least 1.
        public int TournamentSize { get; }


        public TournamentMergeCrossover(IConstraint<TGene> constraint, FitnessPerCostCalculator<TGene> fitnessPerCostCalculator, int tournamentSize = 2)
        {
            if (tournamentSize < 1)
                throw new ArgumentOutOfRangeException(nameof(tournamentSize), tournamentSize, "Tournament size must be at least 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _fitnessPerCostCalculator = fitnessPerCostCalculator ?? throw new ArgumentNullException(nameof(fitnessPerCostCalculator));
            TournamentSize = tournamentSize;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);

            var allGenes = new List<TGene>(firstParent.Count + secondParent.Count);
            allGenes.AddRange(firstParent.Genes);
            allGenes.AddRange(secondParent.Genes);

            var child = new Individual<TGene>(_constraint.CreateTracker());
            while (true)
            {
                _indecesGenesCandidates.Clear();
                for (int i = 0; i < allGenes.Count; i++)
                    if (child.CanAppend(allGenes[i]))
                        _indecesGenesCandidates.Add(i);

                if (_indecesGenesCandidates.Count == 0)
                    return child;

                int chosenCandidateIndx = _indecesGenesCandidates[random.Next(_indecesGenesCandidates.Count)];
                for (int k = 1; k < TournamentSize; k++)
                {
                    int candidateIndx = _indecesGenesCandidates[random.Next(_indecesGenesCandidates.Count)];
                    if (_fitnessPerCostCalculator.Calculate(allGenes[candidateIndx]) > _fitnessPerCostCalculator.Calculate(allGenes[chosenCandidateIndx]))
                        chosenCandidateIndx = candidateIndx;
                }

                child.Append(allGenes[chosenCandidateIndx]);
                allGenes[chosenCandidateIndx] = allGenes[allGenes.Count - 1];
                allGenes.RemoveAt(allGenes.Count - 1);
            }
        }
    }
}
