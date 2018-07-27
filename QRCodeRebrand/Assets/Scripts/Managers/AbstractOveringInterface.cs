using UnityEngine;

public abstract class AbstractOveringInterface : MonoBehaviour
{
   
    /// <summary>
    /// This class is the abstraction for all overing interaction (e.g : put a button in blue when gazing at)
    /// You have to modify focus and unfocus in sub-classe dedicate to the type of your element
    /// Already exist : Sprite, Text.
    /// Should be type : <T>OveringInterface
    /// </summary>

    public Color currentCol;
    public Color OveringTextColor;
    private UIGlobalEventProvider eventProvider;

    public bool isFocused = false;

    // Use this for initialization
    void Start()
    {
        OveringTextColor = UIDisplayerManager.Instance.OveringColor;
        currentCol = UIDisplayerManager.Instance.StandardColor;
        eventProvider = this.GetComponent<UIGlobalEventProvider>();
        eventProvider.Focused += ObjectFocused;
        eventProvider.Unfocused += ObjectUnfocused;
        eventProvider.Selected += ObjectUnfocused;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public abstract void ObjectFocused(GameObject go);


    public abstract void ObjectUnfocused(GameObject go);
    
}
