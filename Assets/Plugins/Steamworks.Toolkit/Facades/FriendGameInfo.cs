#if !DISABLESTEAMWORKS && STEAMWORKS_NET

using System;

namespace Steamworks.Toolkit.Facades
{
	[Serializable]
	public struct FriendGameInfo
	{
		public          FriendGameInfo_t Data;
		public readonly GameData         Game      => Data.m_gameID;
		public readonly String           IpAddress => SteamUtilities.IPUintToString( Data.m_unGameIP );
		public readonly UInt32           IpInt     => Data.m_unGameIP;
		public readonly UInt16           GamePort  => Data.m_usGamePort;
		public readonly UInt16           QueryPort => Data.m_usQueryPort;
		public readonly LobbyData        Lobby     => Data.m_steamIDLobby;

		public static implicit operator FriendGameInfo( FriendGameInfo_t native )  => new( ) { Data = native };
		public static implicit operator FriendGameInfo_t( FriendGameInfo heathen ) => heathen.Data;
	}
}
#endif