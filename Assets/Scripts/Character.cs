using Assets.Scripts;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class Character : Entity, IScorer
{
    private static char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public int MovementRange = 6;
    
    public bool IsPlayer;

    [SerializeField]
    float moveTime = 0.25f;

    [SerializeField]
    Mesh[] meshAlphabet;

    Vector3 fromPosition;
    float progress;
    MaterialPropertyBlock block;
    new MeshRenderer renderer;
    bool currentlyHeld;
    Color defaultColor;

    private void Awake()
    {
        InitBoardPosition();

        // This sucks
        _letter = Alphabet[Random.Range(0, Alphabet.Length)].ToString();
        name = _letter;
        var @case = Random.Range(0, 2) == 0 ? "Upper" : "Lower";
        var mesh = meshAlphabet.First(x => x.name == $"{Letter}_{@case}");
        var meshFilter = GetComponentInChildren<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        renderer = GetComponentInChildren<MeshRenderer>();
        block = new MaterialPropertyBlock();
        defaultColor = renderer.material.color;

        if (IsPlayer)
        {
            Board.Instance.RegisterScorer(this);
        }
    }

    private void Update()
    {
        if (currentlyHeld)
        {
            return;
        }

        var boardPosition3D = new Vector3(BoardPosition.x, 0f, BoardPosition.y);
        var t = Mathf.SmoothStep(0.0f, 1.0f, progress += (Time.deltaTime / moveTime));
        Position = Vector3.Lerp(fromPosition, boardPosition3D, t);

        if (IsPlayer)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))  Move(Vector2Int.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Move(Vector2Int.right);
            if (Input.GetKeyDown(KeyCode.UpArrow))    Move(Vector2Int.up);
            if (Input.GetKeyDown(KeyCode.DownArrow))  Move(Vector2Int.down);
        }
    }

    private void Move(Vector2Int translation)
    {
        var newPos = BoardPosition + translation;

        if (Board.Instance.GetAtPosition(newPos) != null)
        {
            // Something occupies that position already.
            return;
        }

        if (Board.Instance.MoveEntity(this, newPos))
        {
            fromPosition = Position;
            BoardPosition = newPos;
            progress = 0f;
            Board.Instance.CheckForMatches();
        }
    }

    public override void SetHighlight(bool highlight)
    {
        block.SetColor("_Color", highlight ? Color.red : defaultColor);
        renderer.SetPropertyBlock(block);
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

    public override void Destroy()
    {
        base.Destroy();

        Board.Instance.DeregisterScorer(this);
    }
}
