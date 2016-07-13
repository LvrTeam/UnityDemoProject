//Created by hcq

using UnityEngine;
using System.Collections;

public class BaseBehaviour : MonoBehaviour
{
	private AndroidJavaObject activity;

	protected AndroidJavaObject getActivity ()
	{
		if (activity == null) {
			AndroidJavaClass activityClass = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			activity = activityClass.GetStatic<AndroidJavaObject> ("currentActivity");
		}
		return activity;
	}

	protected void runOnAndroidUiThread (Runnable runnable)
	{
		getActivity ().Call ("runOnUiThread", new AndroidJavaRunnable (() => {
			runnable.run ();
		}));
	}

	protected interface Runnable
	{
		void run ();
	}
}

