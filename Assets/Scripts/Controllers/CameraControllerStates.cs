using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public partial class CameraController
    {
        private abstract class BaseState : IState
        {
            public CameraController Owner;

            public bool CanTransition() => true;

            public virtual void Enter() { }

            public virtual IState Execute() => null;

            public virtual void Exit() { }
        }

        private class TrackingState : BaseState
        {
            public TrackingState(CameraController owner)
            {
                Owner = owner;
            }

            public override IState Execute()
            {
                if (Owner.focus == null || CheckScrolling())
                {
                    return new ScrollingState
                    {
                        Owner = Owner
                    };
                }

                TrackFocus();

                return null;
            }

            private bool CheckScrolling()
            {
                var mousePos = Input.mousePosition;
                var scrolling = false;

                // Scrolling by touching the screen edge
                scrolling |= mousePos.x < Owner.scrollMargin || mousePos.x > Screen.width - Owner.scrollMargin;
                scrolling |= mousePos.y < Owner.scrollMargin || mousePos.y > Screen.height - Owner.scrollMargin;

                // Scrolling with arrow keys
                scrolling |= Input.GetAxis("CameraHorizontal") != 0;
                scrolling |= Input.GetAxis("CameraVertical") != 0;

                return scrolling;
            }

            private void TrackFocus()
            {
                // TEMP
                var desired = Owner.focus.position;
                Owner.focusPoint = Vector3.Lerp(Owner.focusPoint, desired, 0.002f);
            }
        }

        private class ScrollingState : BaseState
        {
            public override IState Execute()
            {
                ScreenEdgeScroll();
                KeyboardScroll();
                return null;
            }

            private void ScreenEdgeScroll()
            {
                var transform = Owner.transform;
                var scrollMargin = Owner.scrollMargin;
                var scrollSpeed = Owner.scrollSpeed;
                ref var focusOffset = ref Owner.focusOffset;

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

            private void KeyboardScroll()
            {
                var transform = Owner.transform;
                var scrollSpeed = Owner.scrollSpeed;
                ref var focusOffset = ref Owner.focusOffset;

                var h = Input.GetAxis("CameraHorizontal");
                var v = Input.GetAxis("CameraVertical");

                if (h == 0 && v == 0)
                {
                    return;
                }

                var rightAxis = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                var forwardAxis = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

                focusOffset += rightAxis * h * scrollSpeed * Time.deltaTime;
                focusOffset += forwardAxis * v * scrollSpeed * Time.deltaTime;
            }
        }
    }
}
