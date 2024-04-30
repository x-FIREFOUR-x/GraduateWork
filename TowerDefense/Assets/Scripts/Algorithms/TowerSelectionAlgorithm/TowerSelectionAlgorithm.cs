using System;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Towers;
using TowerDefense.Collections;
using TowerDefense.Algorithms.GeneticAlgorithm;
using TowerDefense.Algorithms.GeneticAlgorithm.Persons;
using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;

namespace TowerDefense.Algorithms.TowerSelectionAlgorithm
{
    public class TowerSelectionAlgorithm
    {
        private Dictionary<TowerType, float> _sharesTowersTypes;
        private PriorityQueueWithDictOfPriorities<Vector2Int, TowerType, int> _priorityQueueOfTiles;
        private Dictionary<TowerType, List<Vector2Int>> _existTowers;

        private TowersHeuristicsCalculator _heuristicsCalculator;
        private GeneticAlgorithm<Person<TowerType>, TowerType> _algoGenerateTower;


        public TowerSelectionAlgorithm(PriorityQueueWithDictOfPriorities<Vector2Int, TowerType, int> priorityQueueOfTiles, Dictionary<TowerType, float> sharesTowersTypes)
        {
            _sharesTowersTypes = sharesTowersTypes;

            _priorityQueueOfTiles = priorityQueueOfTiles;

            _existTowers = new Dictionary<TowerType, List<Vector2Int>>();
            foreach (TowerType towerType in _sharesTowersTypes.Keys)
            {
                _existTowers[towerType] = new List<Vector2Int>();
            }

            _heuristicsCalculator = new TowersHeuristicsCalculator();
            _algoGenerateTower = new GeneticAlgorithm<Person<TowerType>, TowerType>(new PersonFactory<TowerType>(), _heuristicsCalculator, 20, 1000, 100, 1);
        }

        public List<Tuple<TowerType, Vector2Int>> GenerateTowerList(List<TowerType> towersTypes, Dictionary<TowerType, int> towersPrices, int totalPrice)
        {
            _heuristicsCalculator.UpdateData(_sharesTowersTypes, _existTowers);
            List<TowerType> generatedTowersList = _algoGenerateTower.SearchKnapsac(towersTypes, towersPrices, totalPrice);

            List<Tuple<TowerType, Vector2Int>> towersWithCoordinate = new();
            foreach (TowerType towerType in generatedTowersList)
            {
                towersWithCoordinate.Add(new Tuple<TowerType, Vector2Int>(towerType, _priorityQueueOfTiles.Peek(towerType)));
            }

            foreach (Tuple<TowerType, Vector2Int> towerWithCoordinate in towersWithCoordinate)
            {
                _existTowers[towerWithCoordinate.Item1].Add(towerWithCoordinate.Item2);
            }

            return towersWithCoordinate;
        }
    }
}
