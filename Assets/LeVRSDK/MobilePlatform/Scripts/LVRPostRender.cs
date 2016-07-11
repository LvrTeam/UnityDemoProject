using System;
using UnityEngine;
using System.Collections; // required for Coroutines

public class LVRPostRender : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    public static event Action OnCustomPostRender = null;
#endif

    void OnPostRender()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Allow custom code to render before we kick off the plugin
        if (OnCustomPostRender != null)
        {
            OnCustomPostRender();
        }

        LVREye eye = ((RenderEventType)Camera.current.depth == RenderEventType.RightEyeEndFrame) ?
            LVREye.Right : LVREye.Left;
        LVRManager.EndEye(eye);
#endif
    }
}
