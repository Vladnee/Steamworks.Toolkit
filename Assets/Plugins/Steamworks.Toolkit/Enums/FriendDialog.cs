#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;

namespace Steamworks.Toolkit.Enums
{
	public static class FriendDialog
	{
		public const String SteamId             = "steamid";
		public const String Chat                = "chat";
		public const String JoinTrade           = "jointrade";
		public const String Stats               = "stats";
		public const String Achievements        = "achievements";
		public const String AddFriend           = "friendadd";
		public const String RemoveFriend        = "friendremove";
		public const String AcceptFriendRequest = "friendrequestaccept";
		public const String IgnoreFriendRequest = "friendrequestignore";
	}
}
#endif