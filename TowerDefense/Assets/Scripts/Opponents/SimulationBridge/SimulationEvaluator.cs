using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TowerDefense.Main.Units;
using Simulation;


namespace TowerDefense.Opponents.SimulationBridge
{
    public sealed class SimulationEvaluator
    {
        private const int UncappedCastleHealth = int.MaxValue;

        private static readonly Dictionary<UnitType, string> UnitNames =
            Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToDictionary(unit => unit, unit => unit.ToString());

        private readonly SimulationRequest _request;
        private readonly BattleSimulator _simulator;
        private readonly IReadOnlyList<PlacedTower> _builtTowers;
        private readonly Dictionary<string, double> _attackScoreByBattle = new Dictionary<string, double>();
        private readonly StringBuilder _keyBuilder = new StringBuilder();


        public SimulationEvaluator(SimulationRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _simulator = new BattleSimulator(SimulationInputData.Load(request.InputPath));
            _builtTowers = Canonical(request.BuiltTowers);
        }

        public double AttackScore(IReadOnlyList<UnitType> wave) => CachedAttackScore(_builtTowers, wave);

        public double AttackScore(IReadOnlyList<PlacedTower> towers, IReadOnlyList<UnitType> wave) => CachedAttackScore(Canonical(towers), wave);

        private double CachedAttackScore(IReadOnlyList<PlacedTower> towers, IReadOnlyList<UnitType> wave)
        {
            int unitCount = Math.Min(wave.Count, _request.MaxWaveUnits);
            if (unitCount == 0)
                return 0;

            string key = Key(towers, wave, unitCount);
            if (_attackScoreByBattle.TryGetValue(key, out double cachedAttackScore))
                return cachedAttackScore;

            var spawnedUnits = new string[unitCount];
            for (int i = 0; i < unitCount; i++)
                spawnedUnits[i] = UnitNames[wave[i]];

            SimulationOutputData result = _simulator.Run(towers, spawnedUnits, UncappedCastleHealth, _request.Seed);
            double attackScore = result.CastleDamage + Progress(result, unitCount);
            _attackScoreByBattle[key] = attackScore;
            return attackScore;
        }

        private string Key(IReadOnlyList<PlacedTower> towers, IReadOnlyList<UnitType> wave, int unitCount)
        {
            _keyBuilder.Clear();
            for (int i = 0; i < unitCount; i++)
                _keyBuilder.Append((int)wave[i]).Append(',');
            _keyBuilder.Append('|');
            foreach (PlacedTower tower in towers)
                _keyBuilder.Append(tower.Type).Append('@').Append(tower.Cell.Row).Append(':').Append(tower.Cell.Column).Append(';');
            return _keyBuilder.ToString();
        }

        private double Progress(SimulationOutputData result, int unitCount)
        {
            float routeLength = _simulator.RouteLength;
            if (routeLength <= 0f)
                return 1;

            double progress = unitCount - result.DistanceMoved.Count;
            foreach (float distance in result.DistanceMoved)
                progress += Math.Min(1.0, distance / routeLength);
            return progress / unitCount;
        }

        private static IReadOnlyList<PlacedTower> Canonical(IEnumerable<PlacedTower> towers)
        {
            return towers.OrderBy(tower => tower.Cell.Row)
                         .ThenBy(tower => tower.Cell.Column)
                         .ThenBy(tower => tower.Type, StringComparer.Ordinal)
                         .ToList();
        }
    }
}
