namespace GeneticAlgorithms.Randomness
{
    public interface IRandom
    {
        int Next(int maxExclusive);

        int Next(int minInclusive, int maxExclusive);

        double NextDouble();
    }
}
