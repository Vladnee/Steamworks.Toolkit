#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;

namespace Steamworks.Toolkit.Editor
{
	[InitializeOnLoad]
	public static class AnimatedImagesDefineResolver
	{
		private const String _define           = "ANIMATED_IMAGES";
		private const String _requiredAssembly = "AnimatedImages.Runtime";

		static AnimatedImagesDefineResolver( )
		{
			EditorApplication.delayCall             += Refresh;
			CompilationPipeline.compilationFinished += _ => Refresh( );
			EditorApplication.projectChanged        += Refresh;
		}

		private static void Refresh( )
		{
			if( EditorApplication.isCompiling )
				return;

			var isInstalled = IsAssemblyPresentInProject( _requiredAssembly );
			SetDefine( _define, isInstalled );
		}

		private static Boolean IsAssemblyPresentInProject( String assemblyName )
		{
			{
				var asmdefGuids = AssetDatabase.FindAssets( $"{assemblyName} t:asmdef" );

				for( var i = 0; i < asmdefGuids.Length; i++ )
				{
					var path = AssetDatabase.GUIDToAssetPath( asmdefGuids[i] );

					if( path.EndsWith( "/" + assemblyName + ".asmdef", StringComparison.OrdinalIgnoreCase ) ||
					    path.EndsWith( "\\" + assemblyName + ".asmdef", StringComparison.OrdinalIgnoreCase ) )
					{
						return true;
					}
				}
			}

			{
				var anyGuids = AssetDatabase.FindAssets( assemblyName );

				for( var i = 0; i < anyGuids.Length; i++ )
				{
					var path = AssetDatabase.GUIDToAssetPath( anyGuids[i] );

					if( path.EndsWith( "/" + assemblyName + ".dll", StringComparison.OrdinalIgnoreCase ) ||
					    path.EndsWith( "\\" + assemblyName + ".dll", StringComparison.OrdinalIgnoreCase ) )
					{
						return true;
					}
				}
			}

			return false;
		}

		private static void SetDefine( String define, Boolean enabled )
		{
			var buildTarget = NamedBuildTarget.FromBuildTargetGroup( EditorUserBuildSettings.selectedBuildTargetGroup );

			PlayerSettings.GetScriptingDefineSymbols( buildTarget, out var defines );

			var hasDefine = defines.Contains( define );

			if( enabled && !hasDefine )
			{
				PlayerSettings.SetScriptingDefineSymbols( buildTarget, defines.Append( define ).ToArray( ) );
			}
			else if( !enabled && hasDefine )
			{
				PlayerSettings.SetScriptingDefineSymbols( buildTarget, defines.Where( d => d != define ).ToArray( ) );
			}
		}
	}
}
#endif