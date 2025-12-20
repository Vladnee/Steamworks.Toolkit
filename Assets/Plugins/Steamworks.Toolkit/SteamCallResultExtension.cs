#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Steamworks.Toolkit
{
	public static class SteamCallResultObserver<T> where T : struct
	{
		private static readonly List<Action<T, Boolean>> _handlers       = new( );
		private static readonly List<Action<T, Boolean>> _handlersCached = new( );

		public static event Action<T, Boolean> OnTrigger
		{
			add
			{
				if( value == null )
					return;

				_handlers.Add( value );
			}
			remove
			{
				if( value == null )
					return;

				_handlers.Remove( value );

				try
				{
					_handlers.RemoveAll( x => x == null || ( x.Target is UnityEngine.Object uo && uo == null ) );
				}
				catch( Exception ex )
				{
					Debug.LogException( ex );
				}
			}
		}

		internal static void Trigger( T result, Boolean ioError )
		{
			if( _handlers.Count == 0 )
				return;

			_handlersCached.Clear( );

			foreach( var handler in _handlers )
			{
				if( handler == null )
					continue;

				if( handler.Target is UnityEngine.Object uo && uo == null )
					continue;

				_handlersCached.Add( handler );
			}

			foreach( var handler in _handlersCached )
			{
				try
				{
					handler?.Invoke( result, ioError );
				}
				catch( Exception ex )
				{
					Debug.LogException( ex );
				}
			}
		}
	}

	public static class SteamCallResultExtension
	{
		private sealed class CallState<T> where T : struct
		{
			public CallResult<T> CallResult;

			public void Dispose( )
			{
				CallResult?.Dispose( );
				CallResult = null;
			}
		}

		public static UniTask<(T result, Boolean ioError)> ToUniTask<T>( this SteamAPICall_t apiCall, CancellationToken cancellationToken = default ) where T : struct
		{
			if( apiCall == SteamAPICall_t.Invalid )
				return UniTask.FromException<(T result, Boolean ioError)>( new InvalidOperationException( "SteamAPICall_t is invalid." ) );

			var completionSource = new UniTaskCompletionSource<(T result, Boolean ioError)>( );
			var state            = new CallState<T>( );

			state.CallResult =
				CallResult<T>.Create( ( result, ioError ) =>
					{
						state.Dispose( );

						SteamCallResultObserver<T>.Trigger( result, ioError );

						completionSource.TrySetResult( ( result, ioError ) );
					}
				);

			state.CallResult.Set( apiCall );

			if( cancellationToken.CanBeCanceled )
			{
				cancellationToken.Register( ( ) =>
					{
						UniTask.Post( ( ) =>
							{
								state.Dispose( );
								completionSource.TrySetCanceled( );
							}
						);
					}
				);
			}

			return completionSource.Task;
		}

		public static void Observe<T>( this SteamAPICall_t apiCall ) where T : struct
		{
			if( apiCall == SteamAPICall_t.Invalid )
				return;

			var state = new CallState<T>( );

			state.CallResult =
				CallResult<T>.Create( ( result, ioError ) =>
					{
						state.Dispose( );

						SteamCallResultObserver<T>.Trigger( result, ioError );
					}
				);

			state.CallResult.Set( apiCall );
		}

		public static void WithCallback<T>( this SteamAPICall_t apiCall, Action<T, Boolean> onCompleted, CancellationToken cancellationToken = default ) where T : struct
		{
			RunWithCallbackAsync( ).Forget( );

			return;

			async UniTask RunWithCallbackAsync( )
			{
				var (result, ioError) = await apiCall.ToUniTask<T>( cancellationToken );
				onCompleted?.Invoke( result, ioError );
			}
		}
	}
}
#endif