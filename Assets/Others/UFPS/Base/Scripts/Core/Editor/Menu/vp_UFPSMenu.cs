/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UFPSMenu.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	unity editor main menu items for UFPS
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

public class UFPSMenu 
{

	[MenuItem("UFPS/About UFPS", false, 0)]
	public static void About()
	{
		vp_AboutBox.Create();
	}

	/////////////////////////////////////////////////////////////////

	[MenuItem("UFPS/UFPS Manual", false, 22)]
	public static void Manual()
	{
		Application.OpenURL("http://www.opsive.com/assets/UFPS/hub/assets/ufps/manual");
	}


	[MenuItem("UFPS/UFPS Add-ons", false, 23)]
	public static void Addons()
	{
		vp_AddonBrowser.Create();
	}

	/////////////////////////////////////////////////////////////////

	[MenuItem("UFPS/Event Handler", true)]
	public static bool ValidateEventHandler()
	{
		return Application.isPlaying;
	}

	/////////////////////////////////////////////////////////////////

	// AI

	// Mobile

	// Multiplayer

	/////////////////////////////////////////////////////////////////

	[MenuItem("UFPS/Help/F.A.Q.", false, 201)]
	public static void FAQ()
	{
		Application.OpenURL("http://www.opsive.com/assets/UFPS/hub/assets/ufps/faq");
	}

	[MenuItem("UFPS/Help/Support Info", false, 202)]
	public static void SupportInfo()
	{
		Application.OpenURL("http://www.opsive.com/assets/UFPS/hub/assets/ufps/supportinfo");
	}
	
	[MenuItem("UFPS/Help/Release Notes", false, 203)]
	public static void ReleaseNotes()
	{
		Application.OpenURL("http://www.opsive.com/assets/UFPS/hub/assets/ufps/releasenotes");
	}

	[MenuItem("UFPS/Help/Update Instructions", false, 204)]
	public static void UpdateInstructions()
	{
		Application.OpenURL("http://www.opsive.com/assets/UFPS/hub/assets/ufps/upgrading");
	}

	[MenuItem("UFPS/Community/Follow us on Twitter", false, 205)]
	public static void Twitter()
	{
		Application.OpenURL("http://www.opsive.com/assets/UFPS/hub/twitter");
	}

	[MenuItem("UFPS/Community/YouTube Channel", false, 206)]
	public static void YouTube()
	{
		Application.OpenURL("http://www.opsive.com/assets/UFPS/hub/youtube");
	}

	[MenuItem("UFPS/Community/Official UFPS Forum", false, 207)]
	public static void OfficialUFPSForum()
	{
		Application.OpenURL("http://www.opsive.com/assets/UFPS/hub/assets/ufps/forum");
	}

	/////////////////////////////////////////////////////////////////

	[MenuItem("UFPS/Check for Updates", false, 300)]
	public static void CheckForUpdates()
	{
		vp_UpdateDialog.Create("ufps", UFPSInfo.Version);
	}

}
