using UnityEngine;
using UnityEngine.UI;

public class TextOveringInterface : AbstractOveringInterface
{
    public override void ObjectFocused(GameObject go)
    {
        Text rend = GetComponent<Text>();
        rend.color = OveringTextColor;
    }

    public override void ObjectUnfocused(GameObject go)
    {
        Text rend = GetComponent<Text>();
        rend.color = currentCol;
    }
}
