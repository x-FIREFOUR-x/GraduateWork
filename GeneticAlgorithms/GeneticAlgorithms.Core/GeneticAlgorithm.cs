using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GeneticAlgorithms.Crossovers;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Mutations;
using GeneticAlgorithms.Randomness;
using GeneticAlgorithms.Repair;
using GeneticAlgorithms.Selection.Parent;
using GeneticAlgorithms.Selection.Survivor;
using GeneticAlgorithms.Stopping;


namespace GeneticAlgorithms
{
    public sealed class GeneticAlgorithm<TGene>
    {
        private readonly GeneticAlgorithmConfig _config;
        private readonly PopulationInitializer<TGene> _populationInitializer;
        private readonly IFitnessFunction<TGene> _fitnessFunction;
        private readonly IParentSelection<TGene> _parentSelection;
        private readonly ICrossover<TGene> _crossover;
        private readonly IMutation<TGene> _mutation;
        private readonly ISurvivorSelection<TGene> _survivorSelection;
        private readonly IStopCriterion _stopCriterion;
        private readonly IRepair<TGene> _repair;
        private readonly IRandom _random;

        private SearchProgress _searchProgress;

        public GeneticAlgorithm(
            GeneticAlgorithmConfig config,
            PopulationInitializer<TGene> populationInitializer,
            IFitnessFunction<TGene> fitnessFunction,
            IParentSelection<TGene> parentSelection,
            ICrossover<TGene> crossover,
            IMutation<TGene> mutation,
            ISurvivorSelection<TGene> survivorSelection,
            IStopCriterion stopCriterion,
            IRandom random,
            IRepair<TGene> repair = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _config.Validate();
            _populationInitializer = populationInitializer ?? throw new ArgumentNullException(nameof(populationInitializer));
            _fitnessFunction = fitnessFunction ?? throw new ArgumentNullException(nameof(fitnessFunction));
            _parentSelection = parentSelection ?? throw new ArgumentNullException(nameof(parentSelection));
            _crossover = crossover ?? throw new ArgumentNullException(nameof(crossover));
            _mutation = mutation ?? throw new ArgumentNullException(nameof(mutation));
            _survivorSelection = survivorSelection ?? throw new ArgumentNullException(nameof(survivorSelection));
            _stopCriterion = stopCriterion ?? throw new ArgumentNullException(nameof(stopCriterion));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _repair = repair;
        }

        public GeneticAlgorithmResult<TGene> Run(IEnumerable<TGene> warmStartGenes = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            _searchProgress = new SearchProgress();

            var population = _populationInitializer.CreatePopulation(_config, warmStartGenes, _random);
            foreach (var individual in population)
                EvaluateFitness(individual);
            _survivorSelection.Initialize(population);

            var comparerIndividuals = _survivorSelection.Comparer;
            var bestIndividual = FindBestIndividual(population, comparerIndividuals);
            _searchProgress.BestFitnessValues = bestIndividual.FitnessValues;

            var offspring = new List<Individual<TGene>>(_config.CountOffspring);
            while (true)
            {
                _searchProgress.ElapsedTime = stopwatch.Elapsed;
                if (cancellationToken.IsCancellationRequested)
                {
                    _searchProgress.Cancelled = true;
                    break;
                }
                if (_stopCriterion.ShouldStop(_searchProgress))
                    break;

                offspring.Clear();
                for (int i = 0; i < _config.CountOffspring; i++)
                    offspring.Add(CreateOffspring(population, comparerIndividuals));

                _survivorSelection.Replace(population, offspring);
                _searchProgress.Iteration++;

                var individual = FindBestIndividual(population, comparerIndividuals);
                if (comparerIndividuals.Compare(individual, bestIndividual) > 0)
                {
                    bestIndividual = individual;
                    _searchProgress.BestFitnessValues = bestIndividual.FitnessValues;
                    _searchProgress.LastImprovementIteration = _searchProgress.Iteration;
                }
            }

            _searchProgress.ElapsedTime = stopwatch.Elapsed;
            _searchProgress.IndividualsShareConstraintSatisfying =CalculateIndividualsShareConstraintSatisfying(population);
            return new GeneticAlgorithmResult<TGene>(bestIndividual, population.ToArray(), _searchProgress);
        }

        private Individual<TGene> CreateOffspring(List<Individual<TGene>> population, IIndividualComparer<TGene> comparer)
        {
            var firstParent = _parentSelection.Select(population, comparer, _random);
            var secondParent = _parentSelection.Select(population, comparer, _random);

            var child = RepairIfNeeded(_crossover.Cross(firstParent, secondParent, _random));
            EvaluateFitness(child);

            var mutant = _mutation.Mutate(child, _random);
            if (ReferenceEquals(mutant, child))
                return child;

            mutant = RepairIfNeeded(mutant);
            EvaluateFitness(mutant);

            if (_config.AcceptMutationOnlyIfNotWorse && !comparer.IsNotWorse(mutant, child))
                return child;
            return mutant;
        }

        private Individual<TGene> RepairIfNeeded(Individual<TGene> individual)
        {
            if (_repair == null || individual.ConstraintTracker.IsSatisfied)
                return individual;
            return _repair.Repair(individual);
        }

        private void EvaluateFitness(Individual<TGene> individual)
        {
            if (individual.IsEvaluated)
                return;
            individual.SetFitnessValues(_fitnessFunction.Evaluate(individual.Genes));
            _searchProgress.FitnessFunctionCallCount++;
        }

        private static Individual<TGene> FindBestIndividual(List<Individual<TGene>> population, IComparer<Individual<TGene>> comparer)
        {
            var bestIndividual = population[0];
            for (int i = 1; i < population.Count; i++)
                if (comparer.Compare(population[i], bestIndividual) > 0)
                    bestIndividual = population[i];
            return bestIndividual;
        }

        private static double CalculateIndividualsShareConstraintSatisfying(List<Individual<TGene>> population)
        {
            int feasible = 0;
            foreach (var individual in population)
                if (individual.ConstraintTracker.IsSatisfied)
                    feasible++;
            return (double)feasible / population.Count;
        }
    }
}
