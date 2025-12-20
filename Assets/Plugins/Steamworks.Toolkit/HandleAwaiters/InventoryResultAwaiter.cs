#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Steamworks.Toolkit.Facades;

namespace Steamworks.Toolkit.HandleAwaiters
{
	public sealed class InventoryResultAwaiter : HandleAwaiter<InventoryResultAwaiter, SteamInventoryResult_t, SteamInventoryResultReady_t, InventoryResult>
	{
		protected override Boolean TryGetHandle( SteamInventoryResultReady_t response, out SteamInventoryResult_t handle )
		{
			handle = response.m_handle;

			return true;
		}

		protected override void DestroyHandle( SteamInventoryResult_t handle )
		{
			SteamInventory.DestroyResult( handle );
		}

		protected override InventoryResult GetResult( SteamInventoryResult_t handle, SteamInventoryResultReady_t response )
		{
			var timestamp = DateTime.UnixEpoch.AddSeconds( SteamInventory.GetResultTimestamp( handle ) );

			UInt32 count = 0;
			SteamInventory.GetResultItems( handle, null, ref count );

			var items = Array.Empty<InventoryItem>( );

			if( count > 0 )
			{
				var buffer = new SteamItemDetails_t[count];

				SteamInventory.GetResultItems( handle, buffer, ref count );

				items = new InventoryItem[count];

				for( UInt32 i = 0; i < count; i++ )
					items[i] = InventoryItem.GetFromInventoryResult( handle, i, buffer[i] );
			}

			return new InventoryResult
			{
				Items     = items,
				Result    = response.m_result,
				Timestamp = timestamp
			};
		}
	}

	public static class SteamInventoryResultExtensions
	{
		public static UniTask<InventoryResult> ToUniTask( this SteamInventoryResult_t handle, CancellationToken cancellationToken = default )
			=> InventoryResultAwaiter.Instance.WaitAsync( handle, cancellationToken );

		public static void WithCallback( this SteamInventoryResult_t handle, Action<InventoryResult> onCompleted, CancellationToken cancellationToken = default )
			=> InventoryResultAwaiter.Instance.WithCallback( handle, onCompleted, cancellationToken );
	}
}
#endif