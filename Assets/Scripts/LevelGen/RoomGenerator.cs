using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.LevelGen
{
    [System.Serializable]
    class CharGameObjectDictionary : SerializableDictionaryBase<char, GameObject> { }

    class Room
    {
        public GameObject[] Tiles = new GameObject[100];
    }

    public class RoomGenerator : MonoBehaviour
    {
        const int roomWidth = 10;
        const int roomHeight = 10;

        [SerializeField]
        CharGameObjectDictionary legend = new CharGameObjectDictionary();

        List<string> roomTemplates = new List<string>();

        Dictionary<Vector2Int, Room> rooms = new Dictionary<Vector2Int, Room>();

        void Awake()
        {
            var i = 0;
            while(true)
            {
                var r = Resources.Load($"Rooms/r{i}") as TextAsset;
                if (r == null)
                {
                    break;
                }

                var room = r.text.Replace("\n", "").Replace("\r", "");
                roomTemplates.Add(room);
                i++;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                for (var x = 0; x < 10; x++)
                {
                    for (var y = 0; y < 10; y++)
                    {
                        var pos = new Vector2Int(x * roomWidth, y * roomHeight) - new Vector2Int(50, 50);
                        var chunk = SpawnChunk(pos);
                        rooms.Add(pos, chunk);
                    }
                }
            }
        }

        Room SpawnChunk(Vector2Int atPosition)
        {
            var template = roomTemplates[UnityEngine.Random.Range(0, roomTemplates.Count)]
                .ToArray();

            if (template.Length != roomHeight * roomWidth)
            {
                Debug.LogError("Room template was the wrong size.");
                return null;
            }

            if (UnityEngine.Random.Range(0, 2) == 1)
            {
                // 50% chance to flip any template we use vertically.
                Array.Reverse(template);
            }

            var room = new Room();
            var board = Board.Instance;

            for (var i = 0; i < template.Length; i++)
            {
                var key = template[i];
                if (key == '0' || !legend.ContainsKey(key))
                {
                    continue;
                }

                var tile = legend[key];
                var x = i % roomWidth + atPosition.x;
                var y = i / roomHeight + atPosition.y;

                if (board.GetAtPosition(new Vector2Int(x, y)) != null)
                {
                    continue;
                }

                var position = new Vector3(x, 0f, y);
                var obj = Instantiate(tile, position, Quaternion.identity);
                room.Tiles[i] = obj;
            }

            return room;
        }
    }
}
