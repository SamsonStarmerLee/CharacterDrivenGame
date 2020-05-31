using System.Collections.Generic;
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

        public static int ManhattanDist(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        /// <summary>
        /// Allows KeyValuePair deconstruction (in C#7).
        /// </summary>
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }

        public static void Shuffle<T>(T[] array)
        {
            var p = array.Length;
            for (var n = p - 1; n > 0; n--)
            {
                var r = Random.Range(1, n);
                T t = array[r];
                array[r] = array[n];
                array[n] = t;
            }
        }
    }
}
