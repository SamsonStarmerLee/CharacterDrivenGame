using Assets.Scripts.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.LevelGen
{
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
        Pooler pooler;

        List<string> roomTemplates = new List<string>();
        List<string> entryTemplates = new List<string>();
        List<string> exitTemplates = new List<string>();

        Dictionary<Vector2Int, Room> rooms = new Dictionary<Vector2Int, Room>();

        void Awake()
        {
            LoadTemplates("r", roomTemplates); 
            LoadTemplates("e", entryTemplates);
            LoadTemplates("x", exitTemplates);
        }

        private void Start()
        {
            GenerateDungeon(3, 3);
        }

        static void LoadTemplates(string prefix, List<string> templates)
        {
            var i = 0;
            while (true)
            {
                var r = Resources.Load($"Rooms/{prefix}{i}") as TextAsset;
                if (r == null) break;

                var room = r.text.Replace("\n", "").Replace("\r", "");
                templates.Add(room);
                i++;
            }
        }

        private void GenerateDungeon(int width, int height)
        {
            if (width % 2 == 0 || height % 2 == 0)
            {
                Debug.LogError("GenerateDungeon width and height must be odd.");
                return;
            }

            width = 5;
            height = 5;

            var oX = (width  / 2 * roomWidth)  + (roomWidth  / 2);
            var oY = (height / 2 * roomHeight) + (roomHeight / 2);
            var offset = new Vector2Int(oX, oY);

            // TODO: Static in C#8
            int RoundTo(float num, int roundTo)
            {
                return roundTo * (int)Math.Round((float)num / (float)roundTo);
            }

            var ex = width / 2;
            var ey = height / 2;
            var xx = RoundTo(Mathf.Round(UnityEngine.Random.Range(0, width)),  width - 1);
            var xy = RoundTo(Mathf.Round(UnityEngine.Random.Range(0, height)), height - 1);

            // TODO: Static in C#8
            char[] GetTemplate(List<string> templatePool)
            {
                var index = UnityEngine.Random.Range(0, templatePool.Count);
                return templatePool[index].ToArray();
            }

            for (var x = -1; x <= width; x++)
            {
                for (var y = -1; y <= height; y++)
                {
                    var pos = new Vector2Int(x * roomWidth, y * roomHeight) - offset;

                    if (x == ex && y == ey)
                    {
                        // Spawn entry room
                        var t = GetTemplate(entryTemplates);
                        var room = GenerateRoom(t, pos);
                        rooms.Add(pos, room);
                    }
                    else if (x == xx && y == xy)
                    {
                        // Spawn Exit Room
                        var t = GetTemplate(exitTemplates);
                        var room = GenerateRoom(t, pos);
                        rooms.Add(pos, room);
                    }
                    else if (x == -1 || y == -1 || x == width || y == height)
                    {
                        // Outer rooms are solid walls.
                        var room = GenerateBorderRoom(pos);
                        rooms.Add(pos, room);
                    }
                    else
                    {
                        // Spawn normal room
                        var t = GetTemplate(roomTemplates);
                        var room = GenerateRoom(t, pos);
                        rooms.Add(pos, room);
                    }
                }
            }
        }

        Room CreateEmptyRoom(string name)
        {
            var roomObject = new GameObject();
            roomObject.transform.parent = transform;
            roomObject.name = name;

            var room = new Room();
            room.GameObject = roomObject;
            return room;
        }

        Room GenerateRoom(char[] template, Vector2Int atPosition)
        {
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

            var room = CreateEmptyRoom($"Entry {atPosition}");

            for (var i = 0; i < template.Length; i++)
            {
                var x = i % roomWidth + atPosition.x;
                var y = i / roomHeight + atPosition.y;
                var key = template[i];

                // Spawn floor tiles 
                // Except under walls or exit/entry
                if (key != '#' && key != 'E' && key != 'X')
                {
                    CreateTile(room, x, y, '0');
                }
                
                if (Board.Instance.GetAtPosition(new Vector2Int(x, y)) != null)
                {
                    continue;
                }

                if (key != '0')
                {
                    var tile = CreateTile(room, x, y, key);
                    room.Tiles[i] = tile.gameObject;
                }
            }

            return room;
        }

        Room GenerateBorderRoom(Vector2Int atPosition)
        {
            var room = CreateEmptyRoom($"Room {atPosition}");

            for (var i = 0; i < roomWidth * roomHeight; i++)
            {
                var x = i % roomWidth + atPosition.x;
                var y = i / roomHeight + atPosition.y;

                if (Board.Instance.GetAtPosition(new Vector2Int(x, y)) != null)
                {
                    continue;
                }

                const char key = '#';
                var tile = CreateTile(room, x, y, key);
                room.Tiles[i] = tile.gameObject;
            }

            return room;
        }

        GameObject CreateTile(Room room, int x, int y, char key)
        {
            var tile = pooler.Get(key);
            var position = new Vector3(x, 0f, y);
            tile.transform.position = position;
            tile.transform.rotation = Quaternion.identity;
            tile.transform.parent = room.GameObject.transform;
            return tile.gameObject;
        }
    }
}
