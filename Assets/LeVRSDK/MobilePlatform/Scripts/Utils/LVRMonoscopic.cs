using UnityEngine;

public class LVRMonoscopic : MonoBehaviour {

    public LVRGamepadController.Button	toggleButton = LVRGamepadController.Button.B;
    private bool						monoscopic = false;

    /// <summary>
    /// Check input and toggle monoscopic rendering mode if necessary
    /// See the input mapping setup in the Unity Integration guide
    /// </summary>
    void Update()
    {
        // NOTE: some of the buttons defined in LVRGamepadController.Button are not available on the Android game pad controller
        if (Input.GetButtonDown(LVRGamepadController.ButtonNames[(int)toggleButton]))
        {
            //*************************
            // toggle monoscopic rendering mode
            //*************************
            monoscopic = !monoscopic;
            LVRManager.instance.monoscopic = monoscopic;
        }
    }

}
