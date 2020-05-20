using Assets.Scripts.LevelGen;
using Assets.Scripts.Notifications;
using System;
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
        private Vector3 focusPoint, viewPosition, focusOffset;
        private StateMachine machine = new StateMachine();

        private void Start()
        {
            viewPosition = transform.position;
            Cursor.lockState = CursorLockMode.Confined;

            machine.ChangeState(new TrackingState(this));
        }

        private void OnEnable()
        {
            this.AddObserver(OnJumpToCharacter, CharacterController.JumpSelectNotification);
            this.AddObserver(OnJumpToCharacter, RoomGenerator.PlayerSpawnedNotification);
            this.AddObserver(OnDragStart, CharacterController.BeginDragNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnJumpToCharacter, CharacterController.JumpSelectNotification);
            this.RemoveObserver(OnJumpToCharacter, RoomGenerator.PlayerSpawnedNotification);
            this.RemoveObserver(OnDragStart, CharacterController.BeginDragNotification);
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
    }
}