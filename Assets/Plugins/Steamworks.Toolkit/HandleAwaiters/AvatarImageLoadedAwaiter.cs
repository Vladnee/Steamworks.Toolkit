#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using Steamworks.Toolkit.Facades;

namespace Steamworks.Toolkit.HandleAwaiters
{
	public sealed class AvatarImageLoadedAwaiter : HandleAwaiter<AvatarImageLoadedAwaiter, UserData, AvatarImageLoaded_t, AvatarImageLoaded_t>
	{
		protected override Boolean TryGetHandle( AvatarImageLoaded_t response, out UserData handle )
		{
			handle = response.m_steamID;

			return handle.IsValid;
		}

		protected override void DestroyHandle( UserData handle ) { }

		protected override AvatarImageLoaded_t GetResult( UserData handle, AvatarImageLoaded_t response ) => response;
	}
}
#endif