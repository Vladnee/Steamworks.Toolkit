#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;

namespace Steamworks.Toolkit.Facades
{
	public struct LobbyUpdateData
	{
		public Boolean          IsSuccess;
		public LobbyData        Lobby;
		public LobbyMemberData? Member;

		public static implicit operator LobbyUpdateData( LobbyDataUpdate_t response )
		{
			if( response.m_bSuccess != 1 )
				return new LobbyUpdateData( );

			if( response.m_ulSteamIDLobby != response.m_ulSteamIDMember )
			{
				return new LobbyUpdateData
				{
					IsSuccess = true,
					Lobby     = response.m_ulSteamIDLobby,
					Member    = new LobbyMemberData { lobby = response.m_ulSteamIDLobby, user = response.m_ulSteamIDMember }
				};
			}
			else
			{
				return new LobbyUpdateData
				{
					IsSuccess = true,
					Lobby     = response.m_ulSteamIDLobby,
					Member    = null
				};
			}
		}
	}
}
#endif