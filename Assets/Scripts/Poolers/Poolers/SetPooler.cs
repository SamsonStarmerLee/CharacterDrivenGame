using Pooling;
using System.Collections.Generic;

namespace Assets.Scripts.View
{
    public class SetPooler : BasePooler
    {
        #region Fields / Properties
        public HashSet<Poolable> Collection = new HashSet<Poolable>();
        #endregion

        #region Public
        public override void Enqueue(Poolable item)
        {
            base.Enqueue(item);
            if (this.Collection.Contains(item))
            {
                this.Collection.Remove(item);
            }
        }

        public override Poolable Dequeue()
        {
            Poolable item = base.Dequeue();
            this.Collection.Add(item);
            return item;
        }

        public override void EnqueueAll()
        {
            foreach (Poolable item in this.Collection)
            {
                base.Enqueue(item);
            }

            this.Collection.Clear();
        }
        #endregion
    }
}