#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Steamworks.Toolkit.Facades
{
	[Serializable]
	public struct LobbyData : IEquatable<CSteamID>, IEquatable<UInt64>, IEquatable<LobbyData>
	{
		private static class LobbyMetadata
		{
			public const String Name     = "lobby_name";
			public const String Type     = "lobby_type";
			public const String Version  = "lobby_version";
			public const String KickList = "lobby_kick_list";
		}

		private UInt64 _id;

		public readonly CSteamID SteamId => new(_id);

		public readonly Boolean IsValid
		{
			get
			{
				var sId = SteamId;

				return sId != CSteamID.Nil
				    && sId.GetEAccountType( ) == EAccountType.k_EAccountTypeChat
				    && sId.GetEUniverse( ) == EUniverse.k_EUniversePublic;
			}
		}

		public readonly String this[ String metadataKey ]
		{
			get => SteamMatchmaking.GetLobbyData( this, metadataKey );
			set => SteamMatchmaking.SetLobbyData( this, metadataKey, value );
		}

		public readonly String Name
		{
			get => this[LobbyMetadata.Name];
			set => this[LobbyMetadata.Name] = value;
		}

		public readonly ELobbyType Type
		{
			get
			{
				if( Int32.TryParse( SteamMatchmaking.GetLobbyData( this, LobbyMetadata.Type ), out var enumVal ) )
					return (ELobbyType)enumVal;

				return ELobbyType.k_ELobbyTypePrivate;
			}
			set
			{
				SteamMatchmaking.SetLobbyData( this, LobbyMetadata.Type, ( (Int32)value ).ToString( ) );
				SteamMatchmaking.SetLobbyType( this, value );
			}
		}

		public readonly String GameVersion
		{
			get => this[LobbyMetadata.Version];
			set => this[LobbyMetadata.Version] = value;
		}

		public readonly Boolean IsOwner => UserData.Me == SteamMatchmaking.GetLobbyOwner( this );
		public readonly LobbyMemberData Owner
		{
			get => new( ) { lobby = this, user = SteamMatchmaking.GetLobbyOwner( this ) };
			set => SteamMatchmaking.SetLobbyOwner( this, value.user );
		}

		public readonly Int32 MemberCount => SteamMatchmaking.GetNumLobbyMembers( this );
		public readonly Int32 MaxMembers
		{
			get => SteamMatchmaking.GetLobbyMemberLimit( this );
			set => SteamMatchmaking.SetLobbyMemberLimit( this, value );
		}
		public readonly Boolean IsFull => MaxMembers <= MemberCount;
		public readonly LobbyMemberData[] Members
		{
			get
			{
				var count   = SteamMatchmaking.GetNumLobbyMembers( this );
				var results = new LobbyMemberData[count];

				for( var i = 0; i < count; i++ )
				{
					results[i] = new LobbyMemberData
					{
						lobby = this,
						user  = SteamMatchmaking.GetLobbyMemberByIndex( this, i )
					};
				}

				return results;
			}
		}
		public readonly LobbyMemberData this[ UserData user ] => GetMember( user, out var member )? member: default;
		public readonly Boolean GetMember( UserData user, out LobbyMemberData member )
		{
			var contained = SteamMatchmaking.GetLobbyMemberData( this, user, "anyKey" );

			if( contained == null )
			{
				member = default;

				return false;
			}
			else
			{
				member = new LobbyMemberData { lobby = this, user = user };

				return true;
			}
		}
		public readonly Boolean IsAMember( UserData user ) => GetMember( user, out _ );

		public readonly Boolean SendChatMessage( String message )
		{
			if( String.IsNullOrEmpty( message ) )
				return false;

			var msgBody = System.Text.Encoding.UTF8.GetBytes( message );

			return SteamMatchmaking.SendLobbyChatMsg( this, msgBody, msgBody.Length );
		}
		public readonly Boolean SendChatMessage( Byte[] data )
		{
			if( data == null || data.Length < 1 )
				return false;

			return SteamMatchmaking.SendLobbyChatMsg( this, data, data.Length );
		}
		public readonly Boolean SendChatMessage( Object jsonObject ) => SendChatMessage( System.Text.Encoding.UTF8.GetBytes( JsonUtility.ToJson( jsonObject ) ) );

		public readonly Boolean KickMember( UserData memberId )
		{
			if( !IsOwner )
				return false;

			var kickList = SteamMatchmaking.GetLobbyData( this, LobbyMetadata.KickList );

			if( kickList == null )
				kickList = String.Empty;

			if( !kickList.Contains( "[" + memberId + "]" ) )
				kickList += "[" + memberId + "]";

			return SteamMatchmaking.SetLobbyData( this, LobbyMetadata.KickList, kickList );
		}
		public readonly Boolean KickListContains( UserData memberId )
		{
			var kickList = SteamMatchmaking.GetLobbyData( this, LobbyMetadata.KickList );

			return kickList.Contains( "[" + memberId + "]" );
		}
		public readonly Boolean RemoveFromKickList( UserData memberId )
		{
			if( !IsOwner )
				return false;

			var kickList = SteamMatchmaking.GetLobbyData( this, LobbyMetadata.KickList );

			kickList = kickList.Replace( "[" + memberId + "]", String.Empty );

			return SteamMatchmaking.SetLobbyData( this, LobbyMetadata.KickList, kickList );
		}
		public readonly UserData[] GetKickList( )
		{
			var list = SteamMatchmaking.GetLobbyData( this, LobbyMetadata.KickList );

			if( !String.IsNullOrEmpty( list ) )
			{
				var sArray     = list.Split( new String[] { "][" }, StringSplitOptions.RemoveEmptyEntries );
				var resultList = new List<UserData>( );

				for( var i = 0; i < sArray.Length; i++ )
				{
					var user = UserData.Get( sArray[i].Replace( "[", String.Empty ).Replace( "]", String.Empty ) );

					if( user.IsValid )
						resultList.Add( user );
				}

				return resultList.ToArray( );
			}
			else
				return Array.Empty<UserData>( );
		}
		public readonly Boolean ClearKickList( ) => IsOwner && SteamMatchmaking.DeleteLobbyData( this, LobbyMetadata.KickList );

		public readonly void SetMemberMetadata( String key, String value )
		{
			SteamMatchmaking.SetLobbyMemberData( this, key, value );
		}
		public readonly String GetMemberMetadata( String          key )                  => SteamMatchmaking.GetLobbyMemberData( this, UserData.Me, key );
		public readonly String GetMemberMetadata( UserData        memberId, String key ) => SteamMatchmaking.GetLobbyMemberData( this, memberId, key );
		public readonly String GetMemberMetadata( LobbyMemberData member,   String key ) => SteamMatchmaking.GetLobbyMemberData( this, member.user, key );

		public static LobbyData Get( String accountId )
		{
			var id = Convert.ToUInt32( accountId, 16 );

			if( id > 0 )
				return Get( id );
			else
				return CSteamID.Nil;
		}
		public static LobbyData Get( UInt32      accountId ) => new CSteamID( new AccountID_t( accountId ), 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat );
		public static LobbyData Get( AccountID_t accountId ) => new CSteamID( accountId, 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat );
		public static LobbyData Get( UInt64      id )        => new( ) { _id = id };
		public static LobbyData Get( CSteamID    id )        => new( ) { _id = id.m_SteamID };

		#region IEquatable & IComparable

		public Int32 CompareTo( CSteamID other ) => _id.CompareTo( other );

		public Int32 CompareTo( UInt64 other ) => _id.CompareTo( other );

		public Boolean Equals( CSteamID other ) => _id.Equals( other.m_SteamID );

		public Boolean Equals( UInt64 other ) => _id.Equals( other );

		public override Boolean Equals( Object obj ) => _id.Equals( obj );

		public override Int32 GetHashCode( ) => _id.GetHashCode( );

		public Boolean Equals( LobbyData other ) => _id.Equals( other._id );

		public static Boolean operator==( LobbyData l, LobbyData r ) => l._id == r._id;
		public static Boolean operator==( CSteamID  l, LobbyData r ) => l.m_SteamID == r._id;
		public static Boolean operator==( LobbyData l, CSteamID  r ) => l._id == r.m_SteamID;
		public static Boolean operator==( LobbyData l, UInt64    r ) => l._id == r;
		public static Boolean operator==( UInt64    l, LobbyData r ) => l == r._id;
		public static Boolean operator!=( LobbyData l, LobbyData r ) => l._id != r._id;
		public static Boolean operator!=( CSteamID  l, LobbyData r ) => l.m_SteamID != r._id;
		public static Boolean operator!=( LobbyData l, CSteamID  r ) => l._id != r.m_SteamID;
		public static Boolean operator!=( LobbyData l, UInt64    r ) => l._id != r;
		public static Boolean operator!=( UInt64    l, LobbyData r ) => l != r._id;

		public static implicit operator CSteamID( LobbyData    c )  => c.SteamId;
		public static implicit operator LobbyData( CSteamID    id ) => new( ) { _id = id.m_SteamID };
		public static implicit operator UInt64( LobbyData      id ) => id._id;
		public static implicit operator LobbyData( UInt64      id ) => new( ) { _id = id };
		public static implicit operator LobbyData( AccountID_t id ) => Get( id );
		public static implicit operator LobbyData( UInt32      id ) => Get( id );
		public static implicit operator LobbyData( String      id ) => Get( id );

		#endregion
	}
}
#endif