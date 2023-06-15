using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class SonicObject : MonoBehaviour
	{
		public bool isJumpThrough;
		public bool tunnel;
		
		[HideInInspector] public new BoxCollider2D collider;
		[HideInInspector] public List<Player> collidedPlayers;
		[HideInInspector] public List<Sensor> collidedSensors;
		[HideInInspector] public bool colliding;
		
		[HideInInspector] public bool isMovingPlatform;
		[HideInInspector] public MovingPlatform platformScript;
		
		[HideInInspector] public bool isSpring;
		[HideInInspector] public Spring spring;
		
		[HideInInspector] public bool isRing;
		[HideInInspector] public Ring ring;
		
		[HideInInspector] public List<Player> AttachedPlayers;

		[HideInInspector] public Vector2 velocity;
		
		public void Awake()
		{
			collider = GetComponent<BoxCollider2D>();
			
			//platform
			platformScript = GetComponent<MovingPlatform>();
			if(platformScript != null)
			{
				isMovingPlatform = true;
			}
			else
			{
				isMovingPlatform = false;
			}
			
			//spring
			spring = GetComponent<Spring>();
			if(spring != null)
			{
				isSpring = true;
			}
			else
			{
				isSpring = false;
			}
			
			//ring
			ring = GetComponent<Ring>();
			if(ring != null)
			{
				isRing = true;
			}
			else
			{
				isRing = false;
			}
		}
		
		public void ObjectUpdate(float stepDelta)
		{
			if(isMovingPlatform)
			{
				platformScript.PlatformMovement(stepDelta, this);
			}
			
			if(isSpring)
			{
				spring.SpringUpdate(stepDelta);
			}
			
			if(isRing)
			{
				ring.RingUpdate(stepDelta);
			}
			
			if(collider != null)
			{
				colliding = false;
				
				if(collidedPlayers.Count > 0)
				{
					colliding = true;
				}
				
				collidedPlayers.Clear();
				collidedSensors.Clear();
			}
		}
		
		public void CollidePlayerWithObject(Player player, Sensor sensor)
		{
			if(player == null) return;
			
			if(collidedPlayers.Contains(player)) return;
			
			if(collider == null) return;
			
			collidedPlayers.Add(player);
			
			collidedSensors.Add(sensor);
		}
	}	
}

