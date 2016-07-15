/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.3 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculus.com/licenses/LICENSE-3.3

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Collections;

// required for Coroutines
using System.Runtime.InteropServices;

// required for DllImport
using System;

// requred for IntPtr
using System.IO;

// required for File

/************************************************************************************
Usage:

	Place a simple textured quad surface with the correct aspect ratio in your scene.

	Add the MoviePlayerSample.cs script to the surface object.

	Supply the name of the media file to play:
	This sample assumes the media file is placed in "Assets/StreamingAssets", ie
	"ProjectName/Assets/StreamingAssets/MovieName.mp4".

	On Desktop, Unity MovieTexture functionality is used. Note: the media file
	is loaded at runtime, and therefore expected to be converted to Ogg Theora
	beforehand.

Implementation:

	In the MoviePlayerSample Awake() call, GetNativeTexturePtr() is called on 
	renderer.material.mainTexture.
	
	When the MediaSurface plugin gets the initialization event on the render thread, 
	it creates a new Android SurfaceTexture and Surface object in preparation 
	for receiving media. 

	When the game wants to start the video playing, it calls the StartVideoPlayerOnTextureId()
	script call, which creates an Android MediaPlayer java object, issues a 
	native plugin call to tell the native code to set up the target texture to
	render the video to and return the Android Surface object to pass to MediaPlayer,
	then sets up the media stream and starts it.
	
	Every frame, the SurfaceTexture object is checked for updates.  If there 
	is one, the target texId is re-created at the correct dimensions and format
	if it is the first frame, then the video image is rendered to it and mipmapped.  
	The following frame, instead of Unity drawing the image that was placed 
	on the surface in the Unity editor, it will draw the current video frame.

************************************************************************************/
using System.Runtime.Remoting.Channels;

public class VideoPlayer : BaseBehaviour
{
	public string movieName = string.Empty;
	private string mediaFullPath = string.Empty;
	private bool startedVideo = false;
	private bool videoPaused = false;

	private Texture2D nativeTexture = null;
	private IntPtr nativeTexId = IntPtr.Zero;
	private int textureWidth = 2880;
	private int textureHeight = 1440;
	private AndroidJavaObject mediaPlayer = null;
	private Renderer mediaRenderer = null;

	private enum MediaSurfaceEventType
	{
		Initialize = 0,
		Shutdown = 1,
		Update = 2,
		Max_EventType

	}

	/// <summary>
	/// The start of the numeric range used by event IDs.
	/// </summary>
	/// <description>
	/// If multiple native rundering plugins are in use, the Oculus Media Surface plugin's event IDs
	/// can be re-mapped to avoid conflicts.
	/// 
	/// Set this value so that it is higher than the highest event ID number used by your plugin.
	/// Oculus Media Surface plugin event IDs start at eventBase and end at eventBase plus the highest
	/// value in MediaSurfaceEventType.
	/// </description>
	/// 
	public static int eventBase {
		get { return _eventBase; }
		set {
			_eventBase = value;
			OVR_Media_Surface_SetEventBase (_eventBase);
		}
	}

	private static int _eventBase = 0;

	private static void IssuePluginEvent (MediaSurfaceEventType eventType)
	{
		GL.IssuePluginEvent ((int)eventType + eventBase);
	}

	/// <summary>
	/// Initialization of the movie surface
	/// </summary>
	void Awake ()
	{
		Debug.Log ("MovieSample Awake");

		OVR_Media_Surface_Init ();

		mediaRenderer = GetComponent<Renderer> ();

		if (mediaRenderer.material == null || mediaRenderer.material.mainTexture == null) {
			Debug.LogError ("No material for movie surface");
		}

		if (movieName != string.Empty) {
			RetrieveStreamingAsset (movieName);
		} else {
			Debug.LogError ("No media file name provided");
		}


		nativeTexture = Texture2D.CreateExternalTexture (textureWidth, textureHeight,
			TextureFormat.RGBA32, true, false,
			IntPtr.Zero);
		IssuePluginEvent (MediaSurfaceEventType.Initialize);

	}

