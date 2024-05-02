using System.Collections.Generic;

using TowerDefense.Algorithms.GeneticAlgorithm.Persons;
using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;
using TowerDefense.Algorithms.GeneticAlgorithm.Mutations;


namespace TowerDefense.Algorithms.GeneticAlgorithm
{
    using Random = System.Random;

    public class GeneticAlgorithm<TPerson, TGene> where TPerson : Person<TGene>
    {
        private int _sizePopulation;
        private int _countIteration;

        private float _maxMutationPartPerson;
        private MutationSelector _mutationSelector;

        private int _totalPrice;
        private List<TGene> _genesTypes;
        private Dictionary<TGene, int> _genesPrices;


        private List<TPerson> _currentPopulation;


        private PersonFactory<TGene> _personFactory;
        private HeuristicsCalculator<TGene> _heuristicsCalculator;


        private Random random = new Random();


        public GeneticAlgorithm(
            PersonFactory<TGene> personFactory,
            HeuristicsCalculator<TGene> heuristicsCalculator,
            MutationSelector mutationSelector,
            int sizePopulation = 20,
            int countIteration = 1000,
            float maxMutationPartPerson = 0.5f)
        {
            _personFactory = personFactory;
            _heuristicsCalculator = heuristicsCalculator;

            _sizePopulation = sizePopulation;
            _countIteration = countIteration;
            _mutationSelector = mutationSelector;
            _maxMutationPartPerson = maxMutationPartPerson;
        }

        public List<TGene> SearchKnapsac(List<TGene> genesTypes, Dictionary<TGene, int> genesPrices, int totalPrice)
        {
            _currentPopulation = new();

            _genesTypes = genesTypes;
            _genesPrices = genesPrices;
            _totalPrice = totalPrice;

            CreateStartPopulation();
            SortPopulation();
            for (int i = 0; i < _countIteration; i++)
            {
                CreateNewPopulation();
                SortPopulation();
            }

            return _currentPopulation[0].Genes;
        }


        private void SortPopulation()
        {
            _currentPopulation.Sort((a, b) => b.Value.CompareTo(a.Value));
        }

        private void CreateStartPopulation()
        {
            for (int i = 0; i < _sizePopulation; i++)
            {
                (List<TGene> newPerson, int price) = CreateListGenesForPrice(_totalPrice);
                _currentPopulation.Add((TPerson)_personFactory.CreatePerson(newPerson, price, _heuristicsCalculator.GetHeuristicsValue(newPerson)));
            }
        }

        private void CreateNewPopulation()
        {
            TPerson newBestPerson = (TPerson)_personFactory.CreatePerson();
            newBestPerson.CalculateValue(_heuristicsCalculator);

            for (int i = 0; i < _currentPopulation.Count - 1; i++)
            {
                TPerson newPerson = Crossing(_currentPopulation[i], _currentPopulation[i + 1]);

                if (newPerson.Price <= _totalPrice)
                {
                    newPerson.CalculateValue(_heuristicsCalculator);

                    if (newPerson.Value > newBestPerson.Value)
                    {
                        newBestPerson = newPerson;
                    }
                }
            }

            MutationType randomMutation = _mutationSelector.SelectMutation();
            switch (randomMutation)
            {
                case MutationType.RandomRegenerateRange:
                    newBestPerson = MutationRandomRegenerateRange(newBestPerson);
                    break;
                case MutationType.ChangeRangeOneType:
                    newBestPerson = MutationChangeRangeOneType(newBestPerson);
                    break;
                default:
                    break;
            }

            if (newBestPerson.Value > _currentPopulation[_currentPopulation.Count - 1].Value)
            {
                _currentPopulation[_currentPopulation.Count - 1] = newBestPerson;
            }
        }


        private TPerson Crossing(TPerson person1, TPerson person2)
        {
            TPerson newPerson = (TPerson)_personFactory.CreatePerson();

            int index1Mid = (int)person1.Genes.Count / 2;
            int index2Mid = (int)person2.Genes.Count / 2;

            newPerson.Genes = new(person1.Genes.GetRange(0, index1Mid));
            newPerson.CalculatePrice(_genesPrices);

            for (int i = person2.Genes.Count - 1; i >= 0; i--)
            {
                if (newPerson.Price + _genesPrices[person2.Genes[i]] <= _totalPrice)
                {
                    newPerson.Genes.Add(person2.Genes[i]);
                    newPerson.Price += _genesPrices[person2.Genes[i]];
                }
            }

            return newPerson;
        }


