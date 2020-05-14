using UnityEngine;
using System;
using System.Collections.Generic;

namespace Pooling
{
    public class IndexedPooler : BasePooler 
	{
		#region Events
		public Action<Poolable, int> willEnqueueAtIndex;
		public Action<Poolable, int> didDequeueAtIndex;
		#endregion

		#region Fields / Properties
		public List<Poolable> Collection = new List<Poolable>();
		#endregion

		#region Public
		public Poolable GetItem (int index)
		{
			if (index < 0 || index >= this.Collection.Count)
				return null;
			return this.Collection[index];
		}

		public U GetScript<U> (int index) where U : MonoBehaviour
		{
			Poolable item = GetItem(index);
			if (item != null)
				return item.GetComponent<U>();
			return null;
		}

		public void EnqueueByIndex (int index)
		{
			if (index < 0 || index >= this.Collection.Count)
				return;
			Enqueue(this.Collection[index]);
		}

		public override void Enqueue (Poolable item)
		{
			base.Enqueue(item);
			int index = this.Collection.IndexOf(item);
			if (index != -1)
			{
				if (this.willEnqueueAtIndex != null)
                    this.willEnqueueAtIndex(item, index);
                this.Collection.RemoveAt(index);
			}
		}

		public override Poolable Dequeue ()
		{
			Poolable item = base.Dequeue ();
            this.Collection.Add(item);
			if (this.didDequeueAtIndex != null)
                this.didDequeueAtIndex(item, this.Collection.Count - 1);
			return item;
		}

		public override void EnqueueAll ()
		{
			for (int i = this.Collection.Count - 1; i >= 0; --i)
				Enqueue(this.Collection[i]);
		}
		#endregion
	}
}