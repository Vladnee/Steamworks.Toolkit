#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;

namespace Steamworks.Toolkit.Facades
{
	[Serializable]
	public struct LobbyChatMessage
	{
		public          LobbyData      Lobby;
		public          EChatEntryType Type;
		public          UserData       Sender;
		public          Byte[]         Data;
		public          DateTime       ReceivedTime;
		public          String         Message     => ToString( );
		public override String         ToString( ) => System.Text.Encoding.UTF8.GetString( Data );

		public T FromJson<T>( ) => UnityEngine.JsonUtility.FromJson<T>( ToString( ) );

		public Boolean TryFromJson<T>( out T result )
		{
			try
			{
				result = UnityEngine.JsonUtility.FromJson<T>( ToString( ) );

				return true;
			}
			catch
			{
				result = default;

				return false;
			}
		}

		public static implicit operator LobbyChatMessage( LobbyChatMsg_t result )
		{
			var data  = new Byte[4096];
			var lobby = new CSteamID( result.m_ulSteamIDLobby );
			var ret   = SteamMatchmaking.GetLobbyChatEntry( lobby, (Int32)result.m_iChatID, out var user, data, data.Length, out var chatEntryType );
			Array.Resize( ref data, ret );

			return new LobbyChatMessage
			{
				Lobby        = lobby,
				Type         = chatEntryType,
				Data         = data,
				ReceivedTime = DateTime.UnixEpoch.AddSeconds( SteamUtils.GetServerRealTime( ) ),
				Sender       = user
			};
		}
	}
}
#endif