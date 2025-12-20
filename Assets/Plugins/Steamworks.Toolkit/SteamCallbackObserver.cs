#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Steamworks.Toolkit
{
	public static class SteamCallbackObserver<T> where T : struct
	{
		private static readonly List<Action<T>> _handlers       = new( );
		private static readonly List<Action<T>> _handlersCached = new( );

		private static Callback<T> _callback;

		public static event Action<T> OnTrigger
		{
			add
			{
				if( value == null )
					return;

				_handlers.Add( value );

				_callback ??= Callback<T>.Create( Trigger );
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

				if( _handlers.Count != 0 )
					return;

				_callback?.Dispose( );
				_callback = null;
			}
		}

		public static UniTask<T> WaitAny( CancellationToken cancellationToken = default )
		{
			var completionSource = new UniTaskCompletionSource<T>( );

			Action<T> handler = null;

			handler = value =>
			{
				OnTrigger -= handler;
				completionSource.TrySetResult( value );
			};

			OnTrigger += handler;

			if( cancellationToken.CanBeCanceled )
			{
				cancellationToken.Register( ( ) =>
					{
						UniTask.Post( ( ) =>
							{
								OnTrigger -= handler;
								completionSource.TrySetCanceled( );
							}
						);
					}
				);
			}

			return completionSource.Task;
		}

		private static void Trigger( T param )
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
					handler?.Invoke( param );
				}
				catch( Exception ex )
				{
					Debug.LogException( ex );
				}
			}
		}
	}
}
#endif