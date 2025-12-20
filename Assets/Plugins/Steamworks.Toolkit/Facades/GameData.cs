#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using UnityEngine;
using Object = System.Object;

namespace Steamworks.Toolkit.Facades
{
	[Serializable]
	public struct GameData : IEquatable<AppId_t>, IEquatable<CGameID>, IEquatable<UInt32>, IEquatable<UInt64>, IComparable<AppId_t>, IComparable<UInt32>, IComparable<UInt64>
	{
		[SerializeField]
		private UInt64 _id;

		public static GameData This => SteamUtils.GetAppID( );

		public readonly CGameID GameId => new(_id);

		public readonly UInt64 Id => GameId.m_GameID;

		public readonly AppId_t AppId => GameId.AppID( );

		public readonly Boolean IsThis => this == This;

		public static GameData Get( CGameID gameId ) => gameId;

		public static GameData Get( UInt64 gameId ) => gameId;

		public static GameData Get( UInt32 appId ) => appId;

		public static GameData Get( AppId_t appId ) => appId;

		#region IEquatable & IComparable

		public readonly Int32 CompareTo( GameData other ) => Id.CompareTo( other.Id );

		public readonly Int32 CompareTo( AppId_t other ) => AppId.CompareTo( other );

		public readonly Int32 CompareTo( UInt64 other ) => GameId.m_GameID.CompareTo( other );

		public readonly Int32 CompareTo( UInt32 other ) => AppId.m_AppId.CompareTo( other );

		public readonly override String ToString( ) => GameId.ToString( );

		public readonly Boolean Equals( GameData other ) => Id.Equals( other.Id );

		public readonly Boolean Equals( AppId_t other ) => AppId.Equals( other );

		public readonly Boolean Equals( UInt32 other ) => AppId.m_AppId.Equals( other );

		public readonly Boolean Equals( CGameID other ) => GameId.AppID( ).Equals( other.AppID( ) );

		public readonly Boolean Equals( UInt64 other ) => GameId.AppID( ).Equals( new CGameID( other ).AppID( ) );

		public readonly override Boolean Equals( Object obj ) => GameId.m_GameID.Equals( obj );

		public readonly override Int32 GetHashCode( ) => GameId.GetHashCode( );

		public static Boolean operator==( GameData l, GameData r ) => l.GameId.m_GameID == r.GameId.m_GameID;
		public static Boolean operator==( AppId_t  l, GameData r ) => l == r.AppId;
		public static Boolean operator==( GameData l, AppId_t  r ) => l.AppId == r;
		public static Boolean operator!=( GameData l, GameData r ) => l.GameId.m_GameID != r.GameId.m_GameID;
		public static Boolean operator!=( AppId_t  l, GameData r ) => l != r.AppId;
		public static Boolean operator!=( GameData l, AppId_t  r ) => l.AppId != r;

		public static implicit operator GameData( CGameID id ) => new( ) { _id = id.m_GameID };
		public static implicit operator UInt32( GameData  c )  => c.AppId.m_AppId;
		public static implicit operator UInt64( GameData  c )  => c.Id;
		public static implicit operator GameData( UInt64  id ) => new( ) { _id = id };
		public static implicit operator GameData( UInt32  id ) => new( ) { _id = new CGameID( new AppId_t( id ) ).m_GameID };
		public static implicit operator AppId_t( GameData c )  => c.AppId;
		public static implicit operator GameData( AppId_t id ) => new CGameID( id );

		#endregion
	}
}
#endif