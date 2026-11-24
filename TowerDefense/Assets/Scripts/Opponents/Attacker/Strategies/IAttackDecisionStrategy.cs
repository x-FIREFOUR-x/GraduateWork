using System.Collections.Generic;

using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.State;


namespace TowerDefense.Opponents.Attacker.Strategies
{
    public interface IAttackDecisionStrategy
    {
        AttackDecisionReport LastReport { get; }

        List<UnitType> Decide(AttackerSnapshot snapshot);

        void OnWaveFinished(int castleDamage);
    }
}
