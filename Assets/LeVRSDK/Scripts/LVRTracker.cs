using System;
using System.Runtime.InteropServices;
using UnityEngine;
using LVR;

/// <summary>
/// An infrared camera that tracks the position of a head-mounted display.
/// </summary>
public class LVRTracker
{
    /// <summary>
    /// The (symmetric) visible area in front of the tracker.
    /// </summary>
    public struct Frustum
    {
        /// <summary>
        /// The tracker cannot track the HMD unless it is at least this far away.
        /// </summary>
        public float nearZ;
        /// <summary>
        /// The tracker cannot track the HMD unless it is at least this close.
        /// </summary>
        public float farZ;
        /// <summary>
        /// The tracker's horizontal and vertical fields of view in degrees.
        /// </summary>
        public Vector2 fov;
    }

    /// <summary>
    /// If true, a tracker is attached to the system.
    /// </summary>
    public bool isPresent
    {
        get {
#if !UNITY_ANDROID || UNITY_EDITOR
            return (LVRManager.capiHmd.GetTrackingState().StatusFlags & (uint)StatusBits.PositionConnected) != 0;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// If true, the tracker can see and track the HMD. Otherwise the HMD may be occluded or the system may be malfunctioning.
    /// </summary>
    public bool isPositionTracked
    {
        get {
#if !UNITY_ANDROID || UNITY_EDITOR
            return (LVRManager.capiHmd.GetTrackingState().StatusFlags & (uint)StatusBits.PositionTracked) != 0;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// If this is true and a tracker is available, the system will use position tracking when isPositionTracked is also true.
    /// </summary>
    public bool isEnabled
    {
        get {
#if !UNITY_ANDROID || UNITY_EDITOR
            uint trackingCaps = LVRManager.capiHmd.GetDesc().TrackingCaps;
            return (trackingCaps & (uint)TrackingCaps.Position) != 0;
#else
            return false;
#endif
        }

        set {
#if !UNITY_ANDROID || UNITY_EDITOR
            uint trackingCaps = (uint)TrackingCaps.Orientation | (uint)TrackingCaps.MagYawCorrection;

            if (value)
                trackingCaps |= (uint)TrackingCaps.Position;

            LVRManager.capiHmd.ConfigureTracking(trackingCaps, 0);
#endif
        }
    }

    /// <summary>
    /// Gets the tracker's viewing frustum.
    /// </summary>
    public Frustum frustum
    {
        get {
#if !UNITY_ANDROID || UNITY_EDITOR
            HmdDesc desc = LVRManager.capiHmd.GetDesc();

            return new Frustum
            {
                nearZ = desc.CameraFrustumNearZInMeters,
                farZ = desc.CameraFrustumFarZInMeters,
                fov = Mathf.Rad2Deg * new Vector2(desc.CameraFrustumHFovInRadians, desc.CameraFrustumVFovInRadians)
            };
#else
            return new Frustum
            {
                nearZ = 0.1f,
                farZ = 1000.0f,
                fov = new Vector2(90.0f, 90.0f)
            };
#endif
        }
    }

    /// <summary>
    /// Gets the tracker's pose, relative to the head's pose at the time of the last pose recentering.
    /// </summary>
    public LVRPose GetPose(double predictionTime = 0d)
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        double abs_time_plus_pred = Hmd.GetTimeInSeconds() + predictionTime;

        return LVRManager.capiHmd.GetTrackingState(abs_time_plus_pred).CameraPose.ToPose();
#else
        return new LVRPose
        {
            position = Vector3.zero,
            orientation = Quaternion.identity
        };
#endif
    }
}