        //Mutation: randomly regenerate a random interval
        private TPerson MutationRandomRegenerateRange(TPerson person)
        {
            int index = random.Next(0, person.Genes.Count);

            int maxSize = (int)(person.Genes.Count * _maxMutationPartPerson);
            maxSize = (maxSize == 0) ? 1 : maxSize;
            maxSize = (maxSize > person.Genes.Count - index) ? person.Genes.Count - index : maxSize;

            int size = (maxSize != 0) ? random.Next(1, maxSize + 1) : 0;

            int priceOldPart = person.GetPriceRangeGenes(index, size, _genesPrices);
            (List<TGene> newPart, int priceNewPart) = CreateListGenesForPrice(priceOldPart + _totalPrice - person.Price);

            TPerson newPerson = (TPerson)_personFactory.CreatePerson(new List<TGene>(person.Genes));
            newPerson.Genes.RemoveRange(index, size);
            newPerson.Genes.InsertRange(index, newPart);
            newPerson.Price = person.Price - priceOldPart + priceNewPart;

            newPerson.CalculateValue(_heuristicsCalculator);
            if (newPerson.Value > person.Value)
                return newPerson;
            else
                return person;
        }

        //Mutation: change random interval to one type gene
        private TPerson MutationChangeRangeOneType(TPerson person)
        {
            int index = random.Next(0, person.Genes.Count);

            int maxSize = (int)(person.Genes.Count * _maxMutationPartPerson);
            maxSize = (maxSize == 0) ? 1 : maxSize;
            maxSize = (maxSize > person.Genes.Count - index) ? person.Genes.Count - index : maxSize;
            int size = random.Next(1, maxSize + 1);

            TGene type = _genesTypes[random.Next(1, _genesTypes.Count)];

            int priceOldPart = person.GetPriceRangeGenes(index, size, _genesPrices);

            (List<TGene> newPart, int priceNewPart) = CreateListOneTypeGeneForPrice(priceOldPart, type);
            (List<TGene> additionalPart, int priceAdditionalPart) = CreateListOneTypeGeneForPrice(priceOldPart - priceNewPart, _genesTypes[0]);

            priceNewPart += priceAdditionalPart;
            newPart.AddRange(additionalPart);

            TPerson newPerson = (TPerson)_personFactory.CreatePerson(new List<TGene>(person.Genes));
            newPerson.Genes.RemoveRange(index, size);
            newPerson.Genes.InsertRange(index, newPart);
            newPerson.Price = person.Price - priceOldPart + priceNewPart;

            newPerson.CalculateValue(_heuristicsCalculator);
            if (newPerson.Value > person.Value)
                return newPerson;
            else
                return person;
        }




        private (List<TGene>, int) CreateListGenesForPrice(int sumPrice)
        {
            int price = 0;
            List<TGene> newPerson = new();

            bool isFinish = false;
            while (!isFinish)
            {
                Random r = new Random();
                int indexRandomGeneType = random.Next(0, _genesTypes.Count);

                if (price + _genesPrices[_genesTypes[indexRandomGeneType]] <= sumPrice)
                {
                    newPerson.Add(_genesTypes[indexRandomGeneType]);
                    price += _genesPrices[_genesTypes[indexRandomGeneType]];
                }
                else
                {
                    isFinish = true;
                }
            }

            return (newPerson, price);
        }

        private (List<TGene>, int) CreateListOneTypeGeneForPrice(int sumPrice, TGene type)
        {
            int price = 0;
            List<TGene> newPerson = new();

            bool isFinish = false;
            while (!isFinish)
            {
                if (price + _genesPrices[type] <= sumPrice)
                {
                    newPerson.Add(type);
                    price += _genesPrices[type];
                }
                else
                {
                    isFinish = true;
                }
            }

            return (newPerson, price);
        }
    }

}
