using System;


namespace Simulation.Geometry
{
    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        public int Row;
        public int Column;

        public Cell(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public static bool operator ==(Cell a, Cell b) => a.Equals(b);

        public static bool operator !=(Cell a, Cell b) => !a.Equals(b);

        public bool Equals(Cell other) => Row == other.Row && Column == other.Column;

        public override bool Equals(object obj) => obj is Cell other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Row, Column);

        public override string ToString() => $"({Row}, {Column})";
    }
}
