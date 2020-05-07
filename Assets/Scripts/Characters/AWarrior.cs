using UnityEngine;

namespace Assets.Scripts.Characters
{
    [SelectionBase]
    public partial class AWarrior : Entity, IScorer
    {
        public int MovementRange = 6;

        [SerializeField]
        float moveTime = 0.25f;

        Vector3 fromPosition;
        float progress;
        bool currentlyHeld;

        StateMachine machine = new StateMachine();

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

        private void Update()
        {
            machine.Execute();

            if (currentlyHeld)
            {
                return;
            }

            var boardPosition3D = new Vector3(BoardPosition.x, 0f, BoardPosition.y);
            var t = Mathf.SmoothStep(0.0f, 1.0f, progress += (Time.deltaTime / moveTime));
            WorldPosition = Vector3.Lerp(fromPosition, boardPosition3D, t);
        }

        public void Pickup()
        {
            currentlyHeld = true;
        }

        public void Drop(Vector2Int newPosition)
        {
            currentlyHeld = false;
            Board.Instance.MoveEntity(this, newPosition);
            Board.Instance.CheckForMatches();
        }
    }
}




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