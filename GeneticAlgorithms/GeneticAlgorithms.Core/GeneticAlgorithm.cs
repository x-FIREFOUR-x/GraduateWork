using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Crossovers;
using GeneticAlgorithms.Fitness;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.InitialPopulation;
using GeneticAlgorithms.Mutations;
using GeneticAlgorithms.Randomness;
using GeneticAlgorithms.Repairs;
using GeneticAlgorithms.Selection;
using GeneticAlgorithms.Selection.Parent;
using GeneticAlgorithms.Selection.Survivor;
using GeneticAlgorithms.StopCriteria;


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
        private readonly bool _canKeepInfeasibleIndividuals;

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

            ValidateOperatorsKeepLengthForEqualLengthCrossover();
            ValidateSelectionsMatchComparerOrder();

            _canKeepInfeasibleIndividuals = _fitnessFunction.PrefersFeasibleIndividuals || _survivorSelection.Comparer.PrefersFeasibleIndividuals;
            if (_config.CheckInfeasibleIndividuals)
                ValidateParentAndSurvivorSelectionsAgreeOnFeasibleIndividuals();
        }

        public GeneticAlgorithmResult<TGene> Run(IEnumerable<TGene> warmStartGenes = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            _searchProgress = new SearchProgress();

            var population = _populationInitializer.CreatePopulation(_config, warmStartGenes, _random);
            for (int i = 0; i < population.Count; i++)
            {
                population[i] = RepairIfNeeded(population[i]);
                EnsureInfeasibleIndividualCanBeKept(population[i]);
                EvaluateFitness(population[i]);
            }
            _survivorSelection.Initialize(population);

            var comparerIndividuals = _survivorSelection.Comparer;
            var bestIndividual = FindBestIndividual(population, comparerIndividuals, population[0]);
            _searchProgress.BestFitnessValues = bestIndividual.FitnessValues;

            var offspring = new List<Offspring<TGene>>(_config.CountOffspring);
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
                var parentPairs = _parentSelection.SelectParents(population, _config.CountOffspring, _random);
                foreach (var parentPair in parentPairs)
                    offspring.Add(new Offspring<TGene>(CreateOffspring(parentPair.First, parentPair.Second, comparerIndividuals), parentPair));

                _survivorSelection.Replace(population, offspring, _random);
                _searchProgress.IterationCount++;

                var individual = FindBestIndividual(population, comparerIndividuals, bestIndividual);
                if (!ReferenceEquals(individual, bestIndividual))
                {
                    bestIndividual = individual;
                    _searchProgress.BestFitnessValues = bestIndividual.FitnessValues;
                    _searchProgress.NumberLastImprovementIteration = _searchProgress.IterationCount;
                }
            }

            _searchProgress.ElapsedTime = stopwatch.Elapsed;
            _searchProgress.IndividualsShareConstraintSatisfying = CalculateIndividualsShareConstraintSatisfying(population);
            var paretoFront = RemoveDuplicateGenes(ParetoUtilities.FindNondominated(population, comparerIndividuals));
            return new GeneticAlgorithmResult<TGene>(bestIndividual, population.ToArray(), paretoFront, _searchProgress);
        }

        private Individual<TGene> CreateOffspring(Individual<TGene> firstParent, Individual<TGene> secondParent, IIndividualComparer<TGene> comparer)
        {
            var child = RepairIfNeeded(_crossover.Cross(firstParent, secondParent, _random));
            EnsureInfeasibleIndividualCanBeKept(child);
            EvaluateFitness(child);

            var mutant = _mutation.Mutate(child, _random, _searchProgress.IterationCount);
            if (ReferenceEquals(mutant, child))
                return child;

            mutant = RepairIfNeeded(mutant);
            EnsureInfeasibleIndividualCanBeKept(mutant);
            EvaluateFitness(mutant);

            bool improved = comparer.IsBetter(mutant, child);
            if (_mutation is IMutationFeedback feedback)
                feedback.Report(improved);

            if (_config.AcceptMutationOnlyIfNotWorse && !comparer.IsNotWorse(mutant, child))
                return child;
            return mutant;
        }

        // A crossover that needs parents of equal length fails as soon as individuals of different lengths meet as parents,
        // so every mutation and the repair must keep the length; otherwise the combination is rejected before the run.
        private void ValidateOperatorsKeepLengthForEqualLengthCrossover()
        {
            if (!_crossover.RequiresParentsOfEqualLength)
                return;

            if (!_mutation.PreservesIndividualLength)
                throw new ArgumentException(
                    $"{TypeName(_mutation)} changes the length of individuals, but {TypeName(_crossover)} requires parents of equal length. " +
                    "Use length-preserving mutations or a crossover for parents of different lengths.",
                    "mutation");

            if (_repair != null && !_repair.PreservesIndividualLength)
                throw new ArgumentException(
                    $"{TypeName(_repair)} changes the length of individuals, but {TypeName(_crossover)} requires parents of equal length. " +
                    "Use a repair that keeps the length, no repair, or a crossover for parents of different lengths.",
                    "repair");
        }

        // A selection that sorts individuals or reads a single fitness value needs a total order. With a partial order such as Pareto dominance
        // the order of individuals that do not dominate each other would be arbitrary, and a single fitness value would draw the search
        // to one end of the front, so the combination is rejected before the run. A parent selection without a comparer
        // is checked against the comparer of the survivor selection, which decides how the algorithm orders individuals.
        private void ValidateSelectionsMatchComparerOrder()
        {
            const string parentExample = "Use a selection that only compares pairs, for example CrowdedTournamentParentSelection.";
            
            if (_survivorSelection.RequiresTotalOrder && !_survivorSelection.Comparer.IsTotalOrder)
                throw new ArgumentException(
                    $"{TypeName(_survivorSelection)} sorts the population, but its comparer {ComparerName(_survivorSelection.Comparer)} does not order every pair of individuals, " +
                    "so the sorted order would be arbitrary. Use a comparer that orders every pair, for example FitnessComparer, " +
                    "or a selection that supports such comparers, for example NondominatedSortingSurvivorSelection.",
                    "survivorSelection");

            // A parent selection that only compares pairs or picks at random works with any comparer, so only the others are checked below.
            if (!_parentSelection.RequiresTotalOrder)
                return;

            if (_parentSelection.Comparer != null && !_parentSelection.Comparer.IsTotalOrder)
                throw new ArgumentException(
                    $"{TypeName(_parentSelection)} sorts individuals, but its comparer {ComparerName(_parentSelection.Comparer)} does not order every pair of individuals, " +
                    $"so the sorted order would be arbitrary. {parentExample}",
                    "parentSelection");

            if (!_survivorSelection.Comparer.IsTotalOrder)
                throw new ArgumentException(
                    $"{TypeName(_parentSelection)} chooses parents by one fitness value, but {TypeName(_survivorSelection)} keeps survivors by {ComparerName(_survivorSelection.Comparer)}, " +
                    $"which does not order every pair of individuals, so parents and survivors would be chosen by different criteria. {parentExample}",
                    "parentSelection");
        }

        // Name of a comparer with its nested comparer, for example ConstrainedFitnessComparer(ParetoDominanceComparer).
        private static string ComparerName(IIndividualComparer<TGene> comparer)
            => comparer is ConstrainedFitnessComparer<TGene> constrained
                ? $"{TypeName(constrained)}({ComparerName(constrained.FeasibleIndividualComparer)})"
                : TypeName(comparer);

        // Name of the type of a component without the arity suffix of generic types, for example AdditionMutation instead of AdditionMutation`1.
        private static string TypeName(object component) => component.GetType().Name.Split('`')[0];

        // The parent selection and the survivor selection must prefer feasible individuals alike: through the fitness function,
        // which both use, or through their comparers. Otherwise infeasible individuals are preferred in one selection
        // and treated as feasible in the other. A fitness-proportional parent selection prefers individuals by fitness values only,
        // and a parent selection that neither compares nor uses fitness values, such as random selection, never prefers anyone.
        private void ValidateParentAndSurvivorSelectionsAgreeOnFeasibleIndividuals()
        {
            var parentComparer = _parentSelection.Comparer;
            if (parentComparer == null && !_parentSelection.UsesFitnessValues)
                return;

            bool parentSelectionPrefersFeasibleIndividuals = _fitnessFunction.PrefersFeasibleIndividuals || (parentComparer?.PrefersFeasibleIndividuals ?? false);
            bool survivorSelectionPrefersFeasibleIndividuals = _fitnessFunction.PrefersFeasibleIndividuals || _survivorSelection.Comparer.PrefersFeasibleIndividuals;
            if (parentSelectionPrefersFeasibleIndividuals != survivorSelectionPrefersFeasibleIndividuals)
                throw new ArgumentException(
                    $"The parent selection {(parentSelectionPrefersFeasibleIndividuals ? "prefers" : "does not prefer")} feasible individuals, " +
                    $"but the survivor selection {(survivorSelectionPrefersFeasibleIndividuals ? "does" : "does not")}. " +
                    "Give both selections a comparer that prefers feasible individuals, such as ConstrainedFitnessComparer, " +
                    "or use a PenalizedFitnessFunction; fitness-proportional parent selections compare only by fitness values, so they need the penalty.",
                    "parentSelection");
        }

        // An infeasible individual left after the repair would compete by fitness only when neither the fitness function
        // nor the comparer prefers feasible individuals, so it could win over feasible ones; the run is stopped with an explanation.
        private void EnsureInfeasibleIndividualCanBeKept(Individual<TGene> individual)
        {
            if (!_config.CheckInfeasibleIndividuals || _canKeepInfeasibleIndividuals || individual.ConstraintTracker.IsSatisfied)
                return;

            throw new InvalidOperationException(
                "An individual breaks the constraint after the repair, but neither the fitness function nor the comparer prefers feasible individuals, " +
                "so infeasible individuals could win over feasible ones. Use a repair that repairs every individual, " +
                "a PenalizedFitnessFunction or a ConstrainedFitnessComparer. If the constraint cannot be satisfied with the available genes, " +
                "even a repair leaves it violated. Set CheckInfeasibleIndividuals to false in the config to allow infeasible individuals.");
        }

        private Individual<TGene> RepairIfNeeded(Individual<TGene> individual)
        {
            if (_repair == null || individual.ConstraintTracker.IsSatisfied)
                return individual;
            return _repair.Repair(individual, _random);
        }

        private void EvaluateFitness(Individual<TGene> individual)
        {
            if (individual.IsEvaluated)
                return;
            individual.SetFitnessValues(_fitnessFunction.Evaluate(individual.Genes));
            _searchProgress.FitnessFunctionCallCount++;
        }

        // Starting from the current best individual, so the result is not dominated by it or any individual of the population even for a partial order such as Pareto dominance.
        private static Individual<TGene> FindBestIndividual(List<Individual<TGene>> population, IIndividualComparer<TGene> comparer, Individual<TGene> currentBestIndividual)
        {
            var bestIndividual = currentBestIndividual;
            for (int i = 0; i < population.Count; i++)
                if (comparer.IsBetter(population[i], bestIndividual))
                    bestIndividual = population[i];
            return bestIndividual;
        }

        private static List<Individual<TGene>> RemoveDuplicateGenes(List<Individual<TGene>> individuals)
        {
            var uniqueIndividuals = new List<Individual<TGene>>(individuals.Count);
            foreach (var individual in individuals)
                if (!SurvivorSelectionUtilities.PopulationContainsSameIndividual(uniqueIndividuals, individual))
                    uniqueIndividuals.Add(individual);
            return uniqueIndividuals;
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
