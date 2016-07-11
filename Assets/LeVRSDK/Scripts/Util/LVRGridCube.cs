using UnityEngine;
using System.Collections;

/// <summary>
/// Diagnostic display with a regular grid of cubes for visual testing of
/// tracking and distortion.
/// </summary>
public class LVRGridCube : MonoBehaviour
{
    /// <summary>
    /// The key that toggles the grid of cubes.
    /// </summary>
    public KeyCode GridKey                     = KeyCode.G;

    private GameObject 	CubeGrid			   = null;

    private bool 	CubeGridOn		    	   = false;
    private bool 	CubeSwitchColorOld  	   = false;
    private bool 	CubeSwitchColor     	   = false;

    private int   gridSizeX  = 6;
    private int   gridSizeY  = 4;
    private int   gridSizeZ  = 6;
    private float gridScale  = 0.3f;
    private float cubeScale  = 0.03f;

    private float horizontalStep = 0.0f;
    private float verticalStep = 0.0f;

    // Handle to LVRCameraRig
    private LVRCameraRig CameraController = null;

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update ()
    {
        UpdateCubeGrid();
    }

    /// <summary>
    /// Sets the LVR camera controller.
    /// </summary>
    /// <param name="cameraController">Camera controller.</param>
    public void SetLVRCameraController(ref LVRCameraRig cameraController)
    {
        CameraController = cameraController;
    }

    void UpdateCubeGrid()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            LVR3DText.SetButtonString("Button A Is Pressed");

            // Do what you want
            if (CubeGridOn == false)
            {
                CubeGridOn = true;
                Debug.LogWarning("CubeGrid ON");
                if (CubeGrid != null)
                    CubeGrid.SetActive(true);
                else
                    CreateCubeGrid();
            }
            else
            {
                CubeGridOn = false;
                Debug.LogWarning("CubeGrid OFF");

                if (CubeGrid != null)
                    CubeGrid.SetActive(false);
            }
        }

        if (CubeGrid != null)
        {
            // Set cube colors to let user know if camera is tracking
            CubeSwitchColor = !LVRManager.tracker.isPositionTracked;

            if(CubeSwitchColor != CubeSwitchColorOld)
                CubeGridSwitchColor(CubeSwitchColor);
            CubeSwitchColorOld = CubeSwitchColor;
        }
    }

    void CreateCubeGrid()
    {
        Debug.LogWarning("Create CubeGrid");

        // Create the visual cube grid
        CubeGrid = new GameObject("CubeGrid");
        // Set a layer to target a specific camera
        CubeGrid.layer = CameraController.gameObject.layer;

        for (int x = -gridSizeX; x <= gridSizeX; x++)
            for (int y = -gridSizeY; y <= gridSizeY; y++)
                for (int z = -gridSizeZ; z <= gridSizeZ; z++)
            {
                // Set the cube type:
                // 0 = non-axis cube
                // 1 = axis cube
                // 2 = center cube
                int CubeType = 0;
                if ((x == 0 && y == 0) || (x == 0 && z == 0) || (y == 0 && z == 0))
                {
                    if((x == 0) && (y == 0) && (z == 0))
                        CubeType = 2;
                    else
                        CubeType = 1;
                }

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                BoxCollider bc = cube.GetComponent<BoxCollider>();
                bc.enabled = false;        
                cube.layer = CameraController.gameObject.layer;
                // No shadows
                Renderer r = cube.GetComponent<Renderer>();
                r.castShadows    = false;
                r.receiveShadows = false;

                // Cube line is white down the middle
                if (CubeType == 0)
                    r.material.color = Color.red;
                else if (CubeType == 1)
                    r.material.color = Color.white;
                else
                    r.material.color = Color.yellow;

                cube.transform.position =
                    new Vector3(((float)x * gridScale),
                                ((float)y * gridScale),
                                ((float)z * gridScale));

                float s = 0.7f;

                // Axis cubes are bigger
                if(CubeType == 1)
                    s = 1.0f;
                // Center cube is the largest
                if(CubeType == 2)
                    s = 2.0f;

                cube.transform.localScale =
                    new Vector3(cubeScale * s, cubeScale * s, cubeScale * s);

                cube.transform.parent = CubeGrid.transform;
            }
    }

    /// <summary>
    /// Switch the Cube grid color.
    /// </summary>
    /// <param name="CubeSwitchColor">If set to <c>true</c> cube switch color.</param>
    void CubeGridSwitchColor(bool CubeSwitchColor)
    {
        Color c = Color.red;
        if(CubeSwitchColor == true)
            c = Color.blue;

        foreach(Transform child in CubeGrid.transform)
        {
            Material m = child.GetComponent<Renderer>().material;
            // Cube line is white down the middle
            if(m.color == Color.red || m.color == Color.blue)
                m.color = c;
        }
    }
}
