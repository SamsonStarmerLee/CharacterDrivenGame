using Assets.Scripts.Characters;
using Assets.Scripts.Notifications;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Visuals;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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

        private interface IClickableHandlerState
        {
            void OnClick(PointerEventData pointerEventData);
        }

        private class IdleState : BaseState, IClickableHandlerState
        {
            public IdleState(CharacterController owner)
            {
                Owner = owner;
            }

            public override IState Execute()
            {
                if (Owner.input.Locked)
                {
                    return null;
                }

                if (Owner.input.Select.Clicked &&
                    GetMousePosition(Owner.movementLayerMask, out Vector2Int position, out _))
                {
                    var entity = Board.Instance.GetAtPosition(position, Board.OccupantType.Entity);
                    if (entity is ICharacter character && !character.HasActed)
                    {
                        Owner.activeCharacter = character;

                        return new DraggingState(Owner)
                        {
                            From = entity.BoardPosition,
                        };
                    }
                }

                // When we press a keyboard key that matches one of our characters, select it.
                if (Owner.input.AnyAlphabeticalKeyClicked)
                {
                    CheckJumpSelect();
                }

                // Confirm movement and score matches.
                if (Owner.input.Submit.Clicked)
                {
                    // Play the submit turn sfx, at a lower pitch if no actions have been taken.
                    var haveMeaningfulAction = Owner.movement.Count != 0;
                    if (haveMeaningfulAction)
                        Owner.PlaySfx(Owner.submitTurnSfx, 0.95f, 1.15f);
                    else
                        Owner.PlaySfx(Owner.submitTurnSfx, 0.5f, 0.6f);

                    Owner.movement.Clear();
                    Owner.activeCharacter = null;

                    Board.Instance.ScoreMatches();
                    Board.Instance.CollectItems();
                    Board.Instance.RefreshCharacters();

                    this.PostNotification(GameManager.SubmitTurnNotification);

                    ClearPaths();
                }

                return null;
            }

            public void OnClick(PointerEventData pointerEventData)
            {
                //Owner.machine.ChangeState(new BuyLetterState { Owner = Owner });
            }

            private void CheckJumpSelect()
            {
                var pressed = Owner.input.AlphabeticalHeld.ToString();
                var match = Board.Instance.Characters.FirstOrDefault(x =>
                    string.Equals(x.Letter, pressed, System.StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    var tf = (match as MonoBehaviour).transform;
                    this.PostNotification(JumpSelectNotification, tf);
                }
            }

            private void ClearPaths()
            {
                foreach (var kvp in Owner.paths)
                {
                    (var character, var path) = kvp;

                    foreach (var pos in path)
                    {
                        Board.Instance
                            .SetFloorTileOverlay(Overlay.None, pos);
                    }
                }

                Owner.paths.Clear();
            }
        }

        private class DraggingState : BaseState
        {
            public Vector2Int From;

            public DraggingState(CharacterController owner)
            {
                Owner = owner;
            }

            public override void Enter()
            {
                var tf = (Owner.activeCharacter as MonoBehaviour).transform;
                this.PostNotification(BeginDragNotification, tf);

                // Play pick up sfx
                Owner.PlaySfx(Owner.grabCharacterSfx, 0.95f, 1.15f);
            }

            public override IState Execute()
            {
                if (Owner.input.Locked ||
                    !GetMousePosition(
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
                    callbacks,
                    Owner.inRangeTiles);

                // Set the active character's path.
                Owner.paths[Owner.activeCharacter] = path;

                void MoveCharacterToTerminus(bool raised, bool forceDown = false)
                {
                    var terminus = path[0];
                    if (character.BoardPosition != terminus)
                    {
                        Board.Instance.MoveOccupant(character, terminus);
                        Board.Instance.CheckForMatches();

                        var height = raised ? 0.5f : 0.0f;
                        character.WorldPosition = new Vector3(terminus.x, height, terminus.y);

                        var i = Board.Instance.GetAtPosition(terminus, Board.OccupantType.Item);
                        if (i is Money item)
                        {
                            item.Touch();
                        }

                        // Play drag sfx for each tile traversed
                        Owner.PlaySfx(Owner.dragCharacterSfx, 0.95f, 1.15f, 0.5f);
                    }
                    else if (forceDown)
                    {
                        var pos = character.BoardPosition;
                        character.WorldPosition = new Vector3(pos.x, 0.0f, pos.y);
                    }
                }

                // Move agent into position.
                if (path.Count != 0)
                {
                    MoveCharacterToTerminus(true);
                }

                DrawRegion();

                // Release to stop dragging.
                if (Owner.input.Select.Released)
                {
                    if (path.Count != 0)
                    {
                        MoveCharacterToTerminus(false, true);

                        Owner.movement.Add(new DragMovement
                        {
                            Character = character,
                            Path = path,
                        });
                    }
                    
                    return new IdleState(Owner);
                }

                return null;
            }

            public override void Exit()
            {
                ClearRangeTiles();
                this.PostNotification(CompleteDragNotification);

                // Play drop sound effects
                Owner.PlaySfx(Owner.placeCharacterSfx, 0.95f, 1.15f);
                Owner.PlaySfx(Owner.placeCharacterOverlaySfx, 0.95f, 1.15f, 0.75f);
            }

            // TODO: Re-do tile drawing
            private void DrawRegion()
            {
                var inRange = Owner.inRangeTiles;

                foreach (var point in inRange)
                {
                    var tile = Board.Instance.GetCell(point)?.FloorTile;

                    if (tile == null || tile.Overlay == Overlay.Path_Inactive)
                    {
                        continue;
                    }

                    tile.SetOverlay(Overlay.Range);
                }

                foreach (var kvp in Owner.paths)
                {
                    (var character, var path) = kvp;
                    
                    if (path == null || path.Count == 0)
                    {
                        continue;
                    }
                    
                    var overlay = character == Owner.activeCharacter 
                        ? Overlay.Path_Active 
                        : Overlay.Path_Inactive;

                    foreach (var pos in path)
                    {
                        Board.Instance
                            .SetFloorTileOverlay(overlay, pos);
                    }

                    Board.Instance.SetFloorTileOverlay(Overlay.Destination, path[0]);
                }
            }

            private void ClearRangeTiles()
            {
                foreach (var pos in Owner.inRangeTiles)
                {
                    var tile = Board.Instance.GetCell(pos)?.FloorTile;

                    if (tile == null || tile.Overlay == Overlay.Path_Inactive)
                    {
                        continue;
                    }

                    var overlay = tile.Overlay == Overlay.Path_Active
                        ? Overlay.Path_Inactive
                        : Overlay.None;
                    tile.SetOverlay(overlay);
                }

                Owner.inRangeTiles.Clear();
            }
        }

        private class BuyLetterState : BaseState, IClickableHandlerState
        {
            public BuyLetterState(CharacterController owner)
            {
                Owner = owner;
            }

            public void OnClick(PointerEventData pointerEventData)
            {
                Owner.machine.ChangeState(new IdleState(Owner));
            }
        }
    }
}