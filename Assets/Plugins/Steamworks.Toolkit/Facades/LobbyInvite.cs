#if !DISABLESTEAMWORKS && STEAMWORKS_NET

using System;

namespace Steamworks.Toolkit.Facades
{
	[Serializable]
	public struct LobbyInvite : IEquatable<LobbyInvite>, IComparable<LobbyInvite>
	{
		public LobbyInvite_t Data;

		public readonly UserData  FromUser => Data.m_ulSteamIDUser;
		public readonly LobbyData ToLobby  => Data.m_ulSteamIDLobby;
		public readonly GameData  ForGame  => Data.m_ulGameID;

		public static implicit operator LobbyInvite( LobbyInvite_t native )  => new( ) { Data = native };
		public static implicit operator LobbyInvite_t( LobbyInvite heathen ) => heathen.Data;

		public readonly          Boolean Equals( LobbyInvite other ) => Data.m_ulSteamIDLobby == other.Data.m_ulSteamIDLobby && Data.m_ulSteamIDUser == other.Data.m_ulSteamIDUser && Data.m_ulGameID == other.Data.m_ulGameID;
		public readonly override Boolean Equals( Object      obj )   => obj is LobbyInvite other && Equals( other );

		public readonly override Int32 GetHashCode( ) => HashCode.Combine( Data.m_ulSteamIDLobby, Data.m_ulSteamIDUser, Data.m_ulGameID );

		public Int32 CompareTo( LobbyInvite other ) => Data.m_ulSteamIDLobby.CompareTo( other.Data.m_ulSteamIDLobby );

		public static Boolean operator==( LobbyInvite left, LobbyInvite right ) => left.Equals( right );
		public static Boolean operator!=( LobbyInvite left, LobbyInvite right ) => !left.Equals( right );
		public static Boolean operator<( LobbyInvite  left, LobbyInvite right ) => left.CompareTo( right ) < 0;
		public static Boolean operator>( LobbyInvite  left, LobbyInvite right ) => left.CompareTo( right ) > 0;
		public static Boolean operator<=( LobbyInvite left, LobbyInvite right ) => left.CompareTo( right ) <= 0;
		public static Boolean operator>=( LobbyInvite left, LobbyInvite right ) => left.CompareTo( right ) >= 0;

		public override String ToString( ) => $"LobbyInvite from {Data.m_ulSteamIDUser} to {Data.m_ulSteamIDLobby} for {Data.m_ulGameID}";
	}
}
#endif