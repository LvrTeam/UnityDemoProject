using System;
using System.Runtime.InteropServices;
using UnityEngine;
using LVR;

/// <summary>
/// Manages an LeVR Rift head-mounted display (HMD).
/// </summary>
public class LVRDisplay
{
    /// <summary>
    /// Specifies the size and field-of-view for one eye texture.
    /// </summary>
    public struct EyeRenderDesc
    {
        /// <summary>
        /// The horizontal and vertical size of the texture.
        /// </summary>
        public Vector2 resolution;

        /// <summary>
        /// The angle of the horizontal and vertical field of view in degrees.
        /// </summary>
        public Vector2 fov;
    }
    /// <summary>
    /// Contains latency measurements for a single frame of rendering.
    /// </summary>
    public struct LatencyData
    {
        /// <summary>
        /// The time it took to render both eyes in seconds.
        /// </summary>
        public float render;

        /// <summary>
        /// The time it took to perform TimeWarp in seconds.
        /// </summary>
        public float timeWarp;

        /// <summary>
        /// The time between the end of TimeWarp and scan-out in seconds.
        /// </summary>
        public float postPresent;
        public float renderError;
        public float timeWarpError;
    }

    /// <summary>
    /// If true, a physical HMD is attached to the system.
    /// </summary>
    /// <value><c>true</c> if is present; otherwise, <c>false</c>.</value>
    public bool isPresent
    {
        get {
#if !UNITY_ANDROID || UNITY_EDITOR
            return (LVRManager.capiHmd.GetTrackingState().StatusFlags & (uint)StatusBits.HmdConnected) != 0;
#else
            return OVR_IsHMDPresent();
#endif
        }
    }

    private int prevAntiAliasing;
    private int prevScreenWidth;
    private int prevScreenHeight;
    private bool needsConfigureTexture;
    private bool needsSetTexture;
    private bool needsSetDistortionCaps;
    private bool prevFullScreen;
    private LVRPose[] eyePoses = new LVRPose[(int)LVREye.Count];
    private EyeRenderDesc[] eyeDescs = new EyeRenderDesc[(int)LVREye.Count];
    private RenderTexture[] eyeTextures = new RenderTexture[eyeTextureCount];
    private int[] eyeTextureIds = new int[eyeTextureCount];
    private int currEyeTextureIdx = 0;
    private static int frameCount = 0;

#if !UNITY_ANDROID && !UNITY_EDITOR
    private bool needsSetViewport;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private const int eyeTextureCount = 3 * (int)LVREye.Count; // triple buffer
#else
    private const int eyeTextureCount = 1 * (int)LVREye.Count;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private int nextEyeTextureIdx = 0;
#endif

    /// <summary>
    /// Creates an instance of LVRDisplay. Called by LVRManager.
    /// </summary>
    public LVRDisplay()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        needsConfigureTexture = false;
        needsSetTexture = true;
        needsSetDistortionCaps = true;
        prevFullScreen = Screen.fullScreen;
#elif !UNITY_ANDROID && !UNITY_EDITOR
        needsSetViewport = true;
#endif

        ConfigureEyeDesc(LVREye.Left);
        ConfigureEyeDesc(LVREye.Right);

        for (int i = 0; i < eyeTextureCount; i += 2)
        {
            ConfigureEyeTexture(i, LVREye.Left);
            ConfigureEyeTexture(i, LVREye.Right);
        }

        LVRManager.NativeTextureScaleModified += (prev, current) => { needsConfigureTexture = true; };
        LVRManager.EyeTextureAntiAliasingModified += (prev, current) => { needsConfigureTexture = true; };
        LVRManager.EyeTextureDepthModified += (prev, current) => { needsConfigureTexture = true; };
        LVRManager.EyeTextureFormatModified += (prev, current) => { needsConfigureTexture = true; };

        LVRManager.VirtualTextureScaleModified += (prev, current) => { needsSetTexture = true; };
        LVRManager.MonoscopicModified += (prev, current) => { needsSetTexture = true; };

