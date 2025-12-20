#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using Steamworks.Toolkit.Facades;

namespace Steamworks.Toolkit.HandleAwaiters
{
	public sealed class LobbyDataUpdateAwaiter : HandleAwaiter<LobbyDataUpdateAwaiter, LobbyData, LobbyDataUpdate_t, LobbyUpdateData>
	{
		protected override Boolean TryGetHandle( LobbyDataUpdate_t response, out LobbyData handle )
		{
			handle = LobbyData.Get( response.m_ulSteamIDLobby );

			return handle.IsValid;
		}

		protected override void DestroyHandle( LobbyData handle ) { }

		protected override LobbyUpdateData GetResult( LobbyData handle, LobbyDataUpdate_t response ) => response;
	}
}
#endif