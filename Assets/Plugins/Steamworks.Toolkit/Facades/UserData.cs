#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using Object = System.Object;

namespace Steamworks.Toolkit.Facades
{
	[Serializable]
	public struct UserData : IEquatable<CSteamID>, IEquatable<UInt64>, IEquatable<UserData>
	{
		public static UserData Me => SteamUser.GetSteamID( );

		public CSteamID id;

		public readonly Boolean IsMe => id == Me;

		public readonly UInt64 SteamId => id.m_SteamID;

		public Boolean IsValid => id != CSteamID.Nil && id.GetEAccountType( ) == EAccountType.k_EAccountTypeIndividual && id.GetEUniverse( ) == EUniverse.k_EUniversePublic;

		public readonly String Name => SteamFriends.GetFriendPersonaName( id );

		public readonly String Nickname
		{
			get
			{
				var value = SteamFriends.GetPlayerNickname( id );

				return !String.IsNullOrEmpty( value )? value: Name;
			}
		}

		public readonly EPersonaState State => SteamFriends.GetFriendPersonaState( id );

		public readonly Boolean InGame => SteamFriends.GetFriendGamePlayed( id, out _ );

		public readonly Boolean InThisGame => SteamFriends.GetFriendGamePlayed( id, out var friendInfo ) && GameData.Get( friendInfo.m_gameID ).IsThis;

		public readonly FriendGameInfo_t GameInfo
		{
			get
			{
				SteamFriends.GetFriendGamePlayed( id, out var result );

				return result;
			}
		}

		public AccountID_t AccountId => id.GetAccountID( );

		public readonly String HexId => AccountId.m_AccountID.ToString( "X" );

		public readonly Boolean GetGamePlayed( out FriendGameInfo gameInfo )
		{
			var response = SteamFriends.GetFriendGamePlayed( id, out var native );
			gameInfo = native;

			return response;
		}

		public static UserData Get( String accountId )
		{
			var id = Convert.ToUInt32( accountId, 16 );

			if( id > 0 )
				return Get( id );
			else
				return CSteamID.Nil;
		}
		public static UserData Get( UInt64      id )        => new( ) { id = new CSteamID( id ) };
		public static UserData Get( CSteamID    id )        => new( ) { id = id };
		public static UserData Get( UInt32      accountId ) => Get( new AccountID_t( accountId ) );
		public static UserData Get( AccountID_t accountId ) => new CSteamID( accountId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeIndividual );

		#region IEquatable & IComparable

		public readonly Int32 CompareTo( UserData other ) => id.CompareTo( other.id );

		public readonly Int32 CompareTo( CSteamID other ) => id.CompareTo( other );

		public readonly Int32 CompareTo( UInt64 other ) => id.m_SteamID.CompareTo( other );

		public readonly override String ToString( ) => HexId;

		public readonly Boolean Equals( UserData other ) => id.Equals( other.id );

		public readonly Boolean Equals( CSteamID other ) => id.Equals( other );

		public readonly Boolean Equals( UInt64 other ) => id.m_SteamID.Equals( other );

		public readonly override Boolean Equals( Object obj ) => id.m_SteamID.Equals( obj );

		public readonly override Int32 GetHashCode( ) => id.GetHashCode( );

		public static Boolean operator==( UserData l, UserData r ) => l.id == r.id;
		public static Boolean operator==( CSteamID l, UserData r ) => l == r.id;
		public static Boolean operator==( UserData l, CSteamID r ) => l.id == r;
		public static Boolean operator!=( UserData l, UserData r ) => l.id != r.id;
		public static Boolean operator!=( CSteamID l, UserData r ) => l != r.id;
		public static Boolean operator!=( UserData l, CSteamID r ) => l.id != r;

		public static implicit operator UInt64( UserData   c )  => c.id.m_SteamID;
		public static implicit operator UserData( UInt64   id ) => new( ) { id = new CSteamID( id ) };
		public static implicit operator CSteamID( UserData c )  => c.id;
		public static implicit operator UserData( CSteamID id ) => new( ) { id = id };

		#endregion
	}
}
#endif