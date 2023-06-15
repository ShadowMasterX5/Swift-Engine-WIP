using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.Tilemaps;

namespace SonicFramework
{
	public class SonicTileCreator : TileUtility
	{
		public static SonicTile CreateSonicTile()
		{
			return ObjectFactory.CreateInstance<SonicTile>();
		}
		
		[CreateTileFromPalette]	
		public static SonicTile SonicTile(Sprite sprite)
		{
			SonicTile tile = CreateSonicTile();		
			tile.name = sprite.name;
			tile.sprite = sprite;
			tile.color = Color.white;	
			tile.sonicTileData.tileSprite = sprite;
			tile.sonicTileData.tileColor = Color.white;
			
			return tile;
		}
		[CustomEditor(typeof(SonicTileCreator))]
		public class SonicTileCreatorEditor : Editor
		{
		}
	}
}