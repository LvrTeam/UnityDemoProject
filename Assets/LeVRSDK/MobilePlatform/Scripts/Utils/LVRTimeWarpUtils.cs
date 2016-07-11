using UnityEngine;
using System.Runtime.InteropServices; // required for DllImport

public class LVRTimeWarpUtils
{
    public enum VsyncMode
    {
        VSYNC_60FPS = 1,
        VSYNC_30FPS = 2,
        VSYNC_20FPS = 3
    }

    public enum DebugPerfMode
    {
        DEBUG_PERF_OFF,         // data still being collected, just not displayed
        DEBUG_PERF_RUNNING,     // display continuously changing graph
        DEBUG_PERF_FROZEN,      // no new data collection, but displayed
        DEBUG_PERF_MAX,
    }

    public enum DebugPerfValue
    {
        DEBUG_VALUE_DRAW,       // start and end times of the draw
        DEBUG_VALUE_LATENCY,    // seconds from eye buffer orientation time
        DEBUG_VALUE_MAX,
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Support to fix 60/30/20 FPS frame rate for consistency or power savings
    [DllImport("LeVRPlugin")]
    private static extern void OVR_TW_SetMinimumVsyncs(VsyncMode mode);

    [DllImport("LeVRPlugin")]
    private static extern void OVR_TW_SetDebugMode(DebugPerfMode mode, DebugPerfValue val);

    [DllImport("LeVRPlugin")]
    private static extern void OVR_TW_EnableChromaticAberration(bool enable);

    // Allow TW to increase the fov by about 10 degrees if we are not holding 60 fps
    // so there is less black pull-in at the edges.
    [DllImport("LeVRPlugin")]
    private static extern void OVR_TW_AllowFovIncrease(bool allow);
#endif
}
