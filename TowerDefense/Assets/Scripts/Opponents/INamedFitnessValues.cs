using System.Collections.Generic;


namespace TowerDefense.Opponents
{
    public interface INamedFitnessValues
    {
        IReadOnlyList<string> FitnessValueNames { get; }
    }
}
