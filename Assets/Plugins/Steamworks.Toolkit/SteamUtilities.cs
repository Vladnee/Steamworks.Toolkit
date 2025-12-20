#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Cysharp.Threading.Tasks;
using Steamworks.Toolkit.Enums;
using Steamworks.Toolkit.Facades;
using Steamworks.Toolkit.HandleAwaiters;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
#if ANIMATED_IMAGES
using HyTeKGames.AnimatedImages;
#endif

namespace Steamworks.Toolkit
{
	public static class SteamUtilities
	{
		#if ANIMATED_IMAGES
		private static readonly Dictionary<ECommunityProfileItemType, Int32> _maxCachePerFormat = new( )
		{
			{ ECommunityProfileItemType.k_ECommunityProfileItemType_AnimatedAvatar, 20 },
			{ ECommunityProfileItemType.k_ECommunityProfileItemType_AvatarFrame, 10 }
		};
		private static readonly Dictionary<ECommunityProfileItemType, LinkedList<String>>                    _animatedImagesCacheOrders   = new( );
		private static readonly Dictionary<ECommunityProfileItemType, Dictionary<String, AnimatedImageData>> _animatedImagesLoaded        = new( );
		private static readonly Dictionary<String, UniTaskCompletionSource<AnimatedImageData>>               _activeDownloadImageRequests = new( );

		public static async UniTask<AnimatedImageData> GetAnimatedImage( ECommunityProfileItemType type, String imageUrl, CancellationToken cancellationToken = default, Action<AnimatedImageData> callback = null )
		{
			if( String.IsNullOrEmpty( imageUrl ) )
				return null;

			if( _animatedImagesLoaded.TryGetValue( type, out var datas ) )
			{
				if( datas.TryGetValue( imageUrl, out var imageData ) )
				{
					callback?.Invoke( imageData );

					return null;
				}
			}

			var animatedImageData = await DownloadAnimatedImageRequest( type, imageUrl );

			if( animatedImageData == null || cancellationToken.IsCancellationRequested )
				return null;

			callback?.Invoke( animatedImageData );

			return animatedImageData;
		}

		private static async UniTask<AnimatedImageData> DownloadAnimatedImageRequest( ECommunityProfileItemType type, String imageUrl )
		{
			if( _activeDownloadImageRequests.TryGetValue( imageUrl, out var existingSource ) )
				return await existingSource.Task;

			var source = new UniTaskCompletionSource<AnimatedImageData>( );
			_activeDownloadImageRequests[imageUrl] = source;

			try
			{
				using var request = UnityWebRequest.Get( imageUrl );

				await request.SendWebRequest( );

				if( request.result != UnityWebRequest.Result.Success )
				{
					source.TrySetResult( null );

					return null;
				}

				var data = AnimatedImageLoader.LoadFromBytes( request.downloadHandler.data );

				if( !_animatedImagesLoaded.TryGetValue( type, out var animatedImages ) )
				{
					animatedImages                   = new Dictionary<String, AnimatedImageData>( );
					_animatedImagesLoaded[type]      = animatedImages;
					_animatedImagesCacheOrders[type] = new LinkedList<String>( );
				}

				var order = _animatedImagesCacheOrders[type];

				if( animatedImages.ContainsKey( imageUrl ) )
				{
					// remove old record to update
					order.Remove( imageUrl );
				}
				else if( animatedImages.Count >= _maxCachePerFormat[type] )
				{
					var oldest = order.First.Value;
					order.RemoveFirst( );
					animatedImages.Remove( oldest );
				}

				animatedImages[imageUrl] = data;
				order.AddLast( imageUrl );

				source.TrySetResult( data );

				return data;
			}
			catch( Exception e )
			{
				source.TrySetException( e );

				return null;
			}
			finally
			{
				_activeDownloadImageRequests.Remove( imageUrl );
			}
		}

		#endif

		private static readonly Dictionary<Int32, Texture2D> _loadedAvatars = new( );

		public static async UniTask<Texture2D> GetAvatar( EUserAvatarSize avatarSize, UserData user, CancellationToken cancellationToken = default, Action<Texture2D> callback = null )
		{
			if( !SteamFriends.RequestUserInformation( user, false ) )
				return await RequestAvatar( );

			{
				using var timeoutCts = new CancellationTokenSource( TimeSpan.FromSeconds( 10 ) );
				using var linkedCts  = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken, timeoutCts.Token );

				try
				{
					var personaStateChange = await PersonaStateChangeAwaiter.Instance.WaitAsync( user, linkedCts.Token );

					if( personaStateChange.m_nChangeFlags is EPersonaChange.k_EPersonaChangeAvatar )
						return await RequestAvatar( );
				}
				catch( OperationCanceledException ) { }
			}

