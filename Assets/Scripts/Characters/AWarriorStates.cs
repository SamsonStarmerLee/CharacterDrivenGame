using UnityEngine;

namespace Assets.Scripts.Characters
{
    using static Utility;

    public partial class AWarrior : IScorer
    {
        abstract class BaseState : IState
        {
            public AWarrior Owner;

            public bool CanTransition() => true;

            public virtual void Enter() { }

            public virtual IState Execute() => null;

            public virtual void Exit() { }
        }

        class IdleState : BaseState
        {
            public override IState Execute()
            {
                // TEMP: Ability usage
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    return new SelectTargetAndDestState
                    {
                        Owner = Owner,
                    };
                }

                return null;
            }
        }

        class SelectTargetAndDestState : BaseState
        {
            const int characterLayer = 1 << 10;

            Character toThrow;

            public override void Enter()
            {
                Debug.Log("Select a Target");
            }

            public override IState Execute()
            {
                // TODO: Draw valid targets and throw range.

                if (Input.GetMouseButtonDown(0) &&
                    GetMousePosition(characterLayer, out var boardPos, out _))
                {
                    var occupant = Board.Instance.GetAtPosition(boardPos);
                    var distance = ManhattanDistance(Owner.BoardPosition, boardPos);
                    Debug.Log($"Distance: {distance}.");

                    if (occupant is Character character && distance == 1)
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

                // Right-click to cancel.
                if (Input.GetMouseButtonDown(1))
                {
                    return new IdleState
                    {
                        Owner = Owner,
                    };
                }

                return null;   
            }
        }

        class ThrowState : BaseState
        {
            public Character ToThrow;
            public Vector2Int ToBoardPos;
            public Vector3 ToWorldPos;

            Vector3 fromWorldPos;
            float elapsed;

            public override void Enter()
            {
                Debug.Log($"Throwing {ToThrow} to {ToBoardPos}!");

                Board.Instance.MoveEntity(ToThrow, ToBoardPos);
                fromWorldPos = ToThrow.WorldPosition;

                Board.Instance.CheckForMatches();
            }

            public override IState Execute()
            {
                elapsed += Time.deltaTime;
                ToThrow.WorldPosition = Vector3.Lerp(
                    fromWorldPos, 
                    ToWorldPos, 
                    elapsed / Owner.throwTime);

                if (Vector3.Distance(ToThrow.WorldPosition, ToWorldPos) < 0.01f)
                {
                    ToThrow.WorldPosition = ToWorldPos;
                    return new IdleState
                    {
                        Owner = Owner,
                    };
                }

                return null;
            }
        }
    }
}
