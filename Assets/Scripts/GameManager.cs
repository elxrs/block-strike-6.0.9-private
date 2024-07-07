using System;
using System.Collections.Generic;
using BSCM.Game;
using DG.Tweening;
using ExitGames.Client.Photon;
using UnityEngine;

public class GameManager : Photon.MonoBehaviour
{
	public RoundState state;

	public static bool changeWeapons = true;

	public static bool globalChat = true;

	public static CryptoInt maxScore = 20;

	public static CryptoInt blueScore = 0;

	public static CryptoInt redScore = 0;

	public static ControllerManager controller;

	public static bool friendDamage = false;

	public static CryptoBool startDamage = true;

	public static CryptoFloat startDamageTime = 4f;

	public static bool defaultScore = true;

	public static bool loadingLevel = false;

	public static string leaveRoomMessage;

	private static GameManager instance;

	public static PlayerInput player
	{
		get
		{
			return controller.playerInput;
		}
	}

	public static Team team
	{
		get
		{
			return player.PlayerTeam;
		}
		set
		{
			controller.SetTeam(value);
		}
	}

	public static Team winTeam
	{
		get
		{
			if ((int)blueScore >= (int)maxScore)
			{
				return Team.Blue;
			}
			if ((int)redScore >= (int)maxScore)
			{
				return Team.Red;
			}
			return Team.None;
		}
	}

	public static bool checkScore
	{
		get
		{
			if (LevelManager.customScene)
			{
				return false;
			}
			if ((int)blueScore >= (int)maxScore || (int)redScore >= (int)maxScore)
			{
				return true;
			}
			return false;
		}
	}

	public static RoundState roundState
	{
		get
		{
			return instance.state;
		}
		set
		{
			if (PhotonNetwork.isMasterClient)
			{
				PhotonNetwork.room.SetRoundState(value);
				instance.state = value;
			}
		}
	}

	public static GameManager main
	{
		get
		{
			return instance;
		}
	}

	private void Awake()
	{
		instance = this;
		if (!PhotonNetwork.offlineMode && !PhotonNetwork.inRoom)
		{
			LevelManager.LoadLevel("Menu");
		}
	}

	private void Start()
	{
		controller = PhotonNetwork.Instantiate("Player/ControllerManager", Vector3.zero, Quaternion.identity, 0).GetComponent<ControllerManager>();
		PlayerInput.instance = controller.playerInput;
		PlayerRoundManager.SetMode(PhotonNetwork.room.GetGameMode());
		state = PhotonNetwork.room.GetRoundState();
		PhotonRPC.AddMessage("PhotonUpdateScore", PhotonUpdateScore);
		PhotonRPC.AddMessage("PhotonLoadNextLevel", PhotonLoadNextLevel);
		PhotonRPC.AddMessage("UpdateRoundInfo", RoundPlayersData.UpdateRoundInfo);
		DropWeaponManager.ClearScene();
		if (PhotonNetwork.room.isCustomMap())
		{
			SceneSettings.instance.Create();
		}
	}

	private void OnEnable()
	{
		PhotonNetwork.onPhotonPlayerConnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Combine(PhotonNetwork.onPhotonPlayerConnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerConnected));
		PhotonNetwork.onPhotonPlayerDisconnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Combine(PhotonNetwork.onPhotonPlayerDisconnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerDisconnected));
		PhotonNetwork.onPhotonCustomRoomPropertiesChanged = (PhotonNetwork.HashtableDelegate)Delegate.Combine(PhotonNetwork.onPhotonCustomRoomPropertiesChanged, new PhotonNetwork.HashtableDelegate(OnPhotonCustomRoomPropertiesChanged));
	}

	private void OnDisable()
	{
		changeWeapons = true;
		globalChat = true;
		friendDamage = false;
		startDamage = true;
		startDamageTime = 4f;
		blueScore = nValue.int0;
		redScore = nValue.int0;
		maxScore = nValue.int20;
		loadingLevel = false;
		DOTween.Clear(false);
		PhotonRPC.Clear();
		PhotonNetwork.onPhotonPlayerConnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Remove(PhotonNetwork.onPhotonPlayerConnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerConnected));
		PhotonNetwork.onPhotonPlayerDisconnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Remove(PhotonNetwork.onPhotonPlayerDisconnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerDisconnected));
		PhotonNetwork.onPhotonCustomRoomPropertiesChanged = (PhotonNetwork.HashtableDelegate)Delegate.Remove(PhotonNetwork.onPhotonCustomRoomPropertiesChanged, new PhotonNetwork.HashtableDelegate(OnPhotonCustomRoomPropertiesChanged));
		defaultScore = true;
	}

	private void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		UIStatus.ConnectPlayer(player);
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		UIStatus.DisconnectPlayer(player);
	}

	private void OnPhotonCustomRoomPropertiesChanged(Hashtable hash)
	{
		if (hash.ContainsKey(PhotonCustomValue.roundStateKey))
		{
			state = (RoundState)(byte)hash[PhotonCustomValue.roundStateKey];
		}
	}

	public static void SetScore(PhotonPlayer player)
	{
		PhotonDataWrite data = instance.photonView.GetData();
		data.Write((byte)(int)maxScore);
		data.Write((byte)(int)blueScore);
		data.Write((byte)(int)redScore);
		PhotonRPC.RPC("PhotonUpdateScore", player, data);
	}

	public static void SetScore()
	{
		PhotonDataWrite data = instance.photonView.GetData();
		data.Write((byte)(int)maxScore);
		data.Write((byte)(int)blueScore);
		data.Write((byte)(int)redScore);
		PhotonRPC.RPC("PhotonUpdateScore", PhotonTargets.All, data);
	}

	[PunRPC]
	private void PhotonUpdateScore(PhotonMessage message)
	{
		byte b = message.ReadByte();
		byte b2 = message.ReadByte();
		byte b3 = message.ReadByte();
		maxScore = b;
		if ((int)maxScore > 0)
		{
			blueScore = Mathf.Clamp(b2, nValue.int0, maxScore);
			redScore = Mathf.Clamp(b3, nValue.int0, maxScore);
		}
		else
		{
			blueScore = b2;
			redScore = b3;
		}
		if (defaultScore)
		{
			UIScore.UpdateScore(maxScore, blueScore, redScore);
		}
		else
		{
			UIScore2.UpdateScore(maxScore, blueScore, redScore);
		}
		EventManager.Dispatch("UpdateScore");
	}

	public static void LoadNextLevel()
	{
		LoadNextLevel(PhotonNetwork.room.GetGameMode());
	}

	public static void LoadNextLevel(GameMode mode)
	{
		if (loadingLevel)
		{
			return;
		}
		loadingLevel = true;
		if (!LevelManager.customScene)
		{
			UIStatus.Add(Localization.Get("Next map") + ": " + LevelManager.GetNextScene(mode), true);
		}
		TimerManager.In(nValue.int4, delegate
		{
			if (LevelManager.customScene)
			{
				PhotonNetwork.LeaveRoom(true);
			}
			else if (PhotonNetwork.isMasterClient)
			{
				PhotonDataWrite data = instance.photonView.GetData();
				data.Write((byte)mode);
				PhotonRPC.RPC("PhotonLoadNextLevel", PhotonTargets.All, data);
			}
		});
		TimerManager.In(nValue.float15, delegate
		{
			PhotonNetwork.player.ClearProperties();
		});
	}

	[PunRPC]
	private void PhotonLoadNextLevel(PhotonMessage message)
	{
		byte mode = message.ReadByte();
		PhotonNetwork.RemoveRPCs(PhotonNetwork.player);
		PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);
		PhotonNetwork.LoadLevel(LevelManager.GetNextScene((GameMode)mode));
	}

	public static void StartAutoBalance()
	{
		TimerManager.In(nValue.int30, -nValue.int1, nValue.int30, BalanceTeam);
		UIChangeTeam.SetChangeTeam(true);
	}

	public static void BalanceTeam()
	{
		BalanceTeam(false);
	}

	public static void BalanceTeam(bool updateTeam)
	{
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		List<PhotonPlayer> list2 = new List<PhotonPlayer>();
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].GetTeam() == Team.Blue)
			{
				list.Add(playerList[i]);
			}
		}
		for (int j = 0; j < playerList.Length; j++)
		{
			if (playerList[j].GetTeam() == Team.Red)
			{
				list2.Add(playerList[j]);
			}
		}
		if (list.Count > list2.Count + nValue.int1 && PhotonNetwork.player.GetTeam() == Team.Blue)
		{
			list.Sort(UIPlayerStatistics.SortByKills);
			if (list[list.Count - nValue.int1].IsLocal)
			{
				if (updateTeam)
				{
					team = Team.Red;
				}
				EventManager.Dispatch("AutoBalance", Team.Red);
				UIToast.Show(Localization.Get("Autobalance: You moved to another team"));
			}
		}
		if (list2.Count <= list.Count + nValue.int1 || PhotonNetwork.player.GetTeam() != Team.Red)
		{
			return;
		}
		list2.Sort(UIPlayerStatistics.SortByKills);
		if (list2[list2.Count - nValue.int1].IsLocal)
		{
			if (updateTeam)
			{
				team = Team.Blue;
			}
			EventManager.Dispatch("AutoBalance", Team.Blue);
			UIToast.Show(Localization.Get("Autobalance: You moved to another team"));
		}
	}
}
