using UnityEngine;

namespace Assets.Scripts
{
    public static class Utility
    {
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

        public static int ManhattanDistance(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
