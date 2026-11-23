using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Survivor
{
    // Survivor selection: decides which individuals of the current population and the new offspring
    // form the next population. 
    public interface ISurvivorSelection<TGene>
    {
        // Decides which individual is better.
        IIndividualComparer<TGene> Comparer { get; }

        // Whether the selector needs a comparer that orders every pair of individuals, such as FitnessComparer.
        // true: it sorts individuals, so it cannot be used with Pareto dominance.
        // false: it only compares pairs or picks at random, so it runs correctly with Pareto dominance.
        // false does not mean the selection is useful for several objectives: that is shown by its folder (SingleObjective or MultiObjective).
        bool RequiresTotalOrder { get; }


        // Called once per run with the initial population.
        void Initialize(List<Individual<TGene>> population);

        // Replace changes the population in place and returns how many offspring entered it. offspring are the children together with their parents.
        int Replace(List<Individual<TGene>> population, IReadOnlyList<Offspring<TGene>> offspring, IRandom random);
    }
}
