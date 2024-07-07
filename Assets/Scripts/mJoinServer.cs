using System;
using UnityEngine.Networking;
using System.Collections;
using BSCM;
using UnityEngine;

public class mJoinServer
{
	public static RoomInfo room;

	public static Action onBack;

	public static void Join()
	{
		if (string.IsNullOrEmpty(room.GetPassword()))
		{
			if (room.PlayerCount == (int)room.MaxPlayers)
			{
				OnLoadCustomMap(delegate
				{
					mPopUp.ShowPopup(Localization.Get("Do you want to queue up this server?", true), Localization.Get("Queue", true), Localization.Get("Yes", true), delegate ()
					{
						mPhotonSettings.QueueServer(room);
					}, Localization.Get("No", true), delegate ()
					{
						onBack();
					});
				});
			}
			else
			{
				OnLoadCustomMap(delegate
				{
					mPhotonSettings.JoinServer(room);
				});
			}
		}
		else if (room.isOfficialServer())
		{
			if (room.PlayerCount == (int)room.MaxPlayers)
			{
				mPopUp.ShowPopup(Localization.Get("Do you want to queue up this server?", true), Localization.Get("Queue", true), Localization.Get("Yes", true), delegate ()
				{
					mPhotonSettings.QueueServer(room);
				}, Localization.Get("No", true), onBack);
			}
			else
			{
				mPhotonSettings.JoinServer(room);
			}
		}
		else
		{
			mPopUp.ShowInput(string.Empty, Localization.Get("Password", true), 4, UIInput.KeyboardType.NumberPad, null, null, "Ok", delegate
			{
				OnPassword();
			}, Localization.Get("Back", true), delegate
			{
				onBack();
			});
		}
	}

	private static void OnPassword()
	{
		if (room.GetPassword() == mPopUp.GetInputText())
		{
			if (room.PlayerCount == (int)room.MaxPlayers)
			{
				OnLoadCustomMap(delegate
				{
					mPopUp.ShowPopup(Localization.Get("Do you want to queue up this server?", true), Localization.Get("Queue", true), Localization.Get("Yes", true), delegate ()
					{
						mPhotonSettings.QueueServer(room);
					}, Localization.Get("No", true), onBack);
				});
			}
			else
			{
				OnLoadCustomMap(delegate
				{
					mPhotonSettings.JoinServer(room);
				});
			}
		}
		else
		{
			UIToast.Show(Localization.Get("Password is incorrect", true));
#if UNITY_EDITOR
			Debug.Log("Password: " + room.GetPassword());
#endif
		}
	}

	private static void OnLoadCustomMap(Action callback)
	{
		if (!room.isCustomMap())
		{
			LevelManager.customScene = false;
			if (callback != null)
			{
				callback();
			}
			return;
		}
		int hash = Manager.GetBundleHash(room.GetSceneName());
		if (hash != 0 && hash == room.GetCustomMapHash())
		{
			Manager.LoadBundle(Manager.GetBundlePath(room.GetSceneName()));
			LevelManager.customScene = true;
			if (callback != null)
			{
				callback();
			}
		}
		else
		{
			mPopUp.ShowPopup(Localization.Get("Do you really want to download a custom map?", true), Localization.Get("Map", true), Localization.Get("Yes", true), delegate ()
			{
				CoroutineStarter._StartCoroutine(DownloadCustomMap(hash, callback, room.GetCustomMapUrl()));
			}, Localization.Get("No", true), delegate ()
			{
				onBack();
			});
		}
	}

	private static IEnumerator DownloadCustomMap(int hash, Action callback, string CustomMapUrl)
	{
		string url = "https://drive.google.com/uc?export=download&id=" + CustomMapUrl;
		using (UnityWebRequest www = UnityWebRequest.Get(url))
		{
			mPopUp.ShowPopup(Localization.Get("Please wait", true) + "...", Localization.Get("Map", true), Localization.Get("Exit", true), delegate ()
			{
				www.Abort();
				LevelManager.customScene = false;
				onBack();
			});

			yield return www.Send();

			if (www.isError)
			{
				LevelManager.customScene = false;
				UIToast.Show(Localization.Get("Error", true) + ": " + www.error);
				onBack();
			}
			else
			{
				string path = Manager.SaveBundle(room.GetSceneName(), room.GetCustomMapModes(), room.GetCustomMapHash(), room.GetCustomMapUrl(), www.downloadHandler.data);
				hash = Manager.GetBundleHash(room.GetSceneName());
				if (hash != 0 && hash == room.GetCustomMapHash())
				{
					Manager.LoadBundle(path);
					LevelManager.customScene = true;
					if (callback != null)
					{
						callback();
					}
				}
				else
				{
					LevelManager.customScene = false;
					UIToast.Show(Localization.Get("Error", true));
					onBack();
				}
			}
		}
	}
}

public sealed class CoroutineStarter : MonoBehaviour
{
	private static CoroutineStarter instance
	{
		get
		{
			if (m_instance == null)
			{
				GameObject go = new GameObject("StarterRoutine");
				m_instance = go.AddComponent<CoroutineStarter>();
			}
			return m_instance;
		}
	}

	private static CoroutineStarter m_instance;

	public static Coroutine _StartCoroutine(IEnumerator enumerator)
	{
		return instance.StartCoroutine(enumerator);
	}

	public static void _StopCoroutine(IEnumerator enumerator)
	{
		instance.StopCoroutine(enumerator);
	}
}
