using System;
using System.Collections.Generic;
using Simulation.Geometry;


namespace Simulation.InputData.Game.Towers
{
    [Serializable]
    public class TowerSpec
    {
        public string Type;
        public int Price;
        public float ShootRange;
        public float TimeBetweenShoots;
        public Vec3 BuildPositionOffset;
        public float FirePointHeight;
        public bool AimAtGround;
        public List<string> TargetableUnitTypes = new List<string>();
        public AttackKind AttackKind;
        public int ProjectileCount;
        public float ProjectileSpeed;
        public float ProjectileDamage;
        public float ExplosionRadius;
        public float SwingTime;
        public float BeamDamagePerSecond;
        public float SlowPercent;

        public bool CanTarget(string type) => TargetableUnitTypes.Contains(type);

        public float DamagePerSecond
        {
            get
            {
                switch (AttackKind)
                {
                    case AttackKind.Beam:
                        return BeamDamagePerSecond;
                    case AttackKind.Arc:
                        return ProjectileDamage / TimeBetweenShoots;
                    default:
                        return ProjectileDamage * ProjectileCount / TimeBetweenShoots;
                }
            }
        }
    }
}
