using System;

public enum PhotonNetworkingMessage
{
	OnConnectedToPhoton,
	OnLeftRoom,
	OnMasterClientSwitched,
	OnPhotonCreateRoomFailed,
	OnPhotonJoinRoomFailed,
	OnCreatedRoom,
	OnJoinedLobby,
	OnLeftLobby,
	OnDisconnectedFromPhoton,
	OnConnectionFail,
	OnFailedToConnectToPhoton,
	OnReceivedRoomListUpdate,
	OnJoinedRoom,
	OnPhotonPlayerConnected,
	OnPhotonPlayerDisconnected,
	OnPhotonRandomJoinFailed,
	OnConnectedToMaster,
	OnPhotonSerializeView,
	OnPhotonInstantiate,
	OnPhotonMaxCccuReached,
	OnPhotonCustomRoomPropertiesChanged,
	OnPhotonPlayerPropertiesChanged,
	OnUpdatedFriendList,
	OnCustomAuthenticationFailed,
	OnCustomAuthenticationResponse,
	OnWebRpcResponse,
	OnOwnershipRequest,
	OnLobbyStatisticsUpdate,
	OnPhotonPlayerActivityChanged,
	OnOwnershipTransfered
}

public enum PhotonLogLevel
{
	ErrorsOnly,
	Informational,
	Full
}

public enum PhotonTargets
{
	All,
	Others,
	MasterClient,
	AllBuffered,
	OthersBuffered,
	AllViaServer,
	AllBufferedViaServer
}

public enum CloudRegionCode
{
	eu = 0,
	us = 1,
	asia = 2,
	jp = 3,
	au = 5,
	usw = 6,
	sa = 7,
	cae = 8,
	kr = 9,
	@in = 10,
	ru = 11,
	rue = 12,
	none = 4
}

[Flags]
public enum CloudRegionFlag
{
	eu = 1,
	us = 2,
	asia = 4,
	jp = 8,
	au = 0x10,
	usw = 0x20,
	sa = 0x40,
	cae = 0x80,
	kr = 0x100,
	@in = 0x200,
	ru = 0x400,
	rue = 0x800
}

public enum ConnectionState
{
	Disconnected,
	Connecting,
	Connected,
	Disconnecting,
	InitializingApplication
}

public enum EncryptionMode
{
	PayloadEncryption = 0,
	DatagramEncryption = 10
}

public static class EncryptionDataParameters
{
	public const byte Mode = 0;

	public const byte Secret1 = 1;

	public const byte Secret2 = 2;
}
