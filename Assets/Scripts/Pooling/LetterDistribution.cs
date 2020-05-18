using Boo.Lang;
using UnityEngine;

namespace Assets.Scripts.Pooling
{
    [System.Serializable]
    public class LetterCount
    {
        [SerializeField]
        string letters;

        [SerializeField]
        int count;
    }

    [CreateAssetMenu]
    public class LetterDistribution : ScriptableObject
    {
        [SerializeField]
        LetterCount[] distribution;
    }
}
