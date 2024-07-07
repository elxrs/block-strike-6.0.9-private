using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class AndroidNativeFunctions : MonoBehaviour
{
	private class ShowAlertListener : AndroidJavaProxy
	{
		private UnityAction<DialogInterface> action;

		public ShowAlertListener(UnityAction<DialogInterface> a)
			: base("android.content.DialogInterface$OnClickListener")
		{
			action = a;
		}

		public void onClick(AndroidJavaObject obj, int which)
		{
			if (action != null)
			{
				action((DialogInterface)which);
			}
		}
	}

	private class ShowAlertInputListener : AndroidJavaProxy
	{
		private UnityAction<DialogInterface, string> action;

		private AndroidJavaObject editText;

		public ShowAlertInputListener(UnityAction<DialogInterface, string> a, AndroidJavaObject et)
			: base("android.content.DialogInterface$OnClickListener")
		{
			action = a;
			editText = et;
		}

		public void onClick(AndroidJavaObject obj, int which)
		{
			if (action != null)
			{
				action((DialogInterface)which, editText.Call<AndroidJavaObject>("getText", new object[0]).Call<string>("toString", new object[0]));
			}
		}
	}

	private class ShowAlertListListener : AndroidJavaProxy
	{
		private string[] list;

		private UnityAction<string> action;

		public ShowAlertListListener(UnityAction<string> w, string[] a)
			: base("android.content.DialogInterface$OnClickListener")
		{
			action = w;
			list = a;
		}

		public void onClick(AndroidJavaObject obj, int which)
		{
			action(list[which]);
		}
	}

	public delegate void Callback();

	private static bool immersiveMode;

	private static AndroidNativeFunctions instance;

	private static AndroidJavaObject progressDialog;

	private static AndroidJavaObject _currentActivity;

	private static AndroidJavaObject currentActivity
	{
		get
		{
			if (_currentActivity == null)
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					_currentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				}
			}
			return _currentActivity;
		}
	}

	private static void CreateGO()
	{
		if (!(instance != null))
		{
			GameObject gameObject = new GameObject("AndroidNativeFunctions");
			instance = gameObject.AddComponent<AndroidNativeFunctions>();
			DontDestroyOnLoad(gameObject);
		}
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		if (immersiveMode && focusStatus)
		{
			ImmersiveMode();
		}
	}

	public static void ImmersiveMode()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		int @static = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
		if (@static >= 19)
		{
			CreateGO();
			immersiveMode = true;
			currentActivity.Call("runOnUiThread", (AndroidJavaRunnable)delegate
			{
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.view.View");
				AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("findViewById", new object[1] { new AndroidJavaClass("android.R$id").GetStatic<int>("content") });
				androidJavaObject.Call("setSystemUiVisibility", androidJavaClass.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_STABLE") | androidJavaClass.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION") | androidJavaClass.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN") | androidJavaClass.GetStatic<int>("SYSTEM_UI_FLAG_HIDE_NAVIGATION") | androidJavaClass.GetStatic<int>("SYSTEM_UI_FLAG_FULLSCREEN") | androidJavaClass.GetStatic<int>("SYSTEM_UI_FLAG_IMMERSIVE_STICKY"));
			});
		}
	}

	public static string GetExternalStorageDirectory()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return "";
