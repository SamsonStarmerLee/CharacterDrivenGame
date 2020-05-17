using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public partial class CameraController : MonoBehaviour
    {
        [SerializeField]
        Transform focus;

        [SerializeField, Min(0f)]
        float focusRadius = 1f;

        [SerializeField, Range(0f, 1f)]
        float focusCentering = 0.5f;

        [SerializeField]
        float rotationSpeed = 0.1f;

        [SerializeField]
        float scrollMargin = 1f;

        [SerializeField]
        float scrollSpeed = 5f;

        Vector3 focusPoint, relativePosition, focusOffset;

        StateMachine machine = new StateMachine();

        Transform cameraTransform;

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

        void Start()
        {
            relativePosition = transform.position - focus.position;
            Cursor.lockState = CursorLockMode.Confined;
            cameraTransform = transform.Find("Camera");

            machine.ChangeState(new TrackingState
            {
                Owner = this
            });
        }

        void LateUpdate()
        {
            machine.Execute();

            var lookPosition = focusPoint + relativePosition + focusOffset;
            transform.position = lookPosition;
        }

        void UpdateFocusPoint(Vector3 targetPoint) 
        {
            if (focusRadius > 0f)
            {
                var distance = Vector3.Distance(targetPoint, focusPoint);
                if (distance > focusRadius)
                {
                    focusPoint = Vector3.Lerp(
                        targetPoint, focusPoint,
                        focusRadius / distance);
                }
            }
            else
            {
                focusPoint = targetPoint;
            }

            if (focusCentering > 0f)
            {
                focusPoint = Vector3.Lerp(
                    targetPoint, focusPoint,
                    Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime));
            }
        }

        void ScreenEdgeScroll()
        {
            var mousePos = Input.mousePosition;
            var rightAxis = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            var forwardAxis = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            if (mousePos.x < scrollMargin)
            {
                focusOffset -= rightAxis * scrollSpeed * Time.deltaTime;
            }
            else if (mousePos.x > Screen.width - scrollMargin)
            {
                focusOffset += rightAxis * scrollSpeed * Time.deltaTime;
            }

            if (mousePos.y < scrollMargin) 
            {
                focusOffset -= forwardAxis * scrollSpeed * Time.deltaTime;
            }
            else if (mousePos.y > Screen.height - scrollMargin)
            {
                focusOffset += forwardAxis * scrollSpeed * Time.deltaTime;
            }
        }
    }
}
