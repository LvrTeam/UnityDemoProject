using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

[InitializeOnLoad]
class LVRMoonlightLoader
{
    static LVRMoonlightLoader()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            return;

        if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.LandscapeLeft)
        {
            Debug.Log("MoonlightLoader: Setting orientation to Landscape Left");
            // Default screen orientation must be set to landscape left.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        }

        // NOTE: On Adreno Lollipop, it is an error to have antiAliasing set on the
        // main window surface with front buffer rendering enabled. The view will
        // render black.
        // On Adreno KitKat, some tiling control modes will cause the view to render
        // black.
        if (QualitySettings.antiAliasing != 0 && QualitySettings.antiAliasing != 1)
        {
            Debug.Log("MoonlightLoader: Disabling antiAliasing");
            QualitySettings.antiAliasing = 1;
        }

        if (QualitySettings.vSyncCount != 0)
        {
            Debug.Log("MoonlightLoader: Setting vsyncCount to 0");
            // We sync in the TimeWarp, so we don't want unity syncing elsewhere.
            QualitySettings.vSyncCount = 0;
        }
    }
}