#endif
		return new AndroidJavaClass("android.os.Environment").CallStatic<AndroidJavaObject>("getExternalStorageDirectory", new object[0]).Call<string>("toString", new object[0]);
	}

	public static string GetFilesDir()
	{
		return currentActivity.Call<AndroidJavaObject>("getFilesDir", new object[0]).Call<string>("getPath", new object[0]);
	}

	public static void StartApp(string packageName, bool isExitThisApp)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getLaunchIntentForPackage", new object[1] { packageName });
			currentActivity.Call("startActivity", androidJavaObject);
			if (isExitThisApp)
			{
				Application.Quit();
			}
		}
	}

	public static string GetAppName(string packageName)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return string.Empty;
		}
		AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getPackageManager", new object[0]);
		AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getApplicationInfo", new object[2] { packageName, 0 });
		return androidJavaObject.Call<string>("getApplicationLabel", new object[1] { androidJavaObject2 });
	}

	public static List<PackageInfo> GetInstalledApps()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return new List<PackageInfo>();
		}
		AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getInstalledPackages", new object[1] { 0 });
		int num = androidJavaObject.Call<int>("size", new object[0]);
		List<PackageInfo> list = new List<PackageInfo>();
		for (int i = 0; i < num; i++)
		{
			AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("get", new object[1] { i });
			PackageInfo packageInfo = new PackageInfo();
			packageInfo.firstInstallTime = androidJavaObject2.Get<long>("firstInstallTime");
			packageInfo.packageName = androidJavaObject2.Get<string>("packageName");
			packageInfo.lastUpdateTime = androidJavaObject2.Get<long>("lastUpdateTime");
			packageInfo.versionCode = androidJavaObject2.Get<int>("versionCode");
			packageInfo.versionName = androidJavaObject2.Get<string>("versionName");
			list.Add(packageInfo);
		}
		return list;
	}

	public static string[] GetInstalledApps2()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return new string[1] { "Test" };
		}
		AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getInstalledPackages", new object[1] { 128 });
		int num = androidJavaObject.Call<int>("size", new object[0]);
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("get", new object[1] { i });
			array[i] = androidJavaObject2.Get<string>("packageName");
		}
		return array;
	}

	public static string[] GetInstalledApps3()
	{
		List<string> apks = new List<string>();
		try
		{
			AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
			AndroidJavaObject packageInfos = packageManager.Call<AndroidJavaObject>("getInstalledPackages", 0);
			AndroidJavaObject[] packages = packageInfos.Call<AndroidJavaObject[]>("toArray");
			for (int i = 0; i < packages.Length; i++)
			{
				AndroidJavaObject applicationInfo = packages[i].Get<AndroidJavaObject>("applicationInfo");
				if ((applicationInfo.Get<int>("flags") & applicationInfo.GetStatic<int>("FLAG_SYSTEM")) == 0)
				{
					apks.Add(applicationInfo.Get<string>("sourceDir"));
				}
			}
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogWarning(e);
		}
		return apks.ToArray();
	}

	public static PackageInfo GetAppInfo()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return new PackageInfo();
		}
		return GetAppInfo(currentActivity.Call<string>("getPackageName", new object[0]));
	}

	public static PackageInfo GetAppInfo(string packageName)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return null;
		}
		AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getPackageInfo", new object[2] { packageName, 0 });
		PackageInfo packageInfo = new PackageInfo();
		packageInfo.firstInstallTime = androidJavaObject.Get<long>("firstInstallTime");
		packageInfo.packageName = androidJavaObject.Get<string>("packageName");
		packageInfo.lastUpdateTime = androidJavaObject.Get<long>("lastUpdateTime");
		packageInfo.versionCode = androidJavaObject.Get<int>("versionCode");
		packageInfo.versionName = androidJavaObject.Get<string>("versionName");
		return packageInfo;
	}

	public static string GetSourceDir(string packageName)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return null;
		}
		try
		{
			AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getPackageInfo", new object[2] { packageName, 1024 });
			AndroidJavaObject androidJavaObject2 = androidJavaObject.Get<AndroidJavaObject>("applicationInfo");
			return androidJavaObject2.Get<string>("sourceDir");
		}
		catch
		{
			return string.Empty;
		}
	}

	public static DeviceInfo GetDeviceInfo()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return null;
		}
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build$VERSION");
		DeviceInfo deviceInfo = new DeviceInfo();
		deviceInfo.CODENAME = androidJavaClass.GetStatic<string>("CODENAME");
		deviceInfo.INCREMENTAL = androidJavaClass.GetStatic<string>("INCREMENTAL");
		deviceInfo.RELEASE = androidJavaClass.GetStatic<string>("RELEASE");
		deviceInfo.SDK = androidJavaClass.GetStatic<int>("SDK_INT");
		return deviceInfo;
	}

	public static string GetAndroidID()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return SystemInfo.deviceUniqueIdentifier;
		}
		AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getContentResolver", new object[0]);
		return new AndroidJavaClass("android.provider.Settings$Secure").CallStatic<string>("getString", new object[2]
		{
			androidJavaObject,
			"android_id"
		});
	}

	public static string GetAndroidID2()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return Utils.XOR(SystemInfo.deviceUniqueIdentifier, GameSettings.instance.Keys[0], true).Remove(10);
		}
		try
		{
			string text = GetAndroidSerial();
			int @static = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
			if (string.IsNullOrEmpty(text) || text.Contains("0123456789"))
			{
				if (@static >= 26)
				{
					text = "1_" + currentActivity.Call<AndroidJavaObject>("getSystemService", new object[]
					{
					"phone"
					}).Call<string>("getImei", new object[0]);
				}
				else
				{
					text = "1_" + currentActivity.Call<AndroidJavaObject>("getSystemService", new object[]
					{
					"phone"
					}).Call<string>("getDeviceId", new object[0]);
				}
				if (string.IsNullOrEmpty(text) || text == "1_" || text.Contains("012345") || text.Length < 5)
				{
					return "2_" + GetAndroidID();
				}
			}
			try
			{
				if (text.Contains("unknown"))
				{
					AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build");
					text = androidJavaClass.GetStatic<string>("getSerial");
				}
			}
			catch
			{
				if (@static >= 26)
				{
					text = "1_" + currentActivity.Call<AndroidJavaObject>("getSystemService", new object[]
					{
					"phone"
					}).Call<string>("getImei", new object[0]);
				}
				else
				{
					text = "1_" + currentActivity.Call<AndroidJavaObject>("getSystemService", new object[]
					{
					"phone"
					}).Call<string>("getDeviceId", new object[0]);
				}
				if (string.IsNullOrEmpty(text) || text == "1_" || text.Contains("012345") || text.Length < 5)
				{
					return "2_" + GetAndroidID();
				}
			}
			return text;
		}
		catch
		{
			return GetAndroidID();
		}
	}

	public static string GetAndroidSerial()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return SystemInfo.deviceUniqueIdentifier;
		}
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build");
		return androidJavaClass.GetStatic<string>("SERIAL");
	}

	public static void ShareText(string text, string subject, string chooser)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.content.Intent");
			androidJavaObject.Call<AndroidJavaObject>("setAction", new object[1] { "android.intent.action.SEND" });
			androidJavaObject.Call<AndroidJavaObject>("setType", new object[1] { "text/plain" });
			androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.TEXT",
				text
			});
			androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.SUBJECT",
				subject
			});
			AndroidJavaObject androidJavaObject2 = androidJavaObject.CallStatic<AndroidJavaObject>("createChooser", new object[2] { androidJavaObject, chooser });
			currentActivity.Call("startActivity", androidJavaObject2);
		}
	}

	public static void ShareImage(string text, string subject, string chooser, Texture2D picture)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			byte[] array = new AndroidJavaObject("android.util.Base64").CallStatic<byte[]>("decode", new object[2]
			{
				Convert.ToBase64String(picture.EncodeToPNG()),
				0
			});
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.graphics.BitmapFactory").CallStatic<AndroidJavaObject>("decodeByteArray", new object[3] { array, 0, array.Length });
			AndroidJavaObject @static = new AndroidJavaClass("android.graphics.Bitmap$CompressFormat").GetStatic<AndroidJavaObject>("JPEG");
			androidJavaObject.Call<bool>("compress", new object[3]
			{
				@static,
				100,
				new AndroidJavaObject("java.io.ByteArrayOutputStream")
			});
			string text2 = new AndroidJavaClass("android.provider.MediaStore$Images$Media").CallStatic<string>("insertImage", new object[4]
			{
				currentActivity.Call<AndroidJavaObject>("getContentResolver", new object[0]),
				androidJavaObject,
				picture.name,
				string.Empty
			});
			AndroidJavaObject androidJavaObject2 = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", new object[1] { text2 });
			AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("android.content.Intent");
			androidJavaObject3.Call<AndroidJavaObject>("setAction", new object[1] { "android.intent.action.SEND" });
			androidJavaObject3.Call<AndroidJavaObject>("setType", new object[1] { "image/*" });
			androidJavaObject3.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.STREAM",
				androidJavaObject2
			});
			androidJavaObject3.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.TEXT",
				text
			});
			androidJavaObject3.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.SUBJECT",
				subject
			});
			AndroidJavaObject androidJavaObject4 = androidJavaObject3.CallStatic<AndroidJavaObject>("createChooser", new object[2] { androidJavaObject3, chooser });
			currentActivity.Call("startActivity", androidJavaObject4);
		}
	}

	public static void ShareScreenshot(string text, string subject, string chooser, string screenshotPath)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.content.Intent");
			AndroidJavaObject androidJavaObject2 = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", new object[1] { "file://" + screenshotPath });
			androidJavaObject.Call<AndroidJavaObject>("setAction", new object[1] { "android.intent.action.SEND" });
			androidJavaObject.Call<AndroidJavaObject>("setType", new object[1] { "image/png" });
			androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.STREAM",
				androidJavaObject2
			});
			androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.TEXT",
				text
			});
			androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.SUBJECT",
				subject
			});
			AndroidJavaObject androidJavaObject3 = androidJavaObject.CallStatic<AndroidJavaObject>("createChooser", new object[2] { androidJavaObject, chooser });
			currentActivity.Call("startActivity", androidJavaObject3);
		}
	}

	public static void ShowAlert(string message, string title, string positiveButton, string negativeButton, string neutralButton, UnityAction<DialogInterface> action)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		currentActivity.Call("runOnUiThread", (AndroidJavaRunnable)delegate
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android/app/AlertDialog$Builder", currentActivity);
			androidJavaObject.Call<AndroidJavaObject>("setMessage", new object[1] { message });
			androidJavaObject.Call<AndroidJavaObject>("setCancelable", new object[1] { false });
			if (!string.IsNullOrEmpty(title))
			{
				androidJavaObject.Call<AndroidJavaObject>("setTitle", new object[1] { title });
			}
			androidJavaObject.Call<AndroidJavaObject>("setPositiveButton", new object[2]
			{
				positiveButton,
				new ShowAlertListener(action)
			});
			if (!string.IsNullOrEmpty(negativeButton))
			{
				androidJavaObject.Call<AndroidJavaObject>("setNegativeButton", new object[2]
				{
					negativeButton,
					new ShowAlertListener(action)
				});
			}
			if (!string.IsNullOrEmpty(neutralButton))
			{
				androidJavaObject.Call<AndroidJavaObject>("setNeutralButton", new object[2]
				{
					neutralButton,
					new ShowAlertListener(action)
				});
			}
			AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("create", new object[0]);
			androidJavaObject2.Call("show");
		});
	}

	public static void ShowAlertInput(string text, string message, string title, string positiveButton, string negativeButton, UnityAction<DialogInterface, string> action)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		currentActivity.Call("runOnUiThread", (AndroidJavaRunnable)delegate
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android/app/AlertDialog$Builder", currentActivity);
			AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("android.widget.EditText", currentActivity);
			if (!string.IsNullOrEmpty(text))
			{
				androidJavaObject2.Call("setText", text);
			}
			androidJavaObject.Call<AndroidJavaObject>("setView", new object[1] { androidJavaObject2 });
			if (!string.IsNullOrEmpty(message))
			{
				androidJavaObject.Call<AndroidJavaObject>("setMessage", new object[1] { message });
			}
			androidJavaObject.Call<AndroidJavaObject>("setCancelable", new object[1] { false });
			if (!string.IsNullOrEmpty(title))
			{
				androidJavaObject.Call<AndroidJavaObject>("setTitle", new object[1] { title });
			}
			androidJavaObject.Call<AndroidJavaObject>("setPositiveButton", new object[2]
			{
				positiveButton,
				new ShowAlertInputListener(action, androidJavaObject2)
			});
			if (!string.IsNullOrEmpty(negativeButton))
			{
				androidJavaObject.Call<AndroidJavaObject>("setNegativeButton", new object[2]
				{
					negativeButton,
					new ShowAlertInputListener(action, androidJavaObject2)
				});
			}
			AndroidJavaObject androidJavaObject3 = androidJavaObject.Call<AndroidJavaObject>("create", new object[0]);
			androidJavaObject3.Call("show");
		});
	}

	public static void ShowAlertList(string title, string[] list, UnityAction<string> action)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		currentActivity.Call("runOnUiThread", (AndroidJavaRunnable)delegate
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android/app/AlertDialog$Builder", currentActivity);
			androidJavaObject.Call<AndroidJavaObject>("setCancelable", new object[1] { false });
			if (!string.IsNullOrEmpty(title))
			{
				androidJavaObject.Call<AndroidJavaObject>("setTitle", new object[1] { title });
			}
			androidJavaObject.Call<AndroidJavaObject>("setItems", new object[2]
			{
				list,
				new ShowAlertListListener(action, list)
			});
			AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("create", new object[0]);
			androidJavaObject2.Call("show");
		});
	}

	public static void ShowToast(string message)
	{
		ShowToast(message, true);
	}

	public static void ShowToast(string message, bool shortDuration)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			currentActivity.Call("runOnUiThread", (AndroidJavaRunnable)delegate
			{
				AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.widget.Toast", currentActivity);
				androidJavaObject.CallStatic<AndroidJavaObject>("makeText", new object[3]
				{
					currentActivity,
					message,
					(!shortDuration) ? 1 : 0
				}).Call("show");
			});
		}
	}

	public static void ShowProgressDialog(string message)
	{
		ShowProgressDialog(message, string.Empty);
	}

	public static void ShowProgressDialog(string message, string title)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		if (progressDialog != null)
		{
			HideProgressDialog();
		}
		currentActivity.Call("runOnUiThread", (AndroidJavaRunnable)delegate
		{
			progressDialog = new AndroidJavaObject("android.app.ProgressDialog", currentActivity);
			progressDialog.Call("setProgressStyle", 0);
			progressDialog.Call("setIndeterminate", true);
			progressDialog.Call("setCancelable", false);
			progressDialog.Call("setMessage", message);
			if (!string.IsNullOrEmpty(title))
			{
				progressDialog.Call("setTitle", title);
			}
			progressDialog.Call("show");
		});
	}

	public static void HideProgressDialog()
	{
		if (progressDialog != null)
		{
			currentActivity.Call("runOnUiThread", (AndroidJavaRunnable)delegate
			{
				progressDialog.Call("hide");
				progressDialog.Call("dismiss");
				progressDialog = null;
			});
		}
	}

	public static void OpenGooglePlay(string packageName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			Application.OpenURL("market://details?id=" + packageName);
		}
		else
		{
			Application.OpenURL("https://play.google.com/store/apps/details?id=" + packageName);
		}
	}

	public static bool isDeviceRooted()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		string[] array = new string[9]
		{
			"/system/app/Superuser.apk",
			"/sbin/su",
			"/system/bin/su",
			"/system/xbin/su",
			"/data/local/xbin/su",
			"/data/local/bin/su",
			"/system/sd/xbin/su",
			"/system/bin/failsafe/su",
			"/data/local/su"
		};
		string[] array2 = array;
		foreach (string path in array2)
		{
			if (File.Exists(path))
			{
				return true;
			}
		}
		string @static = new AndroidJavaClass("android.os.Build").GetStatic<string>("TAGS");
		if (@static != null && @static.Contains("test-keys"))
		{
			return true;
		}
		return false;
	}

	public static bool isInstalledApp(string packageName)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		try
		{
			currentActivity.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getPackageInfo", new object[2] { packageName, 0 });
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool isTVDevice()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		int num = currentActivity.Call<AndroidJavaObject>("getSystemService", new object[1] { "uimode" }).Call<int>("getCurrentModeType", new object[0]);
		if (num == 4)
		{
			return true;
		}
		return false;
	}

	public static bool isWiredHeadset()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		return currentActivity.Call<AndroidJavaObject>("getSystemService", new object[1] { "audio" }).Call<bool>("isWiredHeadsetOn", new object[0]);
	}

	public static void SetTotalVolume(int volumeLevel)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			volumeLevel = Mathf.Clamp(volumeLevel, 0, 15);
			currentActivity.Call<AndroidJavaObject>("getSystemService", new object[1] { "audio" }).Call("setStreamVolume", 3, volumeLevel, 0);
		}
	}

	public static int GetTotalVolume()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return 0;
		}
		return currentActivity.Call<AndroidJavaObject>("getSystemService", new object[1] { "audio" }).Call<int>("getStreamVolume", new object[1] { 3 });
	}

	public static bool isConnectInternet()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		try
		{
			AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getSystemService", new object[1] { "connectivity" }).Call<AndroidJavaObject>("getActiveNetworkInfo", new object[0]);
			if (androidJavaObject == null)
			{
				return false;
			}
			return androidJavaObject.Call<bool>("isConnectedOrConnecting", new object[0]);
		}
		catch
		{
			return false;
		}
	}

	public static bool isConnectWifi()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		try
		{
			AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getSystemService", new object[1] { "connectivity" }).Call<AndroidJavaObject>("getNetworkInfo", new object[1] { 1 });
			if (androidJavaObject == null)
			{
				return false;
			}
			return androidJavaObject.Call<bool>("isConnectedOrConnecting", new object[0]);
		}
		catch
		{
			return false;
		}
	}

	public static int GetBatteryLevel()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return 0;
		}
		AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getApplicationContext", new object[0]).Call<AndroidJavaObject>("registerReceiver", new object[2]
		{
			null,
			new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED")
		});
		int num = androidJavaObject.Call<int>("getIntExtra", new object[2]
		{
			"level",
			-1
		});
		int num2 = androidJavaObject.Call<int>("getIntExtra", new object[2]
		{
			"scale",
			-1
		});
		if (num == -1 || num2 == -1)
		{
			return 0;
		}
		return (int)((float)num / (float)num2 * 100f);
	}

	public static void SendEmail(string text, string subject, string email)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.content.Intent");
			androidJavaObject.Call<AndroidJavaObject>("setAction", new object[1] { "android.intent.action.SENDTO" });
			androidJavaObject.Call<AndroidJavaObject>("setType", new object[1] { "text/plain" });
			androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.TEXT",
				text
			});
			androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"android.intent.extra.SUBJECT",
				subject
			});
			androidJavaObject.Call<AndroidJavaObject>("setData", new object[1] { new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", new object[1] { "mailto:" + email }) });
			currentActivity.Call("startActivity", androidJavaObject);
		}
	}

	public static bool VerifyGooglePlayPurchase(string purchaseJson, string base64Signature, string publicKey)
	{
		bool result = false;
		using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
		{
			try
			{
				rSACryptoServiceProvider.FromXmlString(publicKey);
				SHA1Managed sHA1Managed = new SHA1Managed();
				byte[] rgbSignature = Convert.FromBase64String(base64Signature);
				byte[] bytes = Encoding.UTF8.GetBytes(purchaseJson);
				byte[] rgbHash = sHA1Managed.ComputeHash(bytes);
				result = rSACryptoServiceProvider.VerifyHash(rgbHash, null, rgbSignature);
			}
			catch (Exception)
            {
			}
		}
		return result;
	}

	public static string GetSignature()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return string.Empty;
		}
		AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getPackageInfo", new object[2]
		{
			GetAppInfo().packageName,
			64
		});
		AndroidJavaObject[] array = androidJavaObject.Get<AndroidJavaObject[]>("signatures");
		return array[0].Call<int>("hashCode", new object[0]).ToString("X");
	}

	public static string[] GetEmails()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return new string[0];
		}
		AndroidJavaObject androidJavaObject = currentActivity.Call<AndroidJavaObject>("getApplication", new object[0]);
		AndroidJavaObject androidJavaObject2 = new AndroidJavaClass("android.accounts.AccountManager").CallStatic<AndroidJavaObject>("get", new object[1] { androidJavaObject }).Call<AndroidJavaObject>("getAccountsByType", new object[1] { "com.google" });
		AndroidJavaObject[] array = AndroidJNIHelper.ConvertFromJNIArray<AndroidJavaObject[]>(androidJavaObject2.GetRawObject());
		string[] array2 = new string[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i].Get<string>("name");
		}
		return array2;
	}

	public static bool CheckPermission(string permission)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return true;
		}
		if (currentActivity.Call<int>("checkCallingOrSelfPermission", new object[1] { permission }) == 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsDebuggable()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		try
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject androidJavaObject = @static.Call<AndroidJavaObject>("getApplicationInfo", new object[0]);
			int num = androidJavaObject.Get<int>("flags");
			return (num & 2) != 0;
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		return true;
	}

	public static void TakeScreenshot(string name, Action<string> action)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CreateGO();
			instance.StartCoroutine(instance.CreateScreenshot(name, action));
		}
	}

	private IEnumerator CreateScreenshot(string name, Action<string> action)
	{
		name += ".png";
		string screenShotPath = Application.persistentDataPath + "/" + name;
		if (File.Exists(screenShotPath))
		{
			File.Delete(screenShotPath);
		}
		Application.CaptureScreenshot(name);
		yield return new WaitForSeconds(0.5f);
		while (!File.Exists(screenShotPath))
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (action != null)
		{
			action(screenShotPath);
		}
	}

	public static string GetAbsolutePath()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return Application.persistentDataPath;
