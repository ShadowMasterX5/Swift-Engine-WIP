using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public enum MaskAngles
{
	CalculateFloorAngle = 0,
	CalculateLWallAngle = 1,
	CalculateRWallAngle = 2,
	CalculateRoofAngle = 3,
}
namespace SonicFramework
{
	public class SonicTileEditorWindow : EditorWindow 
	{	
		public SonicTile SelectedTile;
		
		public float surfaceAngle;
		
		public bool collider;
		
		public bool flagged = false;
		
		public bool automaticCollider;
		
		public Vector2Int angleLineStartPos = new Vector2Int(15, 15);
		
		public Vector2Int angleLineEndPos   = new Vector2Int(15, 0 );
		
		public Vector2Int oldLineStartPos;
		
		public Vector2Int oldLineEndPos;
		
		public MaskAngles maskAngles;
		
		public bool[,] collisionPixelBools = new bool[16, 16];
		
		bool PointSelected;
		
		bool SelectedStartPoint;
		
		int OptionsPanelWidth = 200;
		
		Vector2 TileAreaPosition;
		
		Vector2Int MousePos;
		
		Rect tileRect;
		
		int TilePixelSize   = 25;
		
		int tilePaddingLeft = 30;
		
		int tilePaddingUp   = 30;
		
		Vector2 folderScrollPos;
		
		Vector2 spriteScrollPos;
		
		Vector2 MinSize = new Vector2(860, 469);
		
		Vector2 MaxSize = new Vector2(860, 469);
		
		bool painting = false;
		bool erasing = false;
		
		private string[] m_buttons = { "Save" , "Load", "Revert"};
		private int selectedButton = -1;
		
		private string[] m_buttons2 = { "Select Tile Folder" };
		private int selectedButton2 = -1;
		
		public string[] m_tools = { "Angle Tool" , "Collision Paint"};
		public int selectedTool = 0;
		
		public string tileFolder;
		
		public List<SonicTile> folderTiles = new List<SonicTile>();
		
		int SelectedFolderTile;
		
		private Rect lastRect
		{
			get
			{
				return GUILayoutUtility.GetLastRect();
			}
		}  
		
		public static void Open(SonicTile dataObject) 
		{
			SonicTileEditorWindow window = EditorWindow.GetWindow<SonicTileEditorWindow>("Sonic Tile Editor");
			
			window.SelectedTile = dataObject;
			
			// Load tile editor save data.
			window.collisionPixelBools = window.BoolArrayToMultiBoolArray(dataObject.sonicTileData.collisionPixels, 16);
			window.angleLineStartPos   = dataObject.sonicTileData.angleLineStartPos;
			window.angleLineEndPos     = dataObject.sonicTileData.angleLineEndPos;
			window.automaticCollider   = dataObject.sonicTileData.automaticCollider;
			window.collider            = dataObject.sonicTileData.collider;
			window.surfaceAngle        = dataObject.sonicTileData.surfaceAngle;
			window.flagged             = dataObject.sonicTileData.flagged;
			
			window.selectedTool        = dataObject.sonicTileData.selectedTool;
			window.folderTiles         = window.GetTilesInTileFolderOfTile(dataObject);
			
			if(window.automaticCollider) window.UpdateCollisionPixels();
		}
		
