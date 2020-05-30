using Assets.Scripts;
using DG.Tweening;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IOccupant, IDestroy, IInit
{
    public enum EntityType
    {
        Entity,
        Solid,
        Item
    }

    [SerializeField]
    protected string _letter;
    public string Letter => _letter;

    [SerializeField]
    private Color highlightColor;

    [SerializeField]
    private GameObject deathParticle;
    
    public EntityType Type;

    public Vector3 WorldPosition
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Vector2Int BoardPosition { get; set; }

    private Transform model;
    private new MeshRenderer renderer;
    private MaterialPropertyBlock block;
    private Color defaultColor;

    private void OnEnable()
    {
        // TEMP: Perhaps call init from a manager class?
        Init();
    }

    public virtual void Init()
    {
        InitBoardPosition();

        model = transform.Find("Model");
        renderer = model.GetComponent<MeshRenderer>();
        block = new MaterialPropertyBlock();
        defaultColor = renderer.material.color;

        Board.Instance.Register(this);
    }

    private void OnDisable()
    {
        Board.Instance.Deregister(this);
    }

    public virtual void Destroy()
    {
        WarpOut();
    }

    protected void WarpOut()
    {
        model.DOScaleX(0f, 0.25f);
        model.DOScaleZ(0f, 0.25f);

        DOTween.Sequence()
            .Insert(0.1f, model.DOScaleY(100f, 0.1f))
            .AppendCallback(() => Destroy(gameObject));
    }

    protected void BlowUp()
    {
        Instantiate(deathParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public virtual void SetHighlight(bool highlight)
    {
        block.SetColor("_Color", highlight ? highlightColor : defaultColor);
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
