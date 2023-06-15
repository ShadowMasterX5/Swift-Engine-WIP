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
#if UNITY_EDITOR	
	public class AssetHandler
	{
		[OnOpenAsset()]
		public static bool OpenSonicTileEditor(int instanceId, int line)
		{
			SonicTile tile = EditorUtility.InstanceIDToObject(instanceId) as SonicTile;
			if(tile != null)
			{
				SonicTileEditorWindow.Open(tile);
				return true;
			}
			return false;
		}
	}

	[CustomEditor(typeof(SonicTile))]
	public class SonicTileInspector : Editor
	{
		private Editor TileEditor;
		
		private Rect lastRect
		{
			get
			{
				return GUILayoutUtility.GetLastRect();
			}
		}  
		
		public Tile tile
		{
			get { return (target as Tile); }
		}
		
		public SonicTile sonicTile
		{
			get { return (target as SonicTile); }
		}
		
		public SonicTileData sonicTileData
		{
			get { return sonicTile.sonicTileData; }
		}
		
		public Texture2D previewTexture;
		
		public SerializedProperty tileSprite_Property;
		public SerializedProperty tileColor_Property;
		public SerializedProperty collider_Property;
		
		public SerializedProperty flagged_Property;
		
		public SerializedProperty surfaceAngle_Property;
		
		public SerializedProperty upArray_Property;
		public SerializedProperty rightArray_Property;
		public SerializedProperty downArray_Property;
		public SerializedProperty leftArray_Property;
		
		public SerializedProperty collisionPixels_Property;
		
		public void OnEnable()
		{	
			Tile dummyTile = ScriptableObject.CreateInstance<Tile>();
			System.Type tileEditorType = Editor.CreateEditor(dummyTile).GetType();
	
			TileEditor = Editor.CreateEditor(tile, tileEditorType);
			
			tileSprite_Property   = serializedObject.FindProperty("sonicTileData.tileSprite");
			tileColor_Property    = serializedObject.FindProperty("sonicTileData.tileColor");
			collider_Property     = serializedObject.FindProperty("sonicTileData.collider");
			
			flagged_Property      = serializedObject.FindProperty("sonicTileData.flagged");
			
			surfaceAngle_Property = serializedObject.FindProperty("sonicTileData.surfaceAngle");
			
			upArray_Property      = serializedObject.FindProperty("sonicTileData.upArray");
			rightArray_Property   = serializedObject.FindProperty("sonicTileData.rightArray");
			downArray_Property    = serializedObject.FindProperty("sonicTileData.downArray");
			leftArray_Property    = serializedObject.FindProperty("sonicTileData.leftArray");
			
			collisionPixels_Property = serializedObject.FindProperty("sonicTileData.collisionPixels");
		}
				
		public override void OnInspectorGUI()
		{	
			serializedObject.Update();
			
			EditorGUI.BeginChangeCheck();
			
			bool useCollisionSprites = false;
			
			if(useCollisionSprites)
			{
				sonicTile.sprite = sonicTileData.collisionSprite;
			} 
			else
			{
				sonicTile.sprite = sonicTileData.tileSprite;
			}
			
			sonicTile.color = sonicTileData.tileColor;
			
			Texture2D tileTexture = AssetPreview.GetAssetPreview(sonicTileData.tileSprite) ?? Utilis.FillColorAlpha(new Texture2D(32, 32), Color.clear);
			if(tileTexture != null)
			{
				tileTexture.filterMode = FilterMode.Point;
			}
			
			Texture2D colliderTexture = AssetPreview.GetAssetPreview(sonicTileData.collisionSprite);
			
			if(colliderTexture == null)
			{
				UpdateColliderPreview(sonicTileData);
				colliderTexture = AssetPreview.GetAssetPreview(sonicTileData.collisionSprite) ?? Utilis.FillColorAlpha(new Texture2D(32, 32), Color.clear);
			}
			
			if(colliderTexture != null)
			{
				colliderTexture.filterMode = FilterMode.Point;
			}
		
			int PreviewSize = 64;
			
			GUIContent previewLabel = EditorGUIUtility.TrTextContent("Sprite Preview", "Preview of the tile's sprite");
			
			GUIContent ColliderLabel = EditorGUIUtility.TrTextContent("Collider Preview", "Preview of the tile's collider");
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			{
				Rect guiRect = EditorGUILayout.GetControlRect(false, PreviewSize);
				
				GUI.Label(guiRect, previewLabel, EditorStyles.largeLabel);
				
				guiRect = EditorGUI.PrefixLabel(guiRect, previewLabel);
				
				if (Event.current.type == EventType.Repaint)
				{
					Rect previewRect = new Rect(guiRect.xMin, guiRect.yMin, PreviewSize, PreviewSize);
		   			Rect borderRect = new Rect(guiRect.xMin - 1, guiRect.yMin - 1, PreviewSize + 2, PreviewSize + 2);
				
					EditorStyles.textField.Draw(borderRect, false, false, false, false);
					
					Object[] objs = new Object[1]{tileTexture};
						
					Texture2D texture = RenderStaticPreview(AssetDatabase.GetAssetPath(tileTexture), objs, 16, 16);
				
					EditorGUI.DrawTextureTransparent(previewRect, texture, ScaleMode.ScaleToFit);
				}
			}
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			{
				Rect guiRect2 = EditorGUILayout.GetControlRect(false, PreviewSize);
				
				GUI.Label(guiRect2, ColliderLabel, EditorStyles.largeLabel);
				
				guiRect2 = EditorGUI.PrefixLabel(guiRect2, EditorGUIUtility.TrTextContent(" ", "Preview of the tile's collider"));
				
				Rect _previewRect = new Rect(guiRect2.xMin, guiRect2.yMin, PreviewSize, PreviewSize);
		   		Rect _borderRect = new Rect(guiRect2.xMin - 1, guiRect2.yMin - 1, PreviewSize + 2, PreviewSize + 2);
				
				if (Event.current.type == EventType.Repaint)
					EditorStyles.textField.Draw(_borderRect, false, false, false, false);
					
				EditorGUI.DrawTextureTransparent(_previewRect, colliderTexture, ScaleMode.ScaleToFit);
			}
			GUILayout.EndHorizontal();	
			
			EditorGUILayout.Space();
			
			EditorGUILayout.PropertyField(tileSprite_Property);
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Open Tile In Editor"))
			{
				SonicTileEditorWindow.Open((SonicTile)target);
			}
			GUILayout.EndHorizontal();
			
			EditorGUILayout.PropertyField(tileColor_Property);
			EditorGUILayout.PropertyField(collider_Property);
			if(!sonicTileData.collider) GUI.enabled = false;
			EditorGUILayout.PropertyField(flagged_Property);
			EditorGUILayout.PropertyField(surfaceAngle_Property);
			GUI.enabled = true;
			
			GUI.enabled = false;
			EditorGUILayout.PropertyField(upArray_Property);
			EditorGUILayout.PropertyField(rightArray_Property);
			EditorGUILayout.PropertyField(downArray_Property);
			EditorGUILayout.PropertyField(leftArray_Property);
			
			EditorGUILayout.PropertyField(collisionPixels_Property);
			GUI.enabled = true;
			
			serializedObject.ApplyModifiedProperties();
			
			if(EditorGUI.EndChangeCheck())
			{
				Repaint();
				EditorUtility.SetDirty(target);
			}
			
		}
		
		public void UpdateColliderPreview(SonicTileData tileData)
		{
			tileData.CreateCollisionSprite();
		}
		
		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			Texture2D tex = TileEditor.RenderStaticPreview(assetPath, subAssets, width, height) ?? Utilis.FillColorAlpha(new Texture2D(16, 16), Color.clear);
			tex.filterMode = FilterMode.Point;
			return tex;	
		}
	}
#endif
}