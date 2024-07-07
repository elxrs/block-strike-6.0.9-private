using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoundManager
{
    private static GameMode Mode;

    private static CryptoInt2 XP = 0;

    private static CryptoInt2 Money = 0;

    private static CryptoInt2 Kills = 0;

    private static CryptoInt2 Headshot = 0;

    private static CryptoInt2 Deaths = 0;

    private static List<cFireStat> FireStat = new List<cFireStat>();

    private static float StartTime;

    private static float FinishTime;

    public static GameMode GetMode()
	{
		return Mode;
	}

	public static int GetXP()
	{
		return XP;
	}

	public static int GetMoney()
	{
		return Money;
	}

	public static int GetKills()
	{
		return Kills;
	}

	public static int GetHeadshot()
	{
		return Headshot;
	}

	public static int GetDeaths()
	{
		return Deaths;
	}

	public static float GetTime()
	{
		return FinishTime - StartTime;
	}

	public static void SetMode(GameMode mode)
	{
		Mode = mode;
		if (StartTime == 0f)
		{
			StartTime = Time.time;
		}
	}

	public static void SetXP(int xp)
	{
		SetXP(xp, false);
	}

	public static void SetXP(int xp, bool simple)
	{
		if (LevelManager.customScene)
		{
			return;
		}
		if (simple)
		{
			XP += xp;
			return;
		}
		XP += Mathf.Clamp(Mathf.RoundToInt((float)PhotonNetwork.room.PlayerCount / (float)nValue.int12 * (float)xp), nValue.int0, xp);
	}

	public static void SetMoney(int money)
	{
		SetMoney(money, false);
	}

	public static void SetMoney(int money, bool simple)
	{
		if (LevelManager.customScene)
		{
			return;
		}
		if (simple)
		{
			Money += money;
			return;
		}
		Money += Mathf.Clamp(Mathf.RoundToInt((float)PhotonNetwork.room.PlayerCount / (float)nValue.int12 * (float)money), nValue.int0, money);
	}

	public static void SetKills1()
	{
		if (LevelManager.customScene)
		{
			return;
		}
		Kills += nValue.int1;
	}

	public static void SetHeadshot1()
	{
		if (LevelManager.customScene)
		{
			return;
		}
		Headshot += nValue.int1;
	}

	public static void SetDeaths1()
	{
		if (LevelManager.customScene)
		{
			return;
		}
		Deaths += nValue.int1;
	}

	public static void SetFireStat1(int weapon, int skin)
	{
		if (LevelManager.customScene)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < FireStat.Count; i++)
		{
			if (FireStat[i].weapon == weapon)
			{
				if (FireStat[i].skins.Contains(skin))
				{
					for (int j = 0; j < FireStat[i].skins.Count; j++)
					{
						if (FireStat[i].skins[j] == skin)
						{
							List<CryptoInt> counts;
							int index;
							CryptoInt value = (counts = FireStat[i].counts)[index = j];
							counts[index] = ++value;
						}
					}
				}
				else
				{
					FireStat[i].skins.Add(skin);
					FireStat[i].counts.Add(nValue.int1);
				}
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			cFireStat cFireStat = new cFireStat();
			cFireStat.weapon = weapon;
			cFireStat.skins.Add(skin);
			cFireStat.counts.Add(nValue.int1);
			FireStat.Add(cFireStat);
		}
	}

	public static bool hasValue
	{
		get
		{
			return GetXP() != 0 || GetMoney() != 0 || GetKills() != 0 || GetHeadshot() != 0 || GetDeaths() != 0;
		}
	}

	public static void Show()
	{
		if (!PhotonNetwork.offlineMode)
		{
			FinishTime = Time.time;
			TimerManager.In(0.5f, false, delegate()
			{
				ShowPopup();
			});
		}
	}

	private static void ShowPopup()
	{
		if (hasValue)
		{
			mPlayerRoundManager.Show();
			SetData();
			return;
		}
		Clear();
	}

	private static void SetData()
	{
		AccountManager.SetXP1(GetXP());
		AccountManager.SetKills1(GetKills());
		AccountManager.SetDeaths1(GetDeaths());
		AccountManager.SetHeadshot1(GetHeadshot());
		AccountManager.SetMoney1(GetMoney());
        AccountManager.SetTime1((long)GetTime());
        TimerManager.In(0.03f, delegate()
		{
			AccountManager.UpdatePlayerRoundData(AccountManager.GetXP(), AccountManager.GetKills(), AccountManager.GetMoney(), AccountManager.GetDeaths(), AccountManager.GetHeadshot(), AccountManager.GetTime(), AccountManager.GetLevel(), null, new Action<string>(Failed));
		});
	}

	private static void Failed(string error)
	{
		TimerManager.In(0.1f, delegate()
		{
			mPopUp.ShowPopup(Localization.Get("Error", true) + ": " + error, Localization.Get("Error", true), Localization.Get("Retry", true), delegate()
			{
				mPopUp.HideAll();
				SetData();
			});
		});
	}

	public static void Clear()
	{
		XP = nValue.int0;
		Money = nValue.int0;
		Kills = nValue.int0;
		Headshot = nValue.int0;
		Deaths = nValue.int0;
		StartTime = (float)nValue.int0;
		FinishTime = (float)nValue.int0;
		FireStat.Clear();
	}

	public class cFireStat
	{
		public CryptoInt weapon;

		public List<CryptoInt> skins = new List<CryptoInt>();

		public List<CryptoInt> counts = new List<CryptoInt>();
	}
}
