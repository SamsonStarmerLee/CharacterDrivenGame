using UnityEngine;
using System;
using System.Collections.Generic;

namespace Pooling
{
    public abstract class KeyedPooler<T> : BasePooler
	{
		#region Events
		public Action<Poolable, T> willEnqueueForKey;
		public Action<Poolable, T> didDequeueForKey;
		#endregion

		#region Fields / Properties
		public Dictionary<T, Poolable> Collection = new Dictionary<T, Poolable>();
		#endregion

		#region Public
		public bool HasKey (T key)
		{
			return this.Collection.ContainsKey(key);
		}

		public Poolable GetItem (T key)
		{
			if (this.Collection.ContainsKey(key))
				return this.Collection[key];
			return null;
		}

		public U GetScript<U> (T key) where U : MonoBehaviour
		{
			Poolable item = GetItem(key);
			if (item != null)
				return item.GetComponent<U>();
			return null;
		}

		public virtual void EnqueueByKey (T key)
		{
			Poolable item = GetItem(key);
			if (item != null)
			{
				if (this.willEnqueueForKey != null)
                    this.willEnqueueForKey(item, key);
				Enqueue(item);
                this.Collection.Remove(key);
			}
		}

		public virtual Poolable DequeueByKey (T key)
		{
			if (this.Collection.ContainsKey(key))
				return this.Collection[key];

			Poolable item = Dequeue();
            this.Collection.Add(key, item);
			if (this.didDequeueForKey != null)
                this.didDequeueForKey(item, key);
			return item;
		}

		public virtual U DequeueScriptByKey<U> (T key) where U : MonoBehaviour
		{
			Poolable item = DequeueByKey(key);
			return item.GetComponent<U>();
		}

		public override void EnqueueAll ()
		{
			T[] keys = new T[this.Collection.Count];
            this.Collection.Keys.CopyTo(keys, 0);
			for (int i = 0; i < keys.Length; ++i)
				EnqueueByKey(keys[i]);
		}
		#endregion
	}
}