using UnityEngine;

public class LVRResetOrientation : MonoBehaviour
{
    public LVRGamepadController.Button resetButton = LVRGamepadController.Button.Y;

    /// <summary>
    /// Check input and reset orientation if necessary
    /// See the input mapping setup in the Unity Integration guide
    /// </summary>
    void Update()
    {
        // NOTE: some of the buttons defined in LVRGamepadController.Button are not available on the Android game pad controller
        if (Input.GetButtonDown(LVRGamepadController.ButtonNames[(int)resetButton]))
        {
            //*************************
            // reset orientation
            //*************************
            LVRManager.display.RecenterPose();
        }
    }
}
