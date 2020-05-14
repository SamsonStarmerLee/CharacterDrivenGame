using UnityEngine;
using System;

namespace Pooling
{
    public abstract class BasePooler : MonoBehaviour
	{
		#region Events
		public Action<Poolable> willEnqueue;
		public Action<Poolable> didDequeue;
		#endregion

		#region Fields / Properties
		public string key = string.Empty;
		public GameObject prefab = null;
		public int prepopulate = 0;
		public int maxCount = int.MaxValue;
		public bool autoRegister = true;
		public bool autoClear = true;
		public bool isRegistered { get; private set; }
		#endregion

		#region MonoBehaviour
		protected virtual void Awake ()
		{
			if (this.autoRegister)
				Register();
		}

		protected virtual void OnDestroy ()
		{
			EnqueueAll();
			if (this.autoClear)
				UnRegister();
		}

		protected virtual void OnApplicationQuit()
		{
			EnqueueAll();
		}
		#endregion

		#region Public
		public void Register ()
		{
			if (string.IsNullOrEmpty(this.key))
                this.key = this.prefab.name;
			GameObjectPoolController.AddEntry(this.key, this.prefab, this.prepopulate, this.maxCount);
            this.isRegistered = true;
		}

		public void UnRegister ()
		{
			GameObjectPoolController.ClearEntry(this.key);
            this.isRegistered = false;
		}

		public virtual void Enqueue (Poolable item)
		{
			if (this.willEnqueue != null)
                this.willEnqueue(item);
			GameObjectPoolController.Enqueue(item);
		}

		public virtual void EnqueueObject (GameObject obj)
		{
			Poolable item = obj.GetComponent<Poolable>();
			if (item != null)
				Enqueue (item);
		}

		public virtual void EnqueueScript (MonoBehaviour script)
		{
			Poolable item = script.GetComponent<Poolable>();
			if (item != null)
				Enqueue (item);
		}

		public virtual Poolable Dequeue ()
		{
			Poolable item = GameObjectPoolController.Dequeue(this.key);
			if (this.didDequeue != null)
                this.didDequeue(item);
			return item;
		}

		public virtual U DequeueScript<U> () where U : MonoBehaviour
		{
			Poolable item = Dequeue();
			return item.GetComponent<U>();
		}

		public abstract void EnqueueAll ();
		#endregion
	}
}