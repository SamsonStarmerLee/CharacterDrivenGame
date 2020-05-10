using UnityEngine;

namespace Assets.Scripts
{
    public interface IOccupant : IDestroy
    {
        Vector3 WorldPosition { get; set; }

        Vector2Int BoardPosition { get; set; }

        string Letter { get; }
    }
}