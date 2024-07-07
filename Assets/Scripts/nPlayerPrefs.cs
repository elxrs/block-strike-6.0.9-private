using System.IO;
using FreeJSON;
using UnityEngine;

public static class nPlayerPrefs
{
	private static JsonObject json;

	private static bool accessDenied = false;

	private static string path = AndroidNativeFunctions.GetAbsolutePath() + "/Block Strike/Settings.json";

	private static bool isInit = false;

	private static JsonObject data
	{
		get
		{
			Init();
			return json;
		}
	}

	private static void Init()
	{
		if (isInit)
		{
			return;
		}
		try
		{
			string text = AndroidNativeFunctions.GetAbsolutePath() + "/Block Strike";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (File.Exists(path))
			{
				json = JsonObject.Parse(File.ReadAllText(path));
				isInit = true;
			}
			else
			{
				json = new JsonObject();
				isInit = true;
			}
			accessDenied = false;
		}
		catch
		{
			accessDenied = true;
			json = JsonObject.Parse(PlayerPrefs.GetString("Settings.json", "{}"));
			isInit = true;
		}
	}

	public static void Save()
	{
		try
		{
			if (accessDenied)
			{
				PlayerPrefs.SetString("Settings.json", data.ToString());
			}
			else
			{
				File.WriteAllText(path, data.ToString());
			}
		}
		catch
		{
		}
	}

	public static bool HasKey(string key)
	{
		return data.ContainsKey(key);
	}

	public static void DeleteKey(string key)
	{
		data.Remove(key);
	}

	public static int GetInt(string key)
	{
		return data.Get<int>(key);
	}

	public static int GetInt(string key, int defaultValue)
	{
		return data.Get(key, defaultValue);
	}

	public static void SetInt(string key, int value)
	{
		data.Add(key, value);
	}

	public static float GetFloat(string key)
	{
		return data.Get<float>(key);
	}

	public static float GetFloat(string key, float defaultValue)
	{
		return data.Get(key, defaultValue);
	}

	public static void SetFloat(string key, float value)
	{
		data.Add(key, value);
	}

	public static bool GetBool(string key)
	{
		return data.Get<bool>(key);
	}

	public static bool GetBool(string key, bool defaultValue)
	{
		return data.Get(key, defaultValue);
	}

	public static void SetBool(string key, bool value)
	{
		data.Add(key, value);
	}

	public static string GetString(string key)
	{
		return data.Get<string>(key);
	}

	public static string GetString(string key, string defaultValue)
	{
		return data.Get(key, defaultValue);
	}

	public static void SetString(string key, string value)
	{
		data.Add(key, value);
	}

	public static Vector3 GetVector3(string key)
	{
		return data.Get<Vector3>(key);
	}

	public static Vector3 GetVector3(string key, Vector3 defaultValue)
	{
		return data.Get(key, defaultValue);
	}

	public static void SetVector3(string key, Vector3 value)
	{
		data.Add(key, value);
	}

	public static Vector2 GetVector2(string key)
	{
		return data.Get<Vector2>(key);
	}

	public static Vector2 GetVector2(string key, Vector2 defaultValue)
	{
		return data.Get(key, defaultValue);
	}

	public static void SetVector2(string key, Vector2 value)
	{
		data.Add(key, value);
	}

	public static Rect GetRect(string key)
	{
		return data.Get<Rect>(key);
	}

	public static Rect GetRect(string key, Rect defaultValue)
	{
		return data.Get(key, defaultValue);
	}

	public static void SetRect(string key, Rect value)
	{
		data.Add(key, value);
	}
}
