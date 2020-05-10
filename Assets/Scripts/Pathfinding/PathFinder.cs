using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    using static Utility;

    public class PathFinder
    {
        const int WallCost = 1000;

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> costs = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> distFromOrigin = new Dictionary<Vector2Int, int>();

        public List<Vector2Int> Path = new List<Vector2Int>();

        public bool HasPath => Path != null && Path.Count != 0;

        public Vector2Int Terminus => Path[0];

        public void Generate(Vector2Int origin, Vector2Int goal, int range, IReadOnlyList<IOccupant> ignore)
        {
            cameFrom.Clear();
            costs.Clear();
            distFromOrigin.Clear();
            Path = new List<Vector2Int>();

            var frontier = new SimplePriorityQueue<Vector2Int>();
            frontier.Enqueue(origin, 0);

            cameFrom[origin] = origin;
            costs[origin] = 0;
            distFromOrigin[origin] = 0;

            // In case we do not reach the goal, we want to record whichever place got closest.
            var shortestDistToGoal = int.MaxValue;
            var closest = origin;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                foreach (var next in GetNeighbours(current))
                {
                    var distOrigin = distFromOrigin[current] + 1;
                    var cost = costs[current] + Cost(next, ignore);

                    if (distOrigin > range)
                    {
                        continue;
                    }

                    if (!costs.ContainsKey(next) || cost < costs[next])
                    {
                        cameFrom[next] = current;
                        costs[next] = cost;
                        distFromOrigin[next] = distOrigin;

                        var estDistToGoal = ManhattanDistance(next, goal);

                        var fScore = cost + estDistToGoal;
                        frontier.Enqueue(next, fScore);

                        if (estDistToGoal < shortestDistToGoal)
                        {
                            closest = next;
                            shortestDistToGoal = estDistToGoal;
                        }
                    }
                }

                // TEMP: Show all reachable squares
                if (costs[current] < WallCost)
                {
                    var pos = new Vector3(current.x, 0f, current.y);
                    Debug.DrawRay(pos, Vector3.up * 1f, Color.black);
                }
            }

            // Compose Path
            {
                var reachedGoal = distFromOrigin.ContainsKey(goal);
                var position = reachedGoal ? goal : closest;

                while (
                    position != origin &&
                    cameFrom.TryGetValue(position, out Vector2Int previous))
                {
                    if (costs[position] < WallCost)
                    {
                        Path.Add(position);
                    }

                    position = previous;
                }

                Path.Add(origin);
            }
        }

        static int Cost(Vector2Int position, IReadOnlyList<IOccupant> ignore)
        {
            var occupant = Board.Instance.GetAtPosition(position);
            if (occupant != null && !ignore.Contains(occupant))
            {
                return WallCost;
            }

            return 1;
        }

        static List<Vector2Int> GetNeighbours(Vector2Int location)
        {
            return new List<Vector2Int>()
            {
                Vector2Int.up    + location,
                Vector2Int.down  + location,
                Vector2Int.left  + location,
                Vector2Int.right + location,
            };
        }
    }
}
