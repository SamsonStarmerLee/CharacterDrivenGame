using Assets.Scripts.Characters;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    using static Utility;

    public partial class DragToMoveController
    {
        abstract class BaseState : IState
        {
            public DragToMoveController Owner;

            public bool CanTransition() => true;

            public virtual void Enter() { }

            public virtual IState Execute() => null;

            public virtual void Exit() { }
        }

        private class IdleState : BaseState
        {
            public override IState Execute()
            {
                if (Input.GetMouseButtonDown(0) &&
                    GetMousePosition(Owner.layerMask, out Vector2Int position, out _))
                {
                    var entity = Board.Instance.GetAtPosition(position);
                    if (entity is AWarrior character)
                    {
                        Owner.activeCharacter = character;

                        return new DraggingState
                        {
                            From = entity.BoardPosition,
                            To = position,
                            Owner = Owner,
                        };
                    }
                }

                // Confirm movement and score matches.
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Board.Instance.ScoreMatches();
                    Owner.movement.Clear();
                }

                return null;
            }
        }

        class DraggingState : BaseState
        {
            public Vector2Int From;
            public Vector2Int To;

            public override IState Execute()
            {
                if (!GetMousePosition(
                    Owner.layerMask, 
                    out Vector2Int Destination, 
                    out Vector3 _))
                {
                    return null;
                }

                // If there is a previous movement with this character established,
                // pathfinding must continue from the original position.
                var character = Owner.activeCharacter;
                var movement = Owner
                    .movement
                    .FirstOrDefault(x => x.Character == character);
                if (movement != null)
                {
                    From = movement.From;
                    Owner.movement.Remove(movement);
                }

                var pf = Owner.pathFinder;
                var ignore = new List<Entity> { character };
                pf.Generate(From, Destination, character.MovementRange, ignore);
                DrawPath(pf.Path, Color.blue);

                // TEMP: Move agent into position.
                if (pf.HasPath)
                {
                    var terminus = pf.Terminus;
                    Board.Instance.MoveEntity(character, terminus);
                    Board.Instance.CheckForMatches();
                    character.WorldPosition = new Vector3(terminus.x, 0.5f, terminus.y);
                }

                // Release to stop dragging.
                if (Input.GetMouseButtonUp(0))
                {
                    if (pf.HasPath)
                    {
                        var terminus = pf.Terminus;
                        Board.Instance.MoveEntity(character, terminus);
                        Board.Instance.CheckForMatches();
                        character.WorldPosition = new Vector3(terminus.x, 0f, terminus.y);

                        Owner.movement.Add(new DragMovement
                        {
                            Character = character,
                            Path = pf.Path,
                        });
                    }
                    
                    return new IdleState
                    {
                        Owner = Owner
                    };
                }

                return null;
            }
        }
    }
}



//class DrawingState : BaseState
//{
//    public Vector2Int Origin;
//    public Vector2Int Destination;
//    public Character Moving;

//    public override void Execute()
//    {
//        if (!PathFinder.GetMousePosition(
//            Drawer.layerMask,
//            out Vector2Int Destination,
//            out Vector3 _))
//        {
//            return;
//        }

//        var pf = Drawer.pathFinder;
//        var ignore = new List<Entity> { Moving };
//        pf.Generate(Origin, Destination, Moving.MovementRange, ignore);
//        pf.DrawPath();

//        // TEMP: Move agent into position.
//        if (pf.HasPath)
//        {
//            var terminus = pf.Terminus;
//            Board.Instance.MoveEntity(Moving, terminus);
//            Board.Instance.CheckEntityMatches(Moving.BoardPosition);
//            Moving.Position = new Vector3(terminus.x, 0.5f, terminus.y);
//        }

//        // Release to stop dragging.
//        if (Input.GetMouseButtonUp(0))
//        {
//            if (pf.HasPath)
//            {
//                Moving.Drop(pf.Terminus);
//            }

//            Drawer.machine.ChangeState(new IdleState
//            {
//                Drawer = Drawer
//            });
//            return;
//        }
//    }
//}