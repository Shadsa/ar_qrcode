using UnityEngine;




public class SpriteOveringInterface : AbstractOveringInterface
{
    public override void ObjectFocused(GameObject go)
    {
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        rend.color = OveringTextColor;
    }

    public override void ObjectUnfocused(GameObject go)
    {
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        rend.color = currentCol;
    }
}
