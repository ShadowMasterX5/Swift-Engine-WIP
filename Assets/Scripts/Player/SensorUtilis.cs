using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SonicFramework
{
	public class SensorUtilis : MonoBehaviour
	{
		public static Direction GetSensorDirection(Vector2 d)
		{
			if(Mathf.Abs(d.x) >= Mathf.Abs(d.y))
			{
				if(d.x > 0)
				{
					return Direction.right;
				}
				else
				{
					return Direction.left;
				}
			}
			else
			{
				if(d.y >= 0)
				{
					return Direction.up;
				}
				else
				{
					return Direction.down;
				}
			}
		}
		
		public static Vector2Int GetSensorDirectionVector(Direction sensorDirection)
		{
			switch (sensorDirection) {
				case Direction.down:
					return new Vector2Int(0, -1);
					
				case Direction.right:
					return new Vector2Int(1, 0);
					
				case Direction.up:
					return new Vector2Int(0, 1);
					
				case Direction.left:
					return new Vector2Int(-1, 0);
					
				default :
					return new Vector2Int(0, -1);
			}
		}
		
		public static TileTransform GetTileTransform(Vector2Int pos, Tilemap tilemap)
		{
			TileTransform tileTransform = new TileTransform();
			
			Matrix4x4 matrix = tilemap.GetTransformMatrix((Vector3Int)pos);
			Vector3 transformRotation = matrix.rotation.eulerAngles;
			bool flipped = transformRotation.y != 0;
			float angle = transformRotation.z;
			
			if(flipped) angle = 360 - angle;

			angle = Utilis.WrapAngleFromNegative180To180(Mathf.Round(angle*10)/10);
			
			tileTransform.scale = new Vector2(matrix.m00, matrix.m11);
			tileTransform.flipped = flipped;
			tileTransform.rotation = angle;
			
			return tileTransform;
		}
		
		public static float GetTileAngle(TileTransform transform, SonicTile sonicTile)
		{
			float detectedAngle = Utilis.WrapAngleFromNegative180To180(sonicTile.sonicTileData.surfaceAngle + transform.rotation);
			
			detectedAngle *= transform.flipped ? -1 : 1;
			
			if(transform.flipped)
			{
				if(transform.rotation == -90 || transform.rotation == 90)
				{
					detectedAngle += 180f;
				}
			}
			
			return Utilis.WrapAngleFromNegative180To180(detectedAngle);
		}
		
		public static int GetHeightFromArray(Vector2 anchorPos, Vector2Int tilePos, SonicTile tile, Direction sensorDirection, TileTransform tileTransform)
		{
			int[] useArray = new int[16];
			
			bool flipped = tileTransform.flipped;
			
			Vector2 anchorPosInTile = (anchorPos - tilePos) * 16f;
			
			switch(tileTransform.rotation)
			{
				case 0:
					switch(sensorDirection)
					{
						case Direction.down:
							useArray = tile.sonicTileData.upArray;
							break;
						case Direction.right:
							if(flipped) 
							{
								flipped = !flipped;
								useArray = tile.sonicTileData.rightArray;
								break;
							}
							flipped = !flipped;
							useArray = tile.sonicTileData.leftArray;
							break;
						case Direction.up:
							useArray = tile.sonicTileData.downArray;
							break;
						case Direction.left:
							if(flipped) 
							{
								flipped = !flipped;
								useArray = tile.sonicTileData.leftArray;
								break;
							}
							flipped = !flipped;
							useArray = tile.sonicTileData.rightArray;
							break;
					}

					break;
				case 90:
					switch(sensorDirection)
					{
						case Direction.down:
							if(flipped) 
							{
								useArray = tile.sonicTileData.leftArray;
								break;
							}
							useArray = tile.sonicTileData.rightArray;
							break;
						case Direction.right:
							flipped = !flipped;
							useArray = tile.sonicTileData.upArray;
							break;
						case Direction.up:
							if(flipped) 
							{
								useArray = tile.sonicTileData.rightArray;
								break;
							}
							useArray = tile.sonicTileData.leftArray;
							break;
						case Direction.left:
							flipped = !flipped;
							useArray = tile.sonicTileData.downArray;
							break;
					}

					break;
				case 180:
					switch(sensorDirection)
					{
						case Direction.down:
							useArray = tile.sonicTileData.downArray;
							break;
						case Direction.right:
							if(flipped) 
							{
								flipped = !flipped;
								useArray = tile.sonicTileData.leftArray;
								break;
							}
							flipped = !flipped;
							useArray = tile.sonicTileData.rightArray;
							break;
						case Direction.up:
							useArray = tile.sonicTileData.upArray;
							break;
						case Direction.left:
							if(flipped) 
							{
								flipped = !flipped;
								useArray = tile.sonicTileData.rightArray;
								break;
							}
							flipped = !flipped;
							useArray = tile.sonicTileData.leftArray;
							break;
					}

					break;
				case -90:
					switch(sensorDirection)
					{
						case Direction.down:
							if(flipped) 
							{
								useArray = tile.sonicTileData.rightArray;
								break;
							}
							useArray = tile.sonicTileData.leftArray;
							break;
						case Direction.right:
							flipped = !flipped;
							useArray = tile.sonicTileData.downArray;
							break;
						case Direction.up:
							if(flipped) 
							{
								useArray = tile.sonicTileData.leftArray;
								break;
							}
							useArray = tile.sonicTileData.rightArray;
							break;
						case Direction.left:
							flipped = !flipped;
							useArray = tile.sonicTileData.upArray;
							break;
					}

					break;
			}
			
			int horizontalAxis = 0;
			
			switch(sensorDirection)
			{
				case Direction.down:
					horizontalAxis = Mathf.FloorToInt(anchorPosInTile.x);
					break;
				case Direction.right:
					horizontalAxis = Mathf.FloorToInt(16 - anchorPosInTile.y);
					break;
				case Direction.up:
					horizontalAxis = Mathf.FloorToInt(16 - anchorPosInTile.x);
					break; 
				case Direction.left:
					horizontalAxis = Mathf.FloorToInt(anchorPosInTile.y);
					break;
			}
			
			if(horizontalAxis > 15 || horizontalAxis < 0)
			{
				return 0;
			}
			
			return !flipped ? useArray[horizontalAxis] : useArray[15 - horizontalAxis];
		}
	}
}