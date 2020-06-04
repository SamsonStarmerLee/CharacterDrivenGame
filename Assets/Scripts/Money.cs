using Assets.Scripts.Actions;
using Assets.Scripts.Notifications;
using System.Linq;
using UnityEngine;

public class Money : Entity
{
    const int value = 1;

    protected static char[] Currencies = "YSCEL".ToCharArray();

    [SerializeField]
    private Mesh[] currencyMeshes;

    [SerializeField]
    private float randomRotation;

    [SerializeField]
    private AudioClip[] pickupSfx;

    [SerializeField]
    private AudioClip[] touchSfx;

    private AudioSource audioSource;

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
        modelObj.localScale *= Random.Range(0.5f, 0.7f);
        modelObj.rotation *= Quaternion.Euler(0f, Random.Range(-randomRotation, randomRotation), 0f);

        audioSource = GetComponent<AudioSource>();
    }

    public override void Destroy()
    {
        // TEMP? This should probably not be inside destroy.
        this.PostNotification(
            Notify.Action<ScoreAction>(), 
            new ScoreAction(value, null));

        // Play pickup sound effect
        var sfx = pickupSfx[Random.Range(0, pickupSfx.Length)];
        audioSource.PlayOneShot(sfx);

        base.Destroy();
    }

    public void Touch()
    {
        var sfx = touchSfx[Random.Range(0, touchSfx.Length)];
        audioSource.PlayOneShot(sfx);
    }
}
