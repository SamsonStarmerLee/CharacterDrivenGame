using UnityEngine;
using UnityEngine.PlayerLoop;

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

        Vector3 focusPoint, relativePosition;

        void Start()
        {
            relativePosition = transform.position - focus.position;
        }

        void LateUpdate()
        {
            UpdateFocusPoint(focus.position);

            var lookPosition = focusPoint + relativePosition;

            var toFocus = focusPoint - transform.position;
            var lookFocus = Quaternion.LookRotation(toFocus);
            var lookRotation = Quaternion.Slerp(transform.rotation, lookFocus, rotationSpeed);

            transform.SetPositionAndRotation(lookPosition, lookRotation);
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
    }
}
