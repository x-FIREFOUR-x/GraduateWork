using System;
using System.Collections.Generic;
using System.Text;
using GeneticAlgorithms.Constraints;

namespace GeneticAlgorithms.Individuals
{
    public sealed class Individual<TGene>
    {
        private static readonly EqualityComparer<TGene> GeneComparer = EqualityComparer<TGene>.Default;

        private readonly List<TGene> _genes;
        private double[] _fitnessValues;
        private int? _hash;

        public Individual(IConstraintTracker<TGene> constraintTracker)
            : this(new List<TGene>(), constraintTracker, null)
        {
        }

        private Individual(List<TGene> genes, IConstraintTracker<TGene> constraintTracker, double[] fitnessValues)
        {
            _genes = genes;
            ConstraintTracker = constraintTracker ?? throw new ArgumentNullException(nameof(constraintTracker));
            _fitnessValues = fitnessValues;
        }

        public static Individual<TGene> FromGenes(IConstraint<TGene> constraint, IEnumerable<TGene> genes)
        {
            var individual = new Individual<TGene>(constraint.CreateTracker());
            if (genes != null)
                foreach (var gene in genes)
                    individual.Append(gene);
            return individual;
        }

        public IReadOnlyList<TGene> Genes => _genes;

        public int Count => _genes.Count;

        public IConstraintTracker<TGene> ConstraintTracker { get; }

        public bool IsEvaluated => _fitnessValues != null;

        public IReadOnlyList<double> FitnessValues => _fitnessValues ?? throw new InvalidOperationException("Individual is not evaluated.");

        public double Fitness => FitnessValues[0];

        public void SetFitnessValues(double[] fitnessValues)
        {
            _fitnessValues = fitnessValues ?? throw new ArgumentNullException(nameof(fitnessValues));
        }

        public bool CanAppend(TGene gene) => ConstraintTracker.CanAppend(gene);

        public void Append(TGene gene)
        {
            ConstraintTracker.Append(gene);
            _genes.Add(gene);
            InvalidateFitnessValues();
        }

        public bool TryAppend(TGene gene)
        {
            if (!ConstraintTracker.CanAppend(gene))
                return false;
            Append(gene);
            return true;
        }

        public void Insert(int index, TGene gene)
        {
            ConstraintTracker.Append(gene);
            _genes.Insert(index, gene);
            InvalidateFitnessValues();
        }

        public bool TryInsert(int index, TGene gene)
        {
            if (!ConstraintTracker.CanAppend(gene))
                return false;
            Insert(index, gene);
            return true;
        }

        public void RemoveAt(int index)
        {
            ConstraintTracker.Remove(_genes[index]);
            _genes.RemoveAt(index);
            InvalidateFitnessValues();
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = index; i < index + count; i++)
                ConstraintTracker.Remove(_genes[i]);
            _genes.RemoveRange(index, count);
            InvalidateFitnessValues();
        }

        public void Replace(int index, TGene newGene)
        {
            var currentGene = _genes[index];
            ConstraintTracker.Remove(currentGene);

            ConstraintTracker.Append(newGene);
            _genes[index] = newGene;
            InvalidateFitnessValues();
        }

        public void Move(int geneIndex, int newGeneIndex) => MoveRange(geneIndex, 1, newGeneIndex);

        public void MoveRange(int startIndex, int movedGeneCount, int newStartIndex)
        {
            if (startIndex < 0 || startIndex > _genes.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Start index can't smaller 0 and more genes count in Individual");
            if (movedGeneCount < 0)
                throw new ArgumentOutOfRangeException(nameof(movedGeneCount), movedGeneCount, "Count of the moved genes must be non-negative.");
            if (movedGeneCount > _genes.Count - startIndex)
                throw new ArgumentOutOfRangeException(nameof(movedGeneCount), movedGeneCount, $"Moved genes must end within the genes: start index {startIndex} + count {movedGeneCount} exceeds the genes count {_genes.Count}.");
            if (newStartIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(newStartIndex), newStartIndex, "New start index of the moved genes must be non-negative.");
            if (newStartIndex > _genes.Count - movedGeneCount)
                throw new ArgumentOutOfRangeException(nameof(newStartIndex), newStartIndex, $"New start index of the moved genes must not exceed {_genes.Count - movedGeneCount}, the genes count {_genes.Count} minus the count of the moved genes {movedGeneCount}.");
            if (movedGeneCount == 0 || newStartIndex == startIndex)
                return;

            var movedGenes = _genes.GetRange(startIndex, movedGeneCount);
            _genes.RemoveRange(startIndex, movedGeneCount);
            _genes.InsertRange(newStartIndex, movedGenes);
            InvalidateFitnessValues();
        }

        public void Swap(int first, int second)
        {
            if (first == second)
                return;
            (_genes[first], _genes[second]) = (_genes[second], _genes[first]);
            InvalidateFitnessValues();
        }

        public double TotalCost(ICostModel<TGene> costModel)
        {
            double total = 0;
            foreach (var gene in _genes)
                total += costModel.Cost(gene);
            return total;
        }

        public Individual<TGene> Clone()
        {
            return new Individual<TGene>(new List<TGene>(_genes), ConstraintTracker.Clone(), _fitnessValues);
        }

        public bool HasSameGenes(Individual<TGene> other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other == null || other._genes.Count != _genes.Count || other.GetGenesHash() != GetGenesHash())
                return false;
            for (int i = 0; i < _genes.Count; i++)
                if (!GeneComparer.Equals(_genes[i], other._genes[i]))
                    return false;
            return true;
        }

        public int GetGenesHash()
        {
            if (_hash.HasValue)
                return _hash.Value;

            unchecked
            {
                int hash = 17;
                foreach (var gene in _genes)
                    hash = hash * 31 + (gene == null ? 0 : GeneComparer.GetHashCode(gene));
                _hash = hash;
                return hash;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('[').Append(string.Join(", ", _genes)).Append(']');
            if (_fitnessValues != null)
                builder.Append(" -> (").Append(string.Join("; ", _fitnessValues)).Append(')');
            return builder.ToString();
        }

        private void InvalidateFitnessValues()
        {
            _fitnessValues = null;
            _hash = null;
        }
    }
}
