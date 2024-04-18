using System;
using System.Collections.Generic;

namespace TowerDefense.Collections
{
    public class PriorityQueue<Value, Priority>
    {
		private List<Tuple<Value, Priority>> elements = new List<Tuple<Value, Priority>>();

		private readonly Func<Priority, Priority, bool> _comparer;

		public int Count => elements.Count;

		public PriorityQueue(Func<Priority, Priority, bool> comparer)
		{
			elements = new List<Tuple<Value, Priority>>();
			_comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
		}

		public void Add(Value item, Priority priorityValue)
		{
			elements.Add(Tuple.Create(item, priorityValue));
		}


		public Value Peek()
		{
			int bestPriorityIndex = 0;

			for (int i = 0; i < elements.Count; i++)
			{
				if (_comparer(elements[i].Item2, elements[bestPriorityIndex].Item2))
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
