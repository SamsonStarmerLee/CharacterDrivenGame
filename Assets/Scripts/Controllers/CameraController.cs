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

            machine.ChangeState(new TrackingState
            {
                Owner = this
            });
        }

        private void LateUpdate()
        {
            machine.Execute();

            var lookPosition = focusPoint + viewPosition + focusOffset;
            transform.position = lookPosition;
        }

        public void Track(Transform focus)
        {
            this.focus = focus;

            machine.ChangeState(new TrackingState
            {
                Owner = this,
            });
        }

        public void Jump(Transform focus)
        {
            this.focus = focus;
            focusPoint = focus.position;
            focusOffset = Vector3.zero;

            machine.ChangeState(new TrackingState
            {
                Owner = this,
            });
        }
    }
}