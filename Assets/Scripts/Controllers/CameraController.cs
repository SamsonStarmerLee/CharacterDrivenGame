using Assets.Scripts.Actions;
using Assets.Scripts.Characters;
using Assets.Scripts.InputManagement;
using Assets.Scripts.LevelGen;
using Assets.Scripts.Notifications;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    public partial class CameraController : MonoBehaviour
    {
        [SerializeField]
        InputSource input;

        [SerializeField]
        private Transform focus;

        [SerializeField]
        private float scrollMargin = 1f;

        [SerializeField]
        private float scrollSpeed = 5f;

        /// <summary>
        /// Amount of trauma added when the player is hit by an attack.
        /// </summary>
        [SerializeField, Min(0f)]
        private float traumaOnHit = 0.5f;

        [SerializeField]
        private Image cinematicBarTop, cinematicBarBottom;

        private Vector3 focusPoint, viewPosition, focusOffset;

        private StateMachine machine = new StateMachine();
        private Transform cameraTransform;
        private Shaker cameraShaker;

        private void Start()
        {
            viewPosition = transform.position;
            Cursor.lockState = CursorLockMode.Confined;

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
            this.AddObserver(OnJumpToCharacter, RoomGenerator.PlayerSpawnedNotification);
            this.AddObserver(OnDragStart, CharacterController.BeginDragNotification);
            this.AddObserver(OnPlayerDamaged, Notify.Action<DamagePlayerAction>());
            this.AddObserver(OnGameOver, GameManager.GameOverNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnJumpToCharacter, CharacterController.JumpSelectNotification);
            this.RemoveObserver(OnJumpToCharacter, RoomGenerator.PlayerSpawnedNotification);
            this.RemoveObserver(OnDragStart, CharacterController.BeginDragNotification);
            this.RemoveObserver(OnPlayerDamaged, Notify.Action<DamagePlayerAction>());
            this.RemoveObserver(OnGameOver, GameManager.GameOverNotification);
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
            cameraShaker.AddTrauma(traumaOnHit);
        }

        private void OnGameOver(object sender, object args)
        {
            var toFocus = args as ICharacter;

            focus = (toFocus as MonoBehaviour).transform;
            machine.ChangeState(new CinematicState { Owner = this });
        }
    }
}