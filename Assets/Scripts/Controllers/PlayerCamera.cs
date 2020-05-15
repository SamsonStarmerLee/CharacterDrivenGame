using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PlayerCamera : MonoBehaviour
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

        Vector3 focusPoint, relativePosition;

        void Start()
        {
            relativePosition = transform.position - focus.position;

            Cursor.lockState = CursorLockMode.Confined;
        }

        void LateUpdate()
        {
            //UpdateFocusPoint(focus.position);
            ScreenEdgeScroll();

            var lookPosition = focusPoint + relativePosition;

            //var toFocus = focusPoint - transform.position;
            //var lookFocus = Quaternion.LookRotation(toFocus);
            //var lookRotation = Quaternion.Slerp(transform.rotation, lookFocus, rotationSpeed);

            //transform.SetPositionAndRotation(lookPosition, lookRotation);
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
                relativePosition -= rightAxis * scrollSpeed * Time.deltaTime;
            }
            else if (mousePos.x > Screen.width - scrollMargin)
            {
                relativePosition += rightAxis * scrollSpeed * Time.deltaTime;
            }

            if (mousePos.y < scrollMargin) 
            {
                relativePosition -= forwardAxis * scrollSpeed * Time.deltaTime;
            }
            else if (mousePos.y > Screen.height - scrollMargin)
            {
                relativePosition += forwardAxis * scrollSpeed * Time.deltaTime;
            }
        }
    }
}
