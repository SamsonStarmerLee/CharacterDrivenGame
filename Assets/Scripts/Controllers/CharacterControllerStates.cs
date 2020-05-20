using Assets.Scripts.Characters;
using Assets.Scripts.Notifications;
using Assets.Scripts.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    using static Utility;

    public partial class CharacterController
    {
        public const string JumpSelectNotification = "JumpSelect.Notification";
        public const string BeginDragNotification = "BeginDrag.Notification";
        public const string CompleteDragNotification = "CompleteDrag.Notification";

        private abstract class BaseState : IState
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
                    var entity = Board.Instance.GetAtPosition(position, Board.OccupantType.Entity);
                    if (entity is ICharacter character && !character.HasActed)
                    {
                        Owner.activeCharacter = character;

                        return new DraggingState
                        {
                            From = entity.BoardPosition,
                            Owner = Owner,
                        };
                    }
                }

                // When we press a keyboard key that matches one of our characters, select it.
                if (Input.anyKeyDown && Input.inputString.Length == 1)
                {
                    CheckJumpSelect();
                }

                // Confirm movement and score matches.
                if (Input.GetKeyDown(KeyCode.Return) || 
                    Input.GetKeyDown(KeyCode.KeypadEnter) ||
                    Input.GetKeyDown(KeyCode.Space))
                {
                    Owner.movement.Clear();
                    Owner.activeCharacter = null;

                    Board.Instance.ScoreMatches();
                    Board.Instance.RefreshCharacters();
                }

                return null;
            }

            private void CheckJumpSelect()
            {
                var pressed = Input.inputString;

                if (char.IsLetter(pressed[0]))
                {
                    var match = Board.Instance.Characters.FirstOrDefault(x =>
                        string.Equals(x.Letter, pressed, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                    {
                        var tf = (match as MonoBehaviour).transform;
                        this.PostNotification(JumpSelectNotification, tf);
                    }
                }
            }
        }

        private class DraggingState : BaseState
        {
            public Vector2Int From;

            public override void Enter()
            {
                var tf = (Owner.activeCharacter as MonoBehaviour).transform;
                this.PostNotification(BeginDragNotification, tf);
            }

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

                void MoveCharacterToTerminus()
                {
                    var terminus = path[0];
                    if (character.BoardPosition != terminus)
                    {
                        Board.Instance.MoveOccupant(character, terminus);
                        Board.Instance.CheckForMatches();
                        character.WorldPosition = new Vector3(terminus.x, 0f, terminus.y);
                    }
                }

                // Move agent into position.
                if (path.Count != 0)
                {
                    MoveCharacterToTerminus();
                }

                // Release to stop dragging.
                if (Input.GetMouseButtonUp(0))
                {
                    if (path.Count != 0)
                    {
                        MoveCharacterToTerminus();

                        Owner.movement.Add(new DragMovement
                        {
                            Character = character,
                            Path = path,
                        });
                    }
                    
                    return new IdleState { Owner = Owner };
                }

                return null;
            }

            public override void Exit()
            {
                this.PostNotification(CompleteDragNotification);
            }
        }
    }
}