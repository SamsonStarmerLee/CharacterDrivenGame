using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public interface IMovementCallbacks
    {
        int GetCost(Vector2Int destination, IReadOnlyList<IOccupant> ignore);
    }
}
