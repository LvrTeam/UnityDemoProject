using UnityEngine;
using System;

/// <summary>
/// Interface class to a touchpad.
/// </summary>
public static class LVRTouchpad
{
    /// <summary>
    /// Touch Type.
    /// </summary>
    public enum TouchEvent
    {
        SingleTap,
        Left,
        Right,
        Up,
        Down,
    };

    public class TouchArgs : EventArgs
    {
        public TouchEvent TouchType;
    }

    /// <summary>
    /// Occurs when touched.
    /// </summary>
    public static event EventHandler TouchHandler;

    /// <summary>
    /// Native Touch State.
    /// </summary>
    enum TouchState
    {
        Init,
        Down,
        Stationary,
        Move,
        Up
    };

    static TouchState touchState = TouchState.Init;
    static Vector2 moveAmount;
    static float minMovMagnitude = 100.0f; // Tune this to gage between click and swipe
    // mouse
    static Vector3 moveAmountMouse;
    static float minMovMagnitudeMouse = 25.0f;

    // Disable the unused variable warning
#pragma warning disable 0414
    // Ensures that the TouchpadHelper will be created automatically upon start of the scene.
    static private LVRTouchpadHelper touchpadHelper =
        (new GameObject("LVRTouchpadHelper")).AddComponent<LVRTouchpadHelper>();
#pragma warning restore 0414

    /// <summary>
    /// Add the Touchpad game object into the scene.
    /// </summary>
    static public void Create()
    {
        // Does nothing but call constructor to add game object into scene
    }

    static public void Update()
    {

        Debug.LogWarning("Touchpad is working");

                // TOUCHPAD INPUT
                if (Input.touchCount > 0)
                {
                    switch(Input.GetTouch(0).phase)
                    {
                        case(TouchPhase.Began):
                            Debug.LogWarning("Touchpad is begin");
                            touchState = TouchState.Down;
                            // Get absolute location of touch
                            moveAmount = Input.GetTouch(0).position;
                            break;

                        case(TouchPhase.Moved):
                            Debug.LogWarning("Touchpad is moving");
                            touchState = TouchState.Move;
                            break;

                        case(TouchPhase.Stationary):
                            Debug.LogWarning("Touchpad is stationary");
                            touchState = TouchState.Stationary;
                            break;

                        case(TouchPhase.Ended):
                            Debug.LogWarning("Touchpad is ended");
                            moveAmount -= Input.GetTouch(0).position;
                            HandleInput(touchState, ref moveAmount);
                            touchState = TouchState.Init;
                            break;

                        case(TouchPhase.Canceled):
                            Debug.Log( "CANCELLED\n" );
                            touchState = TouchState.Init;
                            break;
                    }
                }

        // MOUSE INPUT
        if (Input.GetMouseButtonDown(0))
        {
            moveAmountMouse = Input.mousePosition;
            touchState = TouchState.Down;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.LogWarning("Touchpad is up");
            moveAmountMouse -= Input.mousePosition;
            HandleInputMouse(ref moveAmountMouse);
            touchState = TouchState.Init;
        }
    }

    static public void OnDisable()
    {
    }

    /// <summary>
    /// Determines if input was a click or swipe and sends message to all prescribers.
    /// </summary>
    static void HandleInput(TouchState state, ref Vector2 move)
    {
        if ((move.magnitude < minMovMagnitude) || (touchState == TouchState.Stationary))
        {
            //Debug.Log( "CLICK" );
        }
        else if (touchState == TouchState.Move)
        {
            move.Normalize();

            // Left
            if(Mathf.Abs(move.x) > Mathf.Abs (move.y))
            {
                if(move.x > 0.0f)
                {
                    //Debug.Log( "SWIPE: LEFT" );
                }
                else
                {
                    //Debug.Log( "SWIPE: RIGHT" );
                }
            }
            // Right
            else
            {
                if(move.y > 0.0f)
                {
                    //Debug.Log( "SWIPE: DOWN" );
                }
                else
                {
                    //Debug.Log( "SWIPE: UP" );
                }
            }
        }
    }

    static void HandleInputMouse(ref Vector3 move)
    {
        if (move.magnitude < minMovMagnitudeMouse)
        {
            if (TouchHandler != null)
            {
                TouchHandler(null, new TouchArgs() { TouchType = TouchEvent.SingleTap });
            }
        }
        else
        {
            move.Normalize();

            // Left/Right
            if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            {
                if (move.x > 0.0f)
                {
                    if (TouchHandler != null)
                    {
                        TouchHandler(null, new TouchArgs () { TouchType = TouchEvent.Left });
                    }
                }
                else
                {
                    if (TouchHandler != null)
                    {
                        TouchHandler(null, new TouchArgs () { TouchType = TouchEvent.Right });
                    }
                }
            }
            // Up/Down
            else
            {
                if (move.y > 0.0f)
                {
                    if (TouchHandler != null)
                    {
                        TouchHandler(null, new TouchArgs () { TouchType = TouchEvent.Down });
                    }
                }
                else
                {
                    if(TouchHandler != null)
                    {
                        TouchHandler(null, new TouchArgs () { TouchType = TouchEvent.Up });
                    }
                }
            }
        }
    }
}

/// <summary>
/// This singleton class gets created and stays resident in the application. It is used to
/// trap the touchpad values, which get broadcast to any listener on the "Touchpad" channel.
/// </summary>
public sealed class LVRTouchpadHelper : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Add a listener to the LVRMessenger for testing
        LVRTouchpad.TouchHandler += LocalTouchEventCallback;
    }

    void Update()
    {
        LVRTouchpad.Update();
    }

    public void OnDisable()
    {
        LVRTouchpad.OnDisable();
    }

    void LocalTouchEventCallback(object sender, EventArgs args)
    {
        var touchArgs = (LVRTouchpad.TouchArgs)args;
        LVRTouchpad.TouchEvent touchEvent = touchArgs.TouchType;

        switch(touchEvent)
        {
            case LVRTouchpad.TouchEvent.SingleTap:
                //Debug.Log("SINGLE CLICK\n");
                break;
            case LVRTouchpad.TouchEvent.Left:
                //Debug.Log("LEFT SWIPE\n");
                break;
            case LVRTouchpad.TouchEvent.Right:
                //Debug.Log("RIGHT SWIPE\n");
                break;
            case LVRTouchpad.TouchEvent.Up:
                //Debug.Log("UP SWIPE\n");
                break;
            case LVRTouchpad.TouchEvent.Down:
                //Debug.Log("DOWN SWIPE\n");
                break;
        }
    }
}
