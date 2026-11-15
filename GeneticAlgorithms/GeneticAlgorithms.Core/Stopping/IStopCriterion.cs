namespace GeneticAlgorithms.Stopping
{
    public interface IStopCriterion
    {
        bool ShouldStop(SearchProgress progress);
    }
}
