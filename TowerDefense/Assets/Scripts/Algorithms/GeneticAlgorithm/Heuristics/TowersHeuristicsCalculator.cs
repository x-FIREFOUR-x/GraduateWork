using System;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Towers;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Heuristics
{
    public class TowersHeuristicsCalculator : HeuristicsCalculator<TowerType>
    {
        private Dictionary<TowerType, float> _sharesTowersTypes;
        private Dictionary<TowerType, List<Vector2Int>> _existTowers;

        private int _countExistTowers;

        public override float GetHeuristicsValue(List<TowerType> genes)
        {
            if(genes.Count == 0)
                return -int.MaxValue;

            Dictionary<TowerType, int> countsTowersWithInputGenes = new();
            
            foreach (TowerType towerType in _existTowers.Keys)
            {
                countsTowersWithInputGenes[towerType] = _existTowers[towerType].Count;
            }
            foreach (TowerType towerType in genes)
            {
                countsTowersWithInputGenes[towerType] += 1;
            }

            int countTowersWithInputGenes = 0;
            foreach (TowerType towerType in countsTowersWithInputGenes.Keys)
            {
                countTowersWithInputGenes += countsTowersWithInputGenes[towerType];
            }

            if (countTowersWithInputGenes == 0)
                return -int.MaxValue;

            float deviationShares = 0;
            foreach (TowerType towerType in _sharesTowersTypes.Keys)
            {
                float futureSharesTowerType = countsTowersWithInputGenes[towerType] / countTowersWithInputGenes;
                deviationShares += MathF.Abs(_sharesTowersTypes[towerType] - futureSharesTowerType);
            }

                //the greater the deviation of the proportions of towers types from 0, the worse the given list of genes(worse heuristic value),
                //so we invert the value of the deviation
            return 1 - deviationShares;   
        }

        public void UpdateData(Dictionary<TowerType, float> sharesTowersTypes, Dictionary<TowerType, List<Vector2Int>> existTowers)
        {
            _sharesTowersTypes = sharesTowersTypes;
            _existTowers = existTowers;

            _countExistTowers = 0;
            foreach (TowerType towerType in _existTowers.Keys)
            {
                _countExistTowers += _existTowers[towerType].Count;
            }
        }
    }
}
