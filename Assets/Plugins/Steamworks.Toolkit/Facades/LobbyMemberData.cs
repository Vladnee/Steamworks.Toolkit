#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;

namespace Steamworks.Toolkit.Facades
{
	[Serializable]
	public struct LobbyMemberData : IEquatable<LobbyMemberData>
	{
		public LobbyData lobby;

		public UserData user;

		public readonly String this[ String metadataKey ]
		{
			get => SteamMatchmaking.GetLobbyMemberData( lobby, user, metadataKey );
			set
			{
				if( user.IsMe )
					SteamMatchmaking.SetLobbyMemberData( lobby, metadataKey, value );
			}
		}

		public readonly Boolean IsOwner => lobby.Owner.Equals( this );

		public readonly Boolean Equals( LobbyMemberData other ) => other.lobby == lobby && other.user == user;

		public readonly void Kick( ) => lobby.KickMember( user );

		public static LobbyMemberData Get( LobbyData lobby, UserData user ) => new( ) { lobby = lobby, user = user };
	}
}
#endif