using UnityEngine;
using System.Collections;

public class LVRVolumeControl : MonoBehaviour
{
    private const float 		showPopupTime = 3;
    private const float			popupOffsetY = 64.0f / 500.0f;
    private const float			popupDepth = 1.8f;
    private const int 			maxVolume = 15;
    private const int 			numVolumeImages = maxVolume + 1;

    private Transform			myTransform = null;

    void Start()
    {
        DontDestroyOnLoad( gameObject );
        myTransform = transform;
        GetComponent<Renderer>().enabled = false;
    }

    /// <summary>
    /// Updates the position of the volume popup.
    /// </summary>
    public virtual void UpdatePosition(Transform cameraTransform)
    {
        // LVRDevice.GetTimeSinceLastVolumeChange() will return -1 if the volume listener hasn't initialized yet,
        // which sometimes takes place after a frame has run in Unity.
        double timeSinceLastVolumeChange = LVRManager.timeSinceLastVolumeChange;
        if ((timeSinceLastVolumeChange != -1) && (timeSinceLastVolumeChange < showPopupTime))
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.0f, (float)(maxVolume - LVRManager.volumeLevel) / (float)numVolumeImages);
            if (myTransform != null && cameraTransform != null)
            {
                // place in front of camera
                myTransform.rotation = cameraTransform.rotation;
                myTransform.position = cameraTransform.position + (myTransform.forward * popupDepth) + (myTransform.up * popupOffsetY);
            }
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }
    }
}
