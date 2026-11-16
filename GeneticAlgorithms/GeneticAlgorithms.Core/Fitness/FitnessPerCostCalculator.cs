using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Constraints;

namespace GeneticAlgorithms.Fitness
{
    public sealed class FitnessPerCostCalculator<TGene>
    {
        private readonly IFitnessFunction<TGene> _fitnessFunction;
        private readonly ICostModel<TGene> _costModel;
        private readonly Dictionary<TGene, double> _fitnessPerCostByGene;

        public FitnessPerCostCalculator(IFitnessFunction<TGene> fitnessFunction, ICostModel<TGene> costModel)
        {
            _fitnessFunction = fitnessFunction;
            _costModel = costModel;
            _fitnessPerCostByGene = new Dictionary<TGene, double>();
        }

        public double Calculate(TGene gene)
        {
            if (_fitnessPerCostByGene.TryGetValue(gene, out double fitnessPerCost))
                return fitnessPerCost;

            double fitness = _fitnessFunction.EvaluateGene(gene);
            double cost = _costModel.Cost(gene);
            fitnessPerCost = fitness / cost;

            _fitnessPerCostByGene[gene] = fitnessPerCost;
            return fitnessPerCost;
        }

        public List<TGene> SortDescending(IEnumerable<TGene> genes)
        {
            return genes.OrderByDescending(Calculate).ToList();
        }
    }
}
