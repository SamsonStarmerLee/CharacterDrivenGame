using UnityEngine;

namespace Assets.Scripts
{
    public interface IScorer
    {
        Vector2Int BoardPosition { get; set; }
    }
}
