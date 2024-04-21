using System;
using System.Collections.Generic;


namespace TowerDefense.Collections
{
    public class PriorityQueueWithDictOfPriorities<Value, KeyPriority, Priority>
	{
		private List<Tuple<Value, Dictionary<KeyPriority, Priority>>> elements = new();

		private readonly Func<Priority, Priority, bool> _comparer;

		public int Count => elements.Count;

		public PriorityQueueWithDictOfPriorities(Func<Priority, Priority, bool> comparer)
		{
			elements = new();
			_comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
		}

		public void Add(Value item, Dictionary<KeyPriority, Priority> priorityValue)
		{
			elements.Add(Tuple.Create(item, priorityValue));
		}

		public Value Peek(KeyPriority keyPriority)
		{
			int bestPriorityIndex = 0;

			for (int i = 0; i < elements.Count; i++)
			{
				if (_comparer(elements[i].Item2[keyPriority], elements[bestPriorityIndex].Item2[keyPriority]))
				{
					bestPriorityIndex = i;
				}
			}

			Value bestItem = elements[bestPriorityIndex].Item1;
			elements.RemoveAt(bestPriorityIndex);
			return bestItem;
		}
	}
}
