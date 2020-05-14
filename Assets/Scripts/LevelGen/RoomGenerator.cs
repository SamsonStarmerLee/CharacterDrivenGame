using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
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
                var pos = new Vector2Int(10, 10);
                var chunk = SpawnChunk(pos);
                rooms.Add(pos, chunk);
            }
        }

        Room SpawnChunk(Vector2Int atPosition)
        {
            var template = roomTemplates[0];
            if (template.Length != 100)
            {
                Debug.LogError("Room template was the wrong size.");
                return null;
            }

            var room = new Room();

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
                var position = new Vector3(x, 0f, y);

                var obj = Instantiate(tile, position, Quaternion.identity);
                room.Tiles[i] = obj;
            }

            return room;
        }
    }
}
