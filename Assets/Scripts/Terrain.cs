using UnityEngine;

public class Terrain : Entity
{
    MaterialPropertyBlock block;
    new MeshRenderer renderer;
    Color defaultColor;

    private void Awake()
    {
        InitBoardPosition();

        transform.position = new Vector3(
            BoardPosition.x,
            transform.position.y,
            BoardPosition.y);

        renderer = GetComponentInChildren<MeshRenderer>();
        block = new MaterialPropertyBlock();
        defaultColor = renderer.material.color;
    }

    public override void SetHighlight(bool highlight)
    {
        block.SetColor("_Color", highlight ? Color.red : defaultColor);
        renderer.SetPropertyBlock(block);
    }
}
