using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repair
{
    // Turns an individual that breaks the constraint into one that satisfies it, or gets as close to it as the repair can.
    // Returns the individual itself when nothing changes, otherwise a changed copy; the given individual is never changed.
    // random - the random number generator, or null when there is none. Deterministic repairs ignore it;
    // repairs that make random choices require it and throw on null. Composite repairs pass it to the nested ones.
    public interface IRepair<TGene>
    {
        Individual<TGene> Repair(Individual<TGene> individual, IRandom random = null);
    }
}
