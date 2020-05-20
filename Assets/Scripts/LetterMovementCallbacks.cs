using Assets.Scripts;
using Assets.Scripts.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class LetterMovementCallbacks : IMovementCallbacks
{
    private const int WallCost = 100;

    public int GetCost(Vector2Int destination, IReadOnlyList<IOccupant> ignore)
    {
        var occupant = Board.Instance.GetAtPosition(destination, Board.OccupantType.Entity);
        if (occupant != null && !ignore.Contains(occupant))
        {
            return WallCost;
        }

        return 1;
    }
}
