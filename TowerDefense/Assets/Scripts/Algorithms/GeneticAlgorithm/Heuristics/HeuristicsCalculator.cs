using System.Collections.Generic;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Heuristics
{
    public abstract class HeuristicsCalculator<TGene>
    {
        public abstract float GetHeuristicsValue(List<TGene> genes);
        
    }
}