        LVRManager.HdrModified += (prev, current) => { needsSetDistortionCaps = true; };
    }

    /// <summary>
    /// Updates the internal state of the LVRDisplay. Called by LVRManager.
    /// </summary>
    public void Update()
    {
        UpdateDistortionCaps();
        UpdateViewport();
        UpdateTextures();
    }

    /// <summary>
    /// Marks the beginning of all rendering.
    /// </summary>
    public void BeginFrame()
    {
        bool updateFrameCount = !(LVRManager.instance.timeWarp && LVRManager.instance.freezeTimeWarp);
        if (updateFrameCount)
        {
            frameCount++;
        }

        LVRPluginEvent.IssueWithData(RenderEventType.BeginFrame, frameCount);
    }

    /// <summary>
    /// Marks the end of all rendering.
    /// </summary>
    public void EndFrame()
    {
        LVRPluginEvent.Issue(RenderEventType.EndFrame);
    }

    /// <summary>
    /// Gets the head pose at the current time or predicted at the given time.
    /// </summary>
    public LVRPose GetHeadPose(double predictionTime = 0d)
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        double abs_time_plus_pred = Hmd.GetTimeInSeconds() + predictionTime;

        TrackingState state = LVRManager.capiHmd.GetTrackingState(abs_time_plus_pred);

        return state.HeadPose.ThePose.ToPose();
#else
        float px = 0.0f, py = 0.0f, pz = 0.0f, ow = 0.0f, ox = 0.0f, oy = 0.0f, oz = 0.0f;

        double atTime = Time.time + predictionTime;
        OVR_GetCameraPositionOrientation(ref  px, ref  py, ref  pz,
                                         ref  ox, ref  oy, ref  oz, ref  ow, atTime);

        return new LVRPose
        {
            position = new Vector3(px, py, -pz),
            orientation = new Quaternion(-ox, -oy, oz, ow),
        };
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private float w = 0.0f, x = 0.0f, y = 0.0f, z = 0.0f, fov = 90.0f;
#endif

    /// <summary>
    /// Gets the pose of the given eye, predicted for the time when the current frame will scan out.
    /// </summary>
    public LVRPose GetEyePose(LVREye eye)
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        bool updateEyePose = !(LVRManager.instance.timeWarp && LVRManager.instance.freezeTimeWarp);
        if (updateEyePose)
        {
            eyePoses[(int)eye] = OVR_GetRenderPose(frameCount, (int)eye).ToPose();
        }

        return eyePoses[(int)eye];
#else
        if (eye == LVREye.Left)
            OVR_GetSensorState(
                    LVRManager.instance.monoscopic,
                    ref w,
                    ref x,
                    ref y,
                    ref z,
                    ref fov,
                    ref LVRManager.timeWarpViewNumber);

        Quaternion rot = new Quaternion(-x, -y, z, w);

        float eyeOffsetX = 0.5f * LVRManager.profile.ipd;
        eyeOffsetX = (eye == LVREye.Left) ? -eyeOffsetX : eyeOffsetX;

        float neckToEyeHeight = LVRManager.profile.eyeHeight - LVRManager.profile.neckHeight;
        Vector3 headNeckModel = new Vector3(0.0f, neckToEyeHeight, LVRManager.profile.eyeDepth);
        Vector3 pos = rot * (new Vector3(eyeOffsetX, 0.0f, 0.0f) + headNeckModel);

        // Subtract the HNM pivot to avoid translating the camera when level
        pos -= headNeckModel;

        return new LVRPose
        {
            position = pos,
            orientation = rot,
        };
#endif
    }

    /// <summary>
    /// Gets the given eye's projection matrix.
    /// </summary>
    /// <param name="eyeId">Specifies the eye.</param>
    /// <param name="nearClip">The distance to the near clipping plane.</param>
    /// <param name="farClip">The distance to the far clipping plane.</param>
    public Matrix4x4 GetProjection(int eyeId, float nearClip, float farClip)
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        FovPort fov = LVRManager.capiHmd.GetDesc().DefaultEyeFov[eyeId];

        uint projectionModFlags = (uint)Hmd.ProjectionModifier.RightHanded;

        return Hmd.GetProjection(fov, nearClip, farClip, projectionModFlags).ToMatrix4x4();
#else
        return new Matrix4x4();
#endif
    }

    /// <summary>
    /// Occurs when the head pose is reset.
    /// </summary>
    public event System.Action RecenteredPose;

    /// <summary>
    /// Recenters the head pose.
    /// </summary>
    public void RecenterPose()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        LVRManager.capiHmd.RecenterPose();
#else
        OVR_ResetSensorOrientation();
