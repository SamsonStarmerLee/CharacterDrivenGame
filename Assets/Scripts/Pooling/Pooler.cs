using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using System;

namespace Assets.Scripts.Pooling
{
    [Serializable]
    class CharGameObjectDictionary : SerializableDictionaryBase<char, GameObject> { }

    [CreateAssetMenu]
    public class Pooler : ScriptableObject
    {
        [SerializeField]
        CharGameObjectDictionary prefabs;

        Dictionary<char, List<Poolable>> pools = new Dictionary<char, List<Poolable>>();

        public Poolable Get(char id)
        {
            if (!pools.ContainsKey(id))
            {
                CreatePool(id);
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
            var id = poolable.Id;
            if (!pools.ContainsKey(id))
            {
                CreatePool(id);
            }

            pools[poolable.Id].Add(poolable);
            poolable.transform.parent = null;
            poolable.gameObject.SetActive(false);
        }

        void CreatePool(char id)
        {
            pools.Add(id, new List<Poolable>());
        }
    }
}
