using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGlobalEventProvider : MonoBehaviour, IFocusable {

    //This class is used to create a dynamic call to each part of UI when there is interaction on. Mainly call in UIDisplayManager

    public delegate void ComponentEvent(GameObject go); //This.gameObject return the container of this script. So, the EventProvider always return the object which trigger the event
    public  event ComponentEvent Focused;
    public  event ComponentEvent Unfocused;
    public  event ComponentEvent Selected;


    //Call when selected after a tip gesture while gazing at this object
    public void OnSelect() 
    {
        if(Selected != null)
        {
            Selected(this.gameObject);
        }
    }

    //Call when raycast hit the collider
    public void OnFocusEnter()
    {

        if (Focused != null)
        {
            Focused(this.gameObject);
        }
    }

    //Call when raycast left the collider
    public void OnFocusExit()
    {
        if (Unfocused != null)
        {
            Unfocused(this.gameObject);
        }
    }
    

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
