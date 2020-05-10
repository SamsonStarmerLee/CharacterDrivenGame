﻿using UnityEngine;

namespace Assets.Scripts.Characters
{
    [SelectionBase]
    public partial class AWarrior : Entity, ICharacter, IScorer
    {
        [SerializeField]
        float moveTime = 0.25f;

        [SerializeField]
        int throwRange = 3;

        [SerializeField]
        float throwTime = 0.25f;

        [SerializeField]
        int _movementRange = 6;

        StateMachine machine = new StateMachine();

        public int MovementRange => _movementRange;

        public override void Init()
        {
            base.Init();

            Board.Instance.RegisterScorer(this);

            machine.ChangeState(new IdleState
            {
                Owner = this
            });
        }

        public override void Destroy()
        {
            base.Destroy();

            Board.Instance.DeregisterScorer(this);
        }

        void Update()
        {
            machine.Execute();
        }
    }
}

//var boardPosition3D = new Vector3(BoardPosition.x, 0f, BoardPosition.y);
//var t = Mathf.SmoothStep(0.0f, 1.0f, progress += (Time.deltaTime / moveTime));
//WorldPosition = Vector3.Lerp(fromPosition, boardPosition3D, t);

//if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(Vector2Int.left);
//if (Input.GetKeyDown(KeyCode.RightArrow)) Move(Vector2Int.right);
//if (Input.GetKeyDown(KeyCode.UpArrow)) Move(Vector2Int.up);
//if (Input.GetKeyDown(KeyCode.DownArrow)) Move(Vector2Int.down);

//private void Move(Vector2Int translation)
//{
//    var newPos = BoardPosition + translation;

//    if (Board.Instance.GetAtPosition(newPos) != null)
//    {
//        // Something occupies that position already.
//        return;
//    }

//    if (Board.Instance.MoveEntity(this, newPos))
//    {
//        fromPosition = WorldPosition;
//        BoardPosition = newPos;
//        progress = 0f;
//        Board.Instance.CheckForMatches();
//    }
//}