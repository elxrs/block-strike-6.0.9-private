using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeathRun : Photon.MonoBehaviour
{
	private static DeathRun instance;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.DeathRun)
		{
			Destroy(this);
		}
	}

	private void Start()
	{
		instance = this;
		photonView.AddMessage("OnSendKillerInfo", OnSendKillerInfo);
		photonView.AddMessage("OnKilledPlayer", OnKilledPlayer);
		photonView.AddMessage("OnFinishRound", OnFinishRound);
		photonView.AddMessage("CheckPlayers", CheckPlayers);
		photonView.AddMessage("PhotonClickButton", PhotonClickButton);
		UIScore.SetActiveScore(true, GameManager.maxScore);
		GameManager.startDamageTime = nValue.float01;
		UIPanelManager.ShowPanel("Display");
		GameManager.maxScore = nValue.int20;
		GameManager.changeWeapons = false;
		GameManager.globalChat = false;
		CameraManager.SetType(CameraType.Static);
		TimerManager.In(nValue.float05, delegate
		{
			if (PhotonNetwork.isMasterClient)
			{
				ActivationWaitPlayer();
			}
			else if (GameManager.player.Dead)
			{
				CameraManager.SetType(CameraType.Spectate);
			}
		});
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void OnEnable()
	{
		PhotonNetwork.onPhotonPlayerConnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Combine(PhotonNetwork.onPhotonPlayerConnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerConnected));
		PhotonNetwork.onPhotonPlayerDisconnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Combine(PhotonNetwork.onPhotonPlayerDisconnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerDisconnected));
	}

	private void OnDisable()
	{
		PhotonNetwork.onPhotonPlayerConnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Remove(PhotonNetwork.onPhotonPlayerConnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerConnected));
		PhotonNetwork.onPhotonPlayerDisconnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Remove(PhotonNetwork.onPhotonPlayerDisconnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerDisconnected));
	}

	private void ActivationWaitPlayer()
	{
		EventManager.Dispatch("WaitPlayer");
		GameManager.roundState = RoundState.WaitPlayer;
		GameManager.team = Team.Blue;
		OnWaitPlayer();
		OnCreatePlayer();
	}

	private void OnWaitPlayer()
	{
		UIStatus.Add(Localization.Get("Waiting for other players"), true);
		TimerManager.In(nValue.int4, delegate
		{
			if (GameManager.roundState == RoundState.WaitPlayer)
			{
				if (PhotonNetwork.playerList.Length <= nValue.int1)
				{
					OnWaitPlayer();
				}
				else
				{
					TimerManager.In(nValue.int4, delegate
					{
						OnStartRound();
					});
				}
			}
		});
	}

	private void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			GameManager.SetScore(playerConnect);
			if (GameManager.roundState != 0)
			{
				CheckPlayers();
			}
		}
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer playerDisconnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			CheckPlayers();
		}
	}

	private void OnStartRound()
	{
		UIDeathScreen.ClearAll();
		DecalsManager.ClearBulletHoles();
		if (PhotonNetwork.playerList.Length <= nValue.int1)
		{
			ActivationWaitPlayer();
		}
		else if (PhotonNetwork.isMasterClient)
		{
			List<PhotonPlayer> list = PhotonNetwork.playerList.ToList();
			int num = OnSelectMaxDeaths(list.Count);
			string text = string.Empty;
			for (int i = 0; i < num; i++)
			{
				int index = UnityEngine.Random.Range(nValue.int0, list.Count);
				text = text + list[index].ID + "#";
				list.RemoveAt(index);
			}
			GameManager.roundState = RoundState.PlayRound;
			PhotonDataWrite data = photonView.GetData();
			data.Write(text);
			photonView.RPC("OnSendKillerInfo", PhotonTargets.All, data);
		}
	}

	[PunRPC]
	private void OnSendKillerInfo(PhotonMessage message)
	{
		string text = message.ReadString();
		string[] array = text.Split("#"[nValue.int0]);
		bool flag = false;
		for (int i = nValue.int0; i < array.Length - nValue.int1; i++)
		{
			if (PhotonNetwork.player.ID == int.Parse(array[i]))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			GameManager.team = Team.Red;
		}
		else
		{
			GameManager.team = Team.Blue;
		}
		EventManager.Dispatch("StartRound");
		OnCreatePlayer();
		TimerManager.In(nValue.int3, delegate
		{
			if (PhotonNetwork.isMasterClient)
			{
				CheckPlayers();
			}
		});
	}

	private void OnCreatePlayer()
	{
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		PlayerInput player = GameManager.player;
		player.SetHealth(nValue.int100);
		CameraManager.SetType(CameraType.None);
		GameManager.controller.ActivePlayer(SpawnManager.GetTeamSpawn().spawnPosition, SpawnManager.GetTeamSpawn().spawnRotation);
		player.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
		if (PhotonNetwork.player.GetTeam() == Team.Blue)
		{
			player.UpdatePlayerSpeed(nValue.float02);
		}
		else
		{
			player.UpdatePlayerSpeed(nValue.float04);
		}
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		EventManager.Dispatch("KillPlayer", damageInfo);
		if (GameManager.roundState == RoundState.PlayRound)
		{
			PhotonNetwork.player.SetDeaths1();
			PlayerRoundManager.SetDeaths1();
		}
		if (GameManager.roundState == RoundState.PlayRound)
		{
			UIStatus.Add(damageInfo);
			UIDeathScreen.Show(damageInfo);
		}
		if (GameManager.roundState == RoundState.PlayRound)
		{
			Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.player.PlayerTransform.position, damageInfo.position);
			CameraManager.SetType(CameraType.Dead, GameManager.player.FPCamera.Transform.position, GameManager.player.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
			GameManager.controller.DeactivePlayer(ragdollForce, damageInfo.headshot);
		}
		else
		{
			OnCreatePlayer();
		}
		if (damageInfo.otherPlayer)
		{
			PhotonDataWrite data = photonView.GetData();
			data.Write(damageInfo.Deserialize());
			photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.player), data);
		}
		if (GameManager.roundState != RoundState.PlayRound)
		{
			return;
		}
		photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
		TimerManager.In(nValue.int3, delegate
		{
			if (GameManager.player.Dead)
			{
				CameraManager.SetType(CameraType.Spectate);
			}
		});
	}

	[PunRPC]
	private void OnKilledPlayer(PhotonMessage message)
	{
		DamageInfo damageInfo = DamageInfo.Serialize(message.ReadBytes());
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetKills1();
		UIDeathScreen.AddKill(message.sender.ID);
		if (damageInfo.headshot)
		{
			PlayerRoundManager.SetXP(nValue.int12);
			PlayerRoundManager.SetMoney(nValue.int10);
			PlayerRoundManager.SetHeadshot1();
		}
		else
		{
			PlayerRoundManager.SetXP(nValue.int6);
			PlayerRoundManager.SetMoney(nValue.int5);
		}
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessage message)
	{
		GameManager.roundState = RoundState.EndRound;
		if (GameManager.checkScore)
		{
			GameManager.LoadNextLevel(GameMode.DeathRun);
			return;
		}
		float num = (float)nValue.int8 - (float)(PhotonNetwork.time - message.timestamp);
		if (num <= (float)nValue.int0)
		{
			num = nValue.float01;
		}
		TimerManager.In(num, delegate
		{
			OnStartRound();
		});
	}

	[PunRPC]
	private void CheckPlayers(PhotonMessage message)
	{
		CheckPlayers();
	}

	private void CheckPlayers()
	{
		if (!PhotonNetwork.isMasterClient || GameManager.roundState == RoundState.EndRound)
		{
			return;
		}
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		bool flag = false;
		bool flag2 = false;
		for (int i = nValue.int0; i < playerList.Length; i++)
		{
			if (playerList[i].GetTeam() == Team.Blue && !playerList[i].GetDead())
			{
				flag = true;
				break;
			}
		}
		for (int j = nValue.int0; j < playerList.Length; j++)
		{
			if (playerList[j].GetTeam() == Team.Red && !playerList[j].GetDead())
			{
				flag2 = true;
				break;
			}
		}
		if (!flag)
		{
			++GameManager.redScore;
			GameManager.SetScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Red Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
		else if (!flag2)
		{
			++GameManager.blueScore;
			GameManager.SetScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Blue Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}

	public static void ClickButton(byte button)
	{
		PhotonDataWrite data = instance.photonView.GetData();
		data.Write(button);
		instance.photonView.RPC("PhotonClickButton", PhotonTargets.All, data);
	}

	[PunRPC]
	private void PhotonClickButton(PhotonMessage message)
	{
		EventManager.Dispatch("DeathRunClickButton", message.ReadByte());
	}

	private int OnSelectMaxDeaths(int maxPlayers)
	{
		if (maxPlayers > nValue.int6)
		{
			return nValue.int2;
		}
		return nValue.int1;
	}
}
