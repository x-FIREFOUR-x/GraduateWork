using Simulation.Combat.Units;
using Simulation.Geometry;
using Simulation.InputData.Game.Towers;


namespace Simulation.Combat.Towers
{
    internal sealed class HomingProjectile
    {
        public Vec3 Position;

        public SimUnit Target { get; }

        public TowerSpec Spec { get; }

        public HomingProjectile(Vec3 position, SimUnit target, TowerSpec spec)
        {
            Position = position;
            Target = target;
            Spec = spec;
        }
    }
}
