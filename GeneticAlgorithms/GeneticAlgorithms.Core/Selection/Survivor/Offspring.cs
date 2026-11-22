using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Selection.Parent;


namespace GeneticAlgorithms.Selection.Survivor
{
    // A new individual together with the pair of parents it was created from.
    public readonly struct Offspring<TGene>
    {
        public Individual<TGene> Child { get; }
        public ParentPair<TGene> Parents { get; }


        public Offspring(Individual<TGene> child, ParentPair<TGene> parents)
        {
            Child = child;
            Parents = parents;
        }
    }
}
