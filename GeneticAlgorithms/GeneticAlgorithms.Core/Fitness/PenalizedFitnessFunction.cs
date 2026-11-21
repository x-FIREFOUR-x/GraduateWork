using System;
using System.Collections.Generic;
using GeneticAlgorithms.Constraints;


namespace GeneticAlgorithms.Fitness
{
    // Penalized fitness function, a static penalty: handles the constraint in the fitness instead of a repair or a comparison.
    // Every fitness value of the nested function is lowered by PenaltyCoefficient times the total violation of the constraint,
    // the sum of UpperBoundViolation and LowerBoundViolation, so infeasible individuals stay in the population
    // but lose to feasible ones, the more the further they are from the feasible region.
    // The violations are relative to their bounds, so the coefficient is the fitness lost for a violation as large as the bound itself,
    // for example for a cost twice the budget. Too small a coefficient lets infeasible individuals win,
    // too large a one keeps the search away from the border of the feasible region, where good solutions often lie.
    // Works with any parent selection and comparer, including selections that use fitness values directly, such as roulette wheel.
    // Higher fitness values are better; with values that can be negative a feasible individual is still better
    // than an infeasible one with the same nested fitness. EvaluateGene is not penalized, as a single gene says nothing about the constraint.
    //
    // Example: PenaltyCoefficient 100, budget 10, gene cost = gene, nested fitness = sum of genes,
    // genes [6, 5] with cost 11 and violation 0.1 -> 11 - 100 * 0.1 = 1; genes [6, 4] -> 10.
    //
    // Complexity: time O(n * c) plus the time of the nested function
    //             memory O(1) plus the memory of the constraint tracker
    // n - the number of genes;
    // c - the time of one update of the constraint tracker.
    public sealed class PenalizedFitnessFunction<TGene> : IFitnessFunction<TGene>
    {
        private readonly IFitnessFunction<TGene> _fitnessFunction;
        private readonly IConstraint<TGene> _constraint;

        // Fitness lost per unit of the total violation of the constraint; non-negative and finite.
        public double PenaltyCoefficient { get; }

        public int FitnessValueCount => _fitnessFunction.FitnessValueCount;


        public PenalizedFitnessFunction(IFitnessFunction<TGene> fitnessFunction, IConstraint<TGene> constraint, double penaltyCoefficient)
        {
            _fitnessFunction = fitnessFunction ?? throw new ArgumentNullException(nameof(fitnessFunction));
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));

            if (!(penaltyCoefficient >= 0) || double.IsInfinity(penaltyCoefficient))
                throw new ArgumentOutOfRangeException(nameof(penaltyCoefficient), penaltyCoefficient, "Penalty coefficient must be a non-negative finite number.");
            PenaltyCoefficient = penaltyCoefficient;
        }

        public double[] Evaluate(IReadOnlyList<TGene> genes)
        {
            var fitnessValues = _fitnessFunction.Evaluate(genes);

            var tracker = _constraint.CreateTracker();
            foreach (var gene in genes)
                tracker.Append(gene);

            double penalty = PenaltyCoefficient * ConstraintUtilities.TotalBoundViolation(tracker);
            if (penalty == 0)
                return fitnessValues;

            var penalizedValues = new double[fitnessValues.Length];
            for (int i = 0; i < fitnessValues.Length; i++)
                penalizedValues[i] = fitnessValues[i] - penalty;
            return penalizedValues;
        }

        public double EvaluateGene(TGene gene) => _fitnessFunction.EvaluateGene(gene);
    }
}
