using Assets.Scripts.LevelGen;
using UnityEngine;

namespace Assets.Scripts.Visuals
{
	[System.Serializable]
	public class MeshItem : PieChanceItem<Mesh> { }

	[System.Serializable]
	public class MeshTable : PieChanceTable<MeshItem, Mesh> { }

	public class MeshRandomizer : MonoBehaviour
    {
        public MeshTable table;

        private void Start()
        {
            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh = table.PickItem();
        }

        private void OnValidate()
        {
            table.ValidateTable();
        }
    }
}
