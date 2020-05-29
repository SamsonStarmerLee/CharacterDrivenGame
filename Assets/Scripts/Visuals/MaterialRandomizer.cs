using Assets.Scripts.LevelGen;
using UnityEngine;

namespace Assets.Scripts.Visuals
{
    [System.Serializable]
    public class MaterialItem : PieChanceItem<Material> { }

    [System.Serializable]
    public class MaterialTable : PieChanceTable<MaterialItem, Material> { }

    public class MaterialRandomizer : MonoBehaviour
    {
        public MaterialTable table;

        private void Start()
        {
            var renderer = GetComponent<MeshRenderer>();
            renderer.material = table.PickItem();
        }

        private void OnValidate()
        {
            table.ValidateTable();
        }
    }
}
