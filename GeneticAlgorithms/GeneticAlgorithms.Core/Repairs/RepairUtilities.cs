using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repairs
{
    internal static class RepairUtilities
    {
        public static void ValidateIndividualNotNull<TGene>(Individual<TGene> individual)
        {
            if (individual == null)
                throw new ArgumentNullException(nameof(individual));
        }

        // For repairs that make random choices and cannot work without a random number generator.
        public static void ValidateRandomNotNull(IRandom random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), "This repair makes random choices and requires a random number generator.");
        }

        public static void ValidateRepairs<TGene>(IReadOnlyList<IRepair<TGene>> repairs, string parameterName)
        {
            if (repairs == null || repairs.Count == 0)
                throw new ArgumentException("At least one repair is required.", parameterName);

            if (repairs.Any(repair => repair == null))
                throw new ArgumentException("Repairs must not contain null.", parameterName);
        }

        // Returns the available genes without repeats, so that every gene is considered once.
        public static List<TGene> CreateDistinctGenes<TGene>(IReadOnlyList<TGene> availableGenes, string parameterName)
        {
            if (availableGenes == null)
                throw new ArgumentNullException(parameterName);

            var genes = availableGenes.Distinct().ToList();
            if (genes.Count == 0)
                throw new ArgumentException("At least one available gene is required.", parameterName);

            return genes;
        }

        // Repair shared by all repairs: the constraint decides what has to be fixed, and the given functions decide which genes change.
        // The constraint tracker reports UpperBoundViolation, how far upper bounds such as a budget or a max count are exceeded,
        // and LowerBoundViolation, how far lower bounds such as a min count or a min weight of a group are not reached:
        // 0. If removeUnavailableGenes is set, genes that are not among the available genes are removed.
        // 1. While UpperBoundViolation is above 0, chooseGeneToRemove picks one of the genes whose removal lowers it, preferring genes
        //    whose removal does not raise LowerBoundViolation. If chooseReplacement gives a gene for it, the gene is replaced, otherwise removed.
        //    Then the removed genes, from the last removed one, are put back if they fit again, so no gene is removed needlessly.
        //    The kept genes stay in their original order and places.
        // 2. While LowerBoundViolation is above 0, chooseGeneToAppend picks one of the available genes that fit and lower it.
        // 3. If fillRemainder is set, chooseGeneToAppend picks one of the available genes that fit while any of them fits.
        // A step stops when no gene helps, so a constraint that cannot be satisfied is left as close to it as the genes allow.
        // Appended genes go to the end of the individual. If nothing changes, the individual itself is returned.
        // The filling of step 3 stops only when the constraint forbids every gene, so every gene must use up some of a limit.
        public static Individual<TGene> RepairConstraint<TGene>(
            Individual<TGene> individual,
            IRandom random,
            IReadOnlyList<TGene> availableGenes,
            bool fillRemainder,
            bool removeUnavailableGenes,
            Func<IReadOnlyList<TGene>, IReadOnlyList<int>, IRandom, int> chooseGeneToRemove,
            Func<IReadOnlyList<TGene>, IRandom, TGene> chooseGeneToAppend,
            Func<IReadOnlyList<TGene>, int, IConstraintTracker<TGene>, (bool Found, TGene Gene)> chooseReplacement = null)
        {
            ValidateIndividualNotNull(individual);

            var repairedIndividual = RemoveGenes(individual, random, availableGenes, removeUnavailableGenes, chooseGeneToRemove, chooseReplacement);
            repairedIndividual = AppendGenes(individual, repairedIndividual, random, availableGenes, true, chooseGeneToAppend);

            if (fillRemainder)
                repairedIndividual = AppendGenes(individual, repairedIndividual, random, availableGenes, false, chooseGeneToAppend);

            return repairedIndividual;
        }

        // Index of the candidate gene with the lowest priority; among equal priorities the later gene.
        public static int ChooseIndexWithLowestPriority<TGene>(IReadOnlyList<TGene> genes, IReadOnlyList<int> candidateIndices, Func<TGene, double> getGenePriority)
        {
            int chosenIndex = candidateIndices[0];
            double chosenPriority = getGenePriority(genes[chosenIndex]);
            for (int i = 1; i < candidateIndices.Count; i++)
            {
                double priority = getGenePriority(genes[candidateIndices[i]]);
                if (priority < chosenPriority || (priority == chosenPriority && candidateIndices[i] > chosenIndex))
                {
                    chosenIndex = candidateIndices[i];
                    chosenPriority = priority;
                }
            }
            return chosenIndex;
        }

        // Candidate gene with the highest priority; among equal priorities the first one.
        public static TGene ChooseGeneWithHighestPriority<TGene>(IReadOnlyList<TGene> candidateGenes, Func<TGene, double> getGenePriority)
        {
            var chosenGene = candidateGenes[0];
            double chosenPriority = getGenePriority(chosenGene);
            for (int i = 1; i < candidateGenes.Count; i++)
            {
                double priority = getGenePriority(candidateGenes[i]);
                if (priority > chosenPriority)
                {
                    chosenGene = candidateGenes[i];
                    chosenPriority = priority;
                }
            }
            return chosenGene;
        }

        // Index of the last candidate gene of the individual.
        public static int ChooseLastIndex(IReadOnlyList<int> candidateIndices)
        {
            int chosenIndex = candidateIndices[0];
            for (int i = 1; i < candidateIndices.Count; i++)
                if (candidateIndices[i] > chosenIndex)
                    chosenIndex = candidateIndices[i];
            return chosenIndex;
        }

        // Steps 0 and 1 of RepairConstraint: removes unavailable genes if asked, then removes or replaces genes
        // until upper bounds are no longer violated, and puts back removed genes that fit again.
        // The work is done on a copy of the genes and of the tracker, with flags for removed and replaced genes,
        // so the individual is cloned only once at the end, and only if something changed.
        private static Individual<TGene> RemoveGenes<TGene>(
            Individual<TGene> originIndividual,
            IRandom random,
            IReadOnlyList<TGene> availableGenes,
            bool removeUnavailableGenes,
            Func<IReadOnlyList<TGene>, IReadOnlyList<int>, IRandom, int> chooseGeneToRemove,
            Func<IReadOnlyList<TGene>, int, IConstraintTracker<TGene>, (bool Found, TGene Gene)> chooseReplacement)
        {
            var individualGenes = originIndividual.Genes.ToArray();
            var tracker = originIndividual.ConstraintTracker.Clone();

            var removedGenes = new bool[individualGenes.Length];
            var replacedGenes = new bool[individualGenes.Length];

            // Step 0: genes that are not available are removed for good and are never put back.
            if (removeUnavailableGenes)
            {
                var comparer = EqualityComparer<TGene>.Default;
                for (int i = 0; i < individualGenes.Length; i++)
                {
                    if (availableGenes.Any(gene => comparer.Equals(gene, individualGenes[i])))
                        continue;
                    tracker.Remove(individualGenes[i]);
                    removedGenes[i] = true;
                }
            }

            // Step 1: one gene per round is removed or replaced, until the upper bounds hold or no gene helps.
            // Genes whose removal does not hurt the lower bounds are preferred; the others are used only when there is no such gene.
            var indicesRemovedGenes = new List<int>();
            var indicesCandidates = new List<int>();
            var fallbackIndicesCandidates = new List<int>();
            while (tracker.UpperBoundViolation > 0)
            {
                CollectRemovalIndicesCandidates(individualGenes, removedGenes, tracker, indicesCandidates, fallbackIndicesCandidates);
                var chosenIndicesCandidates = indicesCandidates.Count > 0 ? indicesCandidates : fallbackIndicesCandidates;
                if (chosenIndicesCandidates.Count == 0)
                    break;

                // The chosen gene is replaced by a cheaper one if the repair gives a replacement, otherwise it is removed;
                // a replaced gene stays a candidate, so it may be replaced again in a later round.
                int index = chooseGeneToRemove(individualGenes, chosenIndicesCandidates, random);
                var replacement = chooseReplacement?.Invoke(individualGenes, index, tracker) ?? default;
                tracker.Remove(individualGenes[index]);
                if (replacement.Found)
                {
                    tracker.Append(replacement.Gene);
                    individualGenes[index] = replacement.Gene;
                    replacedGenes[index] = true;
                }
                else
                {
                    removedGenes[index] = true;
                    indicesRemovedGenes.Add(index);
                }
            }

            // Removing one gene at a time may remove more than needed, for example a cheap gene before an expensive one,
            // so the removed genes, from the last removed one, are put back while they fit. Only done when the upper bounds hold,
            // as putting back into a still violated constraint would only make it worse.
            if (tracker.UpperBoundViolation <= 0)
            {
                for (int i = indicesRemovedGenes.Count - 1; i >= 0; i--)
                {
                    int index = indicesRemovedGenes[i];
                    if (!tracker.CanAppend(individualGenes[index]))
                        continue;
                    tracker.Append(individualGenes[index]);
                    removedGenes[index] = false;
                }
            }

            // Anything change 
            if (Array.IndexOf(removedGenes, true) < 0 && Array.IndexOf(replacedGenes, true) < 0)
                return originIndividual;

            // Replacements keep the positions, so they are applied first; removals go from the end, so earlier indices stay valid.
            var repairedIndividual = originIndividual.Clone();
            for (int i = 0; i < individualGenes.Length; i++)
                if (replacedGenes[i])
                    repairedIndividual.Replace(i, individualGenes[i]);
            for (int i = individualGenes.Length - 1; i >= 0; i--)
                if (removedGenes[i])
                    repairedIndividual.RemoveAt(i);

            return repairedIndividual;
        }

        // Finds the genes whose removal lowers the upper bound violation, by removing each gene from the tracker
        // and appending it back. They are split into two lists:
        // indicesCandidates - the removal does not raise the lower bound violation, so it fixes the upper bounds without hurting the lower ones;
        // fallbackIndicesCandidates - the removal raises the lower bound violation, used only when indicesCandidates is empty.
        // Example: budget 10, at least two odd genes, genes [3, 4, 5] -> indicesCandidates [4], fallbackIndicesCandidates [3, 5].
        private static void CollectRemovalIndicesCandidates<TGene>(
            TGene[] genes,
            bool[] removedGenes,
            IConstraintTracker<TGene> tracker,
            List<int> indicesCandidates,
            List<int> fallbackIndicesCandidates)
        {
            indicesCandidates.Clear();
            fallbackIndicesCandidates.Clear();

            double upperBoundViolation = tracker.UpperBoundViolation;
            double lowerBoundViolation = tracker.LowerBoundViolation;

            for (int i = 0; i < genes.Length; i++)
            {
                if (removedGenes[i])
                    continue;

                tracker.Remove(genes[i]);
                bool lowersUpperBoundViolation = tracker.UpperBoundViolation < upperBoundViolation;
                bool raisesLowerBoundViolation = tracker.LowerBoundViolation > lowerBoundViolation;
                tracker.Append(genes[i]);

                if (lowersUpperBoundViolation)
                    (raisesLowerBoundViolation ? fallbackIndicesCandidates : indicesCandidates).Add(i);
            }
        }

        // Steps 2 and 3 of RepairConstraint: appends one chosen gene per round to the end of the individual.
        // With reduceLowerBoundViolation (step 2) it stops as soon as the lower bounds hold and appends only genes that help them;
        // without it (step 3, filling) it appends any gene that fits until none does.
        // current is the result of the previous step; the individual is cloned only before the first appended gene,
        // so if nothing was changed before and nothing is appended, the individual itself is returned.
        private static Individual<TGene> AppendGenes<TGene>(
            Individual<TGene> originIndividual,
            Individual<TGene> currentIndividual,
            IRandom random,
            IReadOnlyList<TGene> availableGenes,
            bool reduceLowerBoundViolation,
            Func<IReadOnlyList<TGene>, IRandom, TGene> chooseGeneToAppend)
        {
            var repairedIndividual = ReferenceEquals(currentIndividual, originIndividual) ? null : currentIndividual;
            var indicesCandidates = new List<TGene>();
            while (true)
            {
                var latestIndividual = repairedIndividual ?? originIndividual;
                if (reduceLowerBoundViolation && latestIndividual.ConstraintTracker.LowerBoundViolation <= 0)
                    break;

                CollectAppendindicesCandidates(latestIndividual, availableGenes, reduceLowerBoundViolation, indicesCandidates);
                if (indicesCandidates.Count == 0)
                    break;

                var gene = chooseGeneToAppend(indicesCandidates, random);
                repairedIndividual ??= originIndividual.Clone();
                repairedIndividual.Append(gene);
            }
            return repairedIndividual ?? originIndividual;
        }

        // Finds the available genes that fit the upper bounds of the individual.
        // With reduceLowerBoundViolation only the genes whose appending lowers the lower bound violation are kept,
        // checked on a copy of the tracker by appending each gene and removing it back.
        // Example: at least two odd genes, genes [2], available genes [1, 2, 3], all fit ->
        // with reduceLowerBoundViolation [1, 3], without it [1, 2, 3].
        private static void CollectAppendindicesCandidates<TGene>(
            Individual<TGene> individual,
            IReadOnlyList<TGene> availableGenes,
            bool reduceLowerBoundViolation,
            List<TGene> indicesCandidates)
        {
            indicesCandidates.Clear();
            var tracker = reduceLowerBoundViolation ? individual.ConstraintTracker.Clone() : null;

            double lowerBoundViolation = reduceLowerBoundViolation ? tracker.LowerBoundViolation : 0;
            foreach (var gene in availableGenes)
            {
                if (!individual.CanAppend(gene))
                    continue;

                if (reduceLowerBoundViolation)
                {
                    tracker.Append(gene);
                    bool lowersLowerBoundViolation = tracker.LowerBoundViolation < lowerBoundViolation;
                    tracker.Remove(gene);
                    if (!lowersLowerBoundViolation)
                        continue;
                }
                indicesCandidates.Add(gene);
            }
        }
    }
}
