using UnityEngine;
using System.Collections;

public class LVR3DText : MonoBehaviour {

    private float m_DeltaTime;                      // This is the smoothed out time between frames.
    private static TextMesh m_Text;                            // Reference to the component that displays the fps.


    private const float k_SmoothingCoef = 0.1f;     // This is used to smooth out the displayed fps.
                                                    // Handle to LVRCameraRig
    private LVRCameraRig CameraController1 = null;

    private void Start()
    {
        m_Text = GetComponent<TextMesh>();
    }

    private void Update()
    {
        // This line has the effect of smoothing out delta time.
        m_DeltaTime += (Time.deltaTime - m_DeltaTime) * k_SmoothingCoef;

        // The frames per second is the number of frames this frame (one) divided by the time for this frame (delta time).
        float fps = 1.0f / m_DeltaTime;

        // Set the displayed value of the fps to be an integer.
        //m_Text.text = Mathf.FloorToInt(fps) + " fps";

//        Quaternion cameraDirection = LVRManager.display.GetEyePose(LVREye.Left).orientation;
//        m_Text.transform.rotation = cameraDirection;
//        m_Text.transform.Translate(cameraDirection.y * LVRInputMap.keyCodes.axisX, 0, 0);
//        m_Text.transform.rotation = cameraDirection;
    }

    public static void SetButtonString(string str)
    {
        m_Text.text = str;
    }
}
