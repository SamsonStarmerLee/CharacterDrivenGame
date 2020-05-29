using UnityEngine;

public class Shaker : MonoBehaviour
{
    /// <summary>
    /// The transform to shake.
    /// Defaults to self if left unassigned.
    /// </summary>
    [SerializeField]
    private Transform toShake;

    /// <summary>
    /// Maximum translation offset.
    /// </summary>
    [SerializeField, Min(0)]
    private Vector3 maxOffset;

    /// <summary>
    /// Maximum angular (euler) offsets.
    /// Ordered Yaw, Pitch, Roll.
    /// </summary>
    [SerializeField, Min(0)]
    private Vector3 maxRotation;

    /// <summary>
    /// <see cref="trauma"/> is taken to this power when applied.
    /// Higher values lead to smoother falloff.
    /// </summary>
    [SerializeField, Min(1)]
    private float traumaExponent = 2;

    /// <summary>
    /// How much trauma decays per second.
    /// <see cref="trauma"/> is in 0-1 range.
    /// </summary>
    [SerializeField, Min(0f)]
    private float traumaRecoveryPerSecond = 1f;

    /// <summary>
    /// The frequency of shaking. Higher values result in more violent shaking.
    /// Controls the frequency of the perlin noise function.
    /// </summary>
    [SerializeField, Min(1)]
    private float frequency = 25f;

    private float seed;
    private float trauma;

    private void Awake()
    {
        seed = Random.value;

        if (toShake == null)
        {
            toShake = transform;
        }
    }

    private void Update()
    {
        if (trauma == 0 || toShake == null)
        {
            return;
        }

        var shake = Mathf.Pow(trauma, traumaExponent);
        var time = Time.time * frequency;

        float GetPerlinNoiseZeroToOne(float s) =>
            Mathf.PerlinNoise(s, time) * 2f - 1f;

        var yaw = maxRotation.x * shake * GetPerlinNoiseZeroToOne(seed);
        var pitch = maxRotation.y * shake * GetPerlinNoiseZeroToOne(seed + 1);
        var roll = maxRotation.z * shake * GetPerlinNoiseZeroToOne(seed + 2);

        var offsetX = maxOffset.x * shake * GetPerlinNoiseZeroToOne(seed + 3);
        var offsetY = maxOffset.y * shake * GetPerlinNoiseZeroToOne(seed + 4);
        var offsetZ = maxOffset.z * shake * GetPerlinNoiseZeroToOne(seed + 5);

        toShake.localRotation = Quaternion.Euler(yaw, pitch, roll);
        toShake.localPosition = new Vector3(offsetX, offsetY, offsetZ);

        trauma = Mathf.Clamp01(trauma - traumaRecoveryPerSecond * Time.deltaTime);
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }
} 
