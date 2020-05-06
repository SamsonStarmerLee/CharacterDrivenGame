using Assets.Scripts;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class Character : Entity, IScorer
{
    protected static char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    [SerializeField]
    protected Mesh[] meshAlphabet;

    public override void Init()
    {
        base.Init();

        // This sucks
        _letter = Alphabet[Random.Range(0, Alphabet.Length)].ToString();
        name = _letter;
        var @case = Random.Range(0, 2) == 0 ? "Upper" : "Lower";
        var mesh = meshAlphabet.First(x => x.name == $"{Letter}_{@case}");
        var meshFilter = GetComponentInChildren<MeshFilter>();
        meshFilter.sharedMesh = mesh;
    }
}
