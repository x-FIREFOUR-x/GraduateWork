using System;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;

using TowerDefense.Opponents.Defender.Evaluation;
using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Strategies
{
    public sealed class GreedyDefence : IDefenceDecisionStrategy
    {
        private readonly double _balanceWeight;
        private readonly double _fireDensityWeight;

        public DefenceDecisionReport LastReport { get; private set; }


        public GreedyDefence(double balanceWeight, double fireDensityWeight)
        {
            if (!(balanceWeight >= 0 && balanceWeight <= 1))
                throw new ArgumentOutOfRangeException(nameof(balanceWeight), balanceWeight, "Balance weight must be in [0; 1].");
            if (!(fireDensityWeight >= 0))
                throw new ArgumentOutOfRangeException(nameof(fireDensityWeight), fireDensityWeight, "Fire density weight must be non-negative.");
            _balanceWeight = balanceWeight;
            _fireDensityWeight = fireDensityWeight;
        }

        public List<DefenderGene> Decide(DefenderSnapshot snapshot)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<DefenderGene> towers = Build(snapshot, _balanceWeight, _fireDensityWeight);
            var fitness = new DefenceHeuristicFitness(snapshot, _balanceWeight, _fireDensityWeight);
            LastReport = new DefenceDecisionReport("Greedy", snapshot.Budget, snapshot.Cost(towers), towers.ToArray(), fitness.Evaluate(towers), null, 0, stopwatch.Elapsed);
            return towers;
        }

        public void OnWaveFinished(int castleDamage)
        {
        }

        public static List<DefenderGene> Build(DefenderSnapshot snapshot, double balanceWeight, double fireDensityWeight)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            var fitness = new DefenceHeuristicFitness(snapshot, balanceWeight, fireDensityWeight);
            var towers = new List<DefenderGene>();
            var occupiedCells = new HashSet<Vector2Int>();
            int remainingBudget = snapshot.Budget;
            double currentValue = fitness.Value(towers);

            while (true)
            {
                bool found = false;
                DefenderGene bestTower = default;
                double bestGainPerCost = 0;
                double bestValue = currentValue;

                foreach (DefenderGene candidate in snapshot.CandidateGenes)
                {
                    int price = snapshot.TowerPrices[candidate.Type];
                    if (price > remainingBudget || occupiedCells.Contains(candidate.Cell))
                        continue;

                    towers.Add(candidate);
                    double value = fitness.Value(towers);
                    towers.RemoveAt(towers.Count - 1);

                    double gainPerCost = (value - currentValue) / price;
                    if (gainPerCost > bestGainPerCost)
                    {
                        found = true;
                        bestTower = candidate;
                        bestGainPerCost = gainPerCost;
                        bestValue = value;
                    }
                }

                if (!found)
                    return towers;

                towers.Add(bestTower);
                occupiedCells.Add(bestTower.Cell);
                remainingBudget -= snapshot.TowerPrices[bestTower.Type];
                currentValue = bestValue;
            }
        }
    }
}