		List<SonicTile> GetTilesInTileFolderOfTile(SonicTile baseTile)
		{
			List<SonicTile> tilesInFolder = new List<SonicTile>();
			
			string path = AssetDatabase.GetAssetPath(baseTile);
			string directory = "";
			string[] folders;
			
			folders = path.Split("/");
			
			for(int i = 0; i < folders.Length - 1; i++)
			{
				if(i == folders.Length - 2)
				{
					directory = directory + folders[i];
				}
				else
				{
					directory = directory + folders[i] + "/";
				}
			}
			
			tileFolder = directory;
			
			DirectoryInfo dir = new DirectoryInfo(directory);
			
			string[] filePaths = Directory.GetFiles(dir.FullName);
			
			foreach(string file in filePaths) 
			{
				string[] assetDirectoryfolders = file.Split(@"\");
				
				string assetDirectory = "";
				
				for(int i = 0; i < assetDirectoryfolders.Length; i++)
				{
					if(i == assetDirectoryfolders.Length - 1)
					{
						assetDirectory = assetDirectory + assetDirectoryfolders[i];
					}
					else
						assetDirectory = assetDirectory + assetDirectoryfolders[i] + "/";
				}
				
				assetDirectory = "Assets" + assetDirectory.Replace(Application.dataPath, "");
				
				SonicTile tile = AssetDatabase.LoadAssetAtPath(assetDirectory, typeof(SonicTile)) as SonicTile;
				
				if(tile != null) tilesInFolder.Add(tile);
			}	
			return tilesInFolder;
		}

		int ButtonToolbar(string[] texts, float width)
		{
			for(int i = 0; i < texts.Length; i++)
			{
				if (GUILayout.Button(texts[i], GUILayout.Width(width / texts.Length)))
				{
					return i;
				}
			}
			return -1;
		}
				
		void OnEnable()
		{	
			if(automaticCollider && collider)
			{
				if (ArePixelsEmpty(collisionPixelBools))
				{
					UpdateCollisionPixels();
				}
			}
		}
	
		void OnGUI()
		{	
			Color defaultColor = GUI.color;
			
			minSize = MinSize;
			maxSize = MaxSize; 
			wantsMouseMove = true;
			
			using (new EditorGUI.DisabledScope(hasUnsavedChanges))
			{
				bool altered = (false);
				
				if(altered)
					hasUnsavedChanges = true;
			}
			
			GUILayout.BeginHorizontal();
			GUIStyle style = new GUIStyle(GUI.skin.box);
			if (Event.current.type == EventType.Repaint)
			{
				GUI.color = new Color(0f, 0f, 0f, 0.15f);
				GUI.Box(new Rect(0, 0, 860, 25), "", EditorStyles.objectField);
				
				GUI.color = new Color(0f, 0f, 0f, 0.3f);
				GUI.Box(new Rect(0, 0, OptionsPanelWidth + 7, 25), "", EditorStyles.objectField);
				
				GUI.color = new Color(0f, 0f, 0f, 0.3f);
				GUI.Box(new Rect(860 - (OptionsPanelWidth), 0, OptionsPanelWidth, 25), "", EditorStyles.objectField);
			}
			GUILayout.EndHorizontal();
			GUI.color = defaultColor;
			GUILayout.BeginHorizontal();
			Rect buttonToolbarRect = new Rect(3, 3, OptionsPanelWidth , 25 - 6);
			selectedButton = GUI.Toolbar(buttonToolbarRect, selectedButton, m_buttons);
			
			GUILayout.Label("");
			
			Rect folderIconRect = new Rect(860 - OptionsPanelWidth, 0, 25, 25);
			GUI.Box(folderIconRect, EditorGUIUtility.IconContent("Folder Icon"));
			
			Rect buttonToolbarRect2 = new Rect(885 - (OptionsPanelWidth) + 3, 3, OptionsPanelWidth - 31, 25 - 6);
			selectedButton2 = GUI.Toolbar(buttonToolbarRect2, selectedButton2, m_buttons2);
			
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
		
			GUILayout.BeginHorizontal();
			GUILayout.Label("Tile : " + SelectedTile.name, EditorStyles.objectField, GUILayout.Width(200 - 38), GUILayout.Height(32));
			
			Texture2D tileTex = AssetPreview.GetAssetPreview(SelectedTile.sonicTileData.tileSprite);
			if(tileTex != null)
			{
				tileTex.alphaIsTransparency = true;
				tileTex.filterMode = FilterMode.Point;
				
				GUILayout.Label("", GUILayout.Width(32), GUILayout.Height(32));
				GUI.Box(new Rect(lastRect.x - 1, lastRect.y - 1, 34, 34), "", EditorStyles.objectField);
				EditorGUI.DrawTextureTransparent(new Rect(lastRect.x, lastRect.y, 32, 32), tileTex, ScaleMode.ScaleToFit);
			}
			GUILayout.Label("", GUILayout.Width(OptionsPanelWidth), GUILayout.Height(0));
				
			TileAreaPosition = new Vector2(205, lastRect.position.y);
			
			tileRect = new Rect(TileAreaPosition.x + tilePaddingLeft, TilePixelSize * 2, TilePixelSize * 16, TilePixelSize * 16);
				
			MousePos = new Vector2Int
			(
				Mathf.FloorToInt((float)(Event.current.mousePosition.x - tileRect.x) / (float)TilePixelSize),
				Mathf.FloorToInt((float)(Event.current.mousePosition.y - tileRect.y) / (float)TilePixelSize) 
			);	
			Vector2Int MousePosClamp = MousePos;
			
			MousePosClamp.x = Mathf.Clamp(MousePos.x, 0, 15);
			MousePosClamp.y = Mathf.Clamp(MousePos.y, 0, 15);
			
			surfaceAngle = Mathf.Round((Vector2.SignedAngle(angleLineEndPos - angleLineStartPos, Vector2.right)) * 2f) / 2f;
						
			oldLineStartPos = angleLineStartPos;
			oldLineEndPos   = angleLineEndPos;
			
			if(SelectedTile != null)
				DrawSelectedTile(size: TilePixelSize);
				
			Rect toolBarRect = new Rect(tileRect.x, 3, TilePixelSize * 8, TilePixelSize - 6);
			
			selectedTool = GUI.Toolbar(toolBarRect, selectedTool, m_tools, EditorStyles.toggle);
				
			GUILayout.EndHorizontal();
			
			surfaceAngle = Mathf.Round((Vector2.SignedAngle(angleLineEndPos - angleLineStartPos, Vector2.right)) * 2f) / 2f;
			
			if(automaticCollider && collider)
			{
				if (Event.current.type == EventType.MouseDrag)
				{
					UpdateCollisionPixels();
				}
			}
			
			EditorGUILayout.Space();
			
			GUILayout.Label("Tile Settings", EditorStyles.largeLabel);
			
			GUILayout.Label(" ");
			GUI.Label(new Rect(lastRect.x + 10, lastRect.y, lastRect.width, lastRect.height), "Flagged");
			flagged = GUI.Toggle(new Rect(lastRect.x + (OptionsPanelWidth / 2), lastRect.y, OptionsPanelWidth, 20), flagged, ""); 

			GUILayout.BeginHorizontal();
			GUILayout.Label(" ");
			GUI.Label(new Rect(lastRect.x + 10, lastRect.y, lastRect.width, lastRect.height), "Tile Angle");
			GUI.Label(new Rect(lastRect.x + (OptionsPanelWidth / 2), lastRect.y, 37, 18), surfaceAngle.ToString(), EditorStyles.textField);
			
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			bool oldCol     = collider;
			bool oldAutoCol = automaticCollider;

			GUILayout.Label(" ");
			GUI.Label(new Rect(lastRect.x + 10, lastRect.y, lastRect.width, lastRect.height), "Pixel Collider");
			collider = GUI.Toggle(new Rect(lastRect.x + (OptionsPanelWidth / 2), lastRect.y, OptionsPanelWidth, 20), collider, ""); 
			
			if(!collider) GUI.enabled = false;
			GUILayout.Label(" ");
			GUI.Label(new Rect(lastRect.x + 10, lastRect.y, lastRect.width, lastRect.height), "Auto Collider");
			automaticCollider = GUI.Toggle(new Rect(lastRect.x + (OptionsPanelWidth / 2), lastRect.y, OptionsPanelWidth, 20), automaticCollider, ""); 
			GUI.enabled = true;
			
			if(automaticCollider && collider)
			{
				if(!oldAutoCol || !oldCol)
				{
					UpdateCollisionPixels();
				}
			}
			
			EditorGUILayout.Space();
			
			GUILayout.Label("Tile Collision Array Previews", EditorStyles.largeLabel);
			GUILayout.Label("(TODO)", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(OptionsPanelWidth));
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(" ", GUILayout.Height(48));
				GUI.Label(new Rect(lastRect.x + 10, lastRect.y, lastRect.width, lastRect.height), "Up Array");
				EditorGUI.DrawTextureTransparent(new Rect(lastRect.x + (OptionsPanelWidth / 2), lastRect.y, 48, 48), Utilis.FillColorAlpha(new Texture2D(1,1), Color.clear), ScaleMode.ScaleToFit);
				
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(" ", GUILayout.Height(48));
				GUI.Label(new Rect(lastRect.x + 10, lastRect.y, lastRect.width, lastRect.height), "Right Array");
				EditorGUI.DrawTextureTransparent(new Rect(lastRect.x + (OptionsPanelWidth / 2), lastRect.y, 48, 48), Utilis.FillColorAlpha(new Texture2D(1,1), Color.clear), ScaleMode.ScaleToFit); 
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(" ", GUILayout.Height(48));
				GUI.Label(new Rect(lastRect.x + 10, lastRect.y, lastRect.width, lastRect.height), "Down Array");
				EditorGUI.DrawTextureTransparent(new Rect(lastRect.x + (OptionsPanelWidth / 2), lastRect.y, 48, 48), Utilis.FillColorAlpha(new Texture2D(1,1), Color.clear), ScaleMode.ScaleToFit); 
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(" ", GUILayout.Height(48));
				GUI.Label(new Rect(lastRect.x + 10, lastRect.y, lastRect.width, lastRect.height), "Left Array");
				EditorGUI.DrawTextureTransparent(new Rect(lastRect.x + (OptionsPanelWidth / 2), lastRect.y, 48, 48), Utilis.FillColorAlpha(new Texture2D(1,1), Color.clear), ScaleMode.ScaleToFit); 
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			
			angleLineStartPos.x = Mathf.Clamp(angleLineStartPos.x, 0, 15);
			angleLineStartPos.y = Mathf.Clamp(angleLineStartPos.y, 0, 15);
			
			angleLineEndPos.x   = Mathf.Clamp(angleLineEndPos.x  , 0, 15);
			angleLineEndPos.y   = Mathf.Clamp(angleLineEndPos.y  , 0, 15);
			
			if(selectedButton != -1)
			{
				switch (selectedButton) {
					case 0:
						SaveChanges();
						break;
					case 1:
						LoadTile();
						break;
					case 2:
						RevertChanges();
						break;
				}
			}
			
			selectedButton = -1;
			
			if(selectedButton2 != -1)
			{
				switch (selectedButton2) {
					case 0:
						// open tile folder selection.
						SelectTileFolder();
						break;
					case 1:
						SelectTileTexture();
						break;
				}
			}
			
			selectedButton2 = -1;	
			
			int ScrollViewMaxWidth = 4;
			
			// Folder Area
			int FolderViewInsetSize = 10;
			
			Rect folderScrollViewRect = new Rect(860 - OptionsPanelWidth + FolderViewInsetSize, 41 + FolderViewInsetSize - 4 - 17, OptionsPanelWidth + 4 - FolderViewInsetSize * 2, 319 - FolderViewInsetSize);
			
			int folderScrollMaxHeight = ((folderTiles.Count) / ScrollViewMaxWidth) * (32 + 5) + 32 + 30;
			
			Rect folderScrollViewUseRect = new Rect(0, 0, folderScrollViewRect.width - 13, Mathf.Max(folderScrollMaxHeight, folderScrollViewRect.height));
			
			GUI.Label(folderScrollViewRect, "");
			
			folderScrollPos = GUI.BeginScrollView(folderScrollViewRect, folderScrollPos, folderScrollViewUseRect, false, true);
			{
				GUI.color = new Color(0f, 0f, 0f, 0.15f);
				GUI.Box(folderScrollViewUseRect, "", EditorStyles.objectField);
				GUI.color = defaultColor;
				
				Texture2D scrollBGTex = Utilis.FillColorAlpha(new Texture2D(1,1), new Color(0.1f, 0.1f, 0.1f, 1));
				scrollBGTex.alphaIsTransparency = false;
				GUI.DrawTexture(new Rect(6, 7, folderScrollViewUseRect.width - 12, folderScrollViewUseRect.height - 15), scrollBGTex, ScaleMode.StretchToFill, false);
				//GUI.DrawTexture(new Rect(folderScrollViewUseRect.x + 6, folderScrollViewUseRect.y, folderScrollViewUseRect.width - 12, folderScrollViewUseRect.height - 12), new Texture2D(1,1), ScaleMode.ScaleToFit);
				
				Texture2D emptyTex = Texture2D.blackTexture;
				
				for(int i = 0; i < folderTiles.Count; i++) 
				{
					float x = i % ScrollViewMaxWidth;
					float y = (i - x) / ScrollViewMaxWidth;
					
					Rect tileIconRect = new Rect(14 + x * (32 + 5), 15 + y * (32 + 5), 32, 32);
					
					Texture2D folderTileTex = emptyTex;
					
					if(folderTiles[i] != null)
					{
						if(folderTiles[i].sonicTileData.tileSprite != null) 
							folderTileTex = AssetPreview.GetAssetPreview(folderTiles[i].sonicTileData.tileSprite);
					}

					if(folderTileTex == null){
						folderTileTex = emptyTex;
					}
					
					folderTileTex.filterMode = FilterMode.Point;
					folderTileTex.alphaIsTransparency = true;
					
					//GUI.Box(new Rect(tileIconRect.x - 1, tileIconRect.y - 1, tileIconRect.width + 2, tileIconRect.height + 2), "", EditorStyles.objectField);
					EditorGUI.DrawTextureTransparent(tileIconRect, folderTileTex, ScaleMode.StretchToFill);
					
				}
				
				GUI.color = defaultColor;
			}
			GUI.EndScrollView();
			
			// Tile Texture Sprite Area
			/*int SpriteViewInsetSize = 10;
			
			Rect spriteScrollViewRect = new Rect(860 - OptionsPanelWidth + SpriteViewInsetSize, (folderScrollViewRect.y + 8 * TilePixelSize) + SpriteViewInsetSize - 4, OptionsPanelWidth + 4 - SpriteViewInsetSize * 2, 210 - SpriteViewInsetSize - 25);
			
			int spriteScrollMaxHeight = 500;
			
			Rect spriteScrollViewUseRect = new Rect(0, 0, spriteScrollViewRect.width - 13, spriteScrollMaxHeight);
			
			GUI.Label(folderScrollViewRect, "");
			
			spriteScrollPos = GUI.BeginScrollView(spriteScrollViewRect, spriteScrollPos, spriteScrollViewUseRect, false, true);
			{
				GUI.color = new Color(0f, 0f, 0f, 0.15f);
				GUI.Box(spriteScrollViewUseRect, "", EditorStyles.objectField);
				GUI.color = defaultColor;
			}
			GUI.EndScrollView();*/
	
			Repaint();
		}
		
		private bool ArePixelsEmpty(bool[,] array) 
		{
			for (int i = 0; i < 16; i++){
				for (int j = 0; j < 16; j++){
					if (array[i, j] != false) {
						return false;
					}
				}
			}
			return true;
		}

		private void DrawSelectedTile(int size)
		{
			int tileSize = size * 16;
			
			int borderWidth = 3;
			
			Rect borderRect = new Rect(tileRect.xMin - borderWidth, tileRect.yMin - borderWidth, tileRect.width + borderWidth * 2, tileRect.height + borderWidth * 2);
			
			if (Event.current.type == EventType.Repaint)
			{
				GUIStyle style = new GUIStyle(GUI.skin.box);
				style.Draw(new Rect(borderRect.x - 25, 25, borderRect.xMax - borderRect.x + 50, MinSize.y + 20), false, false, false, false);
				EditorStyles.textField.Draw(borderRect, false, false, false, false);
			}
				
			Texture2D tileTex = AssetPreview.GetAssetPreview(SelectedTile.sonicTileData.tileSprite);
			
			if(tileTex == null) tileTex = Utilis.FillColorAlpha(new Texture2D(16, 16), Color.clear);
			
			tileTex.alphaIsTransparency = true;
			tileTex.filterMode = FilterMode.Point;
			
			EditorGUI.DrawTextureTransparent(tileRect, tileTex, ScaleMode.StretchToFill);
			
			Vector2Int TileMousePosClamp = MousePos;
			
			TileMousePosClamp.x = Mathf.Clamp(MousePos.x, 0, 15);
			TileMousePosClamp.y = Mathf.Clamp(MousePos.y, 0, 15);
						
			if((Event.current.type == EventType.MouseDown && Event.current.button == 0))
			{
				if(selectedTool == 0)
				{
					if(angleLineEndPos == MousePos)
					{
						PointSelected = true;
						SelectedStartPoint = false;
					}
					
					if(angleLineStartPos == MousePos)
					{
						PointSelected = true;
						SelectedStartPoint = true;
					}
				}
				
				if(selectedTool == 1)
				{
					if(MousePos == TileMousePosClamp)
					{
						painting = true;
					}
				}
			} 
			
			if((Event.current.type == EventType.MouseDown && Event.current.button == 1))
			{
				if(selectedTool == 1)
				{
					if(MousePos == TileMousePosClamp)
					{
						erasing = true;
					}
				}
			}
			
			if((Event.current.type == EventType.MouseUp && Event.current.button == 0) || selectedTool != 1)
			{
				painting = false;
			}
			if(painting && MousePos == TileMousePosClamp)
			{
				
				collisionPixelBools[TileMousePosClamp.x, TileMousePosClamp.y] = true;
			}
			
			if((Event.current.type == EventType.MouseUp && Event.current.button == 1) || selectedTool != 1)
			{
				erasing = false;
			}
			if(erasing && MousePos == TileMousePosClamp)
			{	
				collisionPixelBools[TileMousePosClamp.x, TileMousePosClamp.y] = false;
			}
		
			if(PointSelected)
			{
				if((Event.current.type == EventType.MouseUp && Event.current.button == 0) || selectedTool != 0)
				{
					PointSelected = false;
				}

				if(SelectedStartPoint)
				{
					angleLineStartPos = TileMousePosClamp;
				}
				else
				{
					angleLineEndPos = TileMousePosClamp;
				}
			}
			
			Color highlightColor = Color.gray;
			highlightColor.a = 0.35f;
			
			Color colliderHatchLineColor = new Color(0, 0, 0, 0.4f);
			
			Color arrowColor = new Color(0.1f, 0.5f, 0.9f, 1f);
			
			Color boxColor = new Color(1f, 0.3f, 0.3f, 1f);

			if (Event.current.type == EventType.Repaint)
			{	
				int thickness = 3;
				
				if((MousePos.x >= 0) && (MousePos.x <= 15) && (MousePos.y >= 0) && (MousePos.y <= 15))
				{
					GuiUtilis.DrawRect(new Rect(tileRect.x + TileMousePosClamp.x * size, tileRect.y + TileMousePosClamp.y * size, size, size), highlightColor);
				}
				else
				{
					GuiUtilis.DrawRect(new Rect(tileRect.x + TileMousePosClamp.x * size, tileRect.y + TileMousePosClamp.y * size, size, size), Color.clear);
				}
				
				if(selectedTool == 0)
				{
					Vector2 arrowOffset = new Vector2(0.5f, 0.5f);
				
					Color StartBoxCol    = boxColor;
					
					Color EndBoxCol      = boxColor;
					
					Color AngleLineColor = boxColor;
					
					float ArrowAngle = Mathf.Round((Vector2.SignedAngle(angleLineEndPos - angleLineStartPos, Vector2.right)) * 2f) / 2f;
					
					DrawArrow(tileRect.position + (arrowOffset * size) + ((angleLineEndPos * size) + (angleLineStartPos * size)) / 2, ArrowAngle, size, arrowColor, thickness);
					
					GuiUtilis.DrawLine(
						new Vector2(tileRect.x + (angleLineStartPos.x * size) + (size/2f), tileRect.y + (angleLineStartPos.y * size) + (size/2f)), 
						new Vector2(tileRect.x + (angleLineEndPos.x   * size) + (size/2f), tileRect.y + (angleLineEndPos.y   * size) + (size/2f)),
						AngleLineColor, thickness
					);
					
					if(PointSelected)
					{
						GuiUtilis.DrawRect(new Rect(tileRect.x + TileMousePosClamp.x * size, tileRect.y + TileMousePosClamp.y * size, size, size), Color.white);
					}
					
					GuiUtilis.DrawRectInline(new Rect(tileRect.x + (angleLineStartPos.x * size), tileRect.y + (angleLineStartPos.y * size), size, size), StartBoxCol, thickness);
					
					GuiUtilis.DrawRectInline(new Rect(tileRect.x + (angleLineEndPos.x * size),   tileRect.y + (angleLineEndPos.y   * size), size, size), EndBoxCol, thickness);
				}
				
				if(collider)
				{
					DrawCollisionPixels(tileRect.position, size, colliderHatchLineColor);
				}	
			}
		}
		
		private void UpdateCollisionPixels()
		{
			if(selectedTool != 0) return;
			collisionPixelBools = new bool[16, 16];
			
			for(int i = 0; i < 16; i++) 
			{
				for(int j = 0; j < 16; j++) 
				{
					Vector2Int pos = new Vector2Int(i, j);
					collisionPixelBools[i, j] = UnderLineTest(pos);
				}
			}
		}
		
		private void DrawCollisionPixels(Vector2 tileAreaPosition, int pixelSize, Color color)
		{	 	
			Color hatchLineColor = color;
			hatchLineColor.a = color.a / 2f;
			for(int i = 0; i < 16; i++) 
			{
				for(int j = 0; j < 16; j++) 
				{
					if(collisionPixelBools[i, j])
					{		
						Vector2 sizeOffset = Vector2.zero;							

						if((j == 0) || (!collisionPixelBools[i, j - 1]))
						{
							GuiUtilis.DrawLine(
								tileAreaPosition + new Vector2(i    , j) * pixelSize,
								tileAreaPosition + new Vector2(i + 1, j) * pixelSize,
								color, 2
							);
						}
						
						if((j == 15) || (!collisionPixelBools[i, j + 1]))
						{
							GuiUtilis.DrawLine(
								tileAreaPosition + new Vector2(i    , j + 1) * pixelSize,
								tileAreaPosition + new Vector2(i + 1, j + 1) * pixelSize,
								color, 2
							);
						}
						
						if((i == 0) || (!collisionPixelBools[i - 1, j]))
						{
							GuiUtilis.DrawLine(
								tileAreaPosition + new Vector2(i, j    ) * pixelSize,
								tileAreaPosition + new Vector2(i, j + 1) * pixelSize,
								color, 2
							);
						}
						
						if((i == 15) || (!collisionPixelBools[i + 1, j]))
						{
							GuiUtilis.DrawLine(
								tileAreaPosition + new Vector2(i + 1, j    ) * pixelSize,
								tileAreaPosition + new Vector2(i + 1, j + 1) * pixelSize,
								color, 2
							);
						}
						
						GuiUtilis.DrawHatchLines(new Rect
						(
							tileAreaPosition.x + i * pixelSize,
							tileAreaPosition.y + j * pixelSize,
							pixelSize, pixelSize), 6, hatchLineColor, 1
						);
					}
				}
			}
		}
		
		private bool UnderLineTest(Vector2Int testPoint)
		{
			Vector2Int testPointFlipped = new Vector2Int(testPoint.x, -testPoint.y + 15);
			
			Vector2Int PointLineVector = (angleLineEndPos - angleLineStartPos);
			Vector2Int PointLineVectorFlipped = new Vector2Int(PointLineVector.x, -PointLineVector.y);
			
			float slope = (float)PointLineVectorFlipped.y / (float)PointLineVectorFlipped.x;
			
			float angle = surfaceAngle;
			
			if(surfaceAngle < 0)
			{
				angle += 360;
			}
			
			bool LessThanBool = 
			(
				((testPointFlipped.y) <= slope * (testPointFlipped.x - angleLineStartPos.x - (angle == 270 ? 0.1f : -0.1f)) - (angleLineStartPos.y - 15))
			);
			
			bool GreaterThanBool = 
			(
				(testPointFlipped.y   >= slope * (testPointFlipped.x - angleLineStartPos.x + 0.1f) - (angleLineStartPos.y - 15))
			);
			
			bool checkBool;
			
			if(angle < 270 && angle > 90)
			{
				checkBool = GreaterThanBool;
			}
			else
			{
				checkBool = LessThanBool;
			}
			
			if(checkBool)
			{
				return true;
			}
			else 
			{
				return false;
			}
		}
		
		private void GLDrawRectOutline(Rect rect, Color color)
		{
			GL.Begin(GL.LINES);
			GL.Color(color);
			
			GL.Vertex3(rect.x, rect.y, 0);
			GL.Vertex3(rect.x + rect.width, rect.y, 0);
			
			GL.Vertex3(rect.x + rect.width, rect.y, 0);
			GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0);
			
			GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0);
			GL.Vertex3(rect.x, rect.y + rect.height, 0);
			
