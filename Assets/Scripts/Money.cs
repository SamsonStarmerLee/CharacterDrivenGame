using System.Linq;
using UnityEngine;

public class Money : Entity
{
    protected static char[] Currencies = "YSCEL".ToCharArray();

    [SerializeField]
    private Mesh[] currencyMeshes;

    [SerializeField]
    private float randomRotation;

    public override void Init()
    {
        base.Init();

        _letter = Currencies[Random.Range(0, Currencies.Length)].ToString();
        string meshName;

        switch(Letter)
        {
            // Yen
            case "Y":
                meshName = "Yen";
                break;

            // Dollars
            case "S":
                meshName = "Dollar";
                break;

            // Cents
            case "C":
                meshName = "Cent";
                break;

            // Euro
            case "E":
                meshName = "Euro";
                break;

            // Pounds
            case "L":
                meshName = "Pound";
                break;

            default:
                Debug.LogError("Money was given non-currency letter");
                return;
        }

        var mesh = currencyMeshes.First(x => x.name == $"Symbol_{meshName}");
        var modelObj = transform.Find("Model");
        var meshFilter = modelObj.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        // Shrink and add random rotation to model.
        modelObj.localScale *= Random.Range(0.7f, 0.9f);
        modelObj.rotation *= Quaternion.Euler(0f, 0f, Random.Range(-randomRotation, randomRotation));
    }
}
