namespace GeneticAlgorithms.Mutations
{
    public interface IMutationFeedback
    {
        void Report(bool improved);
    }
}
