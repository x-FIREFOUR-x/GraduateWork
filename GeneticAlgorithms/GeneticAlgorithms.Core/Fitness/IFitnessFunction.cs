using System.Collections.Generic;

namespace GeneticAlgorithms.Fitness
{
    public interface IFitnessFunction<TGene>
    {
        int FitnessValueCount { get; }

        double[] Evaluate(IReadOnlyList<TGene> genes);

        double EvaluateGene(TGene gene) => Evaluate(new[] { gene })[0];
    }
}
