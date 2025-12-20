#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Linq;

namespace Steamworks.Toolkit.Facades
{
	public struct InventoryResult
	{
		public InventoryItem[] Items;
		public EResult         Result;
		public DateTime        Timestamp;

		public Int64 GetInventoryXorHash( )
		{
			return Items?.Aggregate<InventoryItem, Int64>( 0, ( current, item ) => current ^ item.OriginalItemId ) ?? 0;
		}
	}
}
#endif