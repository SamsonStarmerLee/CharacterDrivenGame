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

    private void Start()
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

    public virtual void Destroy()
    {
        WarpOut();
        Board.Instance.Deregister(this);
    }

    protected void WarpOut()
    {
        model.DOScaleX(0f, 0.25f);
        model.DOScaleY(0f, 0.25f);

        //var fadeTween = DOTween.To(
        //    () => renderer.material.color, 
        //    x => 
        //    {
        //        block.SetColor("_Color", x);
        //        renderer.SetPropertyBlock(block);
        //    }, 
        //    new Color(0, 0, 0, 0), 
        //    0.1f);

        DOTween.Sequence()
            //.Insert(0.1f, fadeTween)
            .Insert(0.1f, model.DOScaleZ(100f, 0.1f))
            .AppendCallback(() => Destroy(gameObject));
    }

    protected void BlowUp()
    {
        var obj = Instantiate(deathParticle, transform.position, Quaternion.identity);
        var particleSystem = obj.GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startColor = defaultColor;

        Destroy(gameObject);
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
