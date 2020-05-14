using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pooling
{
    [CreateAssetMenu]
    public class Pooler : ScriptableObject
    {
        [SerializeField]
        Poolable[] prefabs;

        List<Poolable>[] pools;

        public Poolable Get(int id)
        {
            if (pools == null)
            {
                CreatePools();
            }

            Poolable instance;
            var pool = pools[id];
            var lastIndex = pool.Count - 1;

            if (lastIndex >= 0)
            {
                instance = pool[lastIndex];
                instance.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
            else
            {
                instance = Instantiate(prefabs[id]).GetComponent<Poolable>();
                instance.Id = id;
            }

            return instance;
        }

        public void Reclaim(Poolable poolable)
        {
            if (pools == null)
            {
                CreatePools();
            }

            pools[poolable.Id].Add(poolable);
            poolable.transform.parent = null;
            poolable.gameObject.SetActive(false);
        }

        void CreatePools()
        {
            pools = new List<Poolable>[prefabs.Length];
            for (int i = 0; i < pools.Length; i++)
            {
                pools[i] = new List<Poolable>();
            }
        }
    }
}
