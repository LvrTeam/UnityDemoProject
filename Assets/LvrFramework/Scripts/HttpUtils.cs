//Created by hcq

using UnityEngine;
using System.Collections;
using BestHTTP;
using System.Collections.Generic;
using System;
using System.IO;
using System.Security.Policy;
using LitJson;

public class HttpUtils
{

	private static bool isSetup;

	public delegate void OnResponseDelegate (string response, byte[] rawResponse, int statusCode, bool isSuccess);

	public delegate void OnResponseDelegate<T> (T response, int statusCode, bool isSuccess);

	public delegate void OnDownloadFinishDelegate ();

	private static void SetupIfNeed ()
	{
		if (!isSetup) {
			HTTPManager.Setup ();

			//isKeepAlive default is true, isDisableCache default is false
			HTTPManager.KeepAliveDefaultValue = false;
			HTTPManager.IsCachingDisabled = false;

			isSetup = true;
		}
	}

	//默认最大连接数为4
	public static void SetMaxConnection (byte count)
	{
		SetupIfNeed ();

		HTTPManager.MaxConnectionPerServer = count;

	}

	//默认为true
	public static void SetKeepAlive (bool isKeepAlive)
	{
		SetupIfNeed ();

		HTTPManager.KeepAliveDefaultValue = isKeepAlive;
	}

	public static void SetDisableCache (bool isDisableCache)
	{
		SetupIfNeed ();

		HTTPManager.IsCachingDisabled = isDisableCache;
	}

	//默认连接最大空闲时间2分钟
	public static void SetMaxConnectionIdleTime (int seconds)
	{
		SetupIfNeed ();

		HTTPManager.MaxConnectionIdleTime = TimeSpan.FromSeconds (seconds);
	}

	//默认20秒
	public static void SetConnectionTimeOut (int seconds)
	{
		SetupIfNeed ();

		HTTPManager.ConnectTimeout = TimeSpan.FromSeconds (seconds);
	}

	//默认60秒
	public static void SetRequestTimeOut (int seconds)
	{
		SetupIfNeed ();

		HTTPManager.RequestTimeout = TimeSpan.FromSeconds (seconds);
	}

	private static void HandleResponse<T> (HTTPRequest originalRequest, HTTPResponse response, OnResponseDelegate<T> callback)
	{
		//如果请求错误 HTTPResponse对象会返回null
		if (callback != null && response != null) {
			T responseObj = JsonMapper.ToObject<T> (response.DataAsText);
			callback (responseObj, response.StatusCode, response.IsSuccess);
		}
	}

	private static void HandleResponse (HTTPRequest originalRequest, HTTPResponse response, OnResponseDelegate callback)
	{
		//如果请求错误 HTTPResponse对象会返回null
		if (callback != null && response != null) {
			callback (response.DataAsText, response.Data, response.StatusCode, response.IsSuccess);
		}
	}

	private static void AddHeads (HTTPRequest request, Dictionary<string,string> headers)
	{
		if (headers != null) {
			foreach (KeyValuePair<string,string> pair in headers) {
				request.SetHeader (pair.Key, pair.Value);
			}
		}
	}

	private static void AddFields (HTTPRequest request, Dictionary<string,string> requestParams)
	{
		if (requestParams != null) {
			foreach (KeyValuePair<string,string> pair in requestParams) {
				request.AddField (pair.Key, pair.Value);
			}
		}
	}

	public static HTTPRequest Get<T> (string url, Dictionary<string,string> headers, OnResponseDelegate<T> callback)
	{
		return Get (url, headers, delegate(HTTPRequest req, HTTPResponse resp) {
			HandleResponse<T> (req, resp, callback);
		});
	}

	public static HTTPRequest Get (string url, Dictionary<string,string> headers, OnResponseDelegate callback)
	{
		return Get (url, headers, delegate(HTTPRequest req, HTTPResponse resp) {
			HandleResponse (req, resp, callback);
		});
	}

	public static HTTPRequest Get (string url, Dictionary<string,string> headers, OnRequestFinishedDelegate callback)
	{
		SetupIfNeed ();

		HTTPRequest request = new HTTPRequest (new Uri (url), callback);

		AddHeads (request, headers);

		request.Send ();

		return request;
	}

	public static HTTPRequest Post<T> (string url, Dictionary<string,string> headers, Dictionary<string,string> requestParams, OnResponseDelegate<T> callback)
	{
		return Post (url, headers, requestParams, delegate(HTTPRequest req, HTTPResponse resp) {
			HandleResponse<T> (req, resp, callback);
		});
	}

	public static HTTPRequest Post (string url, Dictionary<string,string> headers, Dictionary<string,string> requestParams, OnResponseDelegate callback)
	{
		return Post (url, headers, requestParams, delegate(HTTPRequest req, HTTPResponse resp) {
			HandleResponse (req, resp, callback);
		});
	}

	public static HTTPRequest Post (string url, Dictionary<string,string> headers, Dictionary<string,string> requestParams, OnRequestFinishedDelegate callback)
	{
		SetupIfNeed ();

		HTTPRequest request = new HTTPRequest (new Uri (url), HTTPMethods.Post,
			                      callback);

		AddFields (request, requestParams);

		AddHeads (request, headers);

		request.Send ();

		return request;
	}

	public static HTTPRequest Download (string url, string savePath, OnDownloadFinishDelegate callback)
	{

		SetupIfNeed ();

		HTTPRequest request = new HTTPRequest (new Uri (url), (req, resp) => {
			List<byte[]> fragments = resp.GetStreamedFragments ();

			// Write out the downloaded data to a file:
			using (FileStream fs = new FileStream (savePath, FileMode.Append))
				foreach (byte[] data in fragments)
					fs.Write (data, 0, data.Length);

			if (resp.IsStreamingFinished) {
				if (callback != null) {
					callback ();
				}
			}
		});
		request.UseStreaming = true;
		request.StreamFragmentSize = 1 * 1024 * 1024; // 1 megabyte
		request.DisableCache = true; // already saving to a file, so turn off caching
		request.Send (); 

		return request;
	}


}

