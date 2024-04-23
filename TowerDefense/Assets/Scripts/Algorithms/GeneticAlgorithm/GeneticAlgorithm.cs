using System.Collections.Generic;


using TowerDefense.Algorithms.GeneticAlgorithm.Persons;
using TowerDefense.Algorithms.GeneticAlgorithm.PersonFactories;
using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;


namespace TowerDefense.Algorithms.GeneticAlgorithm
{
    using Random = System.Random;

    public class GeneticAlgorithm<TPerson, TGene> where TPerson : Person<TGene>
    {
        private int _sizePopulation;
        private int _countIteration;

        private int _mutationChance;
        private float _maxMutationPartPerson;


        private int _totalPrice;
        private List<TGene> _genesTypes;
        private Dictionary<TGene, int> _genesPrices;


        private List<TPerson> _currentPopulation;


        private IPersonFactory<TPerson, TGene> _personFactory;
        private HeuristicsCalculator<TGene> _heuristicsCalculator;


        private Random random = new Random();


        public GeneticAlgorithm(
            IPersonFactory<TPerson, TGene> personFactory,
            HeuristicsCalculator<TGene> heuristicsCalculator,
            int sizePopulation = 20,
            int countIteration = 1000,
            int mutationChance = 50,
            float maxMutationPartPerson = 0.5f)
        {
            _personFactory = personFactory;
            _heuristicsCalculator = heuristicsCalculator;

            _sizePopulation = sizePopulation;
            _countIteration = countIteration;
            _mutationChance = mutationChance;
            _maxMutationPartPerson = maxMutationPartPerson;
        }

        public List<TGene> SearchKnapsac(List<TGene> enemysTypes, Dictionary<TGene, int> enemysPrices, int totalPrice)
        {
            _currentPopulation = new();

            _genesTypes = enemysTypes;
            _genesPrices = enemysPrices;
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
                _currentPopulation.Add(_personFactory.CreatePerson(newPerson, price, _heuristicsCalculator.GetHeuristicsValue(newPerson)));
            }
        }

        private void CreateNewPopulation()
        {
            TPerson newBestPerson = _personFactory.CreatePerson();

            for (int i = 0; i < _currentPopulation.Count - 1; i++)
            {
                TPerson newPerson = Crossing(_currentPopulation[i], _currentPopulation[i + 1]);

                if (newPerson.Price <= _totalPrice)
                {
                    newPerson.CalculateValue();

                    if (newPerson.Value > newBestPerson.Value)
                    {
                        newBestPerson = newPerson;
                    }
                }
            }
            int rand = random.Next(0, 100);
            if (rand < _mutationChance)
                newBestPerson = MutationRandomRegenerateRange(newBestPerson);
            if (rand >= _mutationChance && rand < 2 * _mutationChance && _genesTypes.Count > 1)
                newBestPerson = MutationChangeRangeOneType(newBestPerson);

            if (newBestPerson.Value > _currentPopulation[_currentPopulation.Count - 1].Value)
            {
                _currentPopulation[_currentPopulation.Count - 1] = newBestPerson;
            }
        }


        private TPerson Crossing(TPerson person1, TPerson person2)
        {
            TPerson newPerson = _personFactory.CreatePerson();

            int index1Mid = (int)person1.Genes.Count / 2;
            int index2Mid = (int)person2.Genes.Count / 2;

            newPerson.Genes = new(person1.Genes.GetRange(0, index1Mid));
            newPerson.CalculatePrice(_genesPrices);

            //for (int i = index2Mid; i < person2.Enemies.Count; i++)
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
            int size = random.Next(1, maxSize + 1);

            int priceOldPart = person.GetPriceRangeGenes(index, size, _genesPrices);
            (List<TGene> newPart, int priceNewPart) = CreateListGenesForPrice(priceOldPart + _totalPrice - person.Price);

            TPerson newPerson = _personFactory.CreatePerson(new List<TGene>(person.Genes));
            newPerson.Genes.RemoveRange(index, size);
            newPerson.Genes.InsertRange(index, newPart);
            newPerson.Price = person.Price - priceOldPart + priceNewPart;

            newPerson.CalculateValue();
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

            TPerson newPerson = _personFactory.CreatePerson(new List<TGene>(person.Genes));
            newPerson.Genes.RemoveRange(index, size);
            newPerson.Genes.InsertRange(index, newPart);
            newPerson.Price = person.Price - priceOldPart + priceNewPart;

            newPerson.CalculateValue();
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
                int indexRandomEnemy = random.Next(0, _genesTypes.Count);

                if (price + _genesPrices[_genesTypes[indexRandomEnemy]] <= sumPrice)
                {
                    newPerson.Add(_genesTypes[indexRandomEnemy]);
                    price += _genesPrices[_genesTypes[indexRandomEnemy]];
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
