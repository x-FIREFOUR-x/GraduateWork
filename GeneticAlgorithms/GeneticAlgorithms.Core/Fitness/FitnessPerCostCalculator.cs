using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Constraints;

namespace GeneticAlgorithms.Fitness
{
    public sealed class FitnessPerCostCalculator<TGene>
    {
        private readonly IFitnessFunction<TGene> _fitnessFunction;
        private readonly IResourceConstraint<TGene> _constraint;
        private readonly Dictionary<TGene, double> _fitnessPerCostByGene;

        public FitnessPerCostCalculator(IFitnessFunction<TGene> fitnessFunction, IResourceConstraint<TGene> constraint)
        {
            _fitnessFunction = fitnessFunction;
            _constraint = constraint;
            _fitnessPerCostByGene = new Dictionary<TGene, double>();
        }

        public double Calculate(TGene gene)
        {
            if (_fitnessPerCostByGene.TryGetValue(gene, out double fitnessPerCost))
                return fitnessPerCost;

            double fitness = _fitnessFunction.EvaluateGene(gene);
            double cost = _constraint.Cost(gene);
            fitnessPerCost = cost > 0 ? fitness / cost : (fitness > 0 ? double.PositiveInfinity : fitness);

            _fitnessPerCostByGene[gene] = fitnessPerCost;
            return fitnessPerCost;
        }

        public List<TGene> SortDescending(IEnumerable<TGene> genes)
        {
            return genes.OrderByDescending(Calculate).ToList();
        }
    }
}
