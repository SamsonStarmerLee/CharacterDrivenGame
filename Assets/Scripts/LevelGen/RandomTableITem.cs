using UnityEngine;

namespace Assets.Scripts.LevelGen
{
    public abstract class RandomTableItem<T>
    {
        [SerializeField]
        private T item;

        [SerializeField]
        private float probabilityWeight;
    }
}
