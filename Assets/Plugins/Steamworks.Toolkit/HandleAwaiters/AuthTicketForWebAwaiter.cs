#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Steamworks.Toolkit.HandleAwaiters
{
	public class AuthTicketForWebAwaiter : HandleAwaiter<AuthTicketForWebAwaiter, HAuthTicket, GetTicketForWebApiResponse_t, String>
	{
		protected override Boolean TryGetHandle( GetTicketForWebApiResponse_t response, out HAuthTicket handle )
		{
			handle = response.m_hAuthTicket;

			return true;
		}
		protected override void DestroyHandle( HAuthTicket handle ) { }

		protected override String GetResult( HAuthTicket handle, GetTicketForWebApiResponse_t response )
		{
			if( handle == default || handle == HAuthTicket.Invalid || response.m_eResult != EResult.k_EResultOK )
				return null;

			var data = new Byte[response.m_cubTicket];
			Array.Copy( response.m_rgubTicket, data, response.m_cubTicket );

			return BitConverter.ToString( data ).Replace( "-", "" );
		}
	}

	public static class HAuthTicketExtensions
	{
		public static UniTask<String> ToUniTask( this HAuthTicket handle, CancellationToken cancellationToken = default )
			=> AuthTicketForWebAwaiter.Instance.WaitAsync( handle, cancellationToken );

		public static void WithCallback( this HAuthTicket handle, Action<String> onCompleted, CancellationToken cancellationToken = default )
			=> AuthTicketForWebAwaiter.Instance.WithCallback( handle, onCompleted, cancellationToken );

		public static void Cancel( this HAuthTicket handle )
		{
			if( handle == default || handle == HAuthTicket.Invalid )
				return;

			SteamUser.CancelAuthTicket( handle );
		}
	}
}
#endif