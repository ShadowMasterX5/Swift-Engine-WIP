using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SonicFramework
{
	public enum Direction
	{
		down = 0,
		right = 1,
		up = 2,
		left = 3,
	}
	
	[System.Serializable]
	public class Sensor
	{
		public Vector2 startPos;
		
		public Vector2 dir;
		
		public float stickRadius;
		
		public bool active;
		
		public bool hitJumpThroughPlatform;
		
		public Color color;

		public int layerMask;

		public SensorInfo info;
		
		public Sensor(Vector2 _startPos, Vector2 _dir, Color _color, Player player, float _stickRadius = 0f, bool _active = true, bool _hitJumpThroughPlatform = false, int _layerMask = Physics2D.DefaultRaycastLayers, float _minDepth = 0f)
		{
			startPos = _startPos;
			dir = _dir;
			stickRadius = _stickRadius;
			layerMask = _layerMask;
			active = _active;
			hitJumpThroughPlatform = _hitJumpThroughPlatform;
			
			if(!active)
			{
				_color *= 0.5f;
			}
			
			color = _color;
			
			info = GetSensorInfo(_startPos, _dir, _color, stickRadius, _active, _hitJumpThroughPlatform, _layerMask, player, _minDepth);
		}
		
		public void DrawRay(Player player, Vector2 RayDebugOffset, float lengthOffset = 0)
		{
			Direction sensorDirection = SensorUtilis.GetSensorDirection(dir);
			
			Vector2 anchor = Vector2.zero;
				
			anchor = startPos + dir;
			
			anchor += RayDebugOffset;
			
			Vector2 start = anchor - dir - (dir.normalized * lengthOffset);
			
			Debug.DrawLine(start, anchor, color);
			Debug.DrawLine(anchor, anchor - (dir.normalized * 1/16f), Color.white * 2);
			
			if(stickRadius != 0)
				Debug.DrawLine(anchor  + (dir.normalized * (1/32f)), anchor + (dir.normalized * (stickRadius + (1/32f))), color/2);
		}
		
		private Vector2 RoundSensorPosition(Vector2 pos)
		{
			return new Vector2(Mathf.Round(pos.x*16f)/16f, Mathf.Floor(pos.y*16f)/16f);
		}
		
		public SensorInfo GetInfoFromSonicTiles(Tilemap hitTilemap, RaycastHit2D hit, Vector2 _startPos, Vector2 _dir, float _stickRadius, int _LayerMask, Player player)
		{
			SensorInfo sensorInfo = new SensorInfo();
		
			Direction  sensorDirection = SensorUtilis.GetSensorDirection(_dir);
			Vector2Int sensorDirVector = SensorUtilis.GetSensorDirectionVector(sensorDirection);
			
			float stickRadius = _stickRadius;
			
			Vector2 anchorPoint = _startPos + _dir + _dir.normalized * stickRadius;
			
			Vector2 tipPoint = _startPos + _dir;
			
			Vector2 point = anchorPoint;
			
			Vector2Int tilePosition      = Vector2Int.zero;
			SonicTile sonicTile          = null;
			Vector2 tileRelativePosition = Vector2.zero;
			
			Tilemap tilemap = hitTilemap;
			
			Collider2D overlapingAnchor = Physics2D.OverlapPoint(anchorPoint - _dir.normalized * -0.001f, _LayerMask);
			Collider2D overlapingTip = Physics2D.OverlapPoint(tipPoint - _dir.normalized * -0.001f, _LayerMask);
		
			if(overlapingAnchor)
			{
				if(player.cachedTilemapCollider != overlapingAnchor)
				{
					CacheTilemapInfo(player, overlapingAnchor);
				}
				tilemap = player.cachedTilemap;
				if(tilemap == null) tilemap = hitTilemap;
				tilePosition = Utilis.Vector2FloorInt(point - _dir.normalized * (- 0.001f));
				sonicTile = tilemap.GetTile((Vector3Int)tilePosition - Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position)) as SonicTile;
				
				tileRelativePosition = (point - tilePosition) * 16f;
			}
			else if(overlapingTip)
			{
				point = tipPoint;
				if(player.cachedTilemapCollider != overlapingTip)
				{
					CacheTilemapInfo(player, overlapingTip);
				}
				tilemap = player.cachedTilemap;
				if(tilemap == null) tilemap = hitTilemap;
				tilePosition = Utilis.Vector2FloorInt(point - _dir.normalized * (-0.001f));
				sonicTile = tilemap.GetTile((Vector3Int)tilePosition - Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position)) as SonicTile;
				tileRelativePosition = (point - tilePosition) * 16f;
			}
			
			TileTransform tileTransform = new TileTransform();
			
			if(sonicTile != null)
			{	
				bool hitSomething = false;
				
				tileTransform = SensorUtilis.GetTileTransform(tilePosition - (Vector2Int)Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position), tilemap);
				
				int defaultDetectedHeight = SensorUtilis.GetHeightFromArray(point, tilePosition, sonicTile, sensorDirection, tileTransform);
					
				int detectedHeight = defaultDetectedHeight;
				
				bool didRegression = false;
				for(int i = 0; i < 1; i++) 
				{
					if(defaultDetectedHeight == 16 || (hitTilemap != tilemap)) // regression
					{
						Vector2Int tilePositionPrev = tilePosition;
						DoRegression(sensorDirVector, sensorDirection, point, player, _LayerMask, stickRadius, ref tilemap, ref tilePosition, ref tileRelativePosition, ref sonicTile, ref tileTransform, ref detectedHeight);
						if(tilePosition != tilePositionPrev)
							didRegression = true;
					}
					
					if(defaultDetectedHeight == 0) // extension
					{	
						DoExtension(sensorDirVector, sensorDirection, point, player, _LayerMask, stickRadius, ref tilemap, ref tilePosition, ref tileRelativePosition, ref sonicTile, ref tileTransform, ref detectedHeight);
					}
				}
				player.cachedTilemap = tilemap;
				
				SonicObject sonicObject = player.cachedSonicObject;
				bool hitPlatform = false;
				
				if(didRegression && (sonicObject != null && sonicObject.isJumpThrough) && sonicTile.sonicTileData.flagged)
				{
					DoExtension(sensorDirVector, sensorDirection, point, player, _LayerMask, stickRadius, ref tilemap, ref tilePosition, ref tileRelativePosition, ref sonicTile, ref tileTransform, ref detectedHeight);
					hitPlatform = true;
				}
				
				if(sensorDirection == Direction.down) // Sensor is pointing down.
				{		
					if((tileRelativePosition.y) <= detectedHeight)
					{
						hitSomething = true;

						sensorInfo.point    = new Vector2(tileRelativePosition.x / 16f + tilePosition.x, (detectedHeight / 16f) + tilePosition.y + stickRadius);
						sensorInfo.distance = Mathf.Abs(startPos.y - (detectedHeight / 16f + tilePosition.y));
					}
				}
				
				if(sensorDirection == Direction.left) // Sensor is pointing left.
				{
					if((tileRelativePosition.x) <= detectedHeight)
					{
						hitSomething = true;

						sensorInfo.point    = new Vector2(detectedHeight / 16f + tilePosition.x + stickRadius, (tileRelativePosition.y / 16f) + tilePosition.y);
						sensorInfo.distance = Mathf.Abs(startPos.x - (detectedHeight / 16f + tilePosition.x));
					}
				}
				
				if(sensorDirection == Direction.up) // Sensor is pointing up.
				{
					if((16 - tileRelativePosition.y) <= detectedHeight)
					{
						hitSomething = true;

						sensorInfo.point    = new Vector2(tileRelativePosition.x / 16f + tilePosition.x, (16 - detectedHeight) / 16f + tilePosition.y - stickRadius);
						sensorInfo.distance = Mathf.Abs(startPos.y - ((16 - detectedHeight) / 16f + tilePosition.y));
					}
				}
				
				if(sensorDirection == Direction.right) // Sensor is pointing right.
				{
					if((16 - tileRelativePosition.x) <= detectedHeight + 1)
					{
						hitSomething = true;

						sensorInfo.point    = new Vector2((16 - detectedHeight) / 16f + tilePosition.x - stickRadius, (tileRelativePosition.y / 16f) + tilePosition.y);
						sensorInfo.distance = Mathf.Abs(startPos.x - ((16 - detectedHeight) / 16f + tilePosition.x));
					}
				}
				
				
				if(hitSomething && !hitPlatform)
				{
					float a = SensorUtilis.GetTileAngle(tileTransform, sonicTile);
					sensorInfo.collided  = active? true : false;
					sensorInfo.sonicTile = sonicTile;
					sensorInfo.flagged   = sonicTile.sonicTileData.flagged;
					sensorInfo.hitObject = sonicObject;
					sensorInfo.normal    = (Vector2)(Quaternion.Euler(0, 0, a) * Vector2.up); // I dont even know.
					sensorInfo.angle     = a;
					sensorInfo.invalid   = false;
					sensorInfo.hit       = hit;
				}	
				else
				{
					sensorInfo.invalid = true;
				}
			}
			return sensorInfo;
		}
		
		private void DoRegression(Vector2Int sensorDirVector, Direction sensorDirection, Vector2 anchorPoint, Player player, int layerMask, float stickRadius, ref Tilemap tilemap, ref Vector2Int tilePos, ref Vector2 relativePosition, ref SonicTile sonicTile, ref TileTransform transform, ref int detectedHeight)
		{
			Collider2D newTilemapCheck = Physics2D.OverlapPoint(tilePos + new Vector2(0.5f, 0.5f) - (Vector2)sensorDirVector, layerMask);
			if(newTilemapCheck)
			{
				Tilemap tm = tilemap;
				if(newTilemapCheck != player.cachedTilemapCollider)
				{
					CacheTilemapInfo(player, newTilemapCheck);
				}
				tilemap = player.cachedTilemap;
				if(tilemap == null)
				{
					player.cachedTilemap = tm;
					tilemap = tm;
				}
			}
					
			if((tilemap.GetTile((Vector3Int)Utilis.Vector2FloorInt((tilePos - (Vector2)sensorDirVector) - (Vector2Int)Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position))) as SonicTile) != null)
			{
				var cacheTilePos          = tilePos;
				var cacheTilemap          = tilemap;
				var cacheSonicTile        = sonicTile;
				var cacheRelativePosition = relativePosition;
				var cacheTransform        = transform;
				var cacheDetectedHeight   = detectedHeight;

				tilePos = Utilis.Vector2FloorInt(tilePos - (Vector2)sensorDirVector);
				sonicTile = tilemap.GetTile((Vector3Int)tilePos - Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position)) as SonicTile;
				if(sonicTile == null)
				{
					tilePos   = cacheTilePos;
					tilemap   = cacheTilemap;
					sonicTile = cacheSonicTile;
					
					return;
				}
				relativePosition = (anchorPoint - tilePos) * 16f;
				transform = SensorUtilis.GetTileTransform(tilePos - (Vector2Int)Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position), tilemap);

				int height = SensorUtilis.GetHeightFromArray(anchorPoint, tilePos, sonicTile, sensorDirection, transform);

				if(height == 0)
				{
					tilePos          = cacheTilePos;
					sonicTile        = cacheSonicTile;
					relativePosition = cacheRelativePosition;
					transform        = cacheTransform;
					detectedHeight   = cacheDetectedHeight;
				}
				else
				{
					detectedHeight = height;
				}
			}
		}

		private void DoExtension(Vector2Int sensorDirVector, Direction sensorDirection, Vector2 anchorPoint, Player player, int layerMask, float stickRadius, ref Tilemap tilemap, ref Vector2Int tilePos, ref Vector2 relativePosition, ref SonicTile sonicTile, ref TileTransform transform, ref int detectedHeight)
		{
			Collider2D newTilemapCheck = Physics2D.OverlapPoint(tilePos + new Vector2(0.5f, 0.5f) + (Vector2)sensorDirVector, layerMask);
			if(newTilemapCheck)
			{
				Tilemap tm = tilemap;
				if(newTilemapCheck != player.cachedTilemapCollider)
				{
					CacheTilemapInfo(player, newTilemapCheck);
				}
				tilemap = player.cachedTilemap;
				if(tilemap == null)
				{
					player.cachedTilemap = tm;
					tilemap = tm;
				}
			}
			
			if((tilemap.GetTile((Vector3Int)Utilis.Vector2FloorInt((tilePos + (Vector2)sensorDirVector) - (Vector2Int)Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position))) as SonicTile) != null)
			{
				var cacheTilePos          = tilePos;
				var cacheTilemap          = tilemap;
				var cacheSonicTile        = sonicTile;
				var cacheRelativePosition = relativePosition;
				var cacheTransform        = transform;
				var cacheDetectedHeight   = detectedHeight;

				tilePos = Utilis.Vector2FloorInt(tilePos + (Vector2)sensorDirVector);
				sonicTile = tilemap.GetTile((Vector3Int)tilePos - Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position)) as SonicTile;
				if(sonicTile == null)
				{
					tilePos   = cacheTilePos;
					tilemap   = cacheTilemap;
					sonicTile = cacheSonicTile;
					return;
				}
				relativePosition = (anchorPoint - tilePos) * 16f;
				transform = SensorUtilis.GetTileTransform(tilePos - (Vector2Int)Utilis.Vector3FloorToInt(tilemap.gameObject.transform.position), tilemap);

				int height = SensorUtilis.GetHeightFromArray(anchorPoint, tilePos, sonicTile, sensorDirection, transform);

				if(height == 0)
				{
					tilePos          = cacheTilePos;
					sonicTile        = cacheSonicTile;
					relativePosition = cacheRelativePosition;
					transform        = cacheTransform;
					detectedHeight   = cacheDetectedHeight;
				}
				else
				{
					detectedHeight = height;
				}
			}
		}
		
		public SensorInfo GetInfoFromRaycastHit(RaycastHit2D hit, Vector2 _dir, float _stickRadius, Player player)
		{
			SensorInfo sensorInfo = new SensorInfo();
			
			sensorInfo.collided  = active? hit : false;
			sensorInfo.flagged   = false;
			sensorInfo.sonicTile = null;
			sensorInfo.point     = hit.point - (_dir * _stickRadius);
			sensorInfo.distance	 = hit.distance;
			sensorInfo.hitObject = hit.collider.gameObject.GetComponent<SonicObject>();
			sensorInfo.normal    = (Vector2)(Quaternion.Euler(0,0,-Vector2.SignedAngle(hit.normal,Vector2.up)) * Vector2.up); // I dont even know.
			sensorInfo.angle     = Mathf.Round(-Vector2.SignedAngle(hit.normal, Vector2.up)*16)/16;
			sensorInfo.invalid   = false;
			sensorInfo.hit       = hit;
			
			return sensorInfo;
		}
		
		private void CacheTilemapInfo(Player player, Collider2D collider)
		{
			player.cachedTilemapCollider = collider;
			player.cachedTilemap = collider.gameObject.GetComponent<Tilemap>();
			player.cachedSonicObject = collider.gameObject.GetComponent<SonicObject>();
		}
		
		private SensorInfo GetInfoFromMapEdge(Vector2 startPos, Vector2 dir, float stickRadius)
		{
			Vector2 anchorPoint = startPos + dir + dir.normalized * stickRadius;
			bool hitEdge = false;
			SensorInfo sensorInfo = new SensorInfo();
			
			if(anchorPoint.x < PhysicsManager.Instance.levelBounds.x)
			{
				hitEdge = true;
				sensorInfo.point = new Vector2(PhysicsManager.Instance.levelBounds.x, anchorPoint.y);
				sensorInfo.distance = Mathf.Abs(startPos.x - sensorInfo.point.x);
			}
			
			if(anchorPoint.x > PhysicsManager.Instance.levelBounds.x + PhysicsManager.Instance.levelBounds.width)
			{
				hitEdge = true;
				sensorInfo.point = new Vector2(PhysicsManager.Instance.levelBounds.x + PhysicsManager.Instance.levelBounds.width, anchorPoint.y);
				sensorInfo.distance = Mathf.Abs(startPos.x - sensorInfo.point.x);
			}
			
			if(anchorPoint.y < PhysicsManager.Instance.levelBounds.y)
			{
				hitEdge = true;
				sensorInfo.point = new Vector2(startPos.x, PhysicsManager.Instance.levelBounds.y + stickRadius);
				sensorInfo.distance = Mathf.Abs(startPos.y - sensorInfo.point.y);
			}
			
			if(anchorPoint.y > PhysicsManager.Instance.levelBounds.y + PhysicsManager.Instance.levelBounds.height)
			{
				hitEdge = true;
				sensorInfo.point = new Vector2(startPos.x, PhysicsManager.Instance.levelBounds.y + PhysicsManager.Instance.levelBounds.height - stickRadius);
				sensorInfo.distance = Mathf.Abs(startPos.y - sensorInfo.point.y);
			}
			
			if(hitEdge)
			{
				sensorInfo.collided  = active && hitEdge;
				sensorInfo.flagged   = true;
				sensorInfo.sonicTile = null;
				sensorInfo.hitObject = null;
				sensorInfo.normal    = Vector2.zero; // I dont even know.
				sensorInfo.angle     = 0;
				sensorInfo.invalid   = false;
			}	
			else
			{
				sensorInfo.invalid = true;
			}
			return sensorInfo;
		}
		
		public SensorInfo GetSensorInfo(Vector2 _startPos, Vector2 _dir, Color _color, float _stickRadius, bool _active, bool _hitJumpThroughPlatform, int _layerMask, Player player, float _minDepth)
		{
			SensorInfo sensorInfo = new SensorInfo();
			RaycastHit2D hit = Physics2D.Raycast(_startPos, _dir.normalized, _dir.magnitude + _stickRadius, _layerMask, _minDepth);
			
			if(!PhysicsManager.Instance.levelBounds.Contains(_startPos + _dir + _dir.normalized * _stickRadius))
			{
				sensorInfo = GetInfoFromMapEdge(_startPos, _dir, _stickRadius);
			}	
			else if(hit)
			{
				if(player.cachedTilemapCollider != hit.collider)
				{
					CacheTilemapInfo(player, hit.collider);
				}
				Tilemap tilemap = player.cachedTilemap;
				if(tilemap != null)
				{
					sensorInfo = GetInfoFromSonicTiles(tilemap, hit, _startPos, _dir, _stickRadius, _layerMask, player);
				}
				else
				{
					sensorInfo = GetInfoFromRaycastHit(hit, _dir.normalized, _stickRadius, player);
				}
			} 
			else
			{
				sensorInfo.invalid = true;
			}
			
			return sensorInfo;
		}
		
		public static Sensor Duplicate(Sensor sensor, Player player)
		{
			Sensor sensorDupe = new Sensor(sensor.startPos, sensor.dir, sensor.color, player, sensor.stickRadius, sensor.active, sensor.hitJumpThroughPlatform, sensor.layerMask);
			
			return sensorDupe;
		}
		
		public static Sensor TestCheckDupe(Sensor sensor, Player player, float extraLength)
		{
			Sensor sensorDupe = new Sensor(sensor.startPos, sensor.dir +(extraLength * sensor.dir.normalized), sensor.color, player, sensor.stickRadius, true, sensor.hitJumpThroughPlatform, sensor.layerMask);
			
			return sensorDupe;
		}
	}
	[System.Serializable]
	public struct SensorInfo
	{
		// Position where the ray collides with a surface.
		public Vector2 point;
		
		// Tile that the ray hits.
		public SonicTile sonicTile;
		
		// Distance from where the collider starts to where it hits.
		public float distance;
		
		// The normal vector on the point's surface.
		public Vector2 normal;
		
		// Object the player hits.
		public SonicObject hitObject;
		
		// Whether or not the tile is flagged.
		public bool flagged;
		
		// Whether or not the sensor collided.
		public bool collided;

		// Angle in degrees of a surface's normal.
		public float angle;
		
		// Whether or not if the info is valid.
		public bool invalid;

		// Raycast hit info of the collision.
		public RaycastHit2D hit;
	}
	
	public struct TileTransform
	{
		public Vector2 scale;
		
		public bool flipped;
		
		public float rotation;	
	}
}

