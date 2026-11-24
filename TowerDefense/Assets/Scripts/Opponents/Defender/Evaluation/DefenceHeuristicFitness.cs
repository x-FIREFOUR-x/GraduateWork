using System;
using System.Collections.Generic;
using System.Linq;

using GeneticAlgorithms.Fitness;

using TowerDefense.Main.Towers;
using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Evaluation
{
    public sealed class DefenceHeuristicFitness : IFitnessFunction<DefenderGene>, INamedFitnessValues
    {
        private static readonly int TowerTypeCount = Enum.GetValues(typeof(TowerType)).Cast<int>().Max() + 1;
        private static readonly string[] ValueNames = { "heuristicDefence" };

        private readonly PathCoverageMap _coverageMap;
        private readonly double _balanceWeight;
        private readonly double _fireDensityWeight;
        private readonly bool[] _isCoveredByBuiltTowers;
        private readonly int[] _lastCountedEvaluationByPathCell;
        private readonly int[] _builtCountByType;
        private readonly double[] _targetShareByType;
        private int _currentEvaluation;

        public int FitnessValueCount => ValueNames.Length;

        public IReadOnlyList<string> FitnessValueNames => ValueNames;

        public bool PrefersFeasibleIndividuals => false;


        public DefenceHeuristicFitness(DefenderSnapshot snapshot, double balanceWeight, double fireDensityWeight)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));
            if (!(balanceWeight >= 0 && balanceWeight <= 1))
                throw new ArgumentOutOfRangeException(nameof(balanceWeight), balanceWeight, "Balance weight must be in [0; 1].");
            if (!(fireDensityWeight >= 0))
                throw new ArgumentOutOfRangeException(nameof(fireDensityWeight), fireDensityWeight, "Fire density weight must be non-negative.");

            _coverageMap = snapshot.CoverageMap;
            _balanceWeight = balanceWeight;
            _fireDensityWeight = fireDensityWeight;

            _isCoveredByBuiltTowers = new bool[_coverageMap.PathCellCount];
            _lastCountedEvaluationByPathCell = new int[_coverageMap.PathCellCount];
            _builtCountByType = new int[TowerTypeCount];
            foreach (DefenderGene tower in snapshot.BuiltTowers)
            {
                _builtCountByType[(int)tower.Type]++;
                foreach (int pathCell in _coverageMap.CoveredPathCells(tower))
                    _isCoveredByBuiltTowers[pathCell] = true;
            }

            _targetShareByType = new double[TowerTypeCount];
            foreach (var targetShare in snapshot.TargetShares)
                _targetShareByType[(int)targetShare.Key] = targetShare.Value;
        }

        public double[] Evaluate(IReadOnlyList<DefenderGene> genes) => new[] { Value(genes) };

        public double Value(IReadOnlyList<DefenderGene> genes) => CoverageScore(genes) * (1 + _balanceWeight * Balance(genes));

        public double CoverageScore(IReadOnlyList<DefenderGene> genes)
        {
            if (_coverageMap.PathCellCount == 0)
                return 0;
            return (CoverageGain(genes) + _fireDensityWeight * FireDensity(genes)) / _coverageMap.PathCellCount;
        }

        private int FireDensity(IReadOnlyList<DefenderGene> genes)
        {
            int fireDensity = 0;
            foreach (DefenderGene gene in genes)
                fireDensity += _coverageMap.CoveredPathCells(gene).Count;
            return fireDensity;
        }

        private int CoverageGain(IReadOnlyList<DefenderGene> genes)
        {
            _currentEvaluation++;
            int coverageGain = 0;
            foreach (DefenderGene gene in genes)
            {
                foreach (int pathCell in _coverageMap.CoveredPathCells(gene))
                {
                    if (_isCoveredByBuiltTowers[pathCell] || _lastCountedEvaluationByPathCell[pathCell] == _currentEvaluation)
                        continue;
                    _lastCountedEvaluationByPathCell[pathCell] = _currentEvaluation;
                    coverageGain++;
                }
            }
            return coverageGain;
        }

        public int[] CountByTypeAfter(IReadOnlyList<DefenderGene> genes)
        {
            int[] countByType = (int[])_builtCountByType.Clone();
            foreach (DefenderGene gene in genes)
                countByType[(int)gene.Type]++;
            return countByType;
        }

        public double Balance(IReadOnlyList<DefenderGene> genes)
        {
            int[] countByType = CountByTypeAfter(genes);
            int totalCount = countByType.Sum();
            if (totalCount == 0)
                return 0;

            double shareDeviation = 0;
            for (int type = 0; type < TowerTypeCount; type++)
                shareDeviation += Math.Abs(_targetShareByType[type] - (double)countByType[type] / totalCount);
            return 1 - shareDeviation;
        }
    }
}
