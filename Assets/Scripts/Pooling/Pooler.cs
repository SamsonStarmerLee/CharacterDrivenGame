using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using System;

namespace Assets.Scripts.Pooling
{
    [Serializable]
    internal class CharGameObjectDictionary : SerializableDictionaryBase<char, GameObject> { }

    [CreateAssetMenu]
    public class Pooler : ScriptableObject
    {
        [SerializeField]
        private CharGameObjectDictionary prefabs;

        private Dictionary<char, List<Poolable>> pools = new Dictionary<char, List<Poolable>>();

        public Poolable Get(char id, Vector3 position, Quaternion rotation, Transform parent)
        {
            // TODO: Re-enable pooling.
            return Instantiate(prefabs[id], position, rotation, parent).GetComponent<Poolable>();

            if (!prefabs.ContainsKey(id))
            {
                Debug.LogError($"Tried to spawn poolable item with unrecognized ID: {id}");
                return null;
            }

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
                instance.gameObject.transform.position = position;
                instance.gameObject.transform.rotation = rotation;
                instance.gameObject.transform.parent = parent;
                instance.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
            else
            {
                instance = Instantiate(prefabs[id], position, rotation, parent).GetComponent<Poolable>();
                instance.Id = id;
            }

            return instance;
        }

        public void Reclaim(Poolable poolable)
        {
            // TODO: Re-enable pooling.
            Destroy(poolable.gameObject);
            return;

            var id = poolable.Id;
            if (!pools.ContainsKey(id))
            {
                CreatePool(id);
            }

            pools[poolable.Id].Add(poolable);
            poolable.transform.parent = null;
            poolable.gameObject.SetActive(false);
        }

        public void Clear()
        {
            pools.Clear();
        }

        private void CreatePool(char id)
        {
            pools.Add(id, new List<Poolable>());
        }
    }
}
