using System;

namespace GeneticAlgorithms.Constraints
{
    internal static class ConstraintUtilities
    {
        // Violation of a bound relative to the size of the bound, so violations of bounds in different units can be summed.
        // boundViolation is how far the value is beyond the bound; 0 or less means the bound is not violated and gives 0.
        // The violation is divided by the absolute value of the bound, so negative bounds give positive violations too;
        // a zero bound gives the absolute violation, as there is nothing to divide by.
        // Example: cost 120 with budget 100 -> 0.2; cost 50 with budget 100 -> 0; count 1 with min count 4 -> 0.75;
        // value -4 with min -2 -> 1.
        public static double RelativeBoundViolation(double boundViolation, double bound)
        {
            if (boundViolation <= 0)
                return 0;

            double boundSize = Math.Abs(bound);
            return boundSize == 0 ? boundViolation : boundViolation / boundSize;
        }

        // Total violation of the constraint: the sum of the violations of its upper and lower bounds; 0 when it is satisfied.
        // Used to compare how far infeasible individuals are from the feasible region.
        // Throws if the tracker reports a negative or NaN violation, as it would make an infeasible individual look better
        // than a feasible one; this can only come from a custom tracker that breaks the contract of IConstraintTracker.
        public static double TotalBoundViolation<TGene>(IConstraintTracker<TGene> tracker)
        {
            double totalBoundViolation = tracker.UpperBoundViolation + tracker.LowerBoundViolation;
            if (!(totalBoundViolation >= 0))
                throw new InvalidOperationException($"Constraint tracker must report non-negative bound violations, but they sum to {totalBoundViolation}.");
            return totalBoundViolation;
        }
    }
}
