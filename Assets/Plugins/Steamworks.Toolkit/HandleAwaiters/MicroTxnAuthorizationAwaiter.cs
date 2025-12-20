#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;

namespace Steamworks.Toolkit.HandleAwaiters
{
	public sealed class MicroTxnAuthorizationAwaiter : HandleAwaiter<MicroTxnAuthorizationAwaiter, UInt64, MicroTxnAuthorizationResponse_t, MicroTxnAuthorizationResponse_t>
	{
		protected override Boolean TryGetHandle( MicroTxnAuthorizationResponse_t response, out UInt64 handle )
		{
			handle = response.m_ulOrderID;

			return handle != 0;
		}

		protected override void DestroyHandle( UInt64 handle ) { }

		protected override MicroTxnAuthorizationResponse_t GetResult( UInt64 handle, MicroTxnAuthorizationResponse_t response ) => response;
	}
}
#endif