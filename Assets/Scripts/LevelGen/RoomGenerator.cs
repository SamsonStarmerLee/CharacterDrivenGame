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

        private void Start()
        {
            GenerateDungeon(3, 3);
        }

        private void GenerateDungeon(int width, int height)
        {
            if (width % 2 == 0 || height % 2 == 0)
            {
                Debug.LogError("GenerateDungeon width and height must be odd.");
                return;
            }

            var oX = (width / 2 * roomWidth) + (roomWidth / 2);
            var oY = (height / 2 * roomHeight) + (roomHeight / 2);
            var offset = new Vector2Int(oX, oY);

            for (var x = 0; x < width + 2; x++)
            {
                for (var y = 0; y < height + 2; y++)
                {
                    var pos = new Vector2Int(x * roomWidth, y * roomHeight) - offset;

                    if (x == 0 || y == 0 || x == width + 1 || y == height + 1)
                    {
                        // Outer rooms are solid walls.
                        var room = GenerateBorderRoom(pos);
                        rooms.Add(pos, room);
                    }
                    else
                    {
                        var room = GenerateRoom(pos);
                        rooms.Add(pos, room);
                    }
                }
            }
        }

        Room GenerateRoom(Vector2Int atPosition)
        {
            var index = UnityEngine.Random.Range(0, roomTemplates.Count);
            var template = roomTemplates[index].ToArray();

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

        Room GenerateBorderRoom(Vector2Int atPosition)
        {
            var board = Board.Instance;

            var roomObject = new GameObject();
            roomObject.transform.parent = transform;
            roomObject.name = $"Room {atPosition}";

            var room = new Room();
            room.GameObject = roomObject;

            for (var i = 0; i < roomWidth * roomHeight; i++)
            {
                var x = i % roomWidth + atPosition.x;
                var y = i / roomHeight + atPosition.y;

                if (board.GetAtPosition(new Vector2Int(x, y)) != null)
                {
                    continue;
                }

                var position = new Vector3(x, 0f, y);
                var obj = pooler.Get(3);
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.identity;
                obj.transform.parent = roomObject.transform;

                room.Tiles[i] = obj.gameObject;
            }

            return room;
        }
    }
}
