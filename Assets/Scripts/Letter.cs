using System.Linq;
using UnityEngine;

[SelectionBase]
public class Letter : Entity
{
    protected static char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    protected static char[] Consonants = "BCDFGHJKLMNPQRSTVWXYZ".ToCharArray();
    protected static char[] Vowels = "AEIOU".ToCharArray();

    [SerializeField]
    protected Mesh[] meshAlphabet;

    public override void Init()
    {
        base.Init();

        // This sucks
        if (string.IsNullOrWhiteSpace(Letter))
        {
            _letter = Alphabet[Random.Range(0, Consonants.Length)].ToString();
            name = _letter;
            var mesh = meshAlphabet.First(x => x.name == $"{Letter}_Upper");
            var meshFilter = GetComponentInChildren<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        }
    }
}
