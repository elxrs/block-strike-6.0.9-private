using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XLua;

public class Admin : EditorWindow
{
#region Icons
    public Texture2D player;
    public Texture2D weapon;
    public Texture2D text;
    public Texture2D room;
#endregion Icons

    int playerSelected = 0;
    private static string[] players = new string[0];

    private static PhotonPlayer[] photonPlayers;
    private static PhotonPlayer selectedPlayer;

    int rifleSelected = 0;
    private static string[] rifles = new string[1] { "AK-47" };
    private static string selectedRifleName = "AK-47";

    int pistolSelected = 0;
    private static string[] pistols = new string[1] { "Deagle" };
    private static string selectedPistolName = "Deagle";

    int knifeSelected = 0;
    private static string[] knifes = new string[1] { "Knife" };
    private static string selectedKnifeName = "Knife";

    private static string lastSelectedWeapon = "AK-47";
    int skinSelected = 0;

    float speedPlayer = 0.18f;
    string chatText = "Hello World";
    int maxPlayers = 0;
    string luaScript = "";
    bool dispose = false;

    public enum EventTargets
    {
        Me = 0,
        Player = 1,
        All = 2
    }

    private static EventTargets eventTarget = EventTargets.Me;

    private static GUILayoutOption[] buttonOptions;
    private static GUILayoutOption[] weaponsPopupOptions;

    int page = 0;
    Vector2 scrollPos;

    private void OnDisable()
    {
        lastSelectedWeapon = "AK-47";
        skinSelected = 0;
    }

    private static WeaponType GetTypeFromName(string name)
    {
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Name == name && GameSettings.instance.Weapons[i].Type == WeaponType.Rifle)
            {
                return WeaponType.Rifle;
            }
            if (GameSettings.instance.Weapons[i].Name == name && GameSettings.instance.Weapons[i].Type == WeaponType.Pistol)
            {
                return WeaponType.Pistol;
            }
            if (GameSettings.instance.Weapons[i].Name == name && GameSettings.instance.Weapons[i].Type == WeaponType.Knife)
            {
                return WeaponType.Knife;
            }
        }
        return WeaponType.Rifle;
    }

    private static int GetIDFromName(string name)
    {
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Name == name)
            {
                return GameSettings.instance.Weapons[i].ID;
            }
        }
        return 0;
    }

    private static int GetSkinsFromName(string name)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Name == name)
            {
                for (int j = 0; j < GameSettings.instance.WeaponsStore[i].Skins.Count - 1; j++)
                {
                    list.Add(GameSettings.instance.WeaponsStore[i].Skins[j].ID);
                }
            }
        }
        return list.Count;
    }


    private static void LoadWeaponsLists()
    {
        int riflesCount = 0;
        int riflesCount2 = 0;
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Type == WeaponType.Rifle)
            {
                riflesCount2++;
            }
        }
        rifles = new string[riflesCount2 + 2];
        rifles[0] = "AK-47";
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Type == WeaponType.Rifle)
            {
                riflesCount++;
                rifles[riflesCount + 1] = GameSettings.instance.Weapons[i].Name;
            }
        }

        int pistolsCount = 0;
        int pistolsCount2 = 0;
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Type == WeaponType.Pistol)
            {
                pistolsCount2++;
            }
        }
        pistols = new string[pistolsCount2 + 2];
        pistols[0] = "Deagle";
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Type == WeaponType.Pistol)
            {
                pistolsCount++;
                pistols[pistolsCount + 1] = GameSettings.instance.Weapons[i].Name;
            }
        }

        int knifesCount = 0;
        int knifesCount2 = 0;
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Type == WeaponType.Knife)
            {
                knifesCount2++;
            }
        }
        knifes = new string[knifesCount2 + 2];
        knifes[0] = "Knife";
        for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
        {
            if (GameSettings.instance.Weapons[i].Type == WeaponType.Knife)
            {
                knifesCount++;
                knifes[knifesCount + 1] = GameSettings.instance.Weapons[i].Name;
            }
        }
    }

    private static PhotonPlayer GetPhotonPlayerFromName(string name)
    {
        for (int i = 0; i < photonPlayers.Length; i++)
        {
            if (photonPlayers[i].NickName == name)
            {
                return photonPlayers[i];
            }
        }
        return null;
    }

    private void Event(params object[] parameters)
    {
        PhotonDataWrite message = new PhotonDataWrite();
        message.Write((string)parameters[0]);
        message.Write((string)parameters[1]);
        if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
        {
            if (eventTarget == EventTargets.Me)
            {
                PhotonRPC.RPC("OnTest", PhotonNetwork.player, message);
            }
            if (eventTarget == EventTargets.Player && selectedPlayer != null)
            {
                PhotonRPC.RPC("OnTest", selectedPlayer, message);
            }
            if (eventTarget == EventTargets.All)
            {
                PhotonRPC.RPC("OnTest", PhotonTargets.All, message);
            }
        }
    }

    private void Event2(params object[] parameters)
    {
        PhotonDataWrite message = new PhotonDataWrite();
        message.Write((string)parameters[0]);
        message.Write((float)parameters[1]);
        if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
        {
            if (eventTarget == EventTargets.Me)
            {
                PhotonRPC.RPC("OnTest", PhotonNetwork.player, message);
            }
            if (eventTarget == EventTargets.Player && selectedPlayer != null)
            {
                PhotonRPC.RPC("OnTest", selectedPlayer, message);
            }
            if (eventTarget == EventTargets.All)
            {
                PhotonRPC.RPC("OnTest", PhotonTargets.All, message);
            }
        }
    }

    private void Event3(params object[] parameters)
    {
        PhotonDataWrite message = new PhotonDataWrite();
        message.Write((string)parameters[0]);
        message.Write((string)parameters[1]);
        message.Write((string)parameters[2]);
        message.Write((bool)parameters[3]);
        if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
        {
            if (eventTarget == EventTargets.Me)
            {
                PhotonRPC.RPC("OnTest", PhotonNetwork.player, message);
            }
            if (eventTarget == EventTargets.Player && selectedPlayer != null)
            {
                PhotonRPC.RPC("OnTest", selectedPlayer, message);
            }
            if (eventTarget == EventTargets.All)
            {
                PhotonRPC.RPC("OnTest", PhotonTargets.All, message);
            }
        }
    }

    public static void BeginContents()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(6f);
        EditorGUILayout.BeginHorizontal("helpBox", GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    public static void EndContents()
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(1f);
        GUILayout.EndHorizontal();
        GUILayout.Space(3f);
    }

    [MenuItem("Window/Rexet Studio/Others/Admin")]
    static void Init()
    {
        Admin window = GetWindow<Admin>("Admin Menu");
        window.minSize = new Vector2(350, 325);
        window.maxSize = new Vector2(350, 325);
        window.Show();
    }

    void OnGUI()
    {
        buttonOptions = new GUILayoutOption[]
        {
            GUILayout.Height(20),
            GUILayout.MinWidth(1),
        };
        weaponsPopupOptions = new GUILayoutOption[]
        {
            GUILayout.MinWidth(1),
        };
        if (page == 0)
        {
            BeginContents();
            if (GUILayout.Button(new GUIContent("Player Settings", player), GUILayout.Height(30)))
            {
                page = 1;
            }
            if (GUILayout.Button(new GUIContent("Give Weapon", weapon), GUILayout.Height(30)))
            {
                page = 2;
            }
            if (GUILayout.Button(new GUIContent("Text on screen", text), GUILayout.Height(30)))
            {
                page = 3;
            }
            if (GUILayout.Button(new GUIContent("Room Settings", room), GUILayout.Height(30)))
            {
                page = 4;
            }
            EndContents();
        }
        if (page == 1)
        {
            #region PlayerSettings
            Texture2D icon;
            icon = EditorGUIUtility.IconContent("d_Profiler.PrevFrame").image as Texture2D;

            if (GUILayout.Button(new GUIContent("", icon), GUILayout.Width(50)))
            {
                page = 0;
            }

            if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
            {
                photonPlayers = PhotonNetwork.playerList;
                players = new string[PhotonNetwork.room.PlayerCount];
                for (int i = 0; i < photonPlayers.Length; i++)
                {
                    players[i] = photonPlayers[i].NickName;
                }
            }
            else
            {
                players = new string[0];
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            playerSelected = EditorGUILayout.Popup("", playerSelected, players);
            if (EditorGUI.EndChangeCheck())
            {
                selectedPlayer = GetPhotonPlayerFromName(players[playerSelected]);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            int btns = -1;
            btns = GUILayout.Toolbar(btns, new string[] { "Me", "Him", "All" }, buttonOptions);
            switch (btns)
            {
                case 0:
                    eventTarget = EventTargets.Me;
                    break;
                case 1:
                    eventTarget = EventTargets.Player;
                    break;
                case 2:
                    eventTarget = EventTargets.All;
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            int btnsOthers = -1;
            btnsOthers = GUILayout.Toolbar(btnsOthers, new string[] { "Kill", "Game Quit" }, buttonOptions);
            switch (btnsOthers)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "11"
                        });
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "12"
                        });
                    }
                    break;
            }

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("\t\tPlayer Speed");

            EditorGUILayout.EndHorizontal();

            speedPlayer = EditorGUILayout.Slider(speedPlayer, 0.18f, 1f);

            if (GUILayout.Button("Set", buttonOptions))
            {
                if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                {
                    Event2(new object[]
                    {
                        "t70h0hzez2",
                        speedPlayer
                    });
                }
            }

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("\t\t  Fly Mode");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            int btnsFlyMode = -1;
            btnsFlyMode = GUILayout.Toolbar(btnsFlyMode, new string[] { "Fly On", "Fly Off" }, buttonOptions);
            switch (btnsFlyMode)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "1"
                        });
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "2"
                        });
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("\t\t  God Mode");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            int btnsGodMode = -1;
            btnsGodMode = GUILayout.Toolbar(btnsGodMode, new string[] { "God On", "God Off" }, buttonOptions);
            switch (btnsGodMode)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "3"
                        });
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "4"
                        });
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("\t\t  Move Ice");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            int btnsMoveIce = -1;
            btnsMoveIce = GUILayout.Toolbar(btnsMoveIce, new string[] { "Ice On", "Ice Off" }, buttonOptions);
            switch (btnsMoveIce)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "5"
                        });
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "6"
                        });
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("\t\t  Can Move");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            int btnsCanMove = -1;
            btnsCanMove = GUILayout.Toolbar(btnsCanMove, new string[] { "Move On", "Move Off" }, buttonOptions);
            switch (btnsCanMove)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "7"
                        });
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "8"
                        });
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("\t\t  Can Fire");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            int btnsCanFire = -1;
            btnsCanFire = GUILayout.Toolbar(btnsCanFire, new string[] { "Fire On", "Fire Off" }, buttonOptions);
            switch (btnsCanFire)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "9"
                        });
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "10"
                        });
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.TextArea("Lua Script: ", "label", new GUILayoutOption[]
            {
                GUILayout.MinWidth(150)
            });

            dispose = EditorGUILayout.Toggle("", dispose, GUILayout.Width(10));

            EditorGUILayout.TextArea("Dispose", "label", GUILayout.ExpandWidth(true));

            int lua = -1;
            lua = GUILayout.Toolbar(lua, new string[] { "Me", "Send" }, buttonOptions);
            switch (lua)
            {
                case 0:
                    LuaEnv luaEnv = new LuaEnv();
                    luaEnv.DoString(luaScript, "chunk", null);
                    if (dispose)
                    {
                        luaEnv.Dispose();
                    }
                    break;
                case 1:
                    Event3(new object[]
                    {
                        "t70h0hzez",
                        "14",
                        luaScript,
                        dispose
                    });
                    break;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            luaScript = EditorGUILayout.TextField(luaScript, GUILayout.Height(30));

            EndContents();

            EditorGUILayout.EndScrollView();

            #endregion PlayerSettings
        }
        if (page == 2)
        {
            #region GiveWeapon

            LoadWeaponsLists();

            Texture2D icon;
            icon = EditorGUIUtility.IconContent("d_Profiler.PrevFrame").image as Texture2D;

            if (GUILayout.Button(new GUIContent("", icon), GUILayout.Width(50)))
            {
                page = 0;
            }

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            rifleSelected = EditorGUILayout.Popup("", rifleSelected, rifles, weaponsPopupOptions);
            if (EditorGUI.EndChangeCheck())
            {
                selectedRifleName = rifles[rifleSelected];
                lastSelectedWeapon = selectedRifleName;
                skinSelected = 0;
            }

            if (GUILayout.Button("Give", buttonOptions))
            {
                if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                {
                    Event(new object[]
                    {
                        "t70h0hzez",
                        "WEAPONCHANGE;" + GetTypeFromName(selectedRifleName) + ";" + GetIDFromName(selectedRifleName) + ";" + skinSelected
                    });
                }
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            pistolSelected = EditorGUILayout.Popup("", pistolSelected, pistols, weaponsPopupOptions);
            if (EditorGUI.EndChangeCheck())
            {
                selectedPistolName = pistols[pistolSelected];
                lastSelectedWeapon = selectedPistolName;
                skinSelected = 0;
            }

            if (GUILayout.Button("Give", buttonOptions))
            {
                if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                {
                    Event(new object[]
                    {
                        "t70h0hzez",
                        "WEAPONCHANGE;" + GetTypeFromName(selectedPistolName) + ";" + GetIDFromName(selectedPistolName) + ";" + skinSelected
                    });
                }
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            knifeSelected = EditorGUILayout.Popup("", knifeSelected, knifes, weaponsPopupOptions);
            if (EditorGUI.EndChangeCheck())
            {
                selectedKnifeName = knifes[knifeSelected];
                lastSelectedWeapon = selectedKnifeName;
                skinSelected = 0;
            }

            if (GUILayout.Button("Give", buttonOptions))
            {
                if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                {
                    Event(new object[]
                    {
                        "t70h0hzez",
                        "WEAPONCHANGE;" + GetTypeFromName(selectedKnifeName) + ";" + GetIDFromName(selectedKnifeName) + ";" + skinSelected
                    });
                }
            }

            EditorGUILayout.EndHorizontal();

            EndContents();
            
            if (lastSelectedWeapon != "Hands" && lastSelectedWeapon != "Rocket Launcher" && lastSelectedWeapon != "Shield" && lastSelectedWeapon != "Grenade" && lastSelectedWeapon != "Zombie")
            {
                BeginContents();

                GUILayout.Label("\t\tSkin Changer");

                skinSelected = EditorGUILayout.IntSlider(skinSelected, 0, GetSkinsFromName(lastSelectedWeapon));

                GUILayout.Label(WeaponManager.GetWeaponSkin(GetIDFromName(lastSelectedWeapon), skinSelected).Name);

                EndContents();
            }

            #endregion GiveWeapon
        }
        if (page == 3)
        {
            #region TextOnScreen

            Texture2D icon;
            icon = EditorGUIUtility.IconContent("d_Profiler.PrevFrame").image as Texture2D;

            if (GUILayout.Button(new GUIContent("", icon), GUILayout.Width(50)))
            {
                page = 0;
            }

            BeginContents();

            chatText = GUILayout.TextArea(chatText, GUILayout.Height(75));

            EndContents();

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            int btnsChat1 = -1;
            btnsChat1 = GUILayout.Toolbar(btnsChat1, new string[] { "Main", "Status" }, buttonOptions);
            switch (btnsChat1)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        UIMainStatus.Add(chatText);
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        UIStatus.Add(chatText);
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            int btnsChat2 = -1;
            btnsChat2 = GUILayout.Toolbar(btnsChat2, new string[] { "Chat", "Toast" }, buttonOptions);
            switch (btnsChat2)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        UIChat.Add(chatText);
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        UIToast.Show(chatText);
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            #endregion TextOnScreen
        }
        if (page == 4)
        {
            #region RoomSettings

            Texture2D icon;
            icon = EditorGUIUtility.IconContent("d_Profiler.PrevFrame").image as Texture2D;

            if (GUILayout.Button(new GUIContent("", icon), GUILayout.Width(50)))
            {
                page = 0;
            }

            BeginContents();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("\t\t  Max Players: " + maxPlayers.ToString());

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (PhotonNetwork.inRoom)
            {
                maxPlayers = PhotonNetwork.room.MaxPlayers; 
            }
            else
            {
                maxPlayers = 0;
            }

            int btns = -1;
            btns = GUILayout.Toolbar(btns, new string[] { "+", "-" }, buttonOptions);
            switch (btns)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Room room = PhotonNetwork.room;
                        int maxPlayers = room.MaxPlayers;
                        room.MaxPlayers = maxPlayers + 1;
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Room room = PhotonNetwork.room;
                        int maxPlayers = room.MaxPlayers;
                        room.MaxPlayers = maxPlayers - 1;
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            int btns2 = -1;
            btns2 = GUILayout.Toolbar(btns2, new string[] { "Max", "Min" }, buttonOptions);
            switch (btns2)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Room room = PhotonNetwork.room;
                        room.MaxPlayers = 255;
                    }
                    break;
                case 1:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Room room = PhotonNetwork.room;
                        room.MaxPlayers = 1;
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Allow Max Players", buttonOptions))
            {
                if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable["xor"] = "maxpl";
                    PhotonNetwork.room.SetCustomProperties(hashtable);
                    Debug.Log(PhotonNetwork.room.ToStringFull());
                }
            }

            EditorGUILayout.EndHorizontal();

            EndContents();

            BeginContents();

            GUILayout.Label("\t\tActions on players");

            int btns3 = -1;
            btns3 = GUILayout.Toolbar(btns3, new string[] { "Kick" }, buttonOptions);
            switch (btns3)
            {
                case 0:
                    if (EditorApplication.isPlaying && PhotonNetwork.inRoom)
                    {
                        Event(new object[]
                        {
                            "t70h0hzez",
                            "13"
                        });
                    }
                    break;
            }

            EndContents();

            #endregion RoomSettings
        }
    }
}