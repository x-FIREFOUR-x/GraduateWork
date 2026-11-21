using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Comparison
{
    // Constrained fitness comparer, the feasibility rules of Deb (2000): handles the constraint by comparison instead of a penalty or a repair.
    // 1. A feasible individual is better than an infeasible one.
    // 2. Of two infeasible individuals, the one with the smaller total violation of the constraint is better,
    //    whatever their fitness, so the search is drawn towards the feasible region.
    // 3. Of two feasible individuals, the nested comparer decides, by default the first fitness value.
    // The total violation is the sum of UpperBoundViolation and LowerBoundViolation of the constraint tracker of the individual,
    // so it is read without evaluating anything again. It needs no penalty coefficient to tune.
    // It is the constraint handling of NSGA-II (constrained domination) for a single objective.
    // When every individual is feasible, for example with a repair that always fixes the constraint, it acts as the nested comparer.
    // Parent selections that use fitness values directly, such as roulette wheel selection, do not use comparers,
    // so for them the constraint has to be handled by a penalty in the fitness function.
    //
    // Example: budget 10, A feasible with fitness 5, B with cost 12 (violation 0.2) and fitness 9, C with cost 15 (violation 0.5)
    // and fitness 20 -> A is better than B and C, and B is better than C.
    //
    // Complexity: time O(c) per comparison
    //             memory O(1)
    // c - the time to read the violations from the constraint tracker.
    public sealed class ConstrainedFitnessComparer<TGene> : IIndividualComparer<TGene>
    {
        // Comparer of feasible individuals.
        public IIndividualComparer<TGene> FeasibleComparer { get; }


        public ConstrainedFitnessComparer(IIndividualComparer<TGene> feasibleComparer = null)
        {
            FeasibleComparer = feasibleComparer ?? new FitnessComparer<TGene>();
        }

        public int Compare(Individual<TGene> x, Individual<TGene> y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));

            double xViolation = ConstraintUtilities.TotalBoundViolation(x.ConstraintTracker);
            double yViolation = ConstraintUtilities.TotalBoundViolation(y.ConstraintTracker);
            if (xViolation == 0 && yViolation == 0)
                return FeasibleComparer.Compare(x, y);

            return yViolation.CompareTo(xViolation);
        }
    }
}
