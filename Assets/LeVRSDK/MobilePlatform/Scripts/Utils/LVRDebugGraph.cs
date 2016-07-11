using UnityEngine;
using System.Runtime.InteropServices; // required for DllImport

public class LVRDebugGraph : MonoBehaviour
{
    public LVRTimeWarpUtils.DebugPerfMode debugMode = LVRTimeWarpUtils.DebugPerfMode.DEBUG_PERF_OFF;
    public LVRGamepadController.Button toggleButton = LVRGamepadController.Button.Start;

#if (UNITY_ANDROID && !UNITY_EDITOR)
    [DllImport("LeVRPlugin")]
    private static extern void OVR_TW_SetDebugMode(LVRTimeWarpUtils.DebugPerfMode mode, LVRTimeWarpUtils.DebugPerfValue val);
#endif

    /// <summary>
    /// Initialize the debug mode
    /// </summary>
    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Turn on/off debug graph
        OVR_TW_SetDebugMode(debugMode, LVRTimeWarpUtils.DebugPerfValue.DEBUG_VALUE_DRAW);
#endif
    }

    /// <summary>
    /// Check input and toggle the debug graph.
    /// See the input mapping setup in the Unity Integration guide.
    /// </summary>
    void Update()
    {
        // NOTE: some of the buttons defined in LVRGamepadController.Button are not available on the Android game pad controller
        if (Input.GetButtonDown( LVRGamepadController.ButtonNames[(int)toggleButton]))
        {
            Debug.Log(" TOGGLE GRAPH ");

            //*************************
            // toggle the debug graph .. off -> running -> paused
            //*************************
            switch (debugMode)
            {
                case LVRTimeWarpUtils.DebugPerfMode.DEBUG_PERF_OFF:
                    debugMode = LVRTimeWarpUtils.DebugPerfMode.DEBUG_PERF_RUNNING;
                    break;
                case LVRTimeWarpUtils.DebugPerfMode.DEBUG_PERF_RUNNING:
                    debugMode = LVRTimeWarpUtils.DebugPerfMode.DEBUG_PERF_FROZEN;
                    break;
                case LVRTimeWarpUtils.DebugPerfMode.DEBUG_PERF_FROZEN:
                    debugMode = LVRTimeWarpUtils.DebugPerfMode.DEBUG_PERF_OFF;
                    break;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            OVR_TW_SetDebugMode(debugMode, LVRTimeWarpUtils.DebugPerfValue.DEBUG_VALUE_DRAW);
#endif
        }
    }
}
