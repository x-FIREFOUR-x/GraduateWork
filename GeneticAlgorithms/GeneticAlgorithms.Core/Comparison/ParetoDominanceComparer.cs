using System;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Comparison
{
    // Pareto dominance comparer for several objectives, all maximized; the objectives are all fitness values of the individual.
    // x dominates y when x is not worse than y in any objective and better in at least one: Compare returns 1,
    // -1 when y dominates x, and 0 when neither dominates the other or both have the same values.
    // Dominance is a partial order, so the comparer must not be used for sorting: individuals that do not dominate each other
    // compare as equal although they are not interchangeable. It serves pairwise checks such as IsBetter and the sorting into fronts of NSGA-II.
    // With ConstrainedFitnessComparer around it, new ConstrainedFitnessComparer<TGene>(new ParetoDominanceComparer<TGene>()),
    // it gives the constrained domination of NSGA-II (Deb et al., 2002).
    //
    // Example: (2, 3) dominates (1, 3); (2, 1) and (1, 2) do not dominate each other.
    //
    // Complexity: time O(k) per comparison
    //             memory O(1)
    // k - the number of objectives.
    public sealed class ParetoDominanceComparer<TGene> : IIndividualComparer<TGene>
    {
        public bool PrefersFeasibleIndividuals => false;

        public bool IsTotalOrder => false;


        public int Compare(Individual<TGene> x, Individual<TGene> y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));

            var xValues = x.FitnessValues;
            var yValues = y.FitnessValues;
            if (xValues.Count != yValues.Count)
                throw new ArgumentException($"Individuals have different numbers of fitness values (different count objectives): {xValues.Count} and {yValues.Count}.", nameof(y));

            bool xIsBetterSomewhere = false;
            bool yIsBetterSomewhere = false;
            for (int i = 0; i < xValues.Count; i++)
            {
                if (xValues[i] > yValues[i])
                    xIsBetterSomewhere = true;
                else if (yValues[i] > xValues[i])
                    yIsBetterSomewhere = true;
                if (xIsBetterSomewhere && yIsBetterSomewhere)
                    return 0;
            }

            if (xIsBetterSomewhere)
                return 1;
            return yIsBetterSomewhere ? -1 : 0;
        }
    }
}