	/// <summary>
	/// Construct the streaming asset path.
	/// Note: For Android, we need to retrieve the data from the apk.
	/// </summary>
	private void RetrieveStreamingAsset (string mediaFileName)
	{
		string persistentPath = "sdcard" + "/" + mediaFileName;
		Debug.Log ("file path: " + persistentPath);
		mediaFullPath = persistentPath;

		Debug.Log ("Movie FullPath: " + mediaFullPath);
	}

	/// <summary>
	/// Auto-starts video playback
	/// </summary>
	IEnumerator DelayedStartVideo ()
	{
		yield return null; // delay 1 frame to allow MediaSurfaceInit from the render thread.

		if (!startedVideo) {
			Debug.Log ("Mediasurface DelayedStartVideo");

			startedVideo = true;
			mediaPlayer = StartVideoPlayerOnTextureId (textureWidth, textureHeight, mediaFullPath);
			mediaRenderer.material.mainTexture = nativeTexture;

		}
	}

	void Start ()
	{
		Debug.Log ("MovieSample Start");
		StartCoroutine (DelayedStartVideo ());
	}

	void Update ()
	{
		IntPtr currTexId = OVR_Media_Surface_GetNativeTexture ();
		if (currTexId != nativeTexId) {
			nativeTexId = currTexId;
			nativeTexture.UpdateExternalTexture (currTexId);
		}

		IssuePluginEvent (MediaSurfaceEventType.Update);

		if ((Input.GetKeyDown (KeyCode.Escape)) || Input.GetMouseButtonDown (1) == true)
			Destroy (this);
	}

	/// <summary>
	/// Pauses video playback when the app loses or gains focus
	/// </summary>
	void OnApplicationPause (bool wasPaused)
	{
		Debug.Log ("OnApplicationPause: " + wasPaused);
		if (mediaPlayer != null) {
			videoPaused = wasPaused;
			try {
				mediaPlayer.Call ((videoPaused) ? "pause" : "start");
			} catch (Exception e) {
				Debug.Log ("Failed to start/pause mediaPlayer with message " + e.Message);
			}
		}

	}

	private void OnApplicationQuit ()
	{
		Debug.Log ("OnApplicationQuit");
		
		// This will trigger the shutdown on the render thread
		IssuePluginEvent (MediaSurfaceEventType.Shutdown);
	}