			GL.Vertex3(rect.x, rect.y + rect.height, 0);
			GL.Vertex3(rect.x, rect.y, 0);
			
			GL.End();
		}
		
		private void DrawArrow(Vector2 position, float angle, float length, Color? color = null, int thickness = 1)
		{
			Color col = color ?? Color.black;
			
			Vector2 dir = Utilis.Vector2ToAngle(angle - 90, Vector2.right) * length;
			dir = new Vector2(-dir.x, dir.y);
			
			GuiUtilis.DrawLine(
				position, 
				position + dir,
				
				col, thickness
			);
			
			Vector2 tipDirLeft = Utilis.Vector2ToAngle(angle - 135, Vector2.right) * (-length);
			tipDirLeft = new Vector2(-tipDirLeft.x, tipDirLeft.y);
			
			Vector2 tipDirRight = Utilis.Vector2ToAngle(angle - 45, Vector2.right) * (-length);
			tipDirRight = new Vector2(-tipDirRight.x, tipDirRight.y);
			
			GuiUtilis.DrawLine(
				position + dir,
				position + dir + tipDirLeft,
				
				col, thickness
			);
			
			GuiUtilis.DrawLine(
				position + dir,
				position + dir + tipDirRight,
				
				col, thickness
			);
		}
		
		private void DrawHatchLines(Rect rect, Color? color = null, int thickness = 1)
		{
			Color col = color ?? Color.black;
			
			GuiUtilis.DrawLine(
				new Vector2(rect.x + rect.width, rect.y),
				new Vector2(rect.x, rect.y + rect.height),
				col, thickness
			);
		}
		
