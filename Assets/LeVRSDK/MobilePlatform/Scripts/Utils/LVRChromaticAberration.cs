using UnityEngine;
using System.Runtime.InteropServices;		// required for DllImport

public class LVRChromaticAberration : MonoBehaviour {

    public LVRGamepadController.Button			toggleButton = LVRGamepadController.Button.X;
    private bool								chromatic = false;

#if (UNITY_ANDROID && !UNITY_EDITOR)
    [DllImport("LeVRPlugin")]
    private static extern void OVR_TW_EnableChromaticAberration( bool enable );
#endif

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start ()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        // Enable/Disable Chromatic Aberration Correction.
        // NOTE: Enabling Chromatic Aberration for mobile has a large performance cost.
        OVR_TW_EnableChromaticAberration(chromatic);
#endif
    }

    /// <summary>
    /// Check input and toggle chromatic aberration correction if necessary.
    /// See the input mapping setup in the Unity Integration guide.
    /// </summary>
    void Update()
    {
        // NOTE: some of the buttons defined in LVRGamepadController.Button are not available on the Android game pad controller
        if (Input.GetButtonDown(LVRGamepadController.ButtonNames[(int)toggleButton]))
        {
            //*************************
            // toggle chromatic aberration correction
            //*************************
            chromatic = !chromatic;
#if (UNITY_ANDROID && !UNITY_EDITOR)
            OVR_TW_EnableChromaticAberration(chromatic);
#endif
        }
    }

}
