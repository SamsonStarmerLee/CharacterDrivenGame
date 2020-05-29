using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Visuals
{
    public class MeshRandomizer : MonoBehaviour
    {
        [SerializeField]
        private List<Mesh> meshes;

        private void Start()
        {
            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh = meshes[Random.Range(0, meshes.Count)];
        }
    }
}
