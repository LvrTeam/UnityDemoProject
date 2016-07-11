using UnityEngine;
using UnityEditor;

class LVRBuild
{
    // Build the Android APK and place into main project folder
    static void PerformBuildAndroidAPK()
    {
        if (Application.isEditor)
        {
            string[] scenes = { EditorApplication.currentScene };
            BuildPipeline.BuildPlayer(scenes, "LeVRUnityDemoScene.apk", BuildTarget.Android, BuildOptions.None);
        }
    }
}

partial class LVRBuildApp
{
    static void SetAndroidTarget()
    {
#if UNITY_5_0
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
#else
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
#endif
    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
        }
    }
}
