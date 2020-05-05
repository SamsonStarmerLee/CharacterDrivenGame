using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public bool Solid;

    [SerializeField]
    protected string _letter;
    public string Letter => _letter;

    public Vector2Int BoardPosition { get; set; }

    public abstract void SetHighlight(bool highlight);

    protected void InitBoardPosition()
    {
        var x = Mathf.RoundToInt(transform.position.x);
        var y = Mathf.RoundToInt(transform.position.z);
        var pos = new Vector2Int(x, y);
        Board.Instance.SetPosition(pos, this);
        BoardPosition = new Vector2Int(x, y);
        transform.position = new Vector3(x, transform.position.y, y);
    }

    public virtual void Destroy()
    {
        Destroy(gameObject);
    }
}