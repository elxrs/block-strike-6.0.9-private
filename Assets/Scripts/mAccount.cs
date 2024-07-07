using System;
using UnityEngine;

public class mAccount : MonoBehaviour
{
	public static string Name;

#if UNITY_EDITOR
	public static string gmail;
#endif

	private void Start()
	{
		if (mPopUp.activePopup)
		{
			mPopUp.HideAll("Menu");
		}
		if (AccountManager.isConnect)
		{
			return;
		}
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			TimerManager.In(0.5f, delegate
			{
				Start2();
			});
		}
		else
		{
#if UNITY_EDITOR
			gmail = PlayerPrefs.GetString("EditorGmail", "tibers28@gmail*com");
			Start2();
#else
            Start2();
#endif
		}
	}

	private void Start2()
	{
#if UNITY_EDITOR
		TimerManager.In(0.1f, delegate
		{
			mPopUp.SetActiveWait(true, Localization.Get("Connect to the account", true) + ": " + gmail.Remove(gmail.LastIndexOf("@")));
		});
		TimerManager.In(0.3f, delegate
		{
			AccountManager.Login(gmail, new Action<bool>(Login), new Action<string>(LoginError));
		});
		return;
#endif
		string id = AndroidNativeFunctions.GetAndroidID2();
		if (string.IsNullOrEmpty(id))
		{
			mPopUp.ShowPopup(Localization.Get("Error") + ": " + Localization.Get("Your device is not found a Google Account"), "Account", Localization.Get("Connect"), Start, Localization.Get("Exit"), Exit);
			return;
		}
		mPopUp.SetActiveWait(true, Localization.Get("Connect to the account"));
		TimerManager.In(0.2f, delegate
		{
			AccountManager.Login(id, Login, LoginError);
		});
	}

	private void Login(bool isCreated)
	{
		if (isCreated)
		{
			mPopUp.SetActiveWait(false);
			EventManager.Dispatch("AccountUpdate");
			WeaponManager.UpdateData();
			if (string.IsNullOrEmpty(AccountManager.instance.Data.AccountName) || AccountManager.instance.Data.AccountName.ToString() == " ")
			{
				SetPlayerName(delegate (string playerName)
				{
					AccountManager.UpdateName(playerName, SetPlayerNameComplete, SetPlayerNameError);
					mPopUp.ShowText(Localization.Get("Please wait") + "...");
				});
			}
		}
		else
		{
			mPopUp.SetActiveWait(false);
			mPopUp.ShowText(Localization.Get("Please wait") + "...");
			SetPlayerName(delegate (string playerName)
			{
				AccountManager.Register(playerName, RegisterComplete, RegisterError);
				mPopUp.ShowText(Localization.Get("Please wait") + "...");
			});
		}
	}

	private void LoginError(string error)
	{
		mPopUp.ShowPopup(Localization.Get("Error") + ": " + error, "Account", Localization.Get("Connect"), LoginErrorTry, Localization.Get("Exit"), Exit);
		mPopUp.SetActiveWait(false);
	}

	private void LoginErrorTry()
	{
		if (mPopUp.activePopup)
		{
			mPopUp.HideAll("Menu");
		}
		AccountManager.Login(AccountManager.AccountID, new Action<bool>(Login), new Action<string>(LoginError));
		string text = AccountManager.AccountID;
		text = text.Remove(text.LastIndexOf("@"));
		mPopUp.SetActiveWait(true, Localization.Get("Connect to the account") + ": " + text);
	}

	private void RegisterComplete()
	{
		EventManager.Dispatch("AccountUpdate");
		WeaponManager.UpdateData();
		mPopUp.HideAll("Menu");
	}

	private void RegisterError(string error)
	{
		mPopUp.ShowPopup(Localization.Get("Error") + ": " + error, "Account", Localization.Get("Connect"), RegisterErrorTry, Localization.Get("Exit"), Exit);
	}

	private void RegisterErrorTry()
	{
		if (mPopUp.activePopup)
		{
			mPopUp.HideAll("Menu");
		}
		SetPlayerName(delegate (string playerName)
		{
			AccountManager.Register(playerName, RegisterComplete, RegisterError);
			mPopUp.ShowText(Localization.Get("Please wait") + "...");
		});
	}

	private void SetPlayerName(Action<string> action)
	{
		mPopUp.SetActiveWait(false);
		mPopUp.ShowInput(string.Empty, Localization.Get("ChangeName"), 12, UIInput.KeyboardType.Default, SetPlayerNameSubmit, SetPlayerNameChange, "Ok", delegate
		{
			SetPlayerNameSave(action);
		}, Localization.Get("Back"), null);
	}

	private void SetPlayerNameSave(Action<string> action)
	{
		string text = NGUIText.StripSymbols(mPopUp.GetInputText());
		string text2 = mChangeName.UpdateSymbols(text, true);
		if (text != text2)
		{
			mPopUp.SetInputText(text2);
		}
		else if (text.Length <= 3 || text == "Null" || text[0].ToString() == " " || text[text.Length - 1].ToString() == " ")
		{
			text = "Player " + UnityEngine.Random.Range(0, 99999);
			mPopUp.SetInputText(text);
		}
		else if (action != null)
		{
			action(text);
		}
	}

	private void SetPlayerNameSubmit()
	{
		string text = mPopUp.GetInputText();
		if (text.Length <= 3 || text == "Null" || text[0].ToString() == " " || text[text.Length - 1].ToString() == " ")
		{
			text = "Player " + UnityEngine.Random.Range(0, 99999);
		}
		text = NGUIText.StripSymbols(text);
		mPopUp.SetInputText(text);
	}

	private void SetPlayerNameChange()
	{
		string inputText = mPopUp.GetInputText();
		string text = mChangeName.UpdateSymbols(inputText, true);
		if (inputText != text)
		{
			mPopUp.SetInputText(text);
		}
	}

	private void SetPlayerNameComplete(string playerName)
	{
		EventManager.Dispatch("AccountUpdate");
		WeaponManager.UpdateData();
		mPopUp.HideAll("Menu");
	}

	private void SetPlayerNameError(string error)
	{
		SetPlayerName(delegate (string playerName)
		{
			AccountManager.UpdateName(playerName, SetPlayerNameComplete, SetPlayerNameError);
			mPopUp.ShowText(Localization.Get("Please wait") + "...");
		});
		string text = error;
		if (text == "Name already taken")
		{
			text = Localization.Get("Name already taken");
		}
		UIToast.Show(Localization.Get("Error") + ": " + text, 3f);
    }

    private void Exit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