#endif

        if (RecenteredPose != null)
        {
            RecenteredPose();
        }
    }

    /// <summary>
    /// Gets the current acceleration of the head.
    /// </summary>
    public Vector3 acceleration
    {
        get {
#if !UNITY_ANDROID || UNITY_EDITOR
            return LVRManager.capiHmd.GetTrackingState().HeadPose.LinearAcceleration.ToVector3();
#else
            float x = 0.0f, y = 0.0f, z = 0.0f;
            OVR_GetAcceleration(ref x, ref y, ref z);
            return new Vector3(x, y, z);
#endif
        }
    }
    /// <summary>
    /// Gets the current angular velocity of the head.
    /// </summary>
    public Vector3 angularVelocity
    {
        get {
#if !UNITY_ANDROID || UNITY_EDITOR
            return LVRManager.capiHmd.GetTrackingState().HeadPose.AngularVelocity.ToVector3();
#else
            float x = 0.0f, y = 0.0f, z = 0.0f;
            OVR_GetAngularVelocity(ref x, ref y, ref z);
            return new Vector3(x, y, z);
#endif
        }
    }

    /// <summary>
    /// Gets the resolution and field of view for the given eye.
    /// </summary>
    public EyeRenderDesc GetEyeRenderDesc(LVREye eye)
    {
        return eyeDescs[(int)eye];
    }

    /// <summary>
    /// Gets the currently active render texture for the given eye.
    /// </summary>
    public RenderTexture GetEyeTexture(LVREye eye)
    {
        return eyeTextures[currEyeTextureIdx + (int)eye];
    }

    /// <summary>
    /// Gets the currently active render texture's native ID for the given eye.
    /// </summary>
    public int GetEyeTextureId(LVREye eye)
    {
        return eyeTextureIds[currEyeTextureIdx + (int)eye];
    }

    /// <summary>
    /// True if the direct mode display driver is active.
    /// </summary>
    public bool isDirectMode
    {
        get
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            uint caps = LVRManager.capiHmd.GetDesc().HmdCaps;
            uint mask = caps & (uint)HmdCaps.ExtendDesktop;
            return mask == 0;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// If true, direct mode rendering will also show output in the main window.
    /// </summary>
    public bool mirrorMode
    {
        get
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            uint caps = LVRManager.capiHmd.GetEnabledCaps();
            return (caps & (uint)HmdCaps.NoMirrorToWindow) == 0;
#else
            return false;
#endif
        }

        set
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            uint caps = LVRManager.capiHmd.GetEnabledCaps();

            if (((caps & (uint)HmdCaps.NoMirrorToWindow) == 0) == value)
                return;

            if (value)
                caps &= ~(uint)HmdCaps.NoMirrorToWindow;
            else
                caps |= (uint)HmdCaps.NoMirrorToWindow;

            LVRManager.capiHmd.SetEnabledCaps(caps);
#endif
        }
    }

    /// <summary>
    /// If true, TimeWarp will be used to correct the output of each LVRCameraRig for rotational latency.
    /// </summary>
    internal bool timeWarp
    {
        get { return (distortionCaps & (int)DistortionCaps.TimeWarp) != 0; }
        set
        {
            if (value != timeWarp)
                distortionCaps ^= (int)DistortionCaps.TimeWarp;
        }
    }

    /// <summary>
    /// If true, VR output will be rendered upside-down.
    /// </summary>
    internal bool flipInput
    {
        get { return (distortionCaps & (int)DistortionCaps.FlipInput) != 0; }
        set
        {
            if (value != flipInput)
                distortionCaps ^= (int)DistortionCaps.FlipInput;
        }
    }

    /// <summary>
    /// Enables and disables distortion rendering capabilities from the Ovr.DistortionCaps enum.
    /// </summary>
    public uint distortionCaps
    {
        get
        {
            return _distortionCaps;
        }

        set
        {
            if (value == _distortionCaps)
                return;

            _distortionCaps = value;
#if !UNITY_ANDROID || UNITY_EDITOR
            OVR_SetDistortionCaps(value);
#endif
        }
    }
    private uint _distortionCaps =
#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
        (uint)DistortionCaps.ProfileNoTimewarpSpinWaits |
