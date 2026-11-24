using System;


namespace Simulation.Geometry
{
    [Serializable]
    public struct Vec3 : IEquatable<Vec3>
    {
        public static readonly Vec3 Zero = new Vec3(0f, 0f, 0f);

        public float X;
        public float Y;
        public float Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length => (float)Math.Sqrt(SquaredLength);

        public float SquaredLength => X * X + Y * Y + Z * Z;

        public Vec3 Flattened => new Vec3(X, 0f, Z);

        public Vec3 Normalized
        {
            get
            {
                float length = Length;
                return length > 1e-6f ? this / length : Zero;
            }
        }

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vec3 operator *(Vec3 vector, float factor) => new Vec3(vector.X * factor, vector.Y * factor, vector.Z * factor);

        public static Vec3 operator /(Vec3 vector, float divisor) => new Vec3(vector.X / divisor, vector.Y / divisor, vector.Z / divisor);

        public static bool operator ==(Vec3 a, Vec3 b) => a.Equals(b);

        public static bool operator !=(Vec3 a, Vec3 b) => !a.Equals(b);

        public static float Distance(Vec3 a, Vec3 b) => (a - b).Length;

        public static float SquaredDistance(Vec3 a, Vec3 b) => (a - b).SquaredLength;

        public bool Equals(Vec3 other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is Vec3 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
