﻿using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Characters
{
    using static Utility;

    public partial class AWarrior
    {
        private abstract class BaseState : IState
        {
            public AWarrior Owner;

            public bool CanTransition() => true;

            public virtual void Enter() { }

            public virtual IState Execute() => null;

            public virtual void Exit() { }
        }

        private class IdleState : BaseState
        {
            public override IState Execute()
            {
                // TEMP: Ability usage
                if (Owner.input.Numbers[1].Clicked)
                {
                    return new SelectTargetAndDestState
                    {
                        Owner = Owner,
                    };
                }

                return null;
            }
        }

        private class SelectTargetAndDestState : BaseState
        {
            private const int characterLayer = 1 << 10;
            private Letter toThrow;

            public override void Enter()
            {
                Debug.Log("Select a Target");
            }

            public override IState Execute()
            {
                // TODO: Draw valid targets and throw range.

                if (Owner.input.Select.Clicked &&
                    GetMousePosition(characterLayer, out var boardPos, out _))
                {
                    var occupant = Board.Instance.GetAtPosition(boardPos, Board.OccupantType.Entity);
                    var distance = ManhattanDist(Owner.BoardPosition, boardPos);
                    Debug.Log($"Distance: {distance}.");

                    if (occupant is Letter character && distance == 1)
                    {
                        // If occupied by a character, set as the target for the throw.
                        toThrow = character;
                        Debug.Log($"Selected {character}.");
                    }

                    else if (occupant == null && 
                            toThrow != null &&
                            distance <= Owner.throwRange)
                    {
                        // If unoccupied and we have a character to throw, 
                        // pick as destination and trigger the throw.
                        var worldPos = new Vector3(boardPos.x, 0f, boardPos.y);
                        return new ThrowState
                        {
                            Owner = Owner,
                            ToThrow = toThrow,
                            ToBoardPos = boardPos,
                            ToWorldPos = worldPos,
                        };
                    }
                }

                // Cancel the action.
                if (Owner.input.Dismiss.Clicked)
                {
                    return new IdleState
                    {
                        Owner = Owner,
                    };
                }

                return null;   
            }
        }

        private class ThrowState : BaseState
        {
            public Letter ToThrow;
            public Vector2Int ToBoardPos;
            public Vector3 ToWorldPos;

            public override void Enter()
            {
                Owner.HasActed = true;
                Debug.Log($"Throwing {ToThrow} to {ToBoardPos}!");

                // Move the target and check for new matches.
                Board.Instance.MoveOccupant(ToThrow, ToBoardPos);
                Board.Instance.CheckForMatches();

                // Visualize the throw.
                ToThrow.transform.DOJump(ToWorldPos, 3f, 1, Owner.throwTime);

                Owner.machine.ChangeState(new IdleState
                {
                    Owner = Owner,
                });
            }
        }
    }
}
