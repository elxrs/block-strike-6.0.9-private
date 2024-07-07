using System.IO;
using FreeJSON;
using UnityEngine;

public class CheckManager : MonoBehaviour
{
	private static bool detected;

	private static bool quit;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		CryptoManager.StartDetection(delegate
		{
			Detected("Crypto Detected");
		});
		CheckAll();
		TimerManager.In(5f, false, -1, 5f, delegate
		{
			if (new AndroidJavaClass("com.rexetstudio.Functions").GetStatic<bool>("isDetected"))
			{
				string @static = new AndroidJavaClass("com.rexetstudio.Functions").GetStatic<string>("detectedText");
				if (string.IsNullOrEmpty(@static))
				{
					Detected("Error 345");
				}
				else
				{
					Detected(@static);
				}
			}
		});
	}

	public static void Detected()
	{
		Detected("Detected");
	}

	public static void Detected(string text)
	{
		Detected(text, string.Empty);
	}

	public static void Detected(string text, string log)
	{
		if (!quit && !detected)
		{
			detected = true;
			GameSettings.instance.PhotonID = string.Empty;
			if (PhotonNetwork.inRoom)
			{
				PhotonNetwork.LeaveRoom(true);
			}
			AndroidNativeFunctions.ShowAlert(text, "Detected", "Ok", string.Empty, string.Empty, delegate
			{
				Application.Quit();
			});
		}
	}

	public static void Quit()
	{
		if (!quit)
		{
			quit = true;
			if (PhotonNetwork.inRoom)
			{
				PhotonNetwork.LeaveRoom(true);
			}
			TimerManager.In(nValue.int1, false, delegate
			{
				Application.Quit();
			});
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			CheckAll();
		}
	}

	private void CheckAll()
	{
		string externalStorageDirectory = AndroidNativeFunctions.GetExternalStorageDirectory();
		if (!externalStorageDirectory.Contains("storage/emulated/") || externalStorageDirectory.Contains("storage/emulated/0"))
		{
			return;
		}
		byte b = 0;
		for (int i = 0; i < 100; i++)
		{
			if (Directory.Exists("/storage/emulated/" + i))
			{
				b++;
			}
		}
		if (b >= 2)
		{
			Detected("Error 41513");
		}
	}

	private void CheckNewApps()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return;
#endif
		string text = new AndroidJavaClass("com.rexetstudio.checkapps").CallStatic<string>("GetNewApps", new object[0]);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		JsonArray jsonArray;
		try
		{
			jsonArray = JsonArray.Parse(text);
		}
		catch
		{
			return;
		}
		string empty = string.Empty;
		long num = 0L;
		int num2 = int.Parse("25000000");
		for (int i = 0; i < jsonArray.Length; i++)
		{
			empty = jsonArray.Get<string>(i);
			empty = empty.Replace("\\", string.Empty);
			empty = empty.Replace("\"", string.Empty);
			if (File.Exists(empty))
			{
				num = new FileInfo(empty).Length;
				if (num < num2 && (lzip.entryExists(empty, "res/raw/chunk8") || lzip.entryExists(empty, "lib/armeabi-v7a/libgg_tibe.so")))
				{
					Detected("Game Guardian detected");
					break;
				}
				continue;
			}
			Detected("Apps Error");
			break;
		}
	}
}
