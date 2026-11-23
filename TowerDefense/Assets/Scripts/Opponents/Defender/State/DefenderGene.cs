using System;

using UnityEngine;

using TowerDefense.Main.Towers;


namespace TowerDefense.Opponents.Defender.State
{
    public readonly struct DefenderGene : IEquatable<DefenderGene>
    {
        public TowerType Type { get; }

        public Vector2Int Cell { get; }


        public DefenderGene(TowerType type, Vector2Int cell)
        {
            Type = type;
            Cell = cell;
        }

        public bool Equals(DefenderGene other) => Type == other.Type && Cell == other.Cell;

        public override bool Equals(object obj) => obj is DefenderGene other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Type, Cell);

        public override string ToString() => $"{Type}({Cell.x},{Cell.y})";
    }
}
