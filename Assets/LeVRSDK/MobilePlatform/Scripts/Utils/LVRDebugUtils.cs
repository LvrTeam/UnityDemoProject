#define DEBUG

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class LVRDebugUtils
{
#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// When running on adroid and connected via adb (either USB of WIFI), you can
    /// use logcat to see output from LVRDebugUtils.Print.  To do so use the following
    /// command at a command prompt / shell:
    /// adb logcat -s LVRDEBUG
    /// </summary>
    static string debugTag = "LVRDEBUG";

    [DllImport("LeVRPlugin")]
    private static extern int OVR_DebugPrint(string tag, string message);

    public static void SetDebugTag(string tag)
    {
        debugTag = tag;
    }
#endif

    /// <summary>
    /// Throws an exception if the condition is false and prints the
    /// the stack for the calling function.
    /// </summary>
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string exprTag = "<UNKNOWN>")
    {
        if (!condition)
        {
            StackTrace st = new StackTrace(new StackFrame(true));
            StackFrame sf = st.GetFrame(1);
            Print("Assertion( " + exprTag + " ): File '" + sf.GetFileName() + "', Line " + sf.GetFileLineNumber() + ".");
            throw new Exception();
        }
    }

    /// <summary>
    /// Prints a message to the Unity console, or to the debug log on Android.
    /// </summary>
    public static void Print(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        OVR_DebugPrint(debugTag, message);
#else
        UnityEngine.Debug.LogWarning(message);
#endif
    }
};
