using System;
using System.Collections.Generic;
using FreeJSON;
using UnityEngine;
using BestHTTP;

public class AccountManager : MonoBehaviour
{
    public AccountData Data = new AccountData();

    public CryptoString[] Links = new CryptoString[0];

    public static bool isConnect;

    public static CryptoString AccountID;

    public static AccountManager instance;

    public static AccountClan Clan = new AccountClan();

    public static CryptoString AccountToken;

    public AccountData DefaultData = new AccountData();

    public static long LastLogin;
    
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
            DontDestroyOnLoad(gameObject);
			return;
		}
        Destroy(gameObject);
	}

	public static void Init()
	{
		new GameObject("AccountManager").AddComponent<AccountManager>();
	}

	public static void Login(string id, Action<bool> complete, Action<string> failed)
	{
		AccountID = id;
        if (string.IsNullOrEmpty(AccountToken))
        {
            Loom.RunAsync(delegate
            {
                AccountToken = new FirebaseToken("ZFpWI7JSa5KQJIlo8aTQqcGrwpEjMeT0x4bLIix3").CreateToken(AccountID);
                Loom.QueueOnMainThread(delegate ()
                {
                    Login(complete, failed);
                });
            });
            return;
        }
        Login(complete, failed);
	}

    private static void Login(Action<bool> complete, Action<string> failed)
    {
        Firebase deviceBansFirebase = new Firebase();
        deviceBansFirebase.Auth = AccountToken;
        deviceBansFirebase.Child("Players").Child("DevicesBans").Child(AndroidNativeFunctions.GetAndroidID2()).GetValue(delegate (string deviceBanResult)
        {
            if (deviceBanResult != "null")
            {
                isConnect = false;
                failed("Device Ban");
                mPopUp.SetActiveWait(false);
                return;
            }
            Firebase firebase = new Firebase();
            firebase.Auth = AccountToken;
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).GetValue(delegate (string result)
            {
                if (result == "null")
                {
                    complete(false);
                    isConnect = false;
                    return;
                }
                if (JsonObject.Parse(result).ContainsKey("Banned"))
                {
                    isConnect = false;
                    failed("Account Ban");
                    mPopUp.SetActiveWait(false);
                    return;
                }
                if (isOldAccountVersion(result))
                {
                    isConnect = false;
                    failed("Account Version");
                    mPopUp.SetActiveWait(false);
                    return;
                }
                instance.DefaultData = AccountConvert.Deserialize(result);
                instance.Data = AccountConvert.Deserialize(result);
                complete(true);
                isConnect = true;
                UpdateLastLogin();
                UpdateSession();
                CheckAndroidEmulator();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                SetAvatar(PlayerPrefs.GetString("EditorAvatar", "https://media.discordapp.net/attachments/1195029412895272991/1197200601088544849/Opz4bZxjeCc.png"));
#else
                SetAvatar(PlayerPrefs.GetString("AndroidAvatar", "https://i.ibb.co/mS19T84/NoAvatar.png"));
#endif
                EventManager.Dispatch("AccountConnected");
            }, delegate (string error)
            {
                firebase = new Firebase();
                firebase.Child("Players").Child("AccountsBans").GetValue(FirebaseParam.Default.OrderByKey().EqualTo(AccountID), delegate (string result)
                {
                    JsonObject jsonObject = JsonObject.Parse(result);
                    if (jsonObject.Length != 0)
                    {
                        failed(jsonObject.Get<string>(AccountID));
                        return;
                    }
                    failed(error);
                }, delegate (string error2)
                {
                    failed(error);
                });
                isConnect = false;
            });
        },
        delegate (string deviceBanError)
        {
            failed(deviceBanError);
            isConnect = false;
        });
    }

    public static void Register(string name, AccountData data, Action complete, Action<string> failed)
    {
        new Firebase
        { 
            Auth = AccountToken
        }.Child("Players").Child("NickNames").Child(name.ToUpper()[0].ToString()).Child(name).GetValue(delegate (string result)
        {
            if (result != "null" && result != "{}")
            {
                failed("Name already taken");
                return;
            }
            string json = AccountConvert.Serialize(name, data, true);
            new Firebase { Auth = AccountToken }.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(json, delegate (string result2)
            {
                instance.DefaultData = AccountConvert.Deserialize(result2);
                instance.Data = AccountConvert.Deserialize(result2);
                complete();
                isConnect = true;
                UpdateLastLogin();
                UpdateSession();

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                SetAvatar(PlayerPrefs.GetString("EditorAvatar", "https://media.discordapp.net/attachments/1195029412895272991/1197200601088544849/Opz4bZxjeCc.png"));
#else
                SetAvatar(PlayerPrefs.GetString("AndroidAvatar", "https://i.ibb.co/mS19T84/NoAvatar.png"));
#endif

                JsonObject jsonAccountIDS = new JsonObject();
                jsonAccountIDS.Add(instance.Data.ID.ToString(), name);
                Firebase firebaseAccountIDS = new Firebase();
                firebaseAccountIDS.Auth = AccountToken;
                firebaseAccountIDS.Child("Players").Child("AccountIDS").UpdateValue(jsonAccountIDS.ToString());

                string parentNew = name.ToUpper()[0].ToString();
                Firebase checkNameFirebase = new Firebase();
                checkNameFirebase.Auth = AccountToken;
                JsonObject nameJjson = new JsonObject();
                nameJjson.Add(name, AccountID);
                checkNameFirebase.Child("Players").Child("NickNames").Child(parentNew).UpdateValue(nameJjson.ToString(), delegate (string result3)
                {
                    CheckAndroidEmulator();
                }, delegate (string error2, string json2)
                {
                    failed(error2);
                    isConnect = false;
                    return;
                });
            }, delegate (string error, string json2)
            {
                failed(error);
                isConnect = false;
            });
        }, delegate (string error)
        {
            failed(error);
        });
    }

    public static void UpdateName(string newName, Action<string> complete, Action<string> failed)
    {
        Firebase firebase = new Firebase();
        firebase.Auth = AccountToken;
        string oldName = instance.Data.AccountName;
        string parentNew = newName.ToUpper()[0].ToString();
        firebase.Child("Players").Child("NickNames").Child(parentNew).Child(newName).GetValue(delegate (string result)
        {
            if (result != "null" && result != "{}")
            {
                failed("Name already taken");
                return;
            }
            JsonObject json = new JsonObject();
            json.Add("AccountName", newName);
            firebase = new Firebase();
            firebase.Auth = AccountToken;
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(json.ToString(), delegate (string result2)
            {
                instance.Data.AccountName = newName;
                complete(newName);

                JsonObject jsonAccountIDS = new JsonObject();
                jsonAccountIDS.Add(instance.Data.ID.ToString(), newName);
                Firebase firebaseAccountIDS = new Firebase();
                firebaseAccountIDS.Auth = AccountToken;
                firebaseAccountIDS.Child("Players").Child("AccountIDS").UpdateValue(jsonAccountIDS.ToString());

                firebase = new Firebase();
                firebase.Auth = AccountToken;
                json = new JsonObject();
                json.Add(newName, AccountID);
                firebase.Child("Players").Child("NickNames").Child(parentNew).UpdateValue(json.ToString(), delegate (string result3)
                {
                    if (!string.IsNullOrEmpty(oldName))
                    {
                        firebase = new Firebase();
                        firebase.Auth = AccountToken;
                        firebase.Child("Players").Child("NickNames").Child(oldName.ToUpper()[0].ToString()).Child(oldName).Delete();
                    }
                }, delegate (string error, string json2)
                {
                    if (!string.IsNullOrEmpty(oldName))
                    {
                        firebase = new Firebase();
                        firebase.Auth = AccountToken;
                        firebase.Child("Players").Child("NickNames").Child(oldName.ToUpper()[0].ToString()).Child(oldName).Delete();
                    }
                });
            }, delegate (string error, string json2)
            {
                failed(error);
            });
        }, delegate (string error)
        {
            failed(error);
        });
    }

    public static void UpdateData(Action failed, Action complete)
    {
    }

    public static void AddFriend(int id, string nickName, Action complete, Action<string> failed)
    {
        List<int> ids = new List<int>();
        Firebase firebaseFriends = new Firebase();
        firebaseFriends.Auth = AccountToken;
        firebaseFriends.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).GetValue(delegate (string result)
        {
            JsonObject idsObject = new JsonObject();
            idsObject = JsonObject.Parse(result);
            ids = idsObject.Get<List<int>>("Friends");
            ids.Add(id);
            Firebase firebase = new Firebase();
            firebase.Auth = AccountToken;
            JsonObject jsonObject = new JsonObject();
            jsonObject.Add("Friends", ids);
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(jsonObject.ToString(), delegate (string udapteResult)
            {
                instance.Data.Friends = ids;
                EventManager.Dispatch("AccountUpdate");
                complete();
            },
            delegate (string error, string updateJson)
            {
                failed(error);
            });
        }, delegate (string error)
        {
            failed(error);
        });
    }

    public static void DeleteFriend(int id, Action complete, Action<string> failed)
    {
        List<int> ids = new List<int>();
        Firebase firebaseFriends = new Firebase();
        firebaseFriends.Auth = AccountToken;
        firebaseFriends.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).GetValue(delegate (string result)
        {
            JsonObject idsObject = new JsonObject();
            idsObject = JsonObject.Parse(result);
            ids = idsObject.Get<List<int>>("Friends");
            ids.Remove(id);
            Firebase firebase = new Firebase();
            firebase.Auth = AccountToken;
            JsonObject jsonObject = new JsonObject();
            jsonObject.Add("Friends", ids);
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(jsonObject.ToString(), delegate (string udapteResult)
            {
                instance.Data.Friends = ids;
                EventManager.Dispatch("AccountUpdate");
            },
            delegate (string error, string updateJson)
            {
                failed(error);
            });

            string nickname = string.Empty;
            string email = string.Empty;
            Firebase nicknameFirebase = new Firebase();
            nicknameFirebase.Auth = AccountToken;
            nicknameFirebase.Child("Players").Child("AccountIDS").GetValue(delegate (string value)
            {
                nickname = JsonObject.Parse(value).Get<string>(id.ToString());
                Firebase emailFirebase = new Firebase();
                emailFirebase.Auth = AccountToken;
                emailFirebase.Child("Players").Child("NickNames").Child(nickname.ToUpper()[0].ToString()).GetValue(delegate (string value2)
                {
                    email = JsonObject.Parse(value2).Get<string>(nickname);

                    List<int> ids2 = new List<int>();
                    Firebase firebaseFriends2 = new Firebase();
                    firebaseFriends2.Auth = new FirebaseToken("ZFpWI7JSa5KQJIlo8aTQqcGrwpEjMeT0x4bLIix3").CreateToken(email);
                    firebaseFriends2.Child("Players").Child("Accounts").Child(email.ToUpper()[0].ToString()).Child(email).GetValue(delegate (string result2)
                    {
                        JsonObject idsObject2 = new JsonObject();
                        idsObject2 = JsonObject.Parse(result2);
                        ids2 = idsObject2.Get<List<int>>("Friends");
                        ids2.Remove(instance.Data.ID);
                        Firebase firebase2 = new Firebase();
                        firebase2.Auth = new FirebaseToken("ZFpWI7JSa5KQJIlo8aTQqcGrwpEjMeT0x4bLIix3").CreateToken(email);
                        JsonObject jsonObject2 = new JsonObject();
                        jsonObject2.Add("Friends", ids2);
                        firebase2.Child("Players").Child("Accounts").Child(email.ToUpper()[0].ToString()).Child(email).UpdateValue(jsonObject2.ToString(), delegate (string udapteResult)
                        {
                            complete();
                        },
                        delegate (string error, string updateJson)
                        {
                            failed(error);
                        });
                    }, delegate (string error)
                    {
                        failed(error);
                    });

                }, delegate (string error)
                {
                    failed(error);

                });
            }, delegate (string error)
            {
                failed(error);
            });
        }, delegate (string error)
        {
            failed(error);
        });
    }

    public static void GetFriendsName(int[] ids, Action complete, Action<string> failed)
    {
		GC.Collect();
		int[] accountIds = new int[20];
		for (int i = 0; i < ids.Length; i++)
		{
			accountIds[i] = ids[i];
		}
		string auth = AccountToken;
		for (int i = 0; i < 20; i++)
		{
			if (accountIds[i] == 0)
			{
				continue;
			}
			string nickname = string.Empty;
			Firebase nicknameFirebase = new Firebase();
			nicknameFirebase.Auth = auth;
			int index = i;
			nicknameFirebase.Child("Players").Child("AccountIDS").GetValue(delegate (string value)
			{
				nickname = JsonObject.Parse(value).Get<string>(accountIds[index].ToString());
				CryptoPrefs.SetString("Friend_#" + accountIds[index], nickname);
				GC.Collect();
			}, delegate (string error)
			{
				failed(error);
			});
		}
		GC.Collect();
	}

    public static void GetFriendsInfo(int id, Action<string> complete, Action<string> failed)
    {
        string nickname = string.Empty;
        string email = string.Empty;
        Firebase nicknameFirebase = new Firebase();
        nicknameFirebase.Auth = AccountToken;
        nicknameFirebase.Child("Players").Child("AccountIDS").GetValue(delegate (string value)
        {
            nickname = JsonObject.Parse(value).Get<string>(id.ToString());
            Firebase emailFirebase = new Firebase();
            emailFirebase.Auth = AccountToken;
            emailFirebase.Child("Players").Child("NickNames").Child(nickname.ToUpper()[0].ToString()).GetValue(delegate (string value2)
            {
                email = JsonObject.Parse(value2).Get<string>(nickname);
                JsonObject accountJson = new JsonObject();
                Firebase accountFire = new Firebase();
                accountFire.Auth = new FirebaseToken("ZFpWI7JSa5KQJIlo8aTQqcGrwpEjMeT0x4bLIix3").CreateToken(email);
                accountFire.Child("Players").Child("Accounts").Child(email.ToUpper()[0].ToString()).Child(email).GetValue(delegate (string result)
                {
                    accountJson = JsonObject.Parse(result);
                    complete(accountJson.ToString());
                }, delegate (string error)
                {
                    failed(error);
                });

            }, delegate (string error)
            {
                failed(error);
            });
        }, delegate (string error)
        {
            failed(error);
        });
    }

    private static void SetAvatar(string url)
	{
        if (string.IsNullOrEmpty(url))
        {
            return;
        }
        if (CacheManager.Exists(url, "Avatars", true))
        {
            instance.Data.Avatar = new Texture2D(96, 96);
            instance.Data.Avatar.LoadImage(CacheManager.Load<byte[]>(url, "Avatars", true));
            instance.Data.Avatar.Apply();
            instance.Data.AvatarUrl = url;
            EventManager.Dispatch("AvatarUpdate");
            return;
        }
        new HTTPRequest(new Uri(url), delegate (HTTPRequest req, HTTPResponse res)
        {
            if (res.IsSuccess)
            {
                instance.Data.Avatar = new Texture2D(96, 96);
                instance.Data.Avatar.LoadImage(res.Data);
                instance.Data.Avatar.Apply();
                instance.Data.AvatarUrl = url;
                EventManager.Dispatch("AvatarUpdate");
                CacheManager.Save<byte[]>(url, "Avatars", res.Data, true);
            }
        }).Send();
    }

	public static bool CheckVersion()
	{
		return Utils.CompareVersion(VersionManager.bundleVersion, instance.Data.GameVersion);
	}

	public static bool CheckAndroidEmulator()
	{
		if (AndroidEmulatorDetector.isEmulator() && !CryptoPrefs.GetBool("AndroidEmulator", false))
		{
			TimerManager.In(0.2f, false, delegate()
			{
				Application.Quit();
			});
			AndroidNativeFunctions.ShowToast("Android Emulator Detected");
			return true;
		}
		return false;
	}

	public static void UpdateData(Action failed)
	{
		UpdateData(failed, null);
	}

	public static void Rewarded(GameCurrency currency, Action complete, Action<string> failed)
	{
		if (currency == GameCurrency.Gold)
		{
			AccountData data = instance.Data;
            data.Gold += 1;
            UpdateGold(data.Gold, complete, failed);
		}
		else
		{
			AccountData data2 = instance.Data;
            data2.Money += 50;
            UpdateMoney(data2.Money, complete, failed);
		}
	}

    public static void DeleteSticker(int id, bool update, Action<bool> complete, Action<string> failed)
    {
        if (GetStickers(id))
        {
            int i = 0;
            while (i < instance.Data.Stickers.Count)
            {
                if (instance.Data.Stickers[i].ID == id)
                {
                    AccountSticker accountSticker = instance.Data.Stickers[i];
                    if (accountSticker.Count == 1)
                    {
                        instance.Data.Stickers.RemoveAt(i);
                        instance.Data.SortStickers();
                        break;
                    }
                    else
                    {
                        accountSticker.Count--;
                        instance.Data.Stickers[i] = accountSticker;
                    }
                    break;
                }
                else
                {
                    i++;
                }
            }
        }
        if (update)
        {
            UpdateDefaultData(null, null);
        }
    }

    public static int GetMoney()
	{
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return 10000;
        }
#endif
        return instance.Data.Money;
	}

	public static int GetGold()
	{
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return 1000;
        }
#endif
        return instance.Data.Gold;
	}

	public static int GetXP()
	{
		if (GetLevel() == 250)
		{
			return GetMaxXP();
		}
		return instance.Data.XP;
	}

	public static int GetMaxXP()
	{
		return 150 + 150 * GetLevel();
	}

	public static int GetLevel()
	{
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return 250;
        }
#endif
        return instance.Data.Level;
	}

	public static List<string> GetInAppPurchase()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < instance.Data.InAppPurchase.Count; i++)
		{
			list.Add(instance.Data.InAppPurchase[i]);
		}
		return list;
	}

	public static int GetDeaths()
	{
		return instance.Data.Deaths;
	}

	public static int GetKills()
	{
		return instance.Data.Kills;
	}

	public static int GetHeadshot()
	{
		return instance.Data.Headshot;
	}

	public static void SetWeaponSelected(WeaponType weaponType, int weaponID)
	{
		switch (weaponType)
		{
		case WeaponType.Knife:
			if ((GetGold() < 0 || GetMoney() < 0) && WeaponManager.GetWeaponData(weaponID).Secret)
			{
				return;
			}
			instance.Data.SelectedKnife = weaponID;
			break;
		case WeaponType.Pistol:
			instance.Data.SelectedPistol = weaponID;
			break;
		case WeaponType.Rifle:
			instance.Data.SelectedRifle = weaponID;
			break;
		}
		instance.Data.UpdateSelectedWeapon = true;
	}

	public static int GetWeaponSelected(WeaponType weaponType)
	{
		switch (weaponType)
		{
		case WeaponType.Knife:
			return instance.Data.SelectedKnife;
		case WeaponType.Pistol:
			return instance.Data.SelectedPistol;
		case WeaponType.Rifle:
			return instance.Data.SelectedRifle;
		default:
			return 0;
		}
	}

	public static void SetPlayerSkinSelected(int id, BodyParts part)
	{
        switch (part)
        {
            case BodyParts.Head:
                instance.Data.PlayerSkin.Select[0] = id;
                break;
            case BodyParts.Body:
                instance.Data.PlayerSkin.Select[1] = id;
                break;
            case BodyParts.Legs:
                instance.Data.PlayerSkin.Select[2] = id;
                break;
        }
        instance.Data.UpdateSelectedPlayerSkin = true;
	}

#if UNITY_EDITOR
    public static bool developerSkin;
#endif

    public static int GetPlayerSkinSelected(BodyParts part)
	{
#if UNITY_EDITOR
            if (developerSkin)
            {
                switch (part)
                {
                    case BodyParts.Head:
                        return 98;
                    case BodyParts.Body:
                        return 98;
                    case BodyParts.Legs:
                        return 98;
                    default:
                        return -1;
                }
            }
            else
            {
                switch (part)
			    {
		    	case BodyParts.Head:
				    return instance.Data.PlayerSkin.Select[0];
		    	case BodyParts.Body:
			    	return instance.Data.PlayerSkin.Select[1];
			    case BodyParts.Legs:
			    	return instance.Data.PlayerSkin.Select[2];
			    default:
			     	return -1;
		    	}
            }
            return -1;
#else
            switch (part)
		    {
		  	case BodyParts.Head:
			    return instance.Data.PlayerSkin.Select[0];
		   	case BodyParts.Body:
			   	return instance.Data.PlayerSkin.Select[1];
		    case BodyParts.Legs:
			    return instance.Data.PlayerSkin.Select[2];
			default:
			    return -1;
		    }
#endif
	}

    public static void UpdatePlayerSkin(Action<string> result, Action<string> error)
    {
        Firebase firebase = new Firebase();
        firebase.Auth = AccountToken;
        JsonObject jsonObjectPlayerSkin = new JsonObject();
        jsonObjectPlayerSkin.Add("Select", (instance.Data.PlayerSkin.Select[0] + "," + instance.Data.PlayerSkin.Select[1] + "," + instance.Data.PlayerSkin.Select[2]));
        jsonObjectPlayerSkin.Add("Head", instance.Data.PlayerSkin.Head);
        jsonObjectPlayerSkin.Add("Body", instance.Data.PlayerSkin.Body);
        jsonObjectPlayerSkin.Add("Legs", instance.Data.PlayerSkin.Legs);
        firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("PlayerSkin").UpdateValue(jsonObjectPlayerSkin.ToString(), delegate (string r)
        {
            result(r);
        }, delegate (string e, string json)
        {
            error(e);
        });
    }

    public static void SetPlayerSkin(int id, BodyParts part)
    {
        if (!GetPlayerSkin(id, part))
        {
            switch (part)
            {
                case BodyParts.Head:
                    instance.Data.PlayerSkin.Head.Add(id);
                    return;
                case BodyParts.Body:
                    instance.Data.PlayerSkin.Body.Add(id);
                    return;
                case BodyParts.Legs:
                    instance.Data.PlayerSkin.Legs.Add(id);
                    break;
                default:
                    return;
            }
        }
    }

	public static bool GetPlayerSkin(int id, BodyParts part)
	{
		if (id == 0)
		{
			return true;
		}
		switch (part)
		{
		case BodyParts.Head:
			return instance.Data.PlayerSkin.Head.Contains(id);
		case BodyParts.Body:
			return instance.Data.PlayerSkin.Body.Contains(id);
		case BodyParts.Legs:
			return instance.Data.PlayerSkin.Legs.Contains(id);
		default:
			return false;
		}
	}

	public static bool GetWeapon(int id)
	{
		if (id == nValue.int12 || id == nValue.int3 || id == nValue.int4)
		{
			return true;
		}
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			if (GameSettings.instance.Weapons[i].ID == id)
			{
				if (GameSettings.instance.Weapons[i].Secret)
				{
					for (int j = 0; j < instance.Data.Weapons.Count; j++)
					{
						if (instance.Data.Weapons[j].ID == id)
						{
							return instance.Data.Weapons[j].Skins != null && instance.Data.Weapons[j].Skins.Count != 0;
						}
					}
				}
				else
				{
					for (int k = 0; k < instance.Data.Weapons.Count; k++)
					{
						if (instance.Data.Weapons[k].ID == id)
						{
							return instance.Data.Weapons[k].Buy;
						}
					}
				}
			}
		}
		return false;
	}

	public static void SetWeaponSkin(int id, int skin)
	{
		int i = 0;
		while (i < instance.Data.Weapons.Count)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				if (!instance.Data.Weapons[i].Skins.Contains(skin))
				{
					instance.Data.Weapons[i].Skins.Add(skin);
					return;
				}
				break;
			}
			else
			{
				i++;
			}
		}
	}

	public static bool GetWeaponSkin(int id, int skin)
	{
		if (skin == 0)
		{
			return true;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				return instance.Data.Weapons[i].Skins.Contains((CryptoInt)skin);
			}
		}
		return false;
	}

	public static void SetWeaponSkinSelected(int id, int skin)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				instance.Data.Weapons[i].LastSkin = instance.Data.Weapons[i].Skin;
				instance.Data.Weapons[i].Skin = skin;
				return;
			}
		}
	}

	public static int GetWeaponSkinSelected(int id)
	{
		if (id == 0)
		{
			return 0;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				return instance.Data.Weapons[i].Skin;
			}
		}
		return 0;
	}

	public static void SetFireStat(int id, int skin)
	{
		if (skin == 0)
		{
			return;
		}
		int i = 0;
		while (i < instance.Data.Weapons.Count)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				if (instance.Data.Weapons[i].FireStats.Count <= skin)
				{
					for (int j = instance.Data.Weapons[i].FireStats.Count - 1; j < skin; j++)
					{
						instance.Data.Weapons[i].FireStats.Add(-1);
					}
					instance.Data.Weapons[i].FireStats[instance.Data.Weapons[i].FireStats.Count - 1] = 0;
					return;
				}
				if (instance.Data.Weapons[i].FireStats[skin] < 0)
				{
					instance.Data.Weapons[i].FireStats[skin] = 0;
					return;
				}
				break;
			}
			else
			{
				i++;
			}
		}
	}

	public static bool GetFireStat(int id, int skin)
	{
		if (skin == 0)
		{
			return false;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				return instance.Data.Weapons[i].FireStats.Count > skin && instance.Data.Weapons[i].FireStats[skin] != -1;
			}
		}
		return false;
	}

	public static void SetFireStatCounter(int id, int skin, int value)
	{
		if (skin == 0)
		{
			return;
		}
		int i = 0;
		while (i < instance.Data.Weapons.Count)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				if (instance.Data.Weapons[i].FireStats.Count > skin)
				{
					instance.Data.Weapons[i].FireStats[skin] = value;
					return;
				}
				for (int j = instance.Data.Weapons[i].FireStats.Count - 1; j < skin; j++)
				{
					instance.Data.Weapons[i].FireStats.Add(-1);
				}
				instance.Data.Weapons[i].FireStats[instance.Data.Weapons[i].FireStats.Count - 1] = value;
				return;
			}
			else
			{
				i++;
			}
		}
	}

	public static int GetFireStatCounter(int id, int skin)
	{
		if (skin == 0)
		{
			return -1;
		}
		int i = 0;
		while (i < instance.Data.Weapons.Count)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				if (instance.Data.Weapons[i].FireStats.Count > skin)
				{
					return instance.Data.Weapons[i].FireStats[skin];
				}
				return -1;
			}
			else
			{
				i++;
			}
		}
		return -1;
	}

	public static string GetClan()
	{
		return instance.Data.Clan;
	}

	public static void SetStickers(int id)
	{
		if (GetStickers(id))
		{
			for (int i = 0; i < instance.Data.Stickers.Count; i++)
			{
				if (instance.Data.Stickers[i].ID == id)
				{
                    instance.Data.Stickers[i].Count++;
                    return;
				}
			}
			return;
		}
		AccountSticker accountSticker = new AccountSticker();
		accountSticker.ID = id;
		accountSticker.Count = 1;
		instance.Data.Stickers.Add(accountSticker);
		instance.Data.SortStickers();
	}

	private static void DeleteSticker(int id)
	{
		if (GetStickers(id))
		{
			int i = 0;
			while (i < instance.Data.Stickers.Count)
			{
				if (instance.Data.Stickers[i].ID == id)
				{
					AccountSticker accountSticker = instance.Data.Stickers[i];
					if (instance.Data.Stickers[i].Count == 0)
					{
						instance.Data.Stickers.RemoveAt(i);
						instance.Data.SortStickers();
						return;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}
	}

	public static bool GetStickers(int id)
	{
		for (int i = 0; i < instance.Data.Stickers.Count; i++)
		{
			if (instance.Data.Stickers[i].ID == id)
			{
				return true;
			}
		}
		return false;
	}

	public static int[] GetStickers()
	{
		int[] array = new int[instance.Data.Stickers.Count];
		for (int i = 0; i < instance.Data.Stickers.Count; i++)
		{
			array[i] = instance.Data.Stickers[i].ID;
		}
		return array;
	}

	public static int GetStickerCount(int id)
	{
		for (int i = 0; i < instance.Data.Stickers.Count; i++)
		{
			if (instance.Data.Stickers[i].ID == id)
			{
				return instance.Data.Stickers[i].Count;
			}
		}
		return 0;
	}

	public static void SetWeaponSticker(int weapon, int skin, int pos, int sticker)
	{
		AccountWeaponStickers weaponStickers = GetWeaponStickers(weapon, skin);
		if (HasWeaponSticker(weapon, skin, pos))
		{
			for (int i = 0; i < weaponStickers.StickerData.Count; i++)
			{
				if (weaponStickers.StickerData[i].Index == pos)
				{
					weaponStickers.StickerData[i].StickerID = sticker;
					return;
				}
			}
			return;
		}
		AccountWeaponStickerData accountWeaponStickerData = new AccountWeaponStickerData();
		accountWeaponStickerData.Index = pos;
		accountWeaponStickerData.StickerID = sticker;
		weaponStickers.StickerData.Add(accountWeaponStickerData);
		weaponStickers.SortWeaponStickerData();
	}

	public static void DeleteWeaponSticker(int weapon, int skin, int pos)
	{
		AccountWeaponStickers weaponStickers = GetWeaponStickers(weapon, skin);
		if (weaponStickers == null)
		{
			return;
		}
		for (int i = 0; i < weaponStickers.StickerData.Count; i++)
		{
			if (weaponStickers.StickerData[i].Index == pos)
			{
				weaponStickers.StickerData.RemoveAt(i);
				weaponStickers.SortWeaponStickerData();
				return;
			}
		}
	}

	public static bool HasWeaponSticker(int weapon, int skin, int pos)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if (instance.Data.Weapons[i].ID == weapon)
			{
				for (int j = 0; j < instance.Data.Weapons[i].Stickers.Count; j++)
				{
					if (instance.Data.Weapons[i].Stickers[j].SkinID == skin)
					{
						for (int k = 0; k < instance.Data.Weapons[i].Stickers[j].StickerData.Count; k++)
						{
							if (instance.Data.Weapons[i].Stickers[j].StickerData[k].Index == pos)
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	public static int GetWeaponSticker(int weapon, int skin, int pos)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if (instance.Data.Weapons[i].ID == weapon)
			{
				for (int j = 0; j < instance.Data.Weapons[i].Stickers.Count; j++)
				{
					if (instance.Data.Weapons[i].Stickers[j].SkinID == skin)
					{
						for (int k = 0; k < instance.Data.Weapons[i].Stickers[j].StickerData.Count; k++)
						{
							if (instance.Data.Weapons[i].Stickers[j].StickerData[k].Index == pos)
							{
								return instance.Data.Weapons[i].Stickers[j].StickerData[k].StickerID;
							}
						}
					}
				}
			}
		}
		return -1;
	}

	public static AccountWeaponStickers GetWeaponStickers(int weapon, int skin)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if (instance.Data.Weapons[i].ID == weapon)
			{
				for (int j = 0; j < instance.Data.Weapons[i].Stickers.Count; j++)
				{
					if (instance.Data.Weapons[i].Stickers[j].SkinID == skin)
					{
						return instance.Data.Weapons[i].Stickers[j];
					}
				}
				AccountWeaponStickers accountWeaponStickers = new AccountWeaponStickers();
				accountWeaponStickers.SkinID = skin;
				instance.Data.Weapons[i].Stickers.Add(accountWeaponStickers);
				return instance.Data.Weapons[i].Stickers[instance.Data.Weapons[i].Stickers.Count - 1];
			}
		}
		return new AccountWeaponStickers();
	}

	private void OnApplicationQuit()
	{
		nPlayerPrefs.Save();
	}

	public static bool isOldAccountVersion(string text)
	{
		return JsonObject.Parse(text).Get<int>("OS") != 4;
	}

    public static void UpdateLastLogin()
	{
		if (!isConnect)
		{
			return;
		}
		TimerManager.In(UnityEngine.Random.value, delegate()
		{
			new Firebase
			{
				Auth = AccountToken
			}.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("LastLogin").SetValue(JsonObject.Parse(Firebase.GetTimeStamp()).ToString(), delegate(string result)
			{
				long.TryParse(result, out LastLogin);
			}, null);
		});
	}

	public static void UpdateSession()
	{
		TimerManager.In(UnityEngine.Random.value, delegate()
		{
			Firebase firebase = new Firebase();
			firebase.Auth = AccountToken;
			JsonObject jsonObject = new JsonObject();
			jsonObject.Add("Session", instance.Data.Session + 1);
			firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(jsonObject.ToString(), delegate(string result)
			{
			}, null);
		});
	}

	public static void SetGold(int gold)
	{
		SetGold(gold, false, null, null);
	}

	public static void CheckSession(Action<bool> action)
	{
		new Firebase
		{
			Auth = AccountToken
		}.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Session").GetValue(delegate(string result)
		{
			if (result == instance.Data.Session.ToString())
			{
				action(true);
				return;
			}
			action(false);
			if (isConnect)
			{
				UIToast.Show(Localization.Get("Session is already outdated", true));
				UIToast.Show(Localization.Get("Restart the game", true));
				isConnect = false;
				PhotonNetwork.Disconnect();
			}
		}, delegate(string error)
		{
			action(false);
			if (isConnect)
			{
				UIToast.Show(Localization.Get("Session is already outdated", true));
				UIToast.Show(Localization.Get("Restart the game", true));
				isConnect = false;
				PhotonNetwork.Disconnect();
			}
		});
	}

	public static void UpdateDefaultData(Action<bool> complete, Action<string, string> failed)
	{
		JsonObject json = AccountConvert.CompareDefaultValue(instance.DefaultData, instance.Data);
		if (json.Length == 0)
		{
			return;
		}
		if (complete != null)
		{
			complete(false);
		}
		CheckSession(delegate(bool session)
		{
			if (!session || !isConnect)
			{
				return;
			}
			AccountData data = AccountConvert.Copy(instance.Data);
			new Firebase
			{
				Auth = AccountToken
			}.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(json.ToString(), delegate(string result)
			{
				AccountConvert.CopyDefaultValue(data, instance.DefaultData);
				if (complete != null)
				{
					complete(true);
				}
			}, delegate(string error, string json2)
			{
				if (failed != null)
				{
					failed(error, json2);
				}
			});
		});
	}

	public static void SetGold(int gold, bool update)
	{
		SetGold(gold, update, null, null);
	}

	public static void SetGold(int gold, bool update, Action<bool> complete, Action<string, string> failed)
	{
		instance.Data.Gold = Mathf.Min(gold, 1000000);
		if (update)
		{
			UpdateDefaultData(complete, failed);
		}
	}

	public static void SetGold1(int gold)
	{
		SetGold1(gold, false, null, null);
	}

	public static void SetGold1(int gold, bool update)
	{
		SetGold1(gold, update, null, null);
	}

	public static void SetGold1(int gold, bool update, Action<bool> complete, Action<string, string> failed)
	{
		SetGold(GetGold() + gold, update, complete, failed);
	}

	public static string AccountParent
	{
		get
		{
			return AccountID.ToString().ToUpper()[0].ToString();
		}
	}

	public static void Register(string playerName, Action complete, Action<string> failed)
	{
		Register(playerName, new AccountData
		{
			Weapons = 
			{
				new AccountWeapon
				{
					ID = 12
				},
				new AccountWeapon
				{
					ID = 4
				},
				new AccountWeapon
				{
					ID = 3
				}
			}
		}, complete, failed);
	}

	public static void SetXP(int xp)
	{
		SetXP(xp, false, null, null);
	}

	public static void SetXP(int xp, bool update)
	{
		SetXP(xp, update, null, null);
	}

	public static void SetXP(int xp, bool update, Action<bool> complete, Action<string, string> failed)
	{
		instance.Data.XP = xp;
		if (update)
		{
			UpdateDefaultData(complete, failed);
		}
	}

	public static void SetXP1(int xp)
	{
		int num = GetLevel();
		int num2 = GetXP() + xp;
		int num3 = GetMaxXP();
		if (num2 >= num3)
		{
			if (num == 250)
			{
				num2 = num3;
			}
			else
			{
				num++;
				num2 -= num3;
				num2 = Mathf.Max(num2, 0);
				num3 = 150 + 150 * num;
				SetLevel(num);
				if (LevelManager.GetSceneName() == "Menu")
				{
					UIToast.Show(Localization.Get("New Level", true) + " " + num);
					SetGold1(10);
				}
			}
		}
		SetXP(num2);
	}

	public static void SetLevel(int level)
	{
		SetLevel(level, false, null, null);
	}

	public static void SetLevel(int level, bool update)
	{
		SetLevel(level, update, null, null);
	}

	public static void SetLevel(int level, bool update, Action<bool> complete, Action<string, string> failed)
	{
		instance.Data.Level = level;
		if (update)
		{
			UpdateDefaultData(complete, failed);
		}
	}

	public static void SetKills(int kills)
	{
		SetKills(kills, false, null, null);
	}

	public static void SetKills(int kills, bool update)
	{
		SetKills(kills, update, null, null);
	}

	public static void SetKills(int kills, bool update, Action<bool> complete, Action<string, string> failed)
	{
		instance.Data.Kills = kills;
		if (update)
		{
			UpdateDefaultData(complete, failed);
		}
	}

	public static void SetKills1(int kills)
	{
		SetKills1(kills, false, null, null);
	}

	public static void SetKills1(int kills, bool update)
	{
		SetKills1(kills, update, null, null);
	}

	public static void SetKills1(int kills, bool update, Action<bool> complete, Action<string, string> failed)
	{
		SetKills(GetKills() + kills, update, complete, failed);
	}

	public static void SetDeaths(int deaths)
	{
		SetDeaths(deaths, false, null, null);
	}

	public static void SetDeaths(int deaths, bool update)
	{
		SetDeaths(deaths, update, null, null);
	}

	public static void SetDeaths(int deaths, bool update, Action<bool> complete, Action<string, string> failed)
	{
		instance.Data.Deaths = deaths;
		if (update)
		{
			UpdateDefaultData(complete, failed);
		}
	}

	public static void SetDeaths1(int deaths)
	{
		SetDeaths1(deaths, false, null, null);
	}

	public static void SetDeaths1(int deaths, bool update)
	{
		SetDeaths1(deaths, update, null, null);
	}

	public static void SetDeaths1(int deaths, bool update, Action<bool> complete, Action<string, string> failed)
	{
		SetDeaths(GetDeaths() + deaths, update, complete, failed);
	}

	public static void SetHeadshot(int headshot)
	{
		SetHeadshot(headshot, false, null, null);
	}

	public static void SetHeadshot(int headshot, bool update)
	{
		SetHeadshot(headshot, update, null, null);
	}

	public static void SetHeadshot(int headshot, bool update, Action<bool> complete, Action<string, string> failed)
	{
		instance.Data.Headshot = headshot;
		if (update)
		{
			UpdateDefaultData(complete, failed);
		}
	}

	public static void SetHeadshot1(int headshot)
	{
		SetHeadshot1(headshot, false, null, null);
	}

	public static void SetHeadshot1(int headshot, bool update)
	{
		SetHeadshot1(headshot, update, null, null);
	}

	public static void SetHeadshot1(int headshot, bool update, Action<bool> complete, Action<string, string> failed)
	{
		SetHeadshot(GetHeadshot() + headshot, update, complete, failed);
	}

	public static void UpdateGold(int gold, Action complete, Action<string> failed)
	{
		Firebase firebase = new Firebase();
		firebase.Auth = AccountToken;
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("Gold", gold);
		firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(jsonObject.ToString(), delegate(string result)
		{
            if (complete != null)
            {
                complete();
            }
		}, delegate(string error, string json)
        {
            if (failed != null)
            {
                failed(error);
            }
        });
	}

	public static void SetFirebaseWeaponsSelected(Action<string> complete, Action<string> failed)
	{
		Firebase firebase = new Firebase();
		firebase.Auth = AccountToken;
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("SelectWeapons", (GetWeaponSelected(WeaponType.Rifle) + "," + GetWeaponSelected(WeaponType.Pistol) + "," + GetWeaponSelected(WeaponType.Knife)));
		firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(jsonObject.ToString(), delegate(string result)
		{
		}, null);
	}

    public static void SetWeaponSkinSelected2(int weaponid, int id, Action<string> complete, Action<string> failed)
	{
        Firebase firebase = new Firebase();
        firebase.Auth = AccountToken;
        JsonObject jsonObject = new JsonObject();
        jsonObject.Add("Skin", id);
        if (weaponid > 9)
        {
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(weaponid.ToString()).UpdateValue(jsonObject.ToString(), delegate (string result)
            {
            }, null);
            return;
        }
        firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child("0" + weaponid.ToString()).UpdateValue(jsonObject.ToString(), delegate (string result)
        {
        }, null);
    }

	public static void SetClanFirebase(string clan)
	{
		Firebase firebase = new Firebase();
		firebase.Auth = AccountToken;
		JsonObject jsonObject = new JsonObject();
		Clan.SendMessage(instance.Data.AccountName + " joined to the the clan", false);
		jsonObject.Add("Clan", clan);
		firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(jsonObject.ToString(), delegate(string result)
        {
		}, null);
	}

	public static void SetFirebaseWeaponsBuy(int id, Action<string> complete, Action<string> failed)
	{
		Firebase firebase = new Firebase();
		firebase.Auth = AccountToken;
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("Buy", 1);
		if (id < 10)
		{
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child("0" + id.ToString()).UpdateValue(jsonObject.ToString(), delegate (string success)
            {
                if (success.Contains("Buy"))
                {
                    complete(success);
                }
            }, delegate (string error, string json)
            {
                failed(error);
            });
        }
		else
		{
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(id.ToString()).UpdateValue(jsonObject.ToString(), delegate(string success) 
            {
                if (success.Contains("Buy"))
                {
                    complete(success);
                }
            }, delegate(string error, string json) 
            {
                failed(error);
            });
		}
    }

    public static void UpdateWeaponSkins(bool secretWeapon, int weaponId, Action<string> complete, Action<string> failed)
	{
        Firebase firebase = new Firebase();
        firebase.Auth = AccountToken;
        JsonObject jsonObject = new JsonObject();
        if (secretWeapon)
        {
            jsonObject.Add("Buy", 1);
        }
        List<string> list = new List<string>();
        for (int i = 0; i < instance.Data.Weapons.Count; i++)
        {
            if (instance.Data.Weapons[i].ID == weaponId)
            {
                for (int j = 0; j < instance.Data.Weapons[i].Skins.Count; j++)
                {
                    if (instance.Data.Weapons[i].Skins[j] != 0)
                    {
                        list.Add(instance.Data.Weapons[i].Skins[j].ToString());
                    }
                }
            }
        }
        jsonObject.Add("Skins", string.Join(",", list.ToArray()));
        firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(weaponId.ToString("D2")).UpdateValue(jsonObject.ToString(), delegate (string result)
        {
            complete(result);
        }, delegate(string error, string json) 
        {
            failed(error);
        });
    }

	public static void UpdateWeaponsData(Action<bool> complete, Action<string, string> failed)
	{
		JsonObject json = AccountConvert.CompareWeaponValue(instance.DefaultData, instance.Data);
		if (json.Length == 0)
		{
			return;
		}
		if (complete != null)
		{
			complete(false);
		}
		CheckSession(delegate(bool session)
		{
			if (!session || !isConnect)
			{
				return;
			}
			AccountData data = AccountConvert.Copy(instance.Data);
			new Firebase
			{
				Auth = AccountToken
			}.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").UpdateValue(json.Get<string>("Weapons"), delegate(string result)
			{
				AccountConvert.CopyWeaponsValue(data, instance.DefaultData);
				if (complete != null)
				{
					complete(true);
				}
			}, delegate(string error, string json2)
			{
                if (failed != null)
                {
                    failed(error, json2);
                }
            });
		});
	}

    public static void UpdateStickersFirebase(Action<string> complete, Action<string> failed)
    {
        Firebase firebase = new Firebase();
        firebase.Auth = AccountToken;
        JsonObject jsonObject = new JsonObject();
        for (int i = 0; i < instance.Data.Stickers.Count; i++)
        {
            jsonObject.Add(instance.Data.Stickers[i].ID.ToString("D2"), instance.Data.Stickers[i].Count);
        }
        firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Stickers").SetValue(jsonObject.ToString(), delegate(string result)
        {
            complete(result);
        }, delegate(string error, string json) 
        {
            failed(error);
        });
    }

    public static void DeleteWeaponStickerFirebase(int weaponid, int skinid, int position)
    {
        Firebase firebase = new Firebase();
        firebase.Auth = AccountToken;
        new JsonObject();
        if (weaponid > 9)
        {
            if (skinid > 9)
            {
                firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(weaponid.ToString()).Child("Stickers").Child(skinid.ToString()).Child(0 + position.ToString()).Delete();
                return;
            }
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(weaponid.ToString()).Child("Stickers").Child(0 + skinid.ToString()).Child(0 + position.ToString()).Delete();
            return;
        }
        else
        {
            if (skinid > 9)
            {
                firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(0 + weaponid.ToString()).Child("Stickers").Child(skinid.ToString()).Child(0 + position.ToString()).Delete();
                return;
            }
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(0 + weaponid.ToString()).Child("Stickers").Child(0 + skinid.ToString()).Child(0 + position.ToString()).Delete();
            return;
        }
    }

    public static void AddWeaponStickerFirebase(int weaponid, int skinid, int stickerid, int position)
    {
        Firebase firebase = new Firebase();
        firebase.Auth = AccountToken;
        JsonObject jsonObject = new JsonObject();
        jsonObject.Add(0 + position.ToString(), stickerid);
        if (weaponid > 9)
        {
            if (skinid > 9)
            {
                firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(weaponid.ToString()).Child("Stickers").Child(skinid.ToString()).UpdateValue(jsonObject.ToString());
                return;
            }
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(weaponid.ToString()).Child("Stickers").Child(0 + skinid.ToString()).UpdateValue(jsonObject.ToString());
            return;
        }
        else
        {
            if (skinid > 9)
            {
                firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(0 + weaponid.ToString()).Child("Stickers").Child(skinid.ToString()).UpdateValue(jsonObject.ToString());
                return;
            }
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(0 + weaponid.ToString()).Child("Stickers").Child(0 + skinid.ToString()).UpdateValue(jsonObject.ToString());
            return;
        }
    }

    public static void SetFireStatFireBase(bool isCase, int weaponId, int skinId, int firestat)
    {
        if (skinId == 0)
        {
            return;
        }
        if (isCase && GetFireStatCounter(weaponId, skinId) > 0)
        {
            return;
        }
        Firebase firebase = new Firebase();
        firebase.Auth = AccountToken;
        JsonObject jsonObject = new JsonObject();
        TimerManager.In(2f, delegate ()
        {
            if (isCase)
            {
                jsonObject.Add(skinId.ToString("D2"), 0);
            }
            else
            {
                jsonObject.Add(skinId.ToString("D2"), firestat);
            }
            firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).Child("Weapons").Child(weaponId.ToString("D2")).Child("FireStats").UpdateValue(jsonObject.ToString());
        });
    }

    public static void SetClan(string text)
	{
		instance.Data.Clan = text;
	}

	public static void SetWeapon(int id)
	{
		SetWeapon(id, false, null, null);
	}

	public static void SetWeapon(int id, bool update)
	{
		SetWeapon(id, update, null, null);
	}

	public static void SetWeapon(int id, bool update, Action<bool> complete, Action<string, string> failed)
	{
		int i = 0;
		while (i < instance.Data.Weapons.Count)
		{
			if (instance.Data.Weapons[i].ID == id)
			{
				instance.Data.Weapons[i].Buy = true;
				if (update)
				{
					UpdateWeaponsData(complete, failed);
					return;
				}
				break;
			}
			else
			{
				i++;
			}
		}
	}

	public static void UpdateMoney(int money, Action complete, Action<string> failed)
	{
		Firebase firebase = new Firebase();
		firebase.Auth = AccountToken;
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("Money", money);
		firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(jsonObject.ToString(), delegate(string result)
		{
            if (complete != null)
            {
                complete();
            }
		}, delegate(string error, string json)
        {
            if (failed != null)
            {
                failed(error);
            }
        });
	}

	public static void SetMoney(int money)
	{
		SetMoney(money, false, null, null);
	}

	public static void SetMoney(int money, bool update)
	{
		SetMoney(money, update, null, null);
	}

	public static void SetMoney(int money, bool update, Action<bool> complete, Action<string, string> failed)
	{
		instance.Data.Money = Mathf.Min(money, 1000000);
		if (update)
		{
			UpdateDefaultData(complete, failed);
		}
	}

	public static void SetMoney1(int money)
	{
		SetMoney1(money, false, null, null);
	}

    public static void SetTime(long time)
    {
        instance.Data.Time = time;
    }

    public static void SetTime1(long time)
    {
        SetTime(GetTime() + time);
    }

    public static long GetTime()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return 1337;
        }
#endif
        return instance.Data.Time;
    }

    public static void SetMoney1(int money, bool update)
	{
		SetMoney1(money, update, null, null);
	}

	public static void SetMoney1(int money, bool update, Action<bool> complete, Action<string, string> failed)
	{
		SetMoney(GetMoney() + money, update, complete, failed);
	}

	public static void UpdatePlayerRoundData(int xp, int kills, int money, int deaths, int headshots, long time, int level, Action<string> complete, Action<string> failed)
	{
		Firebase firebase = new Firebase();
		firebase.Auth = AccountToken;
		JsonObject jsonObject = new JsonObject();
        JsonObject jsonObjectRound = new JsonObject();
        jsonObjectRound.Add("XP", xp);
        jsonObjectRound.Add("Kills", kills);
        jsonObjectRound.Add("Deaths", deaths);
        jsonObjectRound.Add("Head", headshots);
        jsonObjectRound.Add("Time", time);
        jsonObjectRound.Add("Level", level);
        jsonObject.Add("Round", jsonObjectRound);
        jsonObject.Add("Money", money);
		firebase.Child("Players").Child("Accounts").Child(AccountParent).Child(AccountID).UpdateValue(jsonObject.ToString(), delegate(string result)
        {
		}, null);
	}
}
