//Created by hcq

using UnityEngine;
using System.Collections;
using BestHTTP;

public class BaseBehaviour : MonoBehaviour
{
	private AndroidJavaObject activity;

	//获取当前显示的activity
	protected AndroidJavaObject GetActivity ()
	{
		if (activity == null) {
			AndroidJavaClass activityClass = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			activity = activityClass.GetStatic<AndroidJavaObject> ("currentActivity");
		}
		return activity;
	}

	//在android ui线程中执行
	protected void RunOnAndroidUiThread (AndroidJavaRunnable runnable)
	{
		GetActivity ().Call ("runOnUiThread", runnable);
	}
		
}

