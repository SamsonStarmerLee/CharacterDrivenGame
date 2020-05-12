using Assets.Scripts.Characters;
using Assets.Scripts.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    using static Utility;

    public partial class CharacterController
    {
        abstract class BaseState : IState
        {
            public CharacterController Owner;

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
                    GetMousePosition(Owner.movementLayerMask, out Vector2Int position, out _))
                {
                    var entity = Board.Instance.GetAtPosition(position);
                    if (entity is ICharacter character && !character.HasActed)
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
                    Owner.movement.Clear();
                    Owner.activeCharacter = null;

                    Board.Instance.ScoreMatches();
                    Board.Instance.RefreshCharacters();
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
                    Owner.movementLayerMask, 
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

                var range = character.MovementRange;
                var callbacks = character.MovementCallbacks;
                var ignore = new List<ICharacter> { character };

                var path = PathFinder.GenerateFullDepth(
                    From, 
                    Destination, 
                    range, 
                    ignore,
                    callbacks);
                
                DrawPath(path, Color.blue);

                // TEMP: Move agent into position.
                if (path.Count != 0)
                {
                    var terminus = path[0];
                    Board.Instance.MoveOccupant(character, terminus);
                    Board.Instance.CheckForMatches();
                    character.WorldPosition = new Vector3(terminus.x, 0.5f, terminus.y);
                }

                // Release to stop dragging.
                if (Input.GetMouseButtonUp(0))
                {
                    if (path.Count != 0)
                    {
                        var terminus = path[0];
                        Board.Instance.MoveOccupant(character, terminus);
                        Board.Instance.CheckForMatches();
                        character.WorldPosition = new Vector3(terminus.x, 0f, terminus.y);

                        Owner.movement.Add(new DragMovement
                        {
                            Character = character,
                            Path = path,
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