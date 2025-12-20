#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using Steamworks.Toolkit.Facades;

namespace Steamworks.Toolkit.HandleAwaiters
{
	public sealed class PersonaStateChangeAwaiter : HandleAwaiter<PersonaStateChangeAwaiter, UserData, PersonaStateChange_t, PersonaStateChange_t>
	{
		protected override Boolean TryGetHandle( PersonaStateChange_t response, out UserData handle )
		{
			handle = response.m_ulSteamID;

			return handle.IsValid;
		}

		protected override void DestroyHandle( UserData handle ) { }

		protected override PersonaStateChange_t GetResult( UserData handle, PersonaStateChange_t response ) => response;
	}
}
#endif