using Assets.Scripts;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IOccupant, IDestroy, IInit
{
    public bool Solid;

    [SerializeField]
    protected string _letter;
    public string Letter => _letter;

    public Vector3 WorldPosition
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Vector2Int BoardPosition { get; set; }

    new MeshRenderer renderer;
    MaterialPropertyBlock block;
    Color defaultColor;

    private void Start()
    {
        // TEMP: Perhaps call init from a manager class?
        Init();
    }

    public virtual void Init()
    {
        InitBoardPosition();

        renderer = GetComponentInChildren<MeshRenderer>();
        block = new MaterialPropertyBlock();
        defaultColor = renderer.material.color;

        Board.Instance.Register(this);
    }

    public virtual void Destroy()
    {
        Destroy(gameObject);
        Board.Instance.Deregister(this);
    }

    public virtual void SetHighlight(bool highlight)
    {
        block.SetColor("_Color", highlight ? Color.red : defaultColor);
        renderer.SetPropertyBlock(block);
    }

    private void InitBoardPosition()
    {
        var x = Mathf.RoundToInt(transform.position.x);
        var y = Mathf.RoundToInt(transform.position.z);
        var pos = new Vector2Int(x, y);
        Board.Instance.SetPosition(this, pos);
        BoardPosition = new Vector2Int(x, y);
        WorldPosition = new Vector3(x, transform.position.y, y);
    }
}