using UnityEngine;

namespace Assets.Scripts
{
    public interface IOccupant : IDestroy
    {
        Vector3 WorldPosition { get; set; }

        Vector2Int BoardPosition { get; set; }

        string Letter { get; }
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