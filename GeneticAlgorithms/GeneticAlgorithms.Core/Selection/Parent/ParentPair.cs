using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Selection.Parent
{
    // Two parents selected for one crossover that create one offspring individual.
    public readonly struct ParentPair<TGene>
    {
        public Individual<TGene> First { get; }
        public Individual<TGene> Second { get; }


        public ParentPair(Individual<TGene> first, Individual<TGene> second)
        {
            First = first;
            Second = second;
        }
    }
}
