#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Steamworks.Toolkit.HandleAwaiters
{
	public abstract class HandleAwaiter<TSelf, THandle, TResponse, TResult>
		where TSelf : HandleAwaiter<TSelf, THandle, TResponse, TResult>, new( )
		where THandle : struct
		where TResponse : struct
	{
		private static TSelf _instance;
		public static  TSelf Instance => _instance ??= new TSelf( );

		private readonly Dictionary<THandle, List<UniTaskCompletionSource<TResult>>> _waiters    = new( );
		private readonly List<UniTaskCompletionSource<TResult>>                      _anyWaiters = new( );

		protected HandleAwaiter( )
		{
			SteamCallbackObserver<TResponse>.OnTrigger += OnReady;
		}

		public UniTask<TResult> WaitAsync( THandle handle, CancellationToken cancellationToken = default )
		{
			var completionSource = new UniTaskCompletionSource<TResult>( );

			if( !_waiters.TryGetValue( handle, out var list ) )
			{
				list = new List<UniTaskCompletionSource<TResult>>( 1 );
				_waiters.Add( handle, list );
			}

			list.Add( completionSource );

			if( cancellationToken.CanBeCanceled )
			{
				cancellationToken.Register( ( ) =>
					{
						UniTask.Post( ( ) =>
							{
								Cancel( handle, completionSource );
							}
						);
					}
				);
			}

			return completionSource.Task;
		}

		public void WithCallback( THandle handle, Action<TResult> onCompleted, CancellationToken cancellationToken = default )
		{
			WithCallbackAsync( ).Forget( );

			return;

			async UniTask WithCallbackAsync( )
			{
				var result = await WaitAsync( handle, cancellationToken );
				onCompleted?.Invoke( result );
			}
		}

		public UniTask<TResult> WaitAnyAsync( CancellationToken cancellationToken = default )
		{
			var tcs = new UniTaskCompletionSource<TResult>( );

			_anyWaiters.Add( tcs );

			if( cancellationToken.CanBeCanceled )
			{
				cancellationToken.Register( ( ) =>
					{
						UniTask.Post( ( ) =>
							{
								_anyWaiters.Remove( tcs );

								tcs.TrySetCanceled( );
							}
						);
					}
				);
			}

			return tcs.Task;
		}

		public void WithAnyCallback( Action<TResult> onCompleted, CancellationToken cancellationToken = default )
		{
			WithAnyCallbackAsync( ).Forget( );

			return;

			async UniTask WithAnyCallbackAsync( )
			{
				var result = await WaitAnyAsync( cancellationToken );
				onCompleted?.Invoke( result );
			}
		}

		private void OnReady( TResponse response )
		{
			if( !TryGetHandle( response, out var handle ) )
				return;

			_waiters.Remove( handle, out var completionSources );

			try
			{
				var result = GetResult( handle, response );

				if( completionSources != null )
				{
					foreach( var cs in completionSources )
						cs.TrySetResult( result );
				}

				CompleteAnyWaiters( result );
			}
			catch( Exception ex )
			{
				if( completionSources != null )
				{
					foreach( var cs in completionSources )
						cs.TrySetException( ex );
				}

				CompleteAnyWaitersException( ex );
			}
			finally
			{
				DestroyHandle( handle );
			}
		}

		private void Cancel( THandle handle, UniTaskCompletionSource<TResult> completionSource )
		{
			var shouldDestroyHandle = false;

			if( !_waiters.TryGetValue( handle, out var list ) )
				return;

			list.Remove( completionSource );

			if( list.Count == 0 )
			{
				_waiters.Remove( handle );
				shouldDestroyHandle = true;
			}

			completionSource.TrySetCanceled( );

			if( shouldDestroyHandle )
				DestroyHandle( handle );
		}

		private void CompleteAnyWaiters( TResult result )
		{
			if( _anyWaiters.Count == 0 )
				return;

			var waiters = _anyWaiters.ToArray( );
			_anyWaiters.Clear( );

			foreach( var waiter in waiters )
				waiter.TrySetResult( result );
		}

		private void CompleteAnyWaitersException( Exception exception )
		{
			if( _anyWaiters.Count == 0 )
				return;

			var waiters = _anyWaiters.ToArray( );
			_anyWaiters.Clear( );

			foreach( var waiter in waiters )
				waiter.TrySetException( exception );
		}

		protected abstract Boolean TryGetHandle( TResponse response, out THandle handle );
		protected abstract void    DestroyHandle( THandle  handle );
		protected abstract TResult GetResult( THandle      handle, TResponse response );
	}
}
#endif