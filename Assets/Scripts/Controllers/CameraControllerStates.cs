using DG.Tweening;
using System.Collections;
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
                var input = Owner.input;
                var mousePos = input.SelectPosition;
                var scrolling = false;

                // Scrolling by touching the screen edge
                scrolling |= mousePos.x < Owner.scrollMargin || mousePos.x > Screen.width - Owner.scrollMargin;
                scrolling |= mousePos.y < Owner.scrollMargin || mousePos.y > Screen.height - Owner.scrollMargin;

                // Scrolling with arrow keys
                scrolling |= input.CameraAxis != Vector2.zero;

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

                var mousePos = Owner.input.SelectPosition;
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

                var camera = Owner.input.CameraAxis;
                if (camera == Vector2.zero)
                {
                    return;
                }

                var rightAxis = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                var forwardAxis = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

                focusOffset += rightAxis * camera.x * scrollSpeed * Time.deltaTime;
                focusOffset += forwardAxis * camera.y * scrollSpeed * Time.deltaTime;
            }
        }

        private class CinematicState : BaseState
        {
            const float Zoom = 8f;

            public override void Enter()
            {
                Owner.cinematicBarBottom.rectTransform.DOAnchorPosY(0, 0.25f);
                Owner.cinematicBarTop.rectTransform.DOAnchorPosY(0, 0.25f);
            }

            public override IState Execute()
            {
                if (Owner.focus == null)
                {
                    return null;
                }

                var zoomOffset = Owner.cameraTransform.forward * Zoom;

                // TEMP
                var desired = Owner.focus.position;
                Owner.focusPoint = Vector3.Lerp(Owner.focusPoint, desired, 0.1f);
                Owner.focusOffset = Vector3.Lerp(Owner.focusOffset, zoomOffset, 0.1f);

                return null;
            }

            public override void Exit()
            {
                var height = Owner.cinematicBarTop.rectTransform.rect.height;
                Owner.cinematicBarBottom.rectTransform.DOAnchorPosY(-height, 1f);
                Owner.cinematicBarTop.rectTransform.DOAnchorPosY(height, 1f);
            }
        }
    }
}
