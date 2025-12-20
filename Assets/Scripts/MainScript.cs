using System;
using Cysharp.Threading.Tasks;
using Steamworks;
using Steamworks.Toolkit;
using Steamworks.Toolkit.Enums;
using Steamworks.Toolkit.Facades;
using Steamworks.Toolkit.HandleAwaiters;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainScript : MonoBehaviour
{
	[SerializeField]
	private UserData _userData;

	[SerializeField]
	private TextMeshProUGUI _nameTitle;

	[SerializeField]
	private RawImage _staticAvatar;
	[SerializeField]
	private RawImage _animatedAvatar;
	[SerializeField]
	private RawImage _frame;
	[SerializeField]
	private VideoPlayer _profileBackground;

	private void Start( )
	{
		if( _profileBackground && !_profileBackground.targetTexture )
		{
			_profileBackground.targetTexture = new RenderTexture( 100, 100, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D16_UNorm );

			RenderTexture.active = _profileBackground.targetTexture;
			GL.Clear( true, true, Color.clear );
			RenderTexture.active = null;

			_profileBackground.GetComponent<RawImage>( ).texture = _profileBackground.targetTexture;
		}

		SetupUserCard( ).Forget( );
	}

	private async UniTaskVoid SetupUserCard( )
	{
		var userData = _userData.IsValid? _userData: UserData.Me;

		if( SteamFriends.RequestUserInformation( userData, false ) )
			await PersonaStateChangeAwaiter.Instance.WaitAsync( userData );

		_nameTitle.text = userData.Name;

		var (result, ioError) = await SteamFriends.RequestEquippedProfileItems( userData ).ToUniTask<EquippedProfileItems_t>( );

		if( ioError )
		{
			Debug.LogError( $"[{nameof(MainScript)}] - {nameof(SetupUserCard)}: RequestEquippedProfileItems failed." );

			return;
		}

		_staticAvatar.gameObject.SetActive( false );
		_animatedAvatar.gameObject.SetActive( false );

		#if ANIMATED_IMAGES
		if( result.m_bHasAnimatedAvatar )
		{
			_animatedAvatar.gameObject.SetActive( true );

			var newAnimatedAvatarUrl = SteamFriends.GetProfileItemPropertyString( userData, ECommunityProfileItemType.k_ECommunityProfileItemType_AnimatedAvatar, ECommunityProfileItemProperty.k_ECommunityProfileItemProperty_ImageSmall );

			var animatedImageData = await SteamUtilities.GetAnimatedImage( ECommunityProfileItemType.k_ECommunityProfileItemType_AnimatedAvatar, newAnimatedAvatarUrl );

			var animatedAvatarImage = _animatedAvatar.GetOrAddComponent<HyTeKGames.AnimatedImages.AnimatedImage>( );
			animatedAvatarImage.SetImageData( animatedImageData );
			animatedAvatarImage.Play( );
		}
		else
			#endif
		{
			_staticAvatar.gameObject.SetActive( true );

			var avatar = await SteamUtilities.GetAvatar( EUserAvatarSize.Large, userData );
			avatar.wrapMode       = TextureWrapMode.Clamp;
			_staticAvatar.texture = avatar;
		}

		_frame.gameObject.SetActive( false );

		#if ANIMATED_IMAGES
		if( result.m_bHasAvatarFrame )
		{
			_frame.gameObject.SetActive( true );
			var newAvatarFrameUrl = SteamFriends.GetProfileItemPropertyString( userData, ECommunityProfileItemType.k_ECommunityProfileItemType_AvatarFrame, ECommunityProfileItemProperty.k_ECommunityProfileItemProperty_ImageSmall );

			var animatedImageData   = await SteamUtilities.GetAnimatedImage( ECommunityProfileItemType.k_ECommunityProfileItemType_AvatarFrame, newAvatarFrameUrl );
			var animatedAvatarImage = _frame.GetOrAddComponent<HyTeKGames.AnimatedImages.AnimatedImage>( );
			animatedAvatarImage.SetImageData( animatedImageData );
			animatedAvatarImage.Play( );
		}
		#endif

		_profileBackground.gameObject.SetActive( result.m_bHasMiniProfileBackground );

		if( result.m_bHasMiniProfileBackground )
		{
			RenderTexture.active = _profileBackground.targetTexture;
			GL.Clear( true, true, Color.clear );
			RenderTexture.active = null;

			var miniProfileBack = SteamFriends.GetProfileItemPropertyString( userData, ECommunityProfileItemType.k_ECommunityProfileItemType_MiniProfileBackground, ECommunityProfileItemProperty.k_ECommunityProfileItemProperty_MovieMP4 );

			_profileBackground.url = miniProfileBack;
			_profileBackground.Prepare( );

			await UniTask.WaitUntil( ( ) => _profileBackground.isPrepared );

			var renderTexture = _profileBackground.targetTexture;

			var newWidth  = (Int32)_profileBackground.width;
			var newHeight = (Int32)_profileBackground.height;

			if( renderTexture.width != newWidth || renderTexture.height != newHeight )
			{
				renderTexture.Release( );
				renderTexture.width  = newWidth;
				renderTexture.height = newHeight;
				renderTexture.Create( );
			}

			_profileBackground.GetComponent<AspectRatioFitter>( ).aspectRatio = newWidth / (Single)newHeight;

			_profileBackground.time = 0;
			_profileBackground.Play( );
		}
	}
}