		public void SelectTileFolder()
		{
			
		}
		
		public void SelectTileTexture()
		{
			
		}
		
		public void RevertChanges()
		{
			
		}
		
		public void LoadTile()
		{
			
		}
		
		public override void SaveChanges()
		{
			hasUnsavedChanges = false;
			
			// Save tile editor data.
			SelectedTile.sonicTileData.collisionPixels   = MultiBoolArrayToBoolArray(collisionPixelBools);
			SelectedTile.sonicTileData.angleLineStartPos = angleLineStartPos;
			SelectedTile.sonicTileData.angleLineEndPos   = angleLineEndPos;
			SelectedTile.sonicTileData.automaticCollider = automaticCollider;
			SelectedTile.sonicTileData.collider          = collider;
			SelectedTile.sonicTileData.surfaceAngle      = surfaceAngle;
			SelectedTile.sonicTileData.selectedTool      = selectedTool;
			SelectedTile.sonicTileData.flagged           = flagged;
			
			SelectedTile.sonicTileData.UpdateHeightArrays();			
			SelectedTile.sonicTileData.CreateCollisionSprite();

			EditorUtility.SetDirty(SelectedTile as Object);
			
			base.SaveChanges();
		}

		public override void DiscardChanges()
		{
			base.DiscardChanges();
		}
		
		public bool[] MultiBoolArrayToBoolArray(bool[,] muliArray)
		{
			bool[] newArray = new bool[256];
			
			for(int x = 0; x < 16; x++)
			{
				for(int y = 0; y < 16; y++)
				{
					newArray[y * 16 + x] = muliArray[x, y];
				}
			}
			
			return newArray;
		}
		
		public bool[,] BoolArrayToMultiBoolArray(bool[] boolArray, int width)
		{
			bool[,] newArray = new bool[16, 16];
			
			for(int x = 0; x < 16; x++)
			{
				for(int y = 0; y < 16; y++)
				{
					newArray[x, y] = boolArray[y * 16 + x];
				}
			}
			
			return newArray;
		}
	}
}