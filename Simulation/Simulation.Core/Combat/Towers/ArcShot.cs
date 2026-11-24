using Simulation.Geometry;


namespace Simulation.Combat.Towers
{
    internal sealed class ArcShot
    {
        public float TimeLeft;

        public SimTower Tower { get; }

        public Vec3 Aim { get; }

        public ArcShot(SimTower tower, Vec3 aim, float timeLeft)
        {
            Tower = tower;
            Aim = aim;
            TimeLeft = timeLeft;
        }
    }
}
