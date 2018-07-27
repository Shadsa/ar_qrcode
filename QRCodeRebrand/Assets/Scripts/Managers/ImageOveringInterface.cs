using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageOveringInterface : AbstractOveringInterface
{
    public override void ObjectFocused(GameObject go)
    {
        RawImage rend = GetComponent<RawImage>();
        rend.color = OveringTextColor;
    }

    public override void ObjectUnfocused(GameObject go)
    {
        RawImage rend = GetComponent<RawImage>();
        rend.color = currentCol;
    }
}