			callback?.Invoke( null );

			return null;

			async UniTask<Texture2D> RequestAvatar( )
			{
				var avatarHandle = avatarSize switch
				{
					EUserAvatarSize.Small  => SteamFriends.GetSmallFriendAvatar( user ),
					EUserAvatarSize.Medium => SteamFriends.GetMediumFriendAvatar( user ),
					EUserAvatarSize.Large  => SteamFriends.GetLargeFriendAvatar( user ),
					_                      => 0
				};

				if( avatarHandle > 0 )
				{
					if( _loadedAvatars.TryGetValue( avatarHandle, out var avatar ) || DownloadAvatar( avatarHandle, out avatar ) )
					{
						callback?.Invoke( avatar );

						return avatar;
					}

					Debug.LogWarning( $"[{nameof(SteamUtilities)}] - {nameof(GetAvatar)}: Failed to load the requested avatar" );
					callback?.Invoke( null );

					return null;
				}

				using var timeoutCts = new CancellationTokenSource( TimeSpan.FromSeconds( 10 ) );
				using var linkedCts  = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken, timeoutCts.Token );

				try
				{
					var avatarLoaded = await AvatarImageLoadedAwaiter.Instance.WaitAsync( user, linkedCts.Token );

					if( DownloadAvatar( avatarLoaded.m_iImage, out var avatar ) )
					{
						callback?.Invoke( avatar );

						return avatar;
					}
				}
				catch( OperationCanceledException ) { }

				callback?.Invoke( null );

				return null;
			}
		}
		private static Boolean DownloadAvatar( Int32 imageHandle, out Texture2D newAvatar )
		{
			newAvatar = null;

			if( imageHandle <= 0 || !SteamUtils.GetImageSize( imageHandle, out var width, out var height ) || width <= 0 || height <= 0 )
				return false;

			var bufferSize  = width * height * 4;
			var imageBuffer = new Byte[bufferSize];

			if( !SteamUtils.GetImageRGBA( imageHandle, imageBuffer, (Int32)bufferSize ) )
				return false;

			newAvatar = new Texture2D( (Int32)width, (Int32)height, TextureFormat.RGBA32, false );
			newAvatar.LoadRawTextureData( FlipImageBufferVertical( width, height, imageBuffer ) );
			newAvatar.Apply( );

			if( _loadedAvatars.TryGetValue( imageHandle, out var cachedAvatar ) && cachedAvatar != null )
				Object.Destroy( cachedAvatar );

			_loadedAvatars[imageHandle] = newAvatar;

			return true;
		}

		public static UInt32 IPStringToUint( String address )
		{
			var ipBytes = IPStringToBytes( address );
			var ip      = (UInt32)ipBytes[0] << 24;
			ip += (UInt32)ipBytes[1] << 16;
			ip += (UInt32)ipBytes[2] << 8;
			ip += ipBytes[3];

			return ip;
		}

		public static String IPUintToString( UInt32 address )
		{
			var ipBytes       = BitConverter.GetBytes( address );
			var ipBytesRevert = new Byte[4];
			ipBytesRevert[0] = ipBytes[3];
			ipBytesRevert[1] = ipBytes[2];
			ipBytesRevert[2] = ipBytes[1];
			ipBytesRevert[3] = ipBytes[0];

			return new IPAddress( ipBytesRevert ).ToString( );
		}

		public static Byte[] IPStringToBytes( String address )
		{
			var ipAddress = IPAddress.Parse( address );

			return ipAddress.GetAddressBytes( );
		}

		public static Byte[] FlipImageBufferVertical( UInt32 width, UInt32 height, Byte[] buffer )
		{
			var result = new Byte[buffer.Length];

			var xWidth  = width * 4;
			var yHeight = height;

			for( var y = 0; y < yHeight; y++ )
			{
				for( var x = 0; x < xWidth; x++ )
					result[x + ( yHeight - 1 - y ) * xWidth] = buffer[x + xWidth * y];
			}

			return result;
		}
	}
}
#endif