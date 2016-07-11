using System.Collections;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Communicates the editor's Game view rect to the LeVR plugin,
/// allowing distortion rendering to target it.
/// </summary>
[InitializeOnLoad]
public class LVRGameView
{
    private static Vector2 cachedPos;

    static LVRGameView()
    {
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
//        if (LVRManager.instance != null)
//        {
//            Vector2 pos = Handles.GetMainGameViewSize();
//            if (cachedPos != pos)
//            {
//                cachedPos = pos;
//                LVRManager.display.SetViewport(0, 0, (int)pos.x, (int)pos.y);
//            }
//        }
    }
}