#endif
        (uint)DistortionCaps.Chromatic |
        (uint)DistortionCaps.Vignette |
        (uint)DistortionCaps.Overdrive;

    /// <summary>
    /// Gets the current measured latency values.
    /// </summary>
    public LatencyData latency
    {
        get {
#if !UNITY_ANDROID || UNITY_EDITOR
            float[] values = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
            float[] latencies = LVRManager.capiHmd.GetFloatArray("DK2Latency", values);

            return new LatencyData
            {
                render = latencies[0] * 1000.0f,
                timeWarp = latencies[1] * 1000.0f,
                postPresent = latencies[2] * 1000.0f,
                renderError = latencies[3] * 1000.0f,
                timeWarpError = latencies[4] * 1000.0f,
            };
#else
            return new LatencyData
            {
                render = 0.0f,
                timeWarp = 0.0f,
                postPresent = 0.0f,
                renderError = 0.0f,
                timeWarpError = 0.0f,
            };
#endif
        }
    }

    private void UpdateDistortionCaps()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        needsSetDistortionCaps = needsSetDistortionCaps
            || QualitySettings.antiAliasing != prevAntiAliasing;

        if (needsSetDistortionCaps)
        {
            if (QualitySettings.antiAliasing > 0)
            {
                distortionCaps |= (uint)LVR.DistortionCaps.HqDistortion;
            }
            else
            {
                distortionCaps &= ~(uint)LVR.DistortionCaps.HqDistortion;
            }
            if (LVRManager.instance.hdr)
            {
                distortionCaps &= ~(uint)LVR.DistortionCaps.SRGB;
            }
            else
            {
                distortionCaps |= (uint)LVR.DistortionCaps.SRGB;
            }

            prevAntiAliasing = QualitySettings.antiAliasing;

            needsSetDistortionCaps = false;
            needsSetTexture = true;
        }
#endif
    }

    private void UpdateViewport()
    {
#if !UNITY_ANDROID && !UNITY_EDITOR
        needsSetViewport = needsSetViewport
            || Screen.width != prevScreenWidth
            || Screen.height != prevScreenHeight;

        if (needsSetViewport)
        {
            SetViewport(0, 0, Screen.width, Screen.height);

            prevScreenWidth = Screen.width;
            prevScreenHeight = Screen.height;

            needsSetViewport = false;
        }
#endif
    }

    private void UpdateTextures()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        if (needsConfigureTexture)
        {
            ConfigureEyeDesc(LVREye.Left);
            ConfigureEyeDesc(LVREye.Right);

            for (int i = 0; i < eyeTextureCount; i += 2)
            {
                ConfigureEyeTexture(i, LVREye.Left);
                ConfigureEyeTexture(i, LVREye.Right);
            }

            OVR_UnitySetModeChange(true);

            needsConfigureTexture = false;
            needsSetTexture = true;
        }
#endif

        for (int i = 0; i < eyeTextureCount; i++)
        {
            if (!eyeTextures[i].IsCreated())
            {
                eyeTextures[i].Create();
                eyeTextureIds[i] = eyeTextures[i].GetNativeTextureID();

#if !UNITY_ANDROID || UNITY_EDITOR
                needsSetTexture = true;
#endif
            }
        }

#if !UNITY_ANDROID || UNITY_EDITOR
        needsSetTexture = needsSetTexture
            || Screen.fullScreen != prevFullScreen
            || OVR_UnityGetModeChange();

        if (needsSetTexture)
        {
            for (int i = 0; i < eyeTextureCount; i += (int)LVREye.Count)
            {
                int leftEyeIndex = i + (int)LVREye.Left;
                int rightEyeIndex = i + (int)LVREye.Right;

                IntPtr leftEyeTexturePtr = eyeTextures[leftEyeIndex].GetNativeTexturePtr();
                IntPtr rightEyeTexturePtr = eyeTextures[rightEyeIndex].GetNativeTexturePtr();

                if (LVRManager.instance.monoscopic)
                    rightEyeTexturePtr = leftEyeTexturePtr;

                if (leftEyeTexturePtr == System.IntPtr.Zero || rightEyeTexturePtr == System.IntPtr.Zero)
                    return;

                OVR_SetTexture(leftEyeIndex, leftEyeTexturePtr, LVRManager.instance.virtualTextureScale);
                OVR_SetTexture(rightEyeIndex, rightEyeTexturePtr, LVRManager.instance.virtualTextureScale);
            }

            prevFullScreen = Screen.fullScreen;
            OVR_UnitySetModeChange(false);

            needsSetTexture = false;
        }
