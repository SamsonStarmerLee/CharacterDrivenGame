using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    using static Utility;

    public static class PathFinder
    {
        const int WallCost = 100;

        public static List<Vector2Int> GenerateFullDepth(
            Vector2Int origin, 
            Vector2Int goal, 
            int range, 
            IReadOnlyList<IOccupant> ignore,
            IMovementCallbacks movement)
        {
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var costs = new Dictionary<Vector2Int, int>();
            var distFromOrigin = new Dictionary<Vector2Int, int>();
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
                    var cost = costs[current] + movement.GetCost(next, ignore);

                    if (distOrigin > range)
                    {
                        continue;
                    }

                    if (!costs.ContainsKey(next) || cost < costs[next])
                    {
                        cameFrom[next] = current;
                        costs[next] = cost;
                        distFromOrigin[next] = distOrigin;

                        var estDistToGoal = ManhattanDist(next, goal);

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
                var path = new List<Vector2Int>();
                var reachedGoal = distFromOrigin.ContainsKey(goal);
                var position = reachedGoal ? goal : closest;

                while (
                    position != origin &&
                    cameFrom.TryGetValue(position, out Vector2Int previous))
                {
                    if (costs[position] < WallCost)
                    {
                        path.Add(position);
                    }

                    position = previous;
                }

                path.Add(origin);
                return path;
            }
        }

        public static List<Vector2Int> GenerateAStar(
            Vector2Int origin, 
            Vector2Int goal, 
            int range, 
            IReadOnlyList<IOccupant> ignore,
            IMovementCallbacks movement)
        {
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var costs = new Dictionary<Vector2Int, int>();
            var distFromOrigin = new Dictionary<Vector2Int, int>();
            var frontier = new SimplePriorityQueue<Vector2Int>();

            cameFrom[origin] = origin;
            costs[origin] = 0;
            distFromOrigin[origin] = 0;

            frontier.Enqueue(origin, 0);

            // In case we do not reach the goal, 
            // we want to record whichever place got closest.
            var shortestDistToGoal = int.MaxValue;
            var closest = origin;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current == goal)
                {
                    // Early exit 
                    break;
                }

                foreach (var next in GetNeighbours(current))
                {
                    var distOrigin = distFromOrigin[current] + 1;
                    var cost = costs[current] + movement.GetCost(next, ignore);

                    if (distOrigin > range)
                    {
                        continue;
                    }

                    if (!costs.ContainsKey(next) || cost < costs[next])
                    {
                        cameFrom[next] = current;
                        costs[next] = cost;
                        distFromOrigin[next] = distOrigin;

                        var estDistToGoal = ManhattanDist(next, goal);
                        var fScore = cost + estDistToGoal;
                        frontier.Enqueue(next, fScore);

                        if (estDistToGoal < shortestDistToGoal)
                        {
                            closest = next;
                            shortestDistToGoal = estDistToGoal;
                        }
                    }
                }
            }

            // Compose Path
            {
                var path = new List<Vector2Int>();
                var reachedGoal = distFromOrigin.ContainsKey(goal);
                var position = reachedGoal ? goal : closest;

                while (position != origin &&
                       cameFrom.TryGetValue(position, out Vector2Int previous))
                {
                    if (costs[position] < WallCost)
                    {
                        path.Add(position);
                    }

                    position = previous;
                }

                path.Reverse();
                return path;
            }
        }

        public static List<Vector2Int> GenerateAStarClosest(
            Vector2Int origin,
            IReadOnlyList<Vector2Int> goals,
            IReadOnlyList<IOccupant> ignore,
            int range,
            IMovementCallbacks movement)
        {
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var cost = new Dictionary<Vector2Int, int>();
            var estDistOrigin = new Dictionary<Vector2Int, int>();
            var frontier = new SimplePriorityQueue<Vector2Int>();

            foreach (var goal in goals)
            {
                cameFrom[goal] = goal;
                var dG = cost[goal] = 0;
                var dO = estDistOrigin[goal] = ManhattanDist(origin, goal);
                var f = dG + dO;
                frontier.Enqueue(goal, f);
            }

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                if (current == origin)
                {
                    // We reached the target.
                    break;
                }

                foreach (var next in GetNeighbours(current).Shuffle())
                {
                    var dG = cost[current] + movement.GetCost(next, ignore);
                    var dO = ManhattanDist(next, origin);

                    if (dO > range)
                    {
                        // Ignore movement outside sweep range.
                        continue;
                    }

                    if (!cost.ContainsKey(next) || dG < cost[next])
                    {
                        cameFrom[next] = current;
                        cost[next] = dG;
                        estDistOrigin[next] = dO;
                        
                        var f = dG + dO;
                        frontier.Enqueue(next, f);
                    }
                }
            }

            if (cameFrom.ContainsKey(origin))
            {
                var path = new List<Vector2Int>();
                var previous = origin;

                while (previous != cameFrom[previous])
                {
                    var position = cameFrom[previous];

                    path.Add(position);
                    previous = position;
                }

                return path;
            }

            return new List<Vector2Int>();
        }

        static List<Vector2Int> GetNeighbours(Vector2Int location) =>
            new List<Vector2Int>()
            {
                Vector2Int.up    + location,
                Vector2Int.down  + location,
                Vector2Int.left  + location,
                Vector2Int.right + location,
            };

        static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}
