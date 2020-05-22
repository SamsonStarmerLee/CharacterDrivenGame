using Assets.Scripts.Actions;
using Assets.Scripts.LevelGen;
using Assets.Scripts.Notifications;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public partial class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform focus;

        [SerializeField]
        private float scrollMargin = 1f;

        [SerializeField]
        private float scrollSpeed = 5f;

        /// <summary>
        /// Maximum angular/translation offsets for screenshake.
        /// </summary>
        [SerializeField]
        private float maxYaw, maxPitch, maxRoll, maxOffset;

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

        /// <summary>
        /// Amount of trauma added when the player is hit by an attack.
        /// </summary>
        [SerializeField, Min(0f)]
        private float traumaOnHit = 0.5f;

        private Vector3 focusPoint, viewPosition, focusOffset;

        private StateMachine machine = new StateMachine();

        private float seed;
        private float trauma;

        private Transform cameraTransform;

        private void Start()
        {
            viewPosition = transform.position;
            Cursor.lockState = CursorLockMode.Confined;

            seed = Random.value;
            cameraTransform = transform.Find("Camera");

            machine.ChangeState(new TrackingState(this));
        }

        private void OnEnable()
        {
            this.AddObserver(OnJumpToCharacter, CharacterController.JumpSelectNotification);
            this.AddObserver(OnJumpToCharacter, RoomGenerator.PlayerSpawnedNotification);
            this.AddObserver(OnDragStart, CharacterController.BeginDragNotification);
            this.AddObserver(OnPlayerDamaged, Notify.Action<DamagePlayerAction>());
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnJumpToCharacter, CharacterController.JumpSelectNotification);
            this.RemoveObserver(OnJumpToCharacter, RoomGenerator.PlayerSpawnedNotification);
            this.RemoveObserver(OnDragStart, CharacterController.BeginDragNotification);
            this.RemoveObserver(OnPlayerDamaged, Notify.Action<DamagePlayerAction>());
        }

        private void LateUpdate()
        {
            machine.Execute();

            var lookPosition = focusPoint + viewPosition + focusOffset;
            transform.position = lookPosition;

            CameraShake();
        }

        private void OnJumpToCharacter(object sender, object args)
        {
            var tf = args as Transform;
            focus = tf;
            focusPoint = focus.position;
            focusOffset = Vector3.zero;

            machine.ChangeState(new TrackingState(this));
        }

        private void OnDragStart(object sender, object args)
        {
            var tf = args as Transform;
            focus = tf;

            machine.ChangeState(new TrackingState(this));
        }

        private void OnPlayerDamaged(object sender, object args)
        {
            trauma += traumaOnHit;
        }

        private void CameraShake()
        {
            var shake = Mathf.Pow(trauma, traumaExponent);
            var time = Time.time * frequency;

            float GetPerlinNoiseZeroToOne(float s) =>
                Mathf.PerlinNoise(s, time) * 2f - 1f;

            var yaw   = maxYaw * shake * GetPerlinNoiseZeroToOne(seed);
            var pitch = maxPitch * shake * GetPerlinNoiseZeroToOne(seed + 1);
            var roll  = maxRoll * shake * GetPerlinNoiseZeroToOne(seed + 2);

            var offsetX = maxOffset * shake * GetPerlinNoiseZeroToOne(seed + 3);
            var offsetY = maxOffset * shake * GetPerlinNoiseZeroToOne(seed + 4);
            var offsetZ = maxOffset * shake * GetPerlinNoiseZeroToOne(seed + 5);

            cameraTransform.localRotation = Quaternion.Euler(yaw, pitch, roll);
            cameraTransform.localPosition = new Vector3(offsetX, offsetY, offsetZ);

            trauma = Mathf.Clamp01(trauma - traumaRecoveryPerSecond * Time.deltaTime);
        }
    }
}