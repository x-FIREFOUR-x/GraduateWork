namespace GeneticAlgorithms.StopCriteria
{
    public interface IStopCriterion
    {
        bool ShouldStop(SearchProgress progress);
    }
}
