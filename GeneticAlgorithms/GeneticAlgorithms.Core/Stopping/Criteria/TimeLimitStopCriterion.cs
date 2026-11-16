using System;

namespace GeneticAlgorithms.Stopping.Criteria
{
    public sealed class TimeLimitStopCriterion : IStopCriterion
    {
        public TimeSpan TimeLimit { get; }


        public TimeLimitStopCriterion(TimeSpan timeLimit)
        {
            if (timeLimit <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeLimit), timeLimit, "Time limit must be greater than 0.");
            TimeLimit = timeLimit;
        }

        public bool ShouldStop(SearchProgress progress) => progress.ElapsedTime >= TimeLimit;
    }
}
