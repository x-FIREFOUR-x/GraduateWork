using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Randomness;

namespace GeneticAlgorithms.Individuals
{
    public sealed class IndividualBuilder<TGene>
    {
        private readonly IConstraint<TGene> _constraint;
        private readonly List<TGene> _existingGenes;
        private readonly List<TGene> _sortedGenesByFitnessPerCost;
        private readonly List<TGene> _availableGenes = new List<TGene>();

        public IndividualBuilder(
            IConstraint<TGene> constraint,
            IReadOnlyList<TGene> availableGenes,
            FitnessPerCostCalculator<TGene> fitnessPerCostCalculator)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _existingGenes = availableGenes?.Distinct().ToList() ?? throw new ArgumentNullException(nameof(availableGenes));
            if (fitnessPerCostCalculator == null)
                throw new ArgumentNullException(nameof(fitnessPerCostCalculator));
            _sortedGenesByFitnessPerCost = fitnessPerCostCalculator.SortDescending(_existingGenes);
        }

        public IReadOnlyList<TGene> ExistingGenes => _existingGenes;

        public Individual<TGene> CreateEmpty() => new Individual<TGene>(_constraint.CreateTracker());

        public Individual<TGene> CreateFromGenes(IEnumerable<TGene> genes) => Individual<TGene>.FromGenes(_constraint, genes);

        public int FillRandom(Individual<TGene> individual, int index, IRandom random)
        {
            int inserted = 0;
            while (true)
            {
                _availableGenes.Clear();
                foreach (var gene in _existingGenes)
                    if (individual.CanAppend(gene))
                        _availableGenes.Add(gene);

                if (_availableGenes.Count == 0)
                    return inserted;

                individual.Insert(index + inserted, _availableGenes[random.Next(_availableGenes.Count)]);
                inserted++;
            }
        }

        public Individual<TGene> CreateRandom(IRandom random)
        {
            var individual = CreateEmpty();
            FillRandom(individual, 0, random);
            return individual;
        }

        public Individual<TGene> CreateGreedy(int candidatePool, IRandom random)
        {
            if (candidatePool < 1)
                throw new ArgumentOutOfRangeException(nameof(candidatePool));

            var individual = CreateEmpty();
            while (true)
            {
                _availableGenes.Clear();
                foreach (var gene in _sortedGenesByFitnessPerCost)
                {
                    if (!individual.CanAppend(gene))
                        continue;
                    _availableGenes.Add(gene);
                    if (_availableGenes.Count == candidatePool)
                        break;
                }

                if (_availableGenes.Count == 0)
                    return individual;

                individual.Append(_availableGenes[random.Next(_availableGenes.Count)]);
            }
        }
    }
}
