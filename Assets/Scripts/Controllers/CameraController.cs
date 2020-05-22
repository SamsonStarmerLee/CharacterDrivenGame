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

        [SerializeField]
        private float maxYaw, maxPitch, maxRoll, maxOffset;

        [SerializeField, Min(1)]
        private float traumaExponent = 2;

        [SerializeField, Min(1)]
        private float frequency = 25f;

        private Vector3 focusPoint, viewPosition, focusOffset;

        private StateMachine machine = new StateMachine();

        private float seed;

        private void Start()
        {
            viewPosition = transform.position;
            Cursor.lockState = CursorLockMode.Confined;

            seed = UnityEngine.Random.value;

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
            var atk = args as DamagePlayerAction;


        }

        private void CameraShake(float trauma)
        {
            var shake = Mathf.Pow(trauma, traumaExponent);
            var time = Time.time * frequency;

            float GetPerlinNoiseZeroToOne(float seed, float time) =>
                Mathf.PerlinNoise(seed, time) * 2f - 1f;

            var yaw = maxYaw * shake * GetPerlinNoiseZeroToOne(seed, time);
            var pitch = maxPitch * shake * GetPerlinNoiseZeroToOne(seed + 1, time);
            var roll = maxRoll * shake * GetPerlinNoiseZeroToOne(seed + 2, time);

            var offsetX = maxOffset * shake * GetPerlinNoiseZeroToOne(seed + 3, time);
            var offsetY = maxOffset * shake * GetPerlinNoiseZeroToOne(seed + 4, time);
            var offsetZ = maxOffset * shake * GetPerlinNoiseZeroToOne(seed + 5, time);
        }
    }
}