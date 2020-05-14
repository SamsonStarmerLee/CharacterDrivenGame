using Assets.Scripts.Pooling;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.LevelGen
{
    [Serializable]
    class CharGameObjectDictionary : SerializableDictionaryBase<char, GameObject> { }

    class Room
    {
        public GameObject GameObject;
        public GameObject[] Tiles = new GameObject[100];
    }

    public class RoomGenerator : MonoBehaviour
    {
        const int roomWidth = 10;
        const int roomHeight = 10;

        [SerializeField]
        CharGameObjectDictionary legend = new CharGameObjectDictionary();

        [SerializeField]
        Pooler pooler;

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
                if (rooms.Count == 0)
                {
                    for (var x = 0; x < 10; x++)
                    {
                        for (var y = 0; y < 10; y++)
                        {
                            var pos = new Vector2Int(x * roomWidth, y * roomHeight) - new Vector2Int(50, 50);
                            var room = SpawnRoom(pos);
                            rooms.Add(pos, room);
                        }
                    }
                }
                else
                {
                    // TEMP
                    for (var x = 0; x < 10; x++)
                    {
                        for (var y = 0; y < 10; y++)
                        {
                            var pos = new Vector2Int(x * roomWidth, y * roomHeight) - new Vector2Int(50, 50);
                            var room = rooms[pos];
                            var tiles = room.Tiles.Where(t => t != null);

                            foreach (var tile in tiles)
                            {
                                var poolable = tile.GetComponent<Poolable>();
                                if (poolable)
                                {
                                    pooler.Reclaim(poolable);
                                }
                            }

                            Destroy(room.GameObject);
                        }
                    }
                    rooms.Clear();
                    // TEMP
                }
            }
        }

        Room SpawnRoom(Vector2Int atPosition)
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

            var board = Board.Instance;

            var roomObject = new GameObject();
            roomObject.transform.parent = transform;
            roomObject.name = $"Room {atPosition}";

            var room = new Room();
            room.GameObject = roomObject;

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
                var poolable = tile.GetComponent<Poolable>();
                var obj = pooler.Get(poolable.Id);
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.identity;
                obj.transform.parent = roomObject.transform;

                room.Tiles[i] = obj.gameObject;
            }

            return room;
        }
    }
}