	/// <summary>
	/// Set up the video player with the movie surface texture id.
	/// </summary>
	AndroidJavaObject StartVideoPlayerOnTextureId (int texWidth, int texHeight, string mediaPath)
	{

		AndroidJavaObject activity = GetActivity ();

//		activity.Call ("showToast","StartVideoPlayerOnTextureId");

		OVR_Media_Surface_SetTextureParms (textureWidth, textureHeight);
		IntPtr androidSurface = OVR_Media_Surface_GetObject ();
//		AndroidJavaObject mediaPlayer = new AndroidJavaObject("android/media/MediaPlayer");
//		// Can't use AndroidJavaObject.Call() with a jobject, must use low level interface
//		//mediaPlayer.Call("setSurface", androidSurface);
//		IntPtr setSurfaceMethodId = AndroidJNI.GetMethodID(mediaPlayer.GetRawClass(),"setSurface","(Landroid/view/Surface;)V");
//		jvalue[] parms = new jvalue[1];
//		parms[0] = new jvalue();
//		parms[0].l = androidSurface;
//		AndroidJNI.CallVoidMethod(mediaPlayer.GetRawObject(), setSurfaceMethodId, parms);

		AndroidJavaClass jclass = new AndroidJavaClass ("com/letv/spo/mediaplayerex/MediaPlayerEx$Factory");
		AndroidJavaObject mediaPlayer = jclass.CallStatic<AndroidJavaObject> ("newInstance", 2);
//		AndroidJavaObject mediaPlayer= new AndroidJavaObject("com/letv/spo/mediaplayerex/SpoPlayer"); 
//				 Can't use AndroidJavaObject.Call() with a jobject, must use low level interface
//		mediaPlayer.Call("setSurface", androidSurface);
		IntPtr setSurfaceMethodId = AndroidJNI.GetMethodID (mediaPlayer.GetRawClass (), "setSurface", "(Landroid/view/Surface;)V");
		jvalue[] parms = new jvalue[1];
		parms [0] = new jvalue ();
		parms [0].l = androidSurface;
		AndroidJNI.CallVoidMethod (mediaPlayer.GetRawObject (), setSurfaceMethodId, parms);

//		activity.Call ("runOnUiThread", new AndroidJavaRunnable (()=>{
		try {
			AndroidJavaClass uriClass = new AndroidJavaClass ("android/net/Uri");
			AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject> ("parse", "http://127.0.0.1:6999/play?enc=base64&url=aHR0cDovL3BsYXkuZzNwcm94eS5sZWNsb3VkLmNvbS92b2QvdjIvTVRrM0x6STVMekkxTDJ4bGRIWXRkWFJ6THpFMEwzWmxjbDh3TUY4eU1pMHhNRFV6T0RNM01UQXpMV0YyWXkwM09UY3lNVFk1TFdGaFl5MHlOVFl3TURBdE1UVXlNRE0wTFRFMU5qWXpNVGd3TkMwMFpUUTRNMk0yTm1ZNE56QTJZakkwWW1ZME9XSTJZVGhtTVRZME1qSm1ZaTB4TkRZM05qa3hPRE0yTnpZMUxtMXdOQT09P2I9ODIzNyZjdmlkPTEyNDE4MjkwMTQ0NzYma2V5PTA4ZDFhYTg1MWIyYWNmYWZjYThjNTFmZDE2YWIxNTllJmxzYnY9MmZZJmxzZGc9S0RjZURNaWc0bDNIODJmaVlsUms3eDJ5cDFQRyZsc3N0PTEmbHNzdj00ZnV4MUlfVDFfU09KX1ZmaktfUEQyQzA3MzFBQUU1NDk1QTdGNDVDRTgwMEQyRTEzMzU4X0lXX01mTHQ4cm5zc19MQmJvS0UmbHN0bT0xYktqNm8mbW1zaWQ9NTk4MzAxNDMmcGF5ZmY9MCZwaXA9YTdjMmNhOTllMGRlMmI5MzhlN2U3ZTgwMWI4ZDljOWMmcGxhdGlkPTE1JnBsYXlpZD0wJnNwbGF0aWQ9MTUwNyZ0bT0xNDY3Nzk3MjI2JnRzcz1ubyZ2dHlwZT0xNjc=&ext=m3u8&tagtime=1467797228");

			mediaPlayer.Call ("setDataSource", activity, uri);
			mediaPlayer.Call ("prepare");
//				mediaPlayer.Call ("setLooping", true);
			mediaPlayer.Call ("start");
		} catch (Exception e) {

//			logclass.CallStatic ("d","hcq",e.Message);

//			activity.Call ("showToast",e.StackTrace);

			Debug.Log ("Failed to start mediaPlayer with message " + e.Message);
		}
//		}));

		return mediaPlayer;
	}

	[DllImport ("OculusMediaSurface")]
	private static extern void OVR_Media_Surface_Init ();

	[DllImport ("OculusMediaSurface")]
	private static extern void OVR_Media_Surface_SetEventBase (int eventBase);

	// This function returns an Android Surface object that is
	// bound to a SurfaceTexture object on an independent OpenGL texture id.
	// Each frame, before the TimeWarp processing, the SurfaceTexture is checked
	// for updates, and if one is present, the contents of the SurfaceTexture
	// will be copied over to the provided surfaceTexId and mipmaps will be
	// generated so normal Unity rendering can use it.
	[DllImport ("OculusMediaSurface")]
	private static extern IntPtr OVR_Media_Surface_GetObject ();

	[DllImport ("OculusMediaSurface")]
	private static extern IntPtr OVR_Media_Surface_GetNativeTexture ();

	[DllImport ("OculusMediaSurface")]
	private static extern void OVR_Media_Surface_SetTextureParms (int texWidth, int texHeight);
}
