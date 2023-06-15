using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	[System.Serializable]
	public class SonicTileData
	{
		public Sprite tileSprite = null;
		public Color  tileColor  = Color.white;
		
		/// <Summary>
		/// If the tile should use angles or not.
		/// </Summary>
		public bool flagged = false;
		
		/// <Summary>
		/// The angle that the tile will use.
		/// </Summary>
		public float surfaceAngle = 0;

		/// <Summary>
		/// Each array is used by sensors facing in the OPPOSITE direction.
		/// </Summary>		
		public List<int[]> heightArrays = new List<int[]>(4);
		
		public int[] upArray    = new int[16]; // Use with down facing sensors.
		public int[] rightArray = new int[16]; // Use with left facing sensors.
		public int[] downArray  = new int[16]; // Use with up facing sensors.
		public int[] leftArray  = new int[16]; // Use with right facing sensors.

		/// <Summary>
		/// Tile Editor Save Data.
		/// </Summary>	
		public bool[]     collisionPixels = new bool[256];
		public Texture2D  tileTexture;
		public Texture2D  collisionTexture;
		public Sprite     collisionSprite;
		public Vector2Int angleLineStartPos = new Vector2Int(15, 15);
		public Vector2Int angleLineEndPos   = new Vector2Int(15, 0 );
		public bool       automaticCollider = true;
		public int        selectedTool      = 0;
		
		public bool collider = true;
		
		public SonicTileData(int[] _upArray = null, int[] _rightArray = null, int[] _downArray = null, int[] _leftArray = null, bool[] _collisionPixels = null)
		{		
			// Set null or invalid arrays to empty arrays of the correct length.
			upArray    = CheckHeightArray(_upArray)    ? _upArray    : new int[16]; 
			rightArray = CheckHeightArray(_rightArray) ? _rightArray : new int[16];		
			downArray  = CheckHeightArray(_downArray)  ? _downArray  : new int[16];
			leftArray  = CheckHeightArray(_leftArray)  ? _leftArray  : new int[16];	

			// Add new arrays to the class's array.
			heightArrays.Add(upArray);
			heightArrays.Add(rightArray);
			heightArrays.Add(downArray);
			heightArrays.Add(leftArray);
			
			collisionPixels = CheckCollisionArray(_collisionPixels) ? _collisionPixels : new bool[256];
		}
		
		public void UpdateHeightArrays()
		{
			/// <Summary>
			/// UP ARRAY.
			/// </Summary>	
			for(int x = 0; x < 16; x++) 
			{
				bool found = false;
				for(int y = 15; y > -1; y--) 
				{
					if(collisionPixels[y * 16 + x]) 
					{
						upArray[x] = 16 - y;
						found = true;
						continue;
					}
				}
				if(found == false)
				{
					upArray[x] = 0;
				}
			}
			
			/// <Summary>
			/// RIGHT ARRAY.
			/// </Summary>	
			for(int y = 15; y > -1; y--) 
			{
				bool found = false;
				for(int x = 0; x < 16; x++) 
				{
					if(collisionPixels[y * 16 + x]) 
					{
						rightArray[y] = x + 1;
						found = true;
						continue;
					}
				}
				if(found == false)
				{
					rightArray[y] = 0;
				}
			}
			
			/// <Summary>
			/// DOWN ARRAY.
			/// </Summary>	
			for(int x = 0; x < 16; x++) 
			{
				bool found = false;
				for(int y = 0; y < 16; y++) 
				{
					if(collisionPixels[y * 16 + x]) 
					{
						downArray[15 - x] = y + 1;
						found = true;
						continue;
					}
				}
				if(found == false)
				{
					downArray[15 - x] = 0;
				}
			}
			
			/// <Summary>
			/// LEFT ARRAY.
			/// </Summary>	
			for(int y = 15; y > -1; y--) 
			{
				bool found = false;
				for(int x = 15; x > -1; x--) 
				{
					if(collisionPixels[y * 16 + x]) 
					{
						leftArray[15 - y] = 16 - x;
						found = true;
						continue;
					}
				}
				if(found == false)
				{
					leftArray[15 - y] = 0;
				}
			}
			
			if(!collider)
			{
				upArray    = new int[16];
				rightArray = new int[16];
				downArray  = new int[16];
				leftArray  = new int[16];
			}
		}
		
		public void CreateCollisionSprite()
		{
			Texture2D colTex = new Texture2D(16, 16);
#if UNITY_EDITOR
			colTex.alphaIsTransparency = true;
#endif
			colTex.filterMode = FilterMode.Point;
			Color colliderTexColor = Color.black;
			colliderTexColor.g = 0.75f;
			colliderTexColor.a = 0.5f;
			
			if(collider)
			{
				bool CombineCollisionAndSprite = false;
			
				if(CombineCollisionAndSprite)
				{
					Texture2D tileSpriteTexture = textureFromSprite(tileSprite);
#if UNITY_EDITOR
					tileSpriteTexture.alphaIsTransparency = true;
#endif
					tileSpriteTexture.filterMode = FilterMode.Point;
					
					for(int x = 0; x < 16; x++)
					{
						for(int y = 0; y < 16; y++)
						{
							if(collisionPixels[y * 16 + x]) 
							{
								colTex.SetPixel(x, -y - 1, (CombineColors(tileSpriteTexture.GetPixel(x, -y - 1), colliderTexColor)));
							}
							else
							{
								colTex.SetPixel(x, -y - 1, tileSpriteTexture.GetPixel(x, -y - 1));
							}
						}
					}
				}
				else
				{
					for(int x = 0; x < 16; x++)
					{
						for(int y = 0; y < 16; y++)
						{
							if(collisionPixels[y * 16 + x]) 
							{
								colTex.SetPixel(x, -y - 1, colliderTexColor);
							}
							else
							{
								colTex.SetPixel(x, -y - 1, Color.clear);
							}
						}
					}
				}
			}
			else
			{
				for(int x = 0; x < 16; x++)
				{
					for(int y = 0; y < 16; y++)
					{
						colTex.SetPixel(x, -y - 1, Color.clear);
					}
				}
			}
			
			colTex.Apply();
			
			collisionTexture = colTex;
			
			collisionSprite = Sprite.Create
			(
				collisionTexture,
				new Rect(0, 0, collisionTexture.width, collisionTexture.height), 
				new Vector2(0.5f, 0.5f), 16
			);
		}
		
		public void CreateTileTexture(Sprite sprite)
		{

		}
		// Check if the Array is empty or not the right size.
		private bool CheckHeightArray(int[] _array)
		{
			bool r_bool = true;
			
			if(_array == null) r_bool = false;
			if(_array != null && _array.Length != 16) 
			{
				r_bool = false;
				
				Debug.LogError("Array passed into Tile Editor Save Data : \"" + _array + "\", has an invalid length. An empty array has taken its place."); 
			}
			return r_bool;
		}
		
		// Check if the Array is empty or not the right size.
		private bool CheckCollisionArray(bool[] _array2D)
		{
			bool r_bool = true;
			
			if(_array2D == null) r_bool = false;
			if(_array2D != null && (_array2D.Length) != 256)
			{
				r_bool = false;
				
				Debug.LogError( $"Collsion pixel array passed into Tile Editor Save Data : \"{ _array2D }\", has an invalid size. An empty array has taken its place."); 
			} 
			return r_bool;
		}
		
		public static Texture2D textureFromSprite(Sprite sprite)
		{
			if(sprite.rect.width != sprite.texture.width){
				Texture2D newText = new Texture2D((int)sprite.rect.width,(int)sprite.rect.height);
				newText.filterMode = FilterMode.Point;
				Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x, 
															 (int)sprite.textureRect.y, 
															 (int)sprite.textureRect.width, 
															 (int)sprite.textureRect.height);
				newText.SetPixels(newColors);
				newText.Apply();
				return newText;
			} else
				return sprite.texture;
		}
		
		public Color CombineColors(Color col1, Color col2)
		{
			if(col1.a < 0.5f) return col2;
			
			float r = OverlayColorLogic(col2.r, col1.r, col2.a, col1.a);
			float g = OverlayColorLogic(col2.g, col1.g, col2.a, col1.a);
			float b = OverlayColorLogic(col2.b, col1.b, col2.a, col1.a);
			
			float a = (1 - col2.a) * col1.a + col2.a;
			
			return new Color(r, g, b, a);
		}
		
		float OverlayColorLogic(float a, float b, float a_alpha, float b_alpha)
		{
			float result = 0;
			
			result = ((1 - a_alpha) * b_alpha * b + a_alpha * a) / b_alpha;
			
			return result;
		}
	}
}

