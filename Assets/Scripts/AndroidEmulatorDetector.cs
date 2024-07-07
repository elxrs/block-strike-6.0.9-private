using System.IO;
using UnityEngine;

public class AndroidEmulatorDetector
{
	public static bool isEmulator()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return false;
#endif
		if (Check01())
		{
			return true;
		}
		if (Check05())
		{
			return true;
		}
		if (Check07())
		{
			return true;
		}
		return false;
	}

	private static bool Check01()
	{
		string[] array = new string[40]
		{
			"dev/socket/genyd",
			"dev/socket/baseband_genyd",
			"fstab.andy",
			"ueventd.andy.rc",
			"fstab.nox",
			"init.nox.rc",
			"ueventd.nox.rc",
			"dev/socket/qemud",
			"dev/qemu_pipe",
			"ueventd.android_x86.rc",
			"x86.prop",
			"ueventd.ttVM_x86.rc",
			"init.ttVM_x86.rc",
			"fstab.ttVM_x86",
			"fstab.vbox86",
			"init.vbox86.rc",
			"ueventd.vbox86.rc",
			"dev/memufp",
			"dev/memuguest",
			"dev/memuuser",
			"system/lib/memuguest.ko",
			"system/bin/nox-prop",
			"system/bin/noxd",
			"system/lib/libnoxd.so",
			"mnt/prebundledapps/downloads/com.bluestacks.home.apk",
			"mnt/prebundledapps/bst_appdetails_bgptest",
			"system/priv-app/com.bluestacks.bstfolder.apk",
			"fstab.duos",
			"init.duos.rc",
			"ueventd.duos.rc",
			"system/bin/duosconfing",
			"system/bin/ldinit",
			"system/bin/ldmountsf",
			"dev/bst_gps",
			"dev/bst_ime",
			"dev/bstgyro",
			"dev/bstmegn",
			"system/xbin/phoenix_compat",
			"data/system/phoenixlog.addr",
			"system/phoenixos"
		};
		string[] array2 = array;
		foreach (string path in array2)
		{
			if (File.Exists(path))
			{
				return true;
			}
		}
		return false;
	}

	private static bool Check02()
	{
		string[] array = new string[3]
		{
			"com.google.android.launcher.layouts.genymotion",
			"com.bluestacks",
			"com.bignox.app"
		};
		for (int i = 0; i < array.Length; i++)
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getPackageManager", new object[0]);
			try
			{
				AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getLaunchIntentForPackage", new object[1] { array[i] });
				if (androidJavaObject.Call<AndroidJavaObject>("queryIntentActivities", new object[2] { androidJavaObject2, 65536 }) != null)
				{
					return true;
				}
			}
			catch
			{
			}
		}
		return false;
	}

	private static bool Check03()
	{
		string[] array = new string[2]
		{
			"/proc/tty/drivers",
			"/proc/cpuinfo"
		};
		string[] array2 = array;
		foreach (string path in array2)
		{
			if (File.Exists(path))
			{
				string text = File.ReadAllText(path);
				if (text.Contains("goldfish"))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool Check04()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.opengl.GLES20");
		string text = androidJavaClass.CallStatic<string>("glGetString", new object[1] { 7937 });
		if (!string.IsNullOrEmpty(text) && (text.Contains("Bluestacks") || text.Contains("Translator")))
		{
			return true;
		}
		return false;
	}

	private static bool Check05()
	{
		string text = new AndroidJavaClass("android.os.Environment").CallStatic<AndroidJavaObject>("getExternalStorageDirectory", new object[0]).Call<string>("toString", new object[0]);
		if (Directory.Exists(text + "/Android/data/com.bluestacks.home"))
		{
			return true;
		}
		if (Directory.Exists(text + "/windows/BstSharedFolder"))
		{
			return true;
		}
		if (Directory.Exists(text + "/windows/InputMapper/com.bluestacks.setupapp.cfg"))
		{
			return true;
		}
		if (Directory.Exists(text + "/windows/InputMapper/com.bluestacks.appmart.cfg"))
		{
			return true;
		}
		if (File.Exists(text + "/windows/InputMapper/com.bluestacks.appmart.cfg"))
		{
			return true;
		}
		if (Directory.Exists(text + "/Android/data/com.microvirt.guide"))
		{
			return true;
		}
		if (Directory.Exists("data/data/com.microvirt.memuime"))
		{
			return true;
		}
		if (Directory.Exists("data/data/com.microvirt.installer"))
		{
			return true;
		}
		return false;
	}

	private static bool Check06()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build");
		string @static = androidJavaClass.GetStatic<string>("PRODUCT");
		if (@static.Contains("sdk"))
		{
			return true;
		}
		if (@static.Contains("Andy"))
		{
			return true;
		}
		if (@static.Contains("ttVM_Hdragon"))
		{
			return true;
		}
		if (@static.Contains("google_sdk"))
		{
			return true;
		}
		if (@static.Contains("Droid4X"))
		{
			return true;
		}
		if (@static.Contains("nox"))
		{
			return true;
		}
		if (@static.Contains("sdk_x86"))
		{
			return true;
		}
		if (@static.Contains("sdk_google"))
		{
			return true;
		}
		if (@static.Contains("vbox86p"))
		{
			return true;
		}
		string static2 = androidJavaClass.GetStatic<string>("MANUFACTURER");
		if (static2.Equals("unknown"))
		{
			return true;
		}
		if (static2.Equals("Genymotion"))
		{
			return true;
		}
		if (static2.Contains("Andy"))
		{
			return true;
		}
		if (static2.Contains("MIT"))
		{
			return true;
		}
		if (static2.Contains("nox"))
		{
			return true;
		}
		if (static2.Contains("TiantianVM"))
		{
			return true;
		}
		string static3 = androidJavaClass.GetStatic<string>("BRAND");
		if (static3.Equals("generic"))
		{
			return true;
		}
		if (static3.Equals("generic_x86"))
		{
			return true;
		}
		if (static3.Equals("TTVM"))
		{
			return true;
		}
		if (static3.Contains("Andy"))
		{
			return true;
		}
		string static4 = androidJavaClass.GetStatic<string>("DEVICE");
		if (static4.Contains("generic"))
		{
			return true;
		}
		if (static4.Contains("generic_x86"))
		{
			return true;
		}
		if (static4.Contains("Andy"))
		{
			return true;
		}
		if (static4.Contains("ttVM_Hdragon"))
		{
			return true;
		}
		if (static4.Contains("Droid4X"))
		{
			return true;
		}
		if (static4.Contains("nox"))
		{
			return true;
		}
		if (static4.Contains("generic_x86_64"))
		{
			return true;
		}
		if (static4.Contains("vbox86p"))
		{
			return true;
		}
		string static5 = androidJavaClass.GetStatic<string>("MODEL");
		if (static5.Equals("sdk"))
		{
			return true;
		}
		if (static5.Equals("google_sdk"))
		{
			return true;
		}
		if (static5.Contains("Droid4X"))
		{
			return true;
		}
		if (static5.Contains("TiantianVM"))
		{
			return true;
		}
		if (static5.Contains("Andy"))
		{
			return true;
		}
		if (static5.Contains("Android SDK built for x86_64"))
		{
			return true;
		}
		if (static5.Contains("Android SDK built for x86"))
		{
			return true;
		}
		string static6 = androidJavaClass.GetStatic<string>("HARDWARE");
		if (static6.Equals("goldfish"))
		{
			return true;
		}
		if (static6.Equals("vbox86"))
		{
			return true;
		}
		if (static6.Contains("nox"))
		{
			return true;
		}
		if (static6.Contains("ttVM_x86"))
		{
			return true;
		}
		string static7 = androidJavaClass.GetStatic<string>("FINGERPRINT");
		if (static7.Contains("generic/sdk/generic"))
		{
			return true;
		}
		if (static7.Contains("generic_x86/sdk_x86/generic_x86"))
		{
			return true;
		}
		if (static7.Contains("Andy"))
		{
			return true;
		}
		if (static7.Contains("ttVM_Hdragon"))
		{
			return true;
		}
		if (static7.Contains("generic_x86_64"))
		{
			return true;
		}
		if (static7.Contains("generic/google_sdk/generic"))
		{
			return true;
		}
		if (static7.Contains("vbox86p"))
		{
			return true;
		}
		if (static7.Contains("generic/vbox86p/vbox86p"))
		{
			return true;
		}
		return false;
	}

	private static bool Check07()
	{
		if (File.Exists("proc/cpuinfo"))
		{
			string[] array = File.ReadAllLines("proc/cpuinfo");
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ToLower().Contains("intel") && array[i].ToLower().Contains("core"))
				{
					return true;
				}
			}
		}
		return false;
	}
}
