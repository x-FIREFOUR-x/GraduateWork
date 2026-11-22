using System.Collections.Generic;

namespace GeneticAlgorithms.Fitness
{
    public interface IFitnessFunction<TGene>
    {
        int FitnessValueCount { get; }

        double[] Evaluate(IReadOnlyList<TGene> genes);

        double EvaluateGene(TGene gene) => Evaluate(new[] { gene })[0];

        // Whether the fitness values already prefer feasible individuals, for example by a penalty for the violation of the constraint,
        // so that infeasible individuals lose to feasible ones without a repair; false for an ordinary fitness.
        bool PrefersFeasibleIndividuals => false;
    }
}
