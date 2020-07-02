using Assets.Scripts.Actions;
using Assets.Scripts.Characters;
using Assets.Scripts.InputManagement;
using Assets.Scripts.Notifications;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    public partial class CameraController : MonoBehaviour
    {
        /// <summary>
        /// The global (asset-based) source of input.
        /// </summary>
        [SerializeField]
        InputSource input;

        /// <summary>
        /// Padding around the screen borders which will trigger screen-edge scrolling.
        /// </summary>
        [SerializeField]
        private float scrollMargin = 1f;

        /// <summary>
        /// How quickly the camera scrolls.
        /// </summary>
        [SerializeField]
        private float scrollSpeed = 5f;

        /// <summary>
        /// The maximum/minimum distance the camera can scroll on each axis.
        /// </summary>
        [SerializeField]
        private float scrollExtentTop, scrollExtentBot, scrollExtentLeft, scrollExtentRight;

        /// <summary>
        /// Amount of trauma added when the player is hit by an attack.
        /// </summary>
        [SerializeField, Min(0f)]
        private float traumaOnHit = 0.5f;

        /// <summary>
        /// Images used to display cinematic bars during gameover.
        /// </summary>
        [SerializeField]
        private Image cinematicBarTop, cinematicBarBottom;

        private Transform focus;
        private Vector3 focusPoint, viewPosition, focusOffset;
        private StateMachine machine = new StateMachine();
        private Transform cameraTransform;
        private Shaker cameraShaker;
        private bool windowHasFocus;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;

            viewPosition = transform.position;

            cameraTransform = transform.Find("Camera");
            cameraShaker = cameraTransform.GetComponent<Shaker>();

            machine.ChangeState(new TrackingState(this));

            var height = cinematicBarTop.rectTransform.rect.height;
            cinematicBarBottom.rectTransform.anchoredPosition += new Vector2(0, -height);
            cinematicBarTop.rectTransform.anchoredPosition += new Vector2(0, height);
        }

        private void OnEnable()
        {
            this.AddObserver(OnJumpToCharacter, CharacterController.JumpSelectNotification);
            this.AddObserver(OnJumpToCharacter, GameManager.PlayerSpawnedNotification);
            this.AddObserver(OnDragStart, CharacterController.BeginDragNotification);
            this.AddObserver(OnPlayerDamaged, Notify.Action<DamagePlayerAction>());
            this.AddObserver(OnGameOver, GameManager.GameOverNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnJumpToCharacter, CharacterController.JumpSelectNotification);
            this.RemoveObserver(OnJumpToCharacter, GameManager.PlayerSpawnedNotification);
            this.RemoveObserver(OnDragStart, CharacterController.BeginDragNotification);
            this.RemoveObserver(OnPlayerDamaged, Notify.Action<DamagePlayerAction>());
            this.RemoveObserver(OnGameOver, GameManager.GameOverNotification);
        }

        private void LateUpdate()
        {
            machine.Execute();

            var lookPosition = focusPoint + viewPosition + focusOffset;
            var x = Mathf.Clamp(lookPosition.x, -scrollExtentLeft, scrollExtentRight);
            var z = Mathf.Clamp(lookPosition.z, -scrollExtentBot, scrollExtentTop);
            var corrected = new Vector3(x, lookPosition.y, z);
            focusOffset = corrected - focusPoint - viewPosition;

            transform.position = corrected;
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
            cameraShaker.AddTrauma(traumaOnHit);
        }

        private void OnGameOver(object sender, object args)
        {
            var toFocus = args as ICharacter;

            focus = (toFocus as MonoBehaviour).transform;
            machine.ChangeState(new CinematicState(this));
        }

        private void OnApplicationFocus(bool focus)
        {
            windowHasFocus = focus;
        }

        private void OnMouseDown()
        {
            // Unity can only confine the cursor inside WebGL builds inside OnMouseDown/OnKeyDown events.
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}