#endif
		return new AndroidJavaClass("android.os.Environment").CallStatic<AndroidJavaObject>("getExternalStorageDirectory", new object[0]).Call<string>("getAbsolutePath", new object[0]);
	}
}

public class AndroidShell
{
    public static void RunCommand(string command, Action<string> complete, Action<string> error)
    {
        RunCommand("sh", command, complete, error);
    }

    public static void RunCommand(string file, string command, Action<string> complete, Action<string> error)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }
        Process process = new Process();
        process.StartInfo.FileName = file;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.StandardInput.WriteLine(command);
        process.StandardInput.Flush();
        process.StandardInput.Close();
        process.WaitForExit();
        string text = process.StandardOutput.ReadToEnd();
        if (!string.IsNullOrEmpty(text))
        {
            if (complete != null)
            {
                complete(text);
            }
            return;
        }
        text = string.Empty;
        text = process.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(text) && error != null)
        {
            error(text);
        }
    }
}

public class DeviceInfo
{
    public string CODENAME;

    public string INCREMENTAL;

    public string RELEASE;

    public int SDK;
}

public class PackageInfo
{
    public long firstInstallTime;

    public long lastUpdateTime;

    public string packageName;

    public int versionCode;

    public string versionName;
}


public enum DialogInterface
{
    Positive = -1,
    Negative = -2,
    Neutral = -3
}