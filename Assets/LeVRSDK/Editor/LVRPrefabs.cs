using UnityEngine;
using System.Collections;
using UnityEditor;

//-------------------------------------------------------------------------------------
// ***** LVRPrefabs
//
// LeVR Prefabs adds menu items under the LeVR main menu. It allows for quick creation
// of the main LeVR prefabs without having to open the Prefab folder and dragging/dropping
// into the scene.
class LVRPrefabs
{
    static void CreateLVRCameraController ()
    {
        Object ovrcam = AssetDatabase.LoadAssetAtPath ("Assets/LeVRSDK/Prefabs/LVRCameraRig.prefab", typeof(UnityEngine.Object));
        PrefabUtility.InstantiatePrefab(ovrcam);
    }

    static void CreateLVRPlayerController ()
    {
        Object ovrcam = AssetDatabase.LoadAssetAtPath ("Assets/LeVRSDK/Prefabs/LVRPlayerController.prefab", typeof(UnityEngine.Object));
        PrefabUtility.InstantiatePrefab(ovrcam);
    }
}
