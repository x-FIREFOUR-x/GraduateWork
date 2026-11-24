using System;
using System.Collections.Generic;
using Simulation.Combat.Towers;
using Simulation.Combat.Units;
using Simulation.Geometry;
using Simulation.InputData.Game.Towers;


namespace Simulation.Combat.Engine
{
    internal sealed partial class Battle
    {
        private readonly List<SimTower> towers = new List<SimTower>();
        private readonly List<HomingProjectile> projectiles = new List<HomingProjectile>();
        private readonly List<ArcShot> arcShots = new List<ArcShot>();
        private float targetingTimer;

        partial void PlaceTowers(IReadOnlyList<PlacedTower> placedTowers)
        {
            var placedCells = new HashSet<Cell>();
            foreach (PlacedTower placed in placedTowers)
            {
                if (!placedCells.Add(placed.Cell))
                    throw new ArgumentException($"Cell {placed.Cell} already has a tower.", nameof(placedTowers));

                TowerSpec spec = simulator.TowerSpec(placed.Type);
                towers.Add(new SimTower(spec, simulator.TargetableUnits(spec), simulator.TowerPosition(placed, spec)));
            }
        }

        partial void ActTowers()
        {
            if (targetingTimer <= 0f)
            {
                UpdateTargets();
                targetingTimer += simulator.Settings.TowerRetargetIntervalSeconds;
            }
            targetingTimer -= simulator.Settings.FixedTimeStepSeconds;

            foreach (SimTower tower in towers)
            {
                if (tower.Target != null && !tower.Target.CanBeTargeted)
                    tower.Target = null;

                if (tower.Spec.AttackKind == AttackKind.Beam)
                    BeamAt(tower);
                else
                    ShootAt(tower);
            }

            MoveProjectiles();
            ResolveArcShots();
        }

        private static bool IsTargetable(SimTower tower, SimUnit unit) => unit.CanBeTargeted && tower.CanTarget(unit);

        private void UpdateTargets()
        {
            for (int t = 0; t < towers.Count; t++)
            {
                SimTower tower = towers[t];
                bool[] targetableUnits = tower.TargetableUnits;
                float towerX = tower.Position.X;
                float towerY = tower.Position.Y;
                float towerZ = tower.Position.Z;
                SimUnit nearest = null;
                float nearestSquaredDistance = float.PositiveInfinity;
                for (int u = 0; u < units.Count; u++)
                {
                    SimUnit unit = units[u];
                    if (!unit.IsActive || unit.RecoveryTimer > 0f || !targetableUnits[unit.TypeIndex])
                        continue;

                    float dx = unit.Position.X - towerX;
                    float dy = unit.Position.Y - towerY;
                    float dz = unit.Position.Z - towerZ;
                    float squaredDistance = dx * dx + dy * dy + dz * dz;
                    if (squaredDistance < nearestSquaredDistance)
                    {
                        nearest = unit;
                        nearestSquaredDistance = squaredDistance;
                    }
                }

                float range = tower.Spec.ShootRange;
                tower.Target = nearest != null && nearestSquaredDistance <= range * range ? nearest : null;
            }
        }

        private void BeamAt(SimTower tower)
        {
            SimUnit target = tower.Target;
            if (target == null)
                return;

            Damage(target, tower.Spec.BeamDamagePerSecond * simulator.Settings.FixedTimeStepSeconds);
            if (!target.IsActive)
                return;

            target.SlowPercent = tower.Spec.SlowPercent;
            target.SlowTimer = target.Spec.SpeedRecoveryTime;
        }

        private void ShootAt(SimTower tower)
        {
            if (tower.Target != null && tower.TimeToNextFire <= 0f)
            {
                if (tower.Spec.AttackKind == AttackKind.Arc)
                    LaunchStone(tower);
                else
                    FireProjectiles(tower);
                tower.TimeToNextFire = tower.Spec.TimeBetweenShoots;
            }
            tower.TimeToNextFire -= simulator.Settings.FixedTimeStepSeconds;
        }

        private void FireProjectiles(SimTower tower)
        {
            for (int i = 0; i < tower.Spec.ProjectileCount; i++)
                projectiles.Add(new HomingProjectile(tower.FirePoint, tower.Target, tower.Spec));
        }

        private void LaunchStone(SimTower tower)
        {
            Vec3 aim = AimPoint(tower.Target, tower.Spec.AimAtGround);
            float flightTime = Vec3.Distance(tower.FirePoint, aim) / tower.Spec.ProjectileSpeed;
            arcShots.Add(new ArcShot(tower, aim, Math.Max(flightTime, tower.Spec.SwingTime + simulator.Settings.CatapultStoneMinFlightSeconds)));
        }

        private Vec3 AimPoint(SimUnit unit, bool atGround)
        {
            return atGround ? new Vec3(unit.Position.X, simulator.Map.GroundHeight, unit.Position.Z) : unit.Position;
        }

        private void MoveProjectiles()
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                HomingProjectile projectile = projectiles[i];
                if (!projectile.Target.IsActive)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                Vec3 direction = AimPoint(projectile.Target, projectile.Spec.AimAtGround) - projectile.Position;
                float step = projectile.Spec.ProjectileSpeed * simulator.Settings.FixedTimeStepSeconds;
                float distance = direction.Length;
                if (distance <= step)
                {
                    projectiles.RemoveAt(i);
                    Damage(projectile.Target, projectile.Spec.ProjectileDamage);
                }
                else
                {
                    projectile.Position += direction * (step / distance);
                }
            }
        }

        private void ResolveArcShots()
        {
            for (int i = arcShots.Count - 1; i >= 0; i--)
            {
                ArcShot shot = arcShots[i];
                shot.TimeLeft -= simulator.Settings.FixedTimeStepSeconds;
                if (shot.TimeLeft > 0f)
                    continue;

                arcShots.RemoveAt(i);
                int unitCount = units.Count;
                float explosionRadius = shot.Tower.Spec.ExplosionRadius;
                for (int j = 0; j < unitCount; j++)
                {
                    SimUnit unit = units[j];
                    if (IsTargetable(shot.Tower, unit) && Vec3.SquaredDistance(shot.Aim, unit.Position) <= explosionRadius * explosionRadius)
                        Damage(unit, shot.Tower.Spec.ProjectileDamage);
                }
            }
        }
    }
}
