using System.Collections.Generic;

using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Strategies
{
    public interface IDefenceDecisionStrategy
    {
        DefenceDecisionReport LastReport { get; }

        List<DefenderGene> Decide(DefenderSnapshot snapshot);

        void OnWaveFinished(int castleDamage);
    }
}
