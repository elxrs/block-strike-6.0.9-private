using UnityEngine;
using UnityEditor;
using System.IO;

public class VersionManagerEditor : EditorWindow
{
	public bool isFull = true;
	public bool isTest = false;
	public string SelectVersion;
	
	public string fullproductName;
	public string fullbundleIdentifier;
	public string fullbundleVersion;
	public int fullbundleVersionCode;
	
	public string liteproductName;
	public string litebundleIdentifier;
	public string litebundleVersion;
	public int litebundleVersionCode;
	
	public bool isAutoFillPublishingSettings;
	public string keystorePassword;
	public string keyPassword;
	
	[MenuItem("Window/VersionManager")]
	public static void VersionInfo()
	{
		EditorWindow.GetWindow<VersionManagerEditor>(true, "Version Manager", true);
	}
	
	void OnEnable()
	{
		string name = GetProjectName();
		
		SelectVersion = EditorPrefs.GetBool(name + "_" + "Version", true) ? "Full" : "Lite";
		
		isTest = EditorPrefs.GetBool(name + "_" + "TestVersion", false);
		isFull = EditorPrefs.GetBool(name + "_" + "isFull", true);
		
		fullproductName = EditorPrefs.GetString(name + "_" + "fullproductName", PlayerSettings.productName);
		fullbundleIdentifier = EditorPrefs.GetString(name + "_" + "fullbundleIdentifier", PlayerSettings.applicationIdentifier);
		fullbundleVersion = EditorPrefs.GetString(name + "_" + "fullbundleVersion", PlayerSettings.bundleVersion);
		fullbundleVersionCode = EditorPrefs.GetInt(name + "_" + "fullbundleVersionCode", PlayerSettings.Android.bundleVersionCode);
		
		liteproductName = EditorPrefs.GetString(name + "_" + "liteproductName", PlayerSettings.productName);
		litebundleIdentifier = EditorPrefs.GetString(name + "_" + "litebundleIdentifier", PlayerSettings.applicationIdentifier);
		litebundleVersion = EditorPrefs.GetString(name + "_" + "litebundleVersion", PlayerSettings.bundleVersion);
		litebundleVersionCode = EditorPrefs.GetInt(name + "_" + "litebundleVersionCode", PlayerSettings.Android.bundleVersionCode);
		
		isAutoFillPublishingSettings = EditorPrefs.GetBool(name + "_" + "autoFillKeystore", false);
		keystorePassword = EditorPrefs.GetString(name + "_" + "keystorePassword", PlayerSettings.keystorePass);
		keyPassword = EditorPrefs.GetString(name + "_" + "keyPassword", PlayerSettings.keyaliasPass);
	}
	
	void OnDisable()
	{
		string name = GetProjectName();
		bool full = false;
		
		if (SelectVersion == "Full")
		{
			full = true;	
		}
		
		EditorPrefs.SetBool(name + "_" + "Version", full);
		
		EditorPrefs.SetBool(name + "_" + "TestVersion", isTest);
		EditorPrefs.SetBool(name + "_" + "isFull", isFull);
		
		EditorPrefs.SetString(name + "_" + "fullproductName", fullproductName);
		EditorPrefs.SetString(name + "_" + "fullbundleIdentifier", fullbundleIdentifier);
		EditorPrefs.SetString(name + "_" + "fullbundleVersion", fullbundleVersion);
		EditorPrefs.SetInt(name + "_" + "fullbundleVersionCode", fullbundleVersionCode);
		
		EditorPrefs.SetString(name + "_" + "liteproductName", liteproductName);
		EditorPrefs.SetString(name + "_" + "litebundleIdentifier", litebundleIdentifier);
		EditorPrefs.SetString(name + "_" + "litebundleVersion", litebundleVersion);
		EditorPrefs.SetInt(name + "_" + "litebundleVersionCode", litebundleVersionCode);
		
		EditorPrefs.SetBool(name + "_" + "autoFillKeystore", isAutoFillPublishingSettings);
		EditorPrefs.SetString(name + "_" + "keystorePassword", keystorePassword);
		EditorPrefs.SetString(name + "_" + "keyPassword", keyPassword);
	}
	
	public void OnGUI()
	{
		minSize = new Vector2(400f, isAutoFillPublishingSettings ? 275f : 235f);
		maxSize = new Vector2(400f, isAutoFillPublishingSettings ? 275f : 235f);

		EditorGUILayout.Space();
		PlayerSettings.companyName = EditorGUILayout.TextField("Company Name: ", PlayerSettings.companyName);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Select Version :", SelectVersion);
		
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button(isFull ? isTest ? ">Full Test" : ">Full" : isTest ? "Full" : "Full"))
			isFull = true;
		if (GUILayout.Button(isFull ? isTest ? "Lite" : "Lite" : isTest ? ">Lite Test" : ">Lite"))
			isFull = false;
		EditorGUILayout.EndHorizontal();
		
