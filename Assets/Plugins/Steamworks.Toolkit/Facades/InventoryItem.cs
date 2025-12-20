#if !DISABLESTEAMWORKS && STEAMWORKS_NET

using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Steamworks.Toolkit.Enums;

namespace Steamworks.Toolkit.Facades
{
	[Serializable]
	public struct InventoryItem
	{
		public SteamItemDetails_t ItemDetails;
		public Int64              OriginalItemId;
		public Property[]         Properties;
		public Property[]         DynamicProperties;
		public Tag[]              Tags;

		public Boolean               IsValid   => ItemId.m_SteamItemInstanceID != 0;
		public SteamItemInstanceID_t ItemId    => ItemDetails.m_itemId;
		public Int32                 ItemDefId => ItemDetails.m_iDefinition.m_SteamItemDef;
		public UInt16                Quantity  => ItemDetails.m_unQuantity;
		public ESteamItemFlags       Flags     => (ESteamItemFlags)ItemDetails.m_unFlags;

		public DateTime AcquiredTime
		{
			get
			{
				var styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

				return DateTime.TryParseExact( GetProperty( "acquired" ), "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture, styles, out var time )? time: default;
			}
		}

		public String GetFormattedPrice( Int32 quantity = 1 )
		{
			if( quantity <= 0 || !SteamInventory.GetItemPrice( ItemDetails.m_iDefinition, out var currentPrice, out _ ) )
				return 0.0.ToString( "C0", CurrencyCode.CurrencyFormat );

			var totalCents = currentPrice * (UInt64)quantity;
			var price      = totalCents / 100.0;

			return price.ToString( price % 1 == 0? "C0": "C2", CurrencyCode.CurrencyFormat );
		}

		public Boolean TryGetTradableRestriction( out DateTime dateTime )
		{
			var value  = GetProperty( "tradable_after_timestamp" );
			var styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

			dateTime = default;

			return !String.IsNullOrEmpty( value ) && DateTime.TryParseExact( value, "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture, styles, out dateTime );
		}

		public String GetProperty( String        key ) => GetPropertyValue( Properties, key );
		public String GetDynamicProperty( String key ) => GetPropertyValue( DynamicProperties, key );

		public static InventoryItem GetFromInventoryResult( SteamInventoryResult_t result, UInt32 index, SteamItemDetails_t detail )
		{
			var names = GetResultPropertyNames( result, index );

			var props = names.Where( n => n != "tags" && n != "dynamic_props" ).Select( n => new Property
				{
					Key   = n,
					Value = GetResultProperty( result, index, n )
				}
			).ToArray( );

			var originalItemId = Int64.TryParse( props.FirstOrDefault( p => p.Key == "originalitemid" ).Value, out var oid )? oid: 0;

			var tagsRaw         = GetResultProperty( result, index, "tags" );
			var dynamicPropsRaw = GetResultProperty( result, index, "dynamic_props" );

			return new InventoryItem
			{
				ItemDetails       = detail,
				Properties        = props,
				DynamicProperties = ParseDynamicProperties( dynamicPropsRaw ),
				Tags              = ParseTags( tagsRaw ),
				OriginalItemId    = originalItemId
			};
		}

		public static InventoryItem FromJson( JObject jObject )
		{
			var dynamicPropsToken = jObject.TryGetValue( "dynamic_props", out var dyn )
				? dyn as JObject
				: null;

			var properties = jObject.Properties( ).Where( p => p.Name != "tags" && p.Name != "dynamic_props" ).Select( p => new Property
				{
					Key   = p.Name,
					Value = p.Value.ToString( )
				}
			).ToArray( );

			var originalItemId = jObject.TryGetValue( "originalitemid", out var oidToken ) && Int64.TryParse( oidToken.ToString( ), out var oid )? oid: 0;

			return new InventoryItem
			{
				ItemDetails = new SteamItemDetails_t
				{
					m_itemId      = new SteamItemInstanceID_t( UInt64.Parse( jObject["itemid"].ToString( ) ) ),
					m_iDefinition = new SteamItemDef_t( Int32.Parse( jObject["itemdefid"].ToString( ) ) ),
					m_unQuantity  = UInt16.Parse( jObject["quantity"].ToString( ) )
				},

				Properties        = properties,
				DynamicProperties = ParseDynamicProperties( dynamicPropsToken ),
				Tags              = ParseTags( jObject.TryGetValue( "tags", out var tags )? tags.ToString( ): String.Empty ),
				OriginalItemId    = originalItemId
			};
		}

		private static String GetPropertyValue( Property[] properties, String key )
		{
			if( properties == null || properties.Length == 0 || String.IsNullOrEmpty( key ) )
				return String.Empty;

			for( var i = 0; i < properties.Length; i++ )
			{
				if( properties[i].Key == key )
					return properties[i].Value ?? String.Empty;
			}

			return String.Empty;
		}

		private static String[] GetResultPropertyNames( SteamInventoryResult_t result, UInt32 index )
		{
			UInt32 size = 0;
			SteamInventory.GetResultItemProperty( result, index, null, out var value, ref size );
			SteamInventory.GetResultItemProperty( result, index, null, out value, ref size );

			return String.IsNullOrEmpty( value )? Array.Empty<String>( ): value.Split( ',' );
		}

		private static String GetResultProperty( SteamInventoryResult_t result, UInt32 index, String name )
		{
			UInt32 size = 0;
			SteamInventory.GetResultItemProperty( result, index, name, out _, ref size );
			SteamInventory.GetResultItemProperty( result, index, name, out var value, ref size );

			return value ?? String.Empty;
		}

		private static Property[] ParseDynamicProperties( String json )
		{
			if( String.IsNullOrEmpty( json ) )
				return Array.Empty<Property>( );

			try
			{
				return ParseDynamicProperties( JObject.Parse( json ) );
			}
			catch
			{
				return Array.Empty<Property>( );
			}
		}

		private static Property[] ParseDynamicProperties( JObject obj )
		{
			if( obj == null )
				return Array.Empty<Property>( );

			return obj.Properties( ).Select( p => new Property
				{
					Key   = p.Name,
					Value = p.Value.ToString( )
				}
			).ToArray( );
		}

		private static Tag[] ParseTags( String rawTags )
		{
			if( String.IsNullOrEmpty( rawTags ) )
				return Array.Empty<Tag>( );

			return rawTags.Split( ';' ).Select( t =>
				{
					var idx = t.IndexOf( ':' );

					return idx < 0
						? new Tag { Category = t, Name = String.Empty }
						: new Tag
						{
							Category = t[..idx],
							Name     = t[( idx + 1 )..]
						};
				}
			).ToArray( );
		}


		[Serializable]
		public struct Property
		{
			public String Key;
			public String Value;
		}

		[Serializable]
		public struct Tag
		{
			public String Category;
			public String Name;

			public override String ToString( ) => Category + ":" + Name;
		}
	}
}
#endif