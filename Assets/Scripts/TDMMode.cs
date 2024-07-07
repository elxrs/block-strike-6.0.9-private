using System;
using UnityEngine;

public class TDMMode : Photon.MonoBehaviour
{
	public CryptoInt MaxScore = 100;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != 0)
		{
			Destroy(this);
		}
	}

	private void Start()
	{
		photonView.AddMessage("PhotonOnScore", PhotonOnScore);
		photonView.AddMessage("OnKilledPlayer", OnKilledPlayer);
		photonView.AddMessage("PhotonNextLevel", PhotonNextLevel);
		GameManager.roundState = RoundState.PlayRound;
		CameraManager.SetType(CameraType.Static);
		UIScore.SetActiveScore(true, MaxScore);
		UISelectTeam.OnSpectator();
		UISelectTeam.OnStart(OnSelectTeam);
		GameManager.maxScore = MaxScore;
		GameManager.StartAutoBalance();
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
		EventManager.AddListener<Team>("AutoBalance", OnAutoBalance);
	}

	private void OnEnable()
	{
		PhotonNetwork.onPhotonPlayerConnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Combine(PhotonNetwork.onPhotonPlayerConnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerConnected));
	}

	private void OnDisable()
	{
		PhotonNetwork.onPhotonPlayerConnected = (PhotonNetwork.PhotonPlayerDelegate)Delegate.Remove(PhotonNetwork.onPhotonPlayerConnected, new PhotonNetwork.PhotonPlayerDelegate(OnPhotonPlayerConnected));
	}

	private void OnSelectTeam(Team team)
	{
		UIPanelManager.ShowPanel("Display");
		if (team == Team.None)
		{
			CameraManager.SetType(CameraType.Spectate);
			CameraManager.ChangeType = true;
			UIControllerList.Chat.cachedGameObject.SetActive(false);
			UIControllerList.SelectWeapon.cachedGameObject.SetActive(false);
			UISpectator.SetActive(true);
		}
		else
		{
			OnRevivalPlayer();
		}
	}

	private void OnAutoBalance(Team team)
	{
		GameManager.team = team;
		OnRevivalPlayer();
	}

	private void OnRevivalPlayer()
	{
		PlayerInput player = GameManager.player;
		player.SetHealth(nValue.int100);
		CameraManager.SetType(CameraType.None);
		GameManager.controller.ActivePlayer(SpawnManager.GetTeamSpawn().spawnPosition, SpawnManager.GetTeamSpawn().spawnRotation);
		player.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle);
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		PhotonNetwork.player.SetDeaths1();
		PlayerRoundManager.SetDeaths1();
		UIDeathScreen.Show(damageInfo);
		UIStatus.Add(damageInfo);
		Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.player.PlayerTransform.position, damageInfo.position);
		CameraManager.SetType(CameraType.Dead, GameManager.player.FPCamera.Transform.position, GameManager.player.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
		GameManager.controller.DeactivePlayer(ragdollForce, damageInfo.headshot);
		if (damageInfo.otherPlayer)
		{
			PhotonDataWrite data = photonView.GetData();
			data.Write(damageInfo.Deserialize());
			photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.player), data);
			OnScore(damageInfo.team);
		}
		TimerManager.In(nValue.int3, delegate
		{
			OnRevivalPlayer();
		});
	}

	private void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			TimerManager.In(0.5f, delegate
			{
				GameManager.SetScore(playerConnect);
			});
		}
	}

	[PunRPC]
	private void OnKilledPlayer(PhotonMessage message)
	{
		DamageInfo e = DamageInfo.Serialize(message.ReadBytes());
		EventManager.Dispatch("KillPlayer", e);
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetKills1();
		UIDeathScreen.AddKill(message.sender.ID);
		if (e.headshot)
		{
			PlayerRoundManager.SetXP(nValue.int10);
			PlayerRoundManager.SetMoney(nValue.int8);
			PlayerRoundManager.SetHeadshot1();
		}
		else
		{
			PlayerRoundManager.SetXP(nValue.int5);
			PlayerRoundManager.SetMoney(nValue.int4);
		}
	}

	public void OnScore(Team team)
	{
		PhotonDataWrite data = photonView.GetData();
		data.Write((byte)team);
		photonView.RPC("PhotonOnScore", PhotonTargets.MasterClient, data);
	}

	[PunRPC]
	private void PhotonOnScore(PhotonMessage message)
	{
		switch ((Team)message.ReadByte())
		{
		case Team.Blue:
			++GameManager.blueScore;
			break;
		case Team.Red:
			++GameManager.redScore;
			break;
		}
		GameManager.SetScore();
		if (GameManager.checkScore)
		{
			GameManager.roundState = RoundState.EndRound;
			if (GameManager.winTeam == Team.Blue)
			{
				UIMainStatus.Add("[@]", false, nValue.int5, "Blue Win");
			}
			else if (GameManager.winTeam == Team.Red)
			{
				UIMainStatus.Add("[@]", false, nValue.int5, "Red Win");
			}
			photonView.RPC("PhotonNextLevel", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void PhotonNextLevel(PhotonMessage message)
	{
		GameManager.LoadNextLevel(GameMode.TeamDeathmatch);
	}
}
