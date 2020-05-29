using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

// Based on: https://hyperfoxstudios.com/category/tutorial/

namespace Assets.Scripts.LevelGen
{
    /// <summary>
    /// Item that can be picked by a PieChanceTable.
    /// </summary>
    public abstract class PieChanceItem<T>
    {
        // Item it represents - usually GameObject, integer etc...
        public T Item;

        // How many units the item takes - more units, higher chance of being picked
        public float ProbabilityWeight;

        // Displayed only as an information for the designer/programmer. Should not be set manually via inspector!    
		[ProgressBar("Probability Percent", 100, EColor.Red)]
        public float probabilityPercent;

        // These values are assigned via LootDropTable script. They represent from which number to which number if selected, the item will be picked.
        [HideInInspector]
        public float probabilityRangeFrom;
        [HideInInspector]
        public float probabilityRangeTo;
    }

	/// <summary>
	/// Class serves for assigning and picking probabilities.
	/// </summary>
	public abstract class PieChanceTable<T, U> where T : PieChanceItem<U>
	{
		// List where we'll assign the items.
		[SerializeField]
		public List<T> items;

		// Sum of all weights of items.
		[SerializeField, HideInInspector]
		float probabilityTotalWeight;

		/// <summary>
		/// Calculates the percentage and asigns the probabilities how many times
		/// the items can be picked. Function used also to validate data when tweaking numbers in editor.
		/// </summary>	
		public void ValidateTable()
		{
			// Prevent editor from "crying" when the item list is empty :)
			if (items != null && items.Count > 0)
			{

				float currentProbabilityWeightMaximum = 0f;

				// Sets the weight ranges of the selected items.
				foreach (T lootDropItem in items)
				{

					if (lootDropItem.ProbabilityWeight < 0f)
					{
						// Prevent usage of negative weight.
						Debug.Log("You can't have negative weight on an item. Reseting item's weight to 0.");
						lootDropItem.ProbabilityWeight = 0f;
					}
					else
					{
						lootDropItem.probabilityRangeFrom = currentProbabilityWeightMaximum;
						currentProbabilityWeightMaximum += lootDropItem.ProbabilityWeight;
						lootDropItem.probabilityRangeTo = currentProbabilityWeightMaximum;
					}

				}

				probabilityTotalWeight = currentProbabilityWeightMaximum;

				// Calculate percentage of item drop select rate.
				foreach (T lootDropItem in items)
				{
					lootDropItem.probabilityPercent = ((lootDropItem.ProbabilityWeight) / probabilityTotalWeight) * 100;
				}
			}
		}

		/// <summary>
		/// Picks and returns an item based on it's probability.
		/// </summary>
		public U PickItem()
		{
			float pickedNumber = Random.Range(0, probabilityTotalWeight);

			// Find an item whose range contains pickedNumber
			foreach (T item in items)
			{
				// If the picked number matches the item's range, return item
				if (pickedNumber > item.probabilityRangeFrom && pickedNumber < item.probabilityRangeTo)
				{
					return item.Item;
				}
			}

			Debug.Log(pickedNumber);
			// If item wasn't picked... Notify programmer via console and return the first item from the list
			Debug.LogError("Item couldn't be picked... Be sure that all of your active loot drop tables have assigned at least one item!");
			return items[0].Item;
		}
	}
}
