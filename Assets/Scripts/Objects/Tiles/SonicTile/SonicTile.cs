using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace SonicFramework
{
	[CreateAssetMenu(menuName = "sonicFramework/Sonic Tile", fileName = "SonicTile")]
	[System.Serializable]
	public class SonicTile : Tile
	{	
		public SonicTileData sonicTileData = new SonicTileData();
		
		public Texture2D PreviewTexture { get; private set; }
		
		public SonicTile()
		{
			flags = TileFlags.None;
			colliderType = Tile.ColliderType.Grid;
		}
		
		public Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			return PreviewTexture;
		}
	}
}

