using UnityEngine;

public class LVRWaitCursor : MonoBehaviour
{
    public Vector3 rotateSpeeds = new Vector3(0.0f, 0.0f, -60.0f);
    private Transform thisTransform = null;

    /// <summary>
    /// Initialization.
    /// </summary>
    void Awake()
    {
        thisTransform = transform;
    }

    /// <summary>
    /// Auto rotates the attached cursor.
    /// </summary>
    void Update()
    {
        thisTransform.Rotate(rotateSpeeds * Time.smoothDeltaTime);
    }
}
