using Priority_Queue;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.PathDrawing
{
    public partial class DragToMoveController : MonoBehaviour
    {
        [SerializeField]
        LayerMask layerMask;

        StateMachine machine = new StateMachine();
        
        PathFinder pathFinder = new PathFinder();

        List<DragMovement> movement = new List<DragMovement>();

        private void Awake()
        {
            var idleState = new IdleState();
            idleState.Owner = this;
            machine.ChangeState(idleState);
        }

        void Update()
        {
            DrawMovement();

            machine.Execute();
        }

        void DrawMovement()
        {
            foreach (var move in movement)
            {
                DrawPath(move.Path, Color.grey);
            }
        }

        static void DrawPath(IReadOnlyList<Vector2Int> path, Color color)
        {
            for (var i = 0; i < path.Count - 1; i++)
            {
                var p0 = path[i];
                var p1 = path[i + 1];
                var v0 = new Vector3(p0.x, 0f, p0.y);
                var v1 = new Vector3(p1.x, 0f, p1.y);
                Debug.DrawLine(v0, v1, color);
            }
        }
    }

    class DragMovement
    {
        public Character Character;
        public List<Vector2Int> Path;

        public Vector2Int From => Path[Path.Count - 1];

        public Vector2Int To => Path[0];
    }

    class PathFinder
    {
        const int WallCost = 1000;

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> costs = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> distFromOrigin = new Dictionary<Vector2Int, int>();
        
        public List<Vector2Int> Path = new List<Vector2Int>();

        public bool HasPath => Path != null && Path.Count != 0;

        public Vector2Int Terminus => Path[0];

        public void Generate(Vector2Int origin, Vector2Int goal, int range, IReadOnlyList<Entity> ignore)
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

                while(
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

        public static bool GetMousePosition(LayerMask layerMask, out Vector2Int position, out Vector3 position3d)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, layerMask))
            {
                position = default;
                position3d = default;
                return false;
            }

            position3d = hit.point;
            var x = Mathf.RoundToInt(hit.point.x);
            var y = Mathf.RoundToInt(hit.point.z);
            position = new Vector2Int(x, y);
            return true;
        }

        static int ManhattanDistance(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        static int Cost(Vector2Int position, IReadOnlyList<Entity> ignore)
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