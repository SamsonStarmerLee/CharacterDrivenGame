using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public partial class CameraController : MonoBehaviour
    {
        [SerializeField]
        Transform focus;

        [SerializeField]
        float scrollMargin = 1f;

        [SerializeField]
        float scrollSpeed = 5f;

        Vector3 focusPoint, viewPosition, focusOffset;

        StateMachine machine = new StateMachine();

        void Start()
        {
            viewPosition = transform.position;
            Cursor.lockState = CursorLockMode.Confined;

            machine.ChangeState(new TrackingState
            {
                Owner = this
            });
        }

        void LateUpdate()
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