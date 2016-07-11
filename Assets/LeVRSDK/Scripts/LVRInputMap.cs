using UnityEngine;
using System.Collections;

public class LVRInputMap : MonoBehaviour
{
    [SerializeField]
    public bool useController = false;

    private string[] controllers;
    public static LeVRKeyCode keyCodes;
    public static LeVRCoolProKeyCode keyCodesPro;
    public static MogaHIDKeyCode keyCodeMogaHID;


    public struct LeVRKeyCode
    {
        public bool KEYCODE_BUTTON_A;
        public bool KEYCODE_BUTTON_B;
        public bool KEYCODE_BUTTON_R1;
        public bool KEYCODE_BACK;
        public float axisX;
        public float axisY;
    }

    public struct LeVRCoolProKeyCode
    {
        public bool BUTTON_SECONDARY;
    }

    // Keycode for moga pro hid gamepad
    public struct MogaHIDKeyCode
    {
        public bool KEYCODE_BUTTON_A;
        public bool KEYCODE_BUTTON_B;
        public bool KEYCODE_BUTTON_X;
        public bool KEYCODE_BUTTON_Y;
        public bool KEYCODE_BUTTON_R1;
        public bool KEYCODE_BUTTON_R2;
        public bool KEYCODE_BUTTON_L1;
        public bool KEYCODE_BUTTON_L2;
        public bool KEYCODE_BUTTON_START;
        public bool KEYCODE_BUTTON_SELECT;
        public float axisX;
        public float axisY;
        public float rightAxisX;
        public float rightAxisY;
        public float dpadX;
        public float dpadY;
    }

    void Update()
    {
        if (useController)
        {
            // For LeVR joystick
            keyCodes.axisX = Input.GetAxis("Horizontal");
            keyCodes.axisY = Input.GetAxis("Vertical");

            // For axis value
            keyCodeMogaHID.axisX = Input.GetAxis("Horizontal");
            keyCodeMogaHID.axisY = Input.GetAxis("Vertical");
            keyCodeMogaHID.rightAxisX = Input.GetAxis("RightHorizontal");
            keyCodeMogaHID.rightAxisY = Input.GetAxis("RightVertical");

            keyCodeMogaHID.dpadX = Input.GetAxis("Dpad X");
            keyCodeMogaHID.dpadY = Input.GetAxis("Dpad Y");

            // LeVR cool pro button event
            keyCodesPro.BUTTON_SECONDARY = Input.GetMouseButtonDown(1);


            if (Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                keyCodes.KEYCODE_BUTTON_A = true;
                keyCodeMogaHID.KEYCODE_BUTTON_A = true;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                keyCodes.KEYCODE_BUTTON_B = true;
                keyCodeMogaHID.KEYCODE_BUTTON_B = true;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                keyCodes.KEYCODE_BACK = true;
                keyCodeMogaHID.KEYCODE_BUTTON_SELECT = true;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton4))
            {
                keyCodeMogaHID.KEYCODE_BUTTON_L1 = true;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                keyCodes.KEYCODE_BUTTON_R1 = true;
                keyCodeMogaHID.KEYCODE_BUTTON_R1 = true;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                keyCodeMogaHID.KEYCODE_BUTTON_X = true;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                keyCodeMogaHID.KEYCODE_BUTTON_Y = true;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton10))
            {
                keyCodeMogaHID.KEYCODE_BUTTON_START = true;
            }
            else
            {
                // Set LeVR keycode states
                keyCodes.KEYCODE_BUTTON_A = false;
                keyCodes.KEYCODE_BUTTON_B = false;
                keyCodes.KEYCODE_BACK = false;
                keyCodes.KEYCODE_BUTTON_R1 = false;

                // Set moga pro hid key states
                keyCodeMogaHID.KEYCODE_BUTTON_A = false;
                keyCodeMogaHID.KEYCODE_BUTTON_B = false;
                keyCodeMogaHID.KEYCODE_BUTTON_X = false;
                keyCodeMogaHID.KEYCODE_BUTTON_Y = false;
                keyCodeMogaHID.KEYCODE_BUTTON_R1 = false;
                keyCodeMogaHID.KEYCODE_BUTTON_R2 = false;
                keyCodeMogaHID.KEYCODE_BUTTON_L1 = false;
                keyCodeMogaHID.KEYCODE_BUTTON_L2 = false;
                keyCodeMogaHID.KEYCODE_BUTTON_START = false;
                keyCodeMogaHID.KEYCODE_BUTTON_SELECT = false;
            }

            Event e = Event.current;
            if (e.isKey)
            {
                Debug.Log("key:" + e.keyCode);
            }
            else if (e.isMouse)
            {
                Debug.Log("event:");
            }
        }
    }
}
