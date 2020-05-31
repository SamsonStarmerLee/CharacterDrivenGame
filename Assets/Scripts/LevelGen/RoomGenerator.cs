﻿using Assets.Scripts.Characters;
using Assets.Scripts.Notifications;
using Assets.Scripts.Pooling;
using Assets.Scripts.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.LevelGen
{
    public class RoomGenerator : MonoBehaviour
    {
        private class Room
        {
            public GameObject GameObject;
            public GameObject[] Tiles = new GameObject[100];
        }

        public const string PlayerSpawnedNotification = "PlayerSpawned.Notification";

        private const int roomWidth = 10;
        private const int roomHeight = 10;

        [SerializeField]
        private Pooler pooler;

        [SerializeField]
        private GameObject playerPrefab;

        private List<string> roomTemplates = new List<string>();
        private List<string> entryTemplates = new List<string>();
        private List<string> exitTemplates = new List<string>();
        private Dictionary<Vector2Int, Room> rooms = new Dictionary<Vector2Int, Room>();

        private void Awake()
        {
            LoadTemplates("r", roomTemplates); 
            LoadTemplates("e", entryTemplates);
            LoadTemplates("x", exitTemplates);
        }

        public void Generate()
        {
            if (rooms.Count == 0)
            {
                GenerateDungeon(2, 2);
            }
            else
            {
                foreach (var vkp in rooms)
                {
                    Destroy(vkp.Value.GameObject);
                }

                rooms.Clear();
                Board.Instance.ClearFloorReferences();
            }

            // TEMP
            var spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
            var spawned = new List<Transform>();

            var vowels = new[]{ 'A', 'E', 'I', 'O', 'U' };
            Utility.Shuffle(vowels);

            for (var i = 0; i < 3; i++)
            {
                var character = Instantiate(
                    playerPrefab,
                    spawnPoints[i].transform.position,
                    Quaternion.identity);

                var aWar = character.GetComponent<AWarrior>();
                aWar.SetLetter(vowels[i]);

                spawned.Add(character.transform);
            }

            var tf = spawned[0];
            this.PostNotification(PlayerSpawnedNotification, tf);
        }

        private void GenerateDungeon(int width, int height)
        {
            var oX = (width  / 2 * roomWidth)  + (roomWidth  / 2);
            var oY = (height / 2 * roomHeight) + (roomHeight / 2);
            var offset = new Vector2Int(oX, oY);

            var ex = UnityEngine.Random.Range(0, width);
            var ey = UnityEngine.Random.Range(0, height);
            
            if (UnityEngine.Random.Range(0, 2) == 0) { ex = 0; }
            else { ey = 0; }

            // TODO: Static in C#8
            int Flip(int num, int min, int max) => (max + min) - num;

            var xx = Flip(ex, 0, width - 1);
            var xy = Flip(ey, 0, height - 1);

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

        private Room CreateEmptyRoom(string name)
        {
            var roomObject = new GameObject();
            roomObject.transform.parent = transform;
            roomObject.name = name;

            var room = new Room();
            room.GameObject = roomObject;
            return room;
        }

        private Room GenerateRoom(char[] template, Vector2Int atPosition)
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
                var boardPos = new Vector2Int(x, y);

                // Spawn floor tiles 
                // Except under walls or exit/entry
                if (key != '#' && key != 'E' && key != 'X')
                {
                    var obj = CreateTile(room, x, y, '0');
                    var tile = obj.gameObject.GetComponent<FloorTile>();
                    tile.SetOverlay(Overlay.None);
                    Board.Instance.SetFloorTile(tile, boardPos);
                }
                
                if (Board.Instance.GetAtPosition(boardPos, Board.OccupantType.Any) != null)
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

        private Room GenerateBorderRoom(Vector2Int atPosition)
        {
            var room = CreateEmptyRoom($"Room {atPosition}");

            for (var i = 0; i < roomWidth * roomHeight; i++)
            {
                var x = i % roomWidth + atPosition.x;
                var y = i / roomHeight + atPosition.y;

                if (Board.Instance.GetAtPosition(new Vector2Int(x, y), Board.OccupantType.Any) != null)
                {
                    continue;
                }

                const char key = '#';
                var tile = CreateTile(room, x, y, key);
                room.Tiles[i] = tile.gameObject;
            }

            return room;
        }

        private GameObject CreateTile(Room room, int x, int y, char key)
        {
            var position = new Vector3(x, 0f, y);
            var parent = room.GameObject.transform;
            var tile = pooler.Get(key, position, Quaternion.identity, parent);
            return tile.gameObject;
        }

        private static void LoadTemplates(string prefix, List<string> templates)
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
    }
}
