using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.PathDrawing
{
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
                    PathFinder.GetMousePosition(Owner.layerMask, out Vector2Int position, out _))
                {
                    var entity = Board.Instance.GetAtPosition(position);
                    if (entity is AWarrior character)
                    {
                        character.Pickup();
                        return new DraggingState
                        {
                            From = entity.BoardPosition,
                            To = position,
                            Owner = Owner,
                            Character = character,
                        };
                    }
                }

                // Confirm movement and score matches.
                if (Input.GetKeyDown(KeyCode.Return))
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
            public AWarrior Character;

            public override IState Execute()
            {
                if (!PathFinder.GetMousePosition(
                    Owner.layerMask, 
                    out Vector2Int Destination, 
                    out Vector3 _))
                {
                    return null;
                }

                // If there is a previous movement with this character established,
                // pathfinding must continue from the original position.
                var movement = Owner
                    .movement
                    .FirstOrDefault(x => x.Character == Character);
                if (movement != null)
                {
                    From = movement.From;
                    Owner.movement.Remove(movement);
                }

                var pf = Owner.pathFinder;
                var ignore = new List<Entity> { Character };
                pf.Generate(From, Destination, Character.MovementRange, ignore);
                DrawPath(pf.Path, Color.blue);

                // TEMP: Move agent into position.
                if (pf.HasPath)
                {
                    var terminus = pf.Terminus;
                    Board.Instance.MoveEntity(Character, terminus);
                    Board.Instance.CheckForMatches();
                    Character.WorldPosition = new Vector3(terminus.x, 0.5f, terminus.y);
                }

                // Release to stop dragging.
                if (Input.GetMouseButtonUp(0))
                {
                    if (pf.HasPath)
                    {
                        Character.Drop(pf.Terminus);

                        Owner.movement.Add(new DragMovement
                        {
                            Character = Character,
                            Path = pf.Path,
                        });

                        Debug.Log(Owner.movement.Count);
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