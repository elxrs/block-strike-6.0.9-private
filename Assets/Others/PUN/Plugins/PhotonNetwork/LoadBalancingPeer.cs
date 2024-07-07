using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

internal class LoadBalancingPeer : PhotonPeer
{
	private enum RoomOptionBit
	{
		CheckUserOnJoin = 1,
		DeleteCacheOnLeave = 2,
		SuppressRoomEvents = 4,
		PublishUserId = 8,
		DeleteNullProps = 0x10,
		BroadcastPropsChangeToAll = 0x20
	}

	private readonly Dictionary<byte, object> opParameters = new Dictionary<byte, object>();

	private Dictionary<byte, object> opParametersSetProperties = new Dictionary<byte, object>();

	internal bool IsProtocolSecure
	{
		get
		{
			return UsedProtocol == ConnectionProtocol.WebSocketSecure;
		}
	}

	public LoadBalancingPeer(ConnectionProtocol protocolType)
		: base(protocolType)
	{
	}

	public LoadBalancingPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
		: this(protocolType)
	{
		Listener = listener;
	}

	public virtual bool OpGetRegions(string appId)
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary[224] = appId;
		return OpCustom(220, dictionary, true, 0, true);
	}

	public virtual bool OpJoinLobby(TypedLobby lobby = null)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpJoinLobby()");
		}
		Dictionary<byte, object> dictionary = null;
		if (lobby != null && !lobby.IsDefault)
		{
			dictionary = new Dictionary<byte, object>();
			dictionary[213] = lobby.Name;
			dictionary[212] = (byte)lobby.Type;
		}
		return OpCustom(229, dictionary, true);
	}

	public virtual bool OpLeaveLobby()
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpLeaveLobby()");
		}
		return OpCustom(228, null, true);
	}

	private void RoomOptionsToOpParameters(Dictionary<byte, object> op, RoomOptions roomOptions)
	{
		if (roomOptions == null)
		{
			roomOptions = new RoomOptions();
		}
		Hashtable hashtable = new Hashtable();
		hashtable[(byte)253] = roomOptions.IsOpen;
		hashtable[(byte)254] = roomOptions.IsVisible;
		hashtable[(byte)250] = ((roomOptions.CustomRoomPropertiesForLobby != null) ? roomOptions.CustomRoomPropertiesForLobby : new string[0]);
		hashtable.MergeStringKeys(roomOptions.CustomRoomProperties);
		if (roomOptions.MaxPlayers > 0)
		{
			hashtable[byte.MaxValue] = roomOptions.MaxPlayers;
		}
		op[248] = hashtable;
		int num = 0;
		op[241] = roomOptions.CleanupCacheOnLeave;
		if (roomOptions.CleanupCacheOnLeave)
		{
			num |= 2;
			hashtable[(byte)249] = true;
		}
		num |= 1;
		op[232] = true;
		if (roomOptions.PlayerTtl > 0 || roomOptions.PlayerTtl == -1)
		{
			op[235] = roomOptions.PlayerTtl;
		}
		if (roomOptions.EmptyRoomTtl > 0)
		{
			op[236] = roomOptions.EmptyRoomTtl;
		}
		if (roomOptions.SuppressRoomEvents)
		{
			num |= 4;
			op[237] = true;
		}
		if (roomOptions.Plugins != null)
		{
			op[204] = roomOptions.Plugins;
		}
		if (roomOptions.PublishUserId)
		{
			num |= 8;
			op[239] = true;
		}
		if (roomOptions.DeleteNullProperties)
		{
			num |= 0x10;
		}
		op[191] = num;
	}

	public virtual bool OpCreateRoom(EnterRoomParams opParams)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpCreateRoom()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (!string.IsNullOrEmpty(opParams.RoomName))
		{
			dictionary[byte.MaxValue] = opParams.RoomName;
		}
		if (opParams.Lobby != null && !string.IsNullOrEmpty(opParams.Lobby.Name))
		{
			dictionary[213] = opParams.Lobby.Name;
			dictionary[212] = (byte)opParams.Lobby.Type;
		}
		if (opParams.ExpectedUsers != null && opParams.ExpectedUsers.Length > 0)
		{
			dictionary[238] = opParams.ExpectedUsers;
		}
		if (opParams.OnGameServer)
		{
			if (opParams.PlayerProperties != null && opParams.PlayerProperties.Count > 0)
			{
				dictionary[249] = opParams.PlayerProperties;
				dictionary[250] = true;
			}
			RoomOptionsToOpParameters(dictionary, opParams.RoomOptions);
		}
		return OpCustom(227, dictionary, true);
	}

	public virtual bool OpJoinRoom(EnterRoomParams opParams)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpJoinRoom()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (!string.IsNullOrEmpty(opParams.RoomName))
		{
			dictionary[byte.MaxValue] = opParams.RoomName;
		}
		if (opParams.CreateIfNotExists)
		{
			dictionary[215] = (byte)1;
			if (opParams.Lobby != null)
			{
				dictionary[213] = opParams.Lobby.Name;
				dictionary[212] = (byte)opParams.Lobby.Type;
			}
		}
		if (opParams.RejoinOnly)
		{
			dictionary[215] = (byte)3;
		}
		if (opParams.ExpectedUsers != null && opParams.ExpectedUsers.Length > 0)
		{
			dictionary[238] = opParams.ExpectedUsers;
		}
		if (opParams.OnGameServer)
		{
			if (opParams.PlayerProperties != null && opParams.PlayerProperties.Count > 0)
			{
				dictionary[249] = opParams.PlayerProperties;
				dictionary[250] = true;
			}
			if (opParams.CreateIfNotExists)
			{
				RoomOptionsToOpParameters(dictionary, opParams.RoomOptions);
			}
		}
		return OpCustom(226, dictionary, true);
	}

	public virtual bool OpJoinRandomRoom(OpJoinRandomRoomParams opJoinRandomRoomParams)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpJoinRandomRoom()");
		}
		Hashtable hashtable = new Hashtable();
		hashtable.MergeStringKeys(opJoinRandomRoomParams.ExpectedCustomRoomProperties);
		if (opJoinRandomRoomParams.ExpectedMaxPlayers > 0)
		{
			hashtable[byte.MaxValue] = opJoinRandomRoomParams.ExpectedMaxPlayers;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (hashtable.Count > 0)
		{
			dictionary[248] = hashtable;
		}
		if (opJoinRandomRoomParams.MatchingType != 0)
		{
			dictionary[223] = (byte)opJoinRandomRoomParams.MatchingType;
		}
		if (opJoinRandomRoomParams.TypedLobby != null && !string.IsNullOrEmpty(opJoinRandomRoomParams.TypedLobby.Name))
		{
			dictionary[213] = opJoinRandomRoomParams.TypedLobby.Name;
			dictionary[212] = (byte)opJoinRandomRoomParams.TypedLobby.Type;
		}
		if (!string.IsNullOrEmpty(opJoinRandomRoomParams.SqlLobbyFilter))
		{
			dictionary[245] = opJoinRandomRoomParams.SqlLobbyFilter;
		}
		if (opJoinRandomRoomParams.ExpectedUsers != null && opJoinRandomRoomParams.ExpectedUsers.Length > 0)
		{
			dictionary[238] = opJoinRandomRoomParams.ExpectedUsers;
		}
		return OpCustom(225, dictionary, true);
	}

	public virtual bool OpLeaveRoom(bool becomeInactive)
	{
		Dictionary<byte, object> dictionary = null;
		if (becomeInactive)
		{
			dictionary = new Dictionary<byte, object>();
			dictionary[233] = becomeInactive;
		}
		return OpCustom(254, dictionary, true);
	}

	public virtual bool OpGetGameList(TypedLobby lobby, string queryData)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList()");
		}
		if (lobby == null)
		{
			if ((int)DebugOut >= 3)
			{
				Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. Lobby cannot be null.");
			}
			return false;
		}
		if (lobby.Type != LobbyType.SqlLobby)
		{
			if ((int)DebugOut >= 3)
			{
				Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. LobbyType must be SqlLobby.");
			}
			return false;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary[213] = lobby.Name;
		dictionary[212] = (byte)lobby.Type;
		dictionary[245] = queryData;
		return OpCustom(217, dictionary, true);
	}

	public virtual bool OpFindFriends(string[] friendsToFind)
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (friendsToFind != null && friendsToFind.Length > 0)
		{
			dictionary[1] = friendsToFind;
		}
		return OpCustom(222, dictionary, true);
	}

	public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable actorProperties)
	{
		return OpSetPropertiesOfActor(actorNr, actorProperties.StripToStringKeys());
	}

	protected internal bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, Hashtable expectedProperties = null, bool webForward = false)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor()");
		}
		if (actorNr <= 0 || actorProperties == null)
		{
			if ((int)DebugOut >= 3)
			{
				Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor not sent. ActorNr must be > 0 and actorProperties != null.");
			}
			return false;
		}
		opParametersSetProperties.Clear();
		opParametersSetProperties.Add(251, actorProperties);
		opParametersSetProperties.Add(254, actorNr);
		opParametersSetProperties.Add(250, true);
		if (expectedProperties != null && expectedProperties.Count != 0)
		{
			opParametersSetProperties.Add(231, expectedProperties);
		}
		if (webForward)
		{
			opParametersSetProperties[234] = true;
		}
		return OpCustom(252, opParametersSetProperties, true, 0, false);
	}

	protected internal void OpSetPropertyOfRoom(byte propCode, object value)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[propCode] = value;
		OpSetPropertiesOfRoom(hashtable);
	}

	public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId)
	{
		return OpSetPropertiesOfRoom(gameProperties.StripToStringKeys());
	}

	protected internal bool OpSetPropertiesOfRoom(Hashtable gameProperties, Hashtable expectedProperties = null, bool webForward = false)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfRoom()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary.Add(251, gameProperties);
		dictionary.Add(250, true);
		if (expectedProperties != null && expectedProperties.Count != 0)
		{
			dictionary.Add(231, expectedProperties);
		}
		if (webForward)
		{
			dictionary[234] = true;
		}
		return OpCustom(252, dictionary, true, 0, false);
	}

	public virtual bool OpAuthenticate(string appId, string appVersion, AuthenticationValues authValues, string regionCode, bool getLobbyStatistics)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (getLobbyStatistics)
		{
			dictionary[211] = true;
		}
		if (authValues != null && authValues.Token != null)
		{
			dictionary[221] = authValues.Token;
			return OpCustom(230, dictionary, true, 0, false);
		}
		dictionary[220] = appVersion;
		dictionary[224] = appId;
		if (!string.IsNullOrEmpty(regionCode))
		{
			dictionary[210] = regionCode;
		}
		if (authValues != null)
		{
			if (!string.IsNullOrEmpty(authValues.UserId))
			{
				dictionary[225] = authValues.UserId;
			}
			if (authValues.AuthType != CustomAuthenticationType.None)
			{
				if (!IsProtocolSecure && !IsEncryptionAvailable)
				{
					Listener.DebugReturn(DebugLevel.ERROR, "OpAuthenticate() failed. When you want Custom Authentication encryption is mandatory.");
					return false;
				}
				dictionary[217] = (byte)authValues.AuthType;
				if (!string.IsNullOrEmpty(authValues.Token))
				{
					dictionary[221] = authValues.Token;
				}
				else
				{
					if (!string.IsNullOrEmpty(authValues.AuthGetParameters))
					{
						dictionary[216] = authValues.AuthGetParameters;
					}
					if (authValues.AuthPostData != null)
					{
						dictionary[214] = authValues.AuthPostData;
					}
				}
			}
		}
		bool flag = OpCustom(230, dictionary, true, 0, IsEncryptionAvailable);
		if (!flag)
		{
			Listener.DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, AuthValues and if you're connected.");
		}
		return flag;
	}

	public virtual bool OpAuthenticateOnce(string appId, string appVersion, AuthenticationValues authValues, string regionCode, EncryptionMode encryptionMode, ConnectionProtocol expectedProtocol)
	{
		if ((int)DebugOut >= 3)
		{
			Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (authValues != null && authValues.Token != null)
		{
			dictionary[221] = authValues.Token;
			return OpCustom(231, dictionary, true, 0, false);
		}
		if (encryptionMode == EncryptionMode.DatagramEncryption && expectedProtocol != 0)
		{
			Debug.LogWarning("Expected protocol set to UDP, due to encryption mode DatagramEncryption. Changing protocol in PhotonServerSettings from: " + PhotonNetwork.PhotonServerSettings.Protocol);
			PhotonNetwork.PhotonServerSettings.Protocol = ConnectionProtocol.Udp;
			expectedProtocol = ConnectionProtocol.Udp;
		}
		dictionary[195] = (byte)expectedProtocol;
		dictionary[193] = (byte)encryptionMode;
		dictionary[220] = appVersion;
		dictionary[224] = appId;
		if (!string.IsNullOrEmpty(regionCode))
		{
			dictionary[210] = regionCode;
		}
		if (authValues != null)
		{
			if (!string.IsNullOrEmpty(authValues.UserId))
			{
				dictionary[225] = authValues.UserId;
			}
			if (authValues.AuthType != CustomAuthenticationType.None)
			{
				dictionary[217] = (byte)authValues.AuthType;
				if (!string.IsNullOrEmpty(authValues.Token))
				{
					dictionary[221] = authValues.Token;
				}
				else
				{
					if (!string.IsNullOrEmpty(authValues.AuthGetParameters))
					{
						dictionary[216] = authValues.AuthGetParameters;
					}
					if (authValues.AuthPostData != null)
					{
						dictionary[214] = authValues.AuthPostData;
					}
				}
			}
		}
		return OpCustom(231, dictionary, true, 0, IsEncryptionAvailable);
	}

	public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
	{
		if ((int)DebugOut >= 5)
		{
			Listener.DebugReturn(DebugLevel.ALL, "OpChangeGroups()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (groupsToRemove != null)
		{
			dictionary[239] = groupsToRemove;
		}
		if (groupsToAdd != null)
		{
			dictionary[238] = groupsToAdd;
		}
		return OpCustom(248, dictionary, true, 0);
	}

	public virtual bool OpRaiseEvent(byte eventCode, object customEventContent, bool sendReliable, RaiseEventOptions raiseEventOptions)
	{
		nProfiler.BeginSample("LoadBalancingPeer.OpRaiseEvent");
		opParameters.Clear();
		opParameters[244] = ByteObjectCache.bytes[eventCode];
		if (customEventContent != null)
		{
			opParameters[245] = customEventContent;
		}
		if (raiseEventOptions == null)
		{
			raiseEventOptions = RaiseEventOptions.Default;
		}
		else
		{
			if (raiseEventOptions.CachingOption != 0)
			{
				opParameters[247] = ByteObjectCache.bytes[(uint)raiseEventOptions.CachingOption];
			}
			if (raiseEventOptions.Receivers != 0)
			{
				opParameters[246] = ByteObjectCache.bytes[(uint)raiseEventOptions.Receivers];
			}
			if (raiseEventOptions.InterestGroup != 0)
			{
				opParameters[240] = ByteObjectCache.bytes[raiseEventOptions.InterestGroup];
			}
			if (raiseEventOptions.TargetActors != null)
			{
				opParameters[252] = raiseEventOptions.TargetActors;
			}
			if (raiseEventOptions.ForwardToWebhook)
			{
				opParameters[234] = true;
			}
		}
		bool result = OpCustom(253, opParameters, sendReliable, raiseEventOptions.SequenceChannel, raiseEventOptions.Encrypt);
		nProfiler.EndSample();
		return result;
	}

	public virtual bool OpSettings(bool receiveLobbyStats)
	{
		if ((int)DebugOut >= 5)
		{
			Listener.DebugReturn(DebugLevel.ALL, "OpSettings()");
		}
		opParameters.Clear();
		if (receiveLobbyStats)
		{
			opParameters[0] = receiveLobbyStats;
		}
		if (opParameters.Count == 0)
		{
			return true;
		}
		return OpCustom(218, opParameters, true);
	}
}

internal class OpJoinRandomRoomParams
{
	public Hashtable ExpectedCustomRoomProperties;

	public byte ExpectedMaxPlayers;

	public MatchmakingMode MatchingType;

	public TypedLobby TypedLobby;

	public string SqlLobbyFilter;

	public string[] ExpectedUsers;
}

internal class EnterRoomParams
{
	public string RoomName;

	public RoomOptions RoomOptions;

	public TypedLobby Lobby;

	public Hashtable PlayerProperties;

	public bool OnGameServer = true;

	public bool CreateIfNotExists;

	public bool RejoinOnly;

	public string[] ExpectedUsers;
}

public class ErrorCode
{
	public const int Ok = 0;

	public const int OperationNotAllowedInCurrentState = -3;

	[Obsolete("Use InvalidOperation.")]
	public const int InvalidOperationCode = -2;

	public const int InvalidOperation = -2;

	public const int InternalServerError = -1;

	public const int InvalidAuthentication = 32767;

	public const int GameIdAlreadyExists = 32766;

	public const int GameFull = 32765;

	public const int GameClosed = 32764;

	[Obsolete("No longer used, cause random matchmaking is no longer a process.")]
	public const int AlreadyMatched = 32763;

	public const int ServerFull = 32762;

	public const int UserBlocked = 32761;

	public const int NoRandomMatchFound = 32760;

	public const int GameDoesNotExist = 32758;

	public const int MaxCcuReached = 32757;

	public const int InvalidRegion = 32756;

	public const int CustomAuthenticationFailed = 32755;

	public const int AuthenticationTicketExpired = 32753;

	public const int PluginReportedError = 32752;

	public const int PluginMismatch = 32751;

	public const int JoinFailedPeerAlreadyJoined = 32750;

	public const int JoinFailedFoundInactiveJoiner = 32749;

	public const int JoinFailedWithRejoinerNotFound = 32748;

	public const int JoinFailedFoundExcludedUserId = 32747;

	public const int JoinFailedFoundActiveJoiner = 32746;

	public const int HttpLimitReached = 32745;

	public const int ExternalHttpCallFailed = 32744;

	public const int SlotError = 32742;

	public const int InvalidEncryptionParameters = 32741;
}

public class ActorProperties
{
	public const byte PlayerName = byte.MaxValue;

	public const byte IsInactive = 254;

	public const byte UserId = 253;
}

public class GamePropertyKey
{
	public const byte MaxPlayers = byte.MaxValue;

	public const byte IsVisible = 254;

	public const byte IsOpen = 253;

	public const byte PlayerCount = 252;

	public const byte Removed = 251;

	public const byte PropsListedInLobby = 250;

	public const byte CleanupCacheOnLeave = 249;

	public const byte MasterClientId = 248;

	public const byte ExpectedUsers = 247;

	public const byte PlayerTtl = 246;

	public const byte EmptyRoomTtl = 245;
}

public class EventCode
{
	public const byte GameList = 230;

	public const byte GameListUpdate = 229;

	public const byte QueueState = 228;

	public const byte Match = 227;

	public const byte AppStats = 226;

	public const byte LobbyStats = 224;

	[Obsolete("TCP routing was removed after becoming obsolete.")]
	public const byte AzureNodeInfo = 210;

	public const byte Join = byte.MaxValue;

	public const byte Leave = 254;

	public const byte PropertiesChanged = 253;

	[Obsolete("Use PropertiesChanged now.")]
	public const byte SetProperties = 253;

	public const byte ErrorInfo = 251;

	public const byte CacheSliceChanged = 250;

	public const byte AuthEvent = 223;
}

public class ParameterCode
{
	public const byte SuppressRoomEvents = 237;

	public const byte EmptyRoomTTL = 236;

	public const byte PlayerTTL = 235;

	public const byte EventForward = 234;

	[Obsolete("Use: IsInactive")]
	public const byte IsComingBack = 233;

	public const byte IsInactive = 233;

	public const byte CheckUserOnJoin = 232;

	public const byte ExpectedValues = 231;

	public const byte Address = 230;

	public const byte PeerCount = 229;

	public const byte GameCount = 228;

	public const byte MasterPeerCount = 227;

	public const byte UserId = 225;

	public const byte ApplicationId = 224;

	public const byte Position = 223;

	public const byte MatchMakingType = 223;

	public const byte GameList = 222;

	public const byte Secret = 221;

	public const byte AppVersion = 220;

	[Obsolete("TCP routing was removed after becoming obsolete.")]
	public const byte AzureNodeInfo = 210;

	[Obsolete("TCP routing was removed after becoming obsolete.")]
	public const byte AzureLocalNodeId = 209;

	[Obsolete("TCP routing was removed after becoming obsolete.")]
	public const byte AzureMasterNodeId = 208;

	public const byte RoomName = byte.MaxValue;

	public const byte Broadcast = 250;

	public const byte ActorList = 252;

	public const byte ActorNr = 254;

	public const byte PlayerProperties = 249;

	public const byte CustomEventContent = 245;

	public const byte Data = 245;

	public const byte Code = 244;

	public const byte GameProperties = 248;

	public const byte Properties = 251;

	public const byte TargetActorNr = 253;

	public const byte ReceiverGroup = 246;

	public const byte Cache = 247;

	public const byte CleanupCacheOnLeave = 241;

	public const byte Group = 240;

	public const byte Remove = 239;

	public const byte PublishUserId = 239;

	public const byte Add = 238;

	public const byte Info = 218;

	public const byte ClientAuthenticationType = 217;

	public const byte ClientAuthenticationParams = 216;

	public const byte JoinMode = 215;

	public const byte ClientAuthenticationData = 214;

	public const byte MasterClientId = 203;

	public const byte FindFriendsRequestList = 1;

	public const byte FindFriendsResponseOnlineList = 1;

	public const byte FindFriendsResponseRoomIdList = 2;

	public const byte LobbyName = 213;

	public const byte LobbyType = 212;

	public const byte LobbyStats = 211;

	public const byte Region = 210;

	public const byte UriPath = 209;

	public const byte WebRpcParameters = 208;

	public const byte WebRpcReturnCode = 207;

	public const byte WebRpcReturnMessage = 206;

	public const byte CacheSliceIndex = 205;

	public const byte Plugins = 204;

	public const byte NickName = 202;

	public const byte PluginName = 201;

	public const byte PluginVersion = 200;

	public const byte ExpectedProtocol = 195;

	public const byte CustomInitData = 194;

	public const byte EncryptionMode = 193;

	public const byte EncryptionData = 192;

	public const byte RoomOptionFlags = 191;
}

public class OperationCode
{
	[Obsolete("Exchanging encrpytion keys is done internally in the lib now. Don't expect this operation-result.")]
	public const byte ExchangeKeysForEncryption = 250;

	[Obsolete]
	public const byte Join = byte.MaxValue;

	public const byte AuthenticateOnce = 231;

	public const byte Authenticate = 230;

	public const byte JoinLobby = 229;

	public const byte LeaveLobby = 228;

	public const byte CreateGame = 227;

	public const byte JoinGame = 226;

	public const byte JoinRandomGame = 225;

	public const byte Leave = 254;

	public const byte RaiseEvent = 253;

	public const byte SetProperties = 252;

	public const byte GetProperties = 251;

	public const byte ChangeGroups = 248;

	public const byte FindFriends = 222;

	public const byte GetLobbyStats = 221;

	public const byte GetRegions = 220;

	public const byte WebRpc = 219;

	public const byte ServerSettings = 218;

	public const byte GetGameList = 217;
}

public enum JoinMode : byte
{
	Default,
	CreateIfNotExists,
	JoinOrRejoin,
	RejoinOnly
}

public enum MatchmakingMode : byte
{
	FillRoom,
	SerialMatching,
	RandomMatching
}

public enum ReceiverGroup : byte
{
	Others,
	All,
	MasterClient
}

public enum EventCaching : byte
{
	DoNotCache = 0,
	[Obsolete]
	MergeCache = 1,
	[Obsolete]
	ReplaceCache = 2,
	[Obsolete]
	RemoveCache = 3,
	AddToRoomCache = 4,
	AddToRoomCacheGlobal = 5,
	RemoveFromRoomCache = 6,
	RemoveFromRoomCacheForActorsLeft = 7,
	SliceIncreaseIndex = 10,
	SliceSetIndex = 11,
	SlicePurgeIndex = 12,
	SlicePurgeUpToIndex = 13
}

[Flags]
public enum PropertyTypeFlag : byte
{
	None = 0,
	Game = 1,
	Actor = 2,
	GameAndActor = 3
}

public class RoomOptions
{
	private bool isVisibleField = true;

	private bool isOpenField = true;

	public byte MaxPlayers;

	public int PlayerTtl;

	public int EmptyRoomTtl;

	private bool cleanupCacheOnLeaveField = PhotonNetwork.autoCleanUpPlayerObjects;

	public Hashtable CustomRoomProperties;

	public string[] CustomRoomPropertiesForLobby = new string[0];

	public string[] Plugins;

	private bool suppressRoomEventsField;

	private bool publishUserIdField;

	private bool deleteNullPropertiesField;

	public bool IsVisible
	{
		get
		{
			return isVisibleField;
		}
		set
		{
			isVisibleField = value;
		}
	}

	public bool IsOpen
	{
		get
		{
			return isOpenField;
		}
		set
		{
			isOpenField = value;
		}
	}

	public bool CleanupCacheOnLeave
	{
		get
		{
			return cleanupCacheOnLeaveField;
		}
		set
		{
			cleanupCacheOnLeaveField = value;
		}
	}

	public bool SuppressRoomEvents
	{
		get
		{
			return suppressRoomEventsField;
		}
	}

	public bool PublishUserId
	{
		get
		{
			return publishUserIdField;
		}
		set
		{
			publishUserIdField = value;
		}
	}

	public bool DeleteNullProperties
	{
		get
		{
			return deleteNullPropertiesField;
		}
		set
		{
			deleteNullPropertiesField = value;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public bool isVisible
	{
		get
		{
			return isVisibleField;
		}
		set
		{
			isVisibleField = value;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public bool isOpen
	{
		get
		{
			return isOpenField;
		}
		set
		{
			isOpenField = value;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public byte maxPlayers
	{
		get
		{
			return MaxPlayers;
		}
		set
		{
			MaxPlayers = value;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public bool cleanupCacheOnLeave
	{
		get
		{
			return cleanupCacheOnLeaveField;
		}
		set
		{
			cleanupCacheOnLeaveField = value;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public Hashtable customRoomProperties
	{
		get
		{
			return CustomRoomProperties;
		}
		set
		{
			CustomRoomProperties = value;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public string[] customRoomPropertiesForLobby
	{
		get
		{
			return CustomRoomPropertiesForLobby;
		}
		set
		{
			CustomRoomPropertiesForLobby = value;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public string[] plugins
	{
		get
		{
			return Plugins;
		}
		set
		{
			Plugins = value;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public bool suppressRoomEvents
	{
		get
		{
			return suppressRoomEventsField;
		}
	}

	[Obsolete("Use property with uppercase naming instead.")]
	public bool publishUserId
	{
		get
		{
			return publishUserIdField;
		}
		set
		{
			publishUserIdField = value;
		}
	}
}

public class RaiseEventOptions
{
	public static readonly RaiseEventOptions Default = new RaiseEventOptions();

	public EventCaching CachingOption;

	public byte InterestGroup;

	public int[] TargetActors;

	public ReceiverGroup Receivers;

	public byte SequenceChannel;

	public bool ForwardToWebhook;

	public bool Encrypt;

	public void Reset()
	{
		CachingOption = Default.CachingOption;
		InterestGroup = Default.InterestGroup;
		TargetActors = Default.TargetActors;
		Receivers = Default.Receivers;
		SequenceChannel = Default.SequenceChannel;
		ForwardToWebhook = Default.ForwardToWebhook;
		Encrypt = Default.Encrypt;
	}
}

public enum LobbyType : byte
{
	Default = 0,
	SqlLobby = 2,
	AsyncRandomLobby = 3
}

public class TypedLobby
{
	public string Name;

	public LobbyType Type;

	public static readonly TypedLobby Default = new TypedLobby();

	public bool IsDefault
	{
		get
		{
			return Type == LobbyType.Default && string.IsNullOrEmpty(Name);
		}
	}

	public TypedLobby()
	{
		Name = string.Empty;
		Type = LobbyType.Default;
	}

	public TypedLobby(string name, LobbyType type)
	{
		Name = name;
		Type = type;
	}

	public override string ToString()
	{
		return string.Format("lobby '{0}'[{1}]", Name, Type);
	}
}

public class TypedLobbyInfo : TypedLobby
{
	public int PlayerCount;

	public int RoomCount;

	public override string ToString()
	{
		return string.Format("TypedLobbyInfo '{0}'[{1}] rooms: {2} players: {3}", Name, Type, RoomCount, PlayerCount);
	}
}

public enum AuthModeOption
{
	Auth,
	AuthOnce,
	AuthOnceWss
}

public enum CustomAuthenticationType : byte
{
	Custom = 0,
	Steam = 1,
	Facebook = 2,
	Oculus = 3,
	PlayStation = 4,
	Xbox = 5,
	None = byte.MaxValue
}

public class AuthenticationValues
{
	private CustomAuthenticationType authType = CustomAuthenticationType.None;

	public CustomAuthenticationType AuthType
	{
		get
		{
			return authType;
		}
		set
		{
			authType = value;
		}
	}

	public string AuthGetParameters { get; set; }

	public object AuthPostData { get; private set; }

	public string Token { get; set; }

	public string UserId { get; set; }

	public AuthenticationValues()
	{
	}

	public AuthenticationValues(string userId)
	{
		UserId = userId;
	}

	public virtual void SetAuthPostData(string stringData)
	{
		AuthPostData = ((!string.IsNullOrEmpty(stringData)) ? stringData : null);
	}

	public virtual void SetAuthPostData(byte[] byteData)
	{
		AuthPostData = byteData;
	}

	public virtual void SetAuthPostData(Dictionary<string, object> dictData)
	{
		AuthPostData = dictData;
	}

	public virtual void AddAuthParameter(string key, string value)
	{
		string text = ((!string.IsNullOrEmpty(AuthGetParameters)) ? "&" : string.Empty);
		AuthGetParameters = string.Format("{0}{1}{2}={3}", AuthGetParameters, text, Uri.EscapeDataString(key), Uri.EscapeDataString(value));
	}

	public override string ToString()
	{
		return string.Format("AuthenticationValues UserId: {0}, GetParameters: {1} Token available: {2}", UserId, AuthGetParameters, Token != null);
	}
}
