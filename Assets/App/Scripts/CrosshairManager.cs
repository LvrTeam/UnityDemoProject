/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class CrosshairManager : MonoBehaviour
{

    /// <summary>
    /// Get CenterEyeAnchor game object
    /// </summary>
    GameObject cameraAnchor;

    /// <summary>
    /// Crosshair auto depth
    /// </summary>
    bool autoDepth = true;

    /// <summary>
    /// Turn on/off for Crosshair
    /// </summary>
    bool activeCrosshair = true;

    /// <summary>
    /// Default crosshair scale when not using autoscale
    /// </summary>
    float crosshairDefaultScale = 0.5f;

    /// <summary>
    /// distance from camera to crosshair in metres
    /// </summary>
    float crosshairDepth;

    /// <summary>
    /// crosshair default depth
    /// </summary>
    public float crosshairDefaultDepth;

    /// <summary>
    /// When autoscaling, the angular size in degrees of crosshair
    /// </summary>
    public float crosshairTargetAnglularSize;

    /// <summary>
    /// For autoscaling to work correctly this must be the world size of the crosshair object when its scale is 1
    /// </summary>
    public float unscaledCrosshairDiameter;

    public LVRCameraRig cameraController = null;
    private Vector3 originalScale;
    /// <summary>
    /// Start in MonoBehaviour:Initialize crosshair components
    /// </summary>
	void Start () 
    {
        cameraAnchor = GameObject.Find("CenterEyeAnchor");      
        crosshairDepth = crosshairDefaultDepth;
        originalScale = this.transform.localScale;

    }

    /// <summary>
    /// Update in MonoBehaviour
    /// </summary>	
    void Update()
    {
        float newCrossHairDepth = crosshairDefaultDepth;
        //update crosshair direction
        Vector3 cameraPosition = cameraController.centerEyeAnchor.position;
        Vector3 cameraForward = cameraController.centerEyeAnchor.forward;
        //transform.position = cameraAnchor.transform.position + (cameraAnchor.transform.forward * crosshairDepth);
        //this.transform.forward = cameraForward * crosshairDepth;

        RaycastHit hit;
        float distance;
        if (Physics.Raycast(cameraAnchor.transform.position, cameraAnchor.transform.forward, out hit))
        {
            Debug.Log("Hit objname:" + hit.collider.name);
            Debug.Log("Hit tag:" + hit.transform.tag);
            distance = hit.distance;
            switch (hit.transform.tag)
            {
                case "video":
                    newCrossHairDepth = hit.distance;
                    GameObject gameObj = hit.collider.gameObject;
                    Debug.Log("Hit objname:" + gameObj.name + "Hit objlayer:" + gameObj.layer);
                    break;
            }
        }
        else
        {
            distance = 10.0f;
        }
        this.transform.position = cameraAnchor.transform.position +
            cameraAnchor.transform.rotation * Vector3.forward * distance;
        this.transform.position = cameraPosition + (cameraForward * 3);
        this.transform.LookAt(cameraAnchor.transform.position);
        this.transform.Rotate(0.0f,180.0f,0.0f);
        //this.transform.localScale = Vector3.one * distance;
        /*
        if(distance < 10.0f)
        {
            distance *= 1 + 5 * Mathf.Exp(-distance);
        }
        this.transform.localScale = originalScale * distance;
        */
        //crosshairDepth = newCrossHairDepth;
        //transform.localPosition = new Vector3(0.0f, 0.0f, distance);
        /*
        // Calculate size that would be required in order have the target angular size
        float desiredSize = Mathf.Tan(crosshairTargetAnglularSize * Mathf.Deg2Rad * 0.5f) * 2 * distance;
        // Find and set required scale
        float requiredScale = desiredSize / unscaledCrosshairDiameter;
        transform.localScale = new Vector3(requiredScale, requiredScale, requiredScale);
        */
    }

    /// <summary>
    /// Set whether auto scaling is on
    /// </summary>
    /// /
    /*
    public void SetAutoScale(bool on)
    {
        autoScale = on;
        sizeSlider.interactable = autoScale;
        if (!autoScale)
        {
            image.transform.localScale = new Vector3(crosshairDefaultScale, crosshairDefaultScale, crosshairDefaultScale);
        }
    }
    */
}
