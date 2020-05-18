using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public partial class CameraController
    {
        abstract class BaseState : IState
        {
            public CameraController Owner;

            public bool CanTransition() => true;

            public virtual void Enter() { }

            public virtual IState Execute() => null;

            public virtual void Exit() { }
        }

        class TrackingState : BaseState
        {
            public override IState Execute()
            {
                //TEMP
                return new ScrollingState
                {
                    Owner = Owner
                };
                //TEMP

                if (Owner.focus == null)
                {
                    return new ScrollingState
                    {
                        Owner = Owner
                    };
                }

                var pos = Owner.focus.position;
                Owner.UpdateFocusPoint(pos);

                var mousePos = Input.mousePosition;
                var touchingBorder = false;

                touchingBorder |= mousePos.x < Owner.scrollMargin || mousePos.x > Screen.width  - Owner.scrollMargin;
                touchingBorder |= mousePos.y < Owner.scrollMargin || mousePos.y > Screen.height - Owner.scrollMargin;

                if (touchingBorder)
                {
                    return new ScrollingState
                    {
                        Owner = Owner
                    };
                }

                return null;
            }
        }

        class ScrollingState : BaseState
        {
            public override IState Execute()
            {
                Owner.ScreenEdgeScroll();
                Owner.KeyboardScroll();
                return null;
            }
        }
    }
}
