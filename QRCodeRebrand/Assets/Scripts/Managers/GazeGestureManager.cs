using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class GazeGestureManager : Singleton<GazeGestureManager>
{
    public static GazeGestureManager Instance { get; private set; }

    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }
    public int focusMessageSend = 0;
    public int unfocusMessageSend = 0;

    GestureRecognizer recognizer;

    // Use this for initialization
    void Awake()
    {
        Instance = this;

        if (FocusedObject != null)
        {
            FocusedObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
        }

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.Tapped += (args) =>
        {
            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null)
            {
                FocusedObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
            }
        };
        recognizer.StartCapturingGestures();
    }


    // Update is called once per frame
    void Update()
    {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
            if (oldFocusObject == null)
            {
                FocusedObject.SendMessageUpwards("OnFocusEnter", SendMessageOptions.DontRequireReceiver);
                focusMessageSend++;
            }
            else if (oldFocusObject != FocusedObject)
            {
                oldFocusObject.SendMessageUpwards("OnFocusExit", SendMessageOptions.DontRequireReceiver);
                FocusedObject.SendMessageUpwards("OnFocusEnter", SendMessageOptions.DontRequireReceiver);
                focusMessageSend++;
                unfocusMessageSend++;
            }
        }
        else
        {


            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
            if (oldFocusObject != null)
            {
                oldFocusObject.SendMessageUpwards("OnFocusExit", SendMessageOptions.DontRequireReceiver);
                unfocusMessageSend++;
            }
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
    }
}