		if (isFull)
			FullVersion();
		else
			LiteVersion();
		
		isAutoFillPublishingSettings = EditorGUILayout.Toggle("Auto Fill Keystore", isAutoFillPublishingSettings);
		
		if (isAutoFillPublishingSettings)
		{
			keystorePassword = EditorGUILayout.PasswordField("Keystore Password:", keystorePassword);
			keyPassword = EditorGUILayout.PasswordField("Key Alias Password:", keyPassword);
		}
		
		if (GUILayout.Button("Apply"))
			SetSettings();
	}
	
	void FullVersion()
	{
		EditorGUILayout.Space();
		
		EditorGUILayout.Space();
		isTest = EditorGUILayout.Toggle("Test Version", isTest);
		fullproductName = EditorGUILayout.TextField("Product Name: ", fullproductName);	
		fullbundleIdentifier = EditorGUILayout.TextField("Bundle Identifier: ", fullbundleIdentifier);
		fullbundleVersion = EditorGUILayout.TextField("Bundle Version: ", fullbundleVersion);
		fullbundleVersionCode = EditorGUILayout.IntField("Bundle Version Code: ", fullbundleVersionCode);
	}
	
	void LiteVersion()
	{
		EditorGUILayout.Space();
		
		EditorGUILayout.Space();
		isTest = EditorGUILayout.Toggle("Test Version", isTest);
		liteproductName = EditorGUILayout.TextField("Product Name: ", liteproductName);	
		litebundleIdentifier = EditorGUILayout.TextField("Bundle Identifier: ", litebundleIdentifier);
		litebundleVersion = EditorGUILayout.TextField("Bundle Version: ", litebundleVersion);
		litebundleVersionCode = EditorGUILayout.IntField("Bundle Version Code: ", litebundleVersionCode);	
	}
	
	void SetSettings()
	{
		EditorPrefs.SetBool(name + "_" + "Version", isFull);
		SelectVersion = isFull ? "Full" : "Lite";
		
		PlayerSettings.productName = isFull ? fullproductName : liteproductName;
		PlayerSettings.applicationIdentifier = isFull ? fullbundleIdentifier : litebundleIdentifier;
		PlayerSettings.bundleVersion = isFull ? fullbundleVersion : litebundleVersion;
		PlayerSettings.Android.bundleVersionCode = isFull ? fullbundleVersionCode : litebundleVersionCode;
		
		if (isAutoFillPublishingSettings)
		{
			PlayerSettings.keystorePass = keystorePassword;
			PlayerSettings.keyaliasPass = keyPassword;
		}
		
		if (!Directory.Exists(Application.dataPath + "/Resources"))
		{
			Directory.CreateDirectory(Application.dataPath + "/Resources");
		}
		if (!Directory.Exists(Application.dataPath + "/Resources/VersionManager"))
		{
			Directory.CreateDirectory(Application.dataPath + "/Resources/VersionManager");
		}
		
		if (!File.Exists(Application.dataPath + "/Resources/VersionManager/VersionInfo.bytes"))
		{
			File.WriteAllText(Application.dataPath + "/Resources/VersionManager/VersionInfo.bytes", GetInfo());
		}
		else
		{
			File.WriteAllText(Application.dataPath + "/Resources/VersionManager/VersionInfo.bytes", GetInfo());
		}
		
		AssetDatabase.Refresh();
	}
	
	string GetInfo()
	{
		return Utils.XOR(PlayerSettings.productName + "\n" + PlayerSettings.applicationIdentifier + "\n" + PlayerSettings.bundleVersion + "\n" + PlayerSettings.Android.bundleVersionCode + "\n" + isFull + "\n" + isTest.ToString(), true);
	}

	public string GetProjectName()
	{
    	string[] s = Application.dataPath.Split('/');
    	string projectName = s[s.Length - 2];
    	return projectName;
	}
}

[InitializeOnLoad]
public class AutoSetPublishSettings : MonoBehaviour
{
	static AutoSetPublishSettings()
	{
		string[] s = Application.dataPath.Split('/');
    	string projectName = s[s.Length - 2];
		
		if (EditorPrefs.GetBool(projectName + "_" + "autoFillKeystore", false))
		{
			PlayerSettings.keystorePass = EditorPrefs.GetString(projectName + "_" + "keystorePassword", PlayerSettings.keystorePass);
			PlayerSettings.keyaliasPass = EditorPrefs.GetString(projectName + "_" + "keyPassword", PlayerSettings.keyaliasPass);
		}
	}	
}