#else
        currEyeTextureIdx = nextEyeTextureIdx;
        nextEyeTextureIdx = (nextEyeTextureIdx + 2) % eyeTextureCount;
#endif
    }

    private void ConfigureEyeDesc(LVREye eye)
    {
        Vector2 texSize = Vector2.zero;
        Vector2 fovSize = Vector2.zero;

#if !UNITY_ANDROID || UNITY_EDITOR
        FovPort fovPort = LVRManager.capiHmd.GetDesc().DefaultEyeFov[(int)eye];
        fovPort.LeftTan = fovPort.RightTan = Mathf.Max(fovPort.LeftTan, fovPort.RightTan);
        fovPort.UpTan = fovPort.DownTan = Mathf.Max(fovPort.UpTan, fovPort.DownTan);

        texSize = LVRManager.capiHmd.GetFovTextureSize((LVR.Eye)eye, fovPort, LVRManager.instance.nativeTextureScale).ToVector2();
        fovSize = new Vector2(2f * Mathf.Rad2Deg * Mathf.Atan(fovPort.LeftTan), 2f * Mathf.Rad2Deg * Mathf.Atan(fovPort.UpTan));
#else
        texSize = new Vector2(1024, 1024) * LVRManager.instance.nativeTextureScale;
        fovSize = new Vector2(90, 90);
#endif

        eyeDescs[(int)eye] = new EyeRenderDesc()
        {
            resolution = texSize,
            fov = fovSize
        };
    }

    private void ConfigureEyeTexture(int eyeBufferIndex, LVREye eye)
    {
        int eyeIndex = eyeBufferIndex + (int)eye;
        EyeRenderDesc eyeDesc = eyeDescs[(int)eye];

        eyeTextures[eyeIndex] = new RenderTexture(
            (int)eyeDesc.resolution.x,
            (int)eyeDesc.resolution.y,
            (int)LVRManager.instance.eyeTextureDepth,
            LVRManager.instance.eyeTextureFormat);

        eyeTextures[eyeIndex].antiAliasing = (int)LVRManager.instance.eyeTextureAntiAliasing;

        eyeTextures[eyeIndex].Create();
        eyeTextureIds[eyeIndex] = eyeTextures[eyeIndex].GetNativeTextureID();
    }

    public void SetViewport(int x, int y, int w, int h)
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        OVR_SetViewport(x, y, w, h);
#endif
    }

    private const string LibLVR = "LeVRPlugin";

#if UNITY_ANDROID && !UNITY_EDITOR
    //TODO: Get rid of these functions and implement OVR.CAPI.Hmd on Android.

    [DllImport(LibLVR)]
    private static extern bool OVR_ResetSensorOrientation();
    [DllImport(LibLVR)]
    private static extern bool OVR_GetAcceleration(ref float x, ref float y, ref float z);
    [DllImport(LibLVR)]
    private static extern bool OVR_GetAngularVelocity(ref float x, ref float y, ref float z);
    [DllImport(LibLVR)]
    private static extern bool OVR_IsHMDPresent();
    [DllImport(LibLVR)]
    private static extern bool OVR_GetCameraPositionOrientation(
        ref float px,
        ref float py,
        ref float pz,
        ref float ox,
        ref float oy,
        ref float oz,
        ref float ow,
        double atTime);
    [DllImport(LibLVR)]
    private static extern bool OVR_GetSensorState(
        bool monoscopic,
        ref float w,
        ref float x,
        ref float y,
        ref float z,
        ref float fov,
        ref int viewNumber);
#else
    [DllImport(LibLVR, CallingConvention = CallingConvention.Cdecl)]
    private static extern void OVR_SetDistortionCaps(uint distortionCaps);
    [DllImport(LibLVR, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_SetViewport(int x, int y, int w, int h);
    [DllImport(LibLVR, CallingConvention = CallingConvention.Cdecl)]
    private static extern Posef OVR_GetRenderPose(int frameIndex, int eyeId);
    [DllImport(LibLVR, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_SetTexture(int id, System.IntPtr texture, float scale = 1);
    [DllImport(LibLVR, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_UnityGetModeChange();
    [DllImport(LibLVR, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_UnitySetModeChange(bool isChanged);
#endif
}
