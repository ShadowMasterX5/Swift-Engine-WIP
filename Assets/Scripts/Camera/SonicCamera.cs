using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class SonicCamera : MonoBehaviour
	{
		public float zOffset;

		Vector2 pos;
		Vector2 boxPos;
		
		public Vector3 oldPosition;
		
		bool catchingUp;
		bool lastAirBorn;
		float lastYSpeed;
		
		public float camDelay;
		
		Vector2 delayVelocity;

		public float camSpeedCap;
		
		public Vector2 CameraBox;
		
		// list of previous player positions, used to delay the camera by using a past player position to track.
		// saved all the way up to the last 5 seconds.
		List<Vector2> previousCameraPositions = new List<Vector2>(1);
		
		// amount of frames into the past that the camera tracks to.
		// 0 means the player position is not delayed at all.
		public int cameraPositionIndex = 0; 
		
		Vector2 undelayedPosition;
		Vector2 delayedPosition;

		Vector3 currentVelocity;
		
		Camera cam;
		void Start()
		{
			pos    = (Vector2)transform.position;
			boxPos = (Vector2)transform.position;
			delayedPosition = boxPos;
			cam = GetComponent<Camera>();
		}

		public void CameraUpdate(Player player)
		{	
			float height = 2f * cam. orthographicSize;
			float width = height * cam. aspect;

			if(!player.isCurrentPlayer) return;
			
			if(camDelay == 0) previousCameraPositions.Clear();
			
			
			
			Vector2 playerPos = player.transform.position;
			
			Vector2 focalPoint = playerPos;
			
			if(boxPos.x < focalPoint.x - CameraBox.x / 2)
			{
				boxPos.x = focalPoint.x - CameraBox.x / 2;
			}
			if(boxPos.x > focalPoint.x + CameraBox.x / 2)
			{
				boxPos.x = focalPoint.x + CameraBox.x / 2;
			}
			
			if(player.Grounded)
			{
				if(lastAirBorn)
				{	
					lastYSpeed = Mathf.Max(player.Velocity.y);
				}
				if(catchingUp)
				{
					boxPos.y = Mathf.MoveTowards(boxPos.y, focalPoint.y, (Mathf.Abs(focalPoint.y - boxPos.y) + Mathf.Abs(lastYSpeed)) * Time.deltaTime * (60f/16f));
				
					if(boxPos.y == focalPoint.y) catchingUp = false;
				}
				else
				{
					boxPos.y = focalPoint.y;
				}
			}
			else
			{
				lastAirBorn = true;
				catchingUp = true;
				if(boxPos.y < focalPoint.y - CameraBox.y / 2)
				{
					boxPos.y = focalPoint.y - CameraBox.y / 2;
				}
				if(boxPos.y > focalPoint.y + CameraBox.y / 2)
				{
					boxPos.y = focalPoint.y + CameraBox.y / 2;
				}
			}
			
			camSpeedCap = 2f;
			
			if(focalPoint.y == playerPos.y)
			{
				if(player.Velocity.magnitude >= 6) camSpeedCap = 14.5f;
				else camSpeedCap = 6;
			}
			else
			{
				camSpeedCap = 2;
			}
			
			if(boxPos.x < PhysicsManager.Instance.levelBounds.x + width/2f)
			{
				boxPos.x = PhysicsManager.Instance.levelBounds.x + width/2f;
			}
			
			if(boxPos.x > PhysicsManager.Instance.levelBounds.x + PhysicsManager.Instance.levelBounds.width - width/2f)
			{
				boxPos.x = PhysicsManager.Instance.levelBounds.x + PhysicsManager.Instance.levelBounds.width - width/2f;
			}
			
			if(boxPos.y < PhysicsManager.Instance.levelBounds.y + height/2f)
			{
				boxPos.y = PhysicsManager.Instance.levelBounds.y + height/2f;
			}
			
			if(boxPos.y > PhysicsManager.Instance.levelBounds.y + PhysicsManager.Instance.levelBounds.height - height/2f)
			{
				boxPos.y = PhysicsManager.Instance.levelBounds.y + PhysicsManager.Instance.levelBounds.height - height/2f;
			}
		
			pos = Vector2.MoveTowards(pos, boxPos, camSpeedCap * Time.deltaTime * (60f/16f));
			Vector3 roundPos = Vector2.zero;
			if(player.Grounded)
			{
				roundPos = Utilis.Vector2RoundToPixel(new Vector2(pos.x, pos.y + 1/32f), 16f);
				if(!catchingUp) roundPos.y = Mathf.Clamp(playerPos.y, PhysicsManager.Instance.levelBounds.y + height/2f, 
					PhysicsManager.Instance.levelBounds.y + PhysicsManager.Instance.levelBounds.width - height/2f);
			}
			else
			{
				roundPos = Utilis.Vector2RoundToPixel(new Vector2(pos.x, pos.y ), 16f);
			}
			
			if(PhysicsManager.Instance.debugMode)
			{
				DebugUtilis.DrawRect(new Rect(boxPos.x - (CameraBox.x/2f), boxPos.y - (CameraBox.y/2f), CameraBox.x, CameraBox.y), Color.white / 2f);
				Debug.DrawLine(boxPos + new Vector2(CameraBox.x/2, 0), boxPos - new Vector2(CameraBox.x/2, 0), Color.green / 2f);
			}
			
			if(camDelay > 0)
			{
				camDelay -= 1 * Time.deltaTime * 60f;
				if(camDelay < 0)
				{
					camDelay = 0;
				}
			}

			cameraPositionIndex = (Mathf.FloorToInt(Mathf.Clamp(camDelay - 1f, 0, Mathf.Infinity)));
			undelayedPosition = roundPos;
			oldPosition = transform.position;
			QueuePreviousPositions(this);
			delayedPosition = GetPreviousPosition(cameraPositionIndex);
			transform.position = new Vector3(delayedPosition.x, delayedPosition.y, zOffset);
		}
		
		private void QueuePreviousPositions(SonicCamera camera)
		{
			previousCameraPositions.Add(camera.undelayedPosition);
			
			while(previousCameraPositions.Count > 300)
			{
				previousCameraPositions.RemoveAt(0);
			}
		}
		
		private Vector2 GetPreviousPosition(int index)
		{	
			List<Vector2> posList = new List<Vector2>(previousCameraPositions);
			posList.Reverse();
			if(index < (posList.Count - 1))
			{
				return posList[index];
			}
			else
			{
				return posList[posList.Count - 1];
			}
		}
	}	
}

