using Boo.Lang;
using UnityEngine;

namespace Assets.Scripts.Pooling
{
    [System.Serializable]
    public class LetterCount
    {
        [SerializeField]
        private string letters;

        [SerializeField]
        private int count;
    }

    [CreateAssetMenu]
    public class LetterDistribution : ScriptableObject
    {
        [SerializeField]
        private LetterCount[] distribution;
    }
}
