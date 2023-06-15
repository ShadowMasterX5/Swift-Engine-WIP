using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SonicFramework
{
	public class LoopCollision : MonoBehaviour
	{
		private Player player;
		public Hitbox playerHitbox;
		
		public TilemapCollider2D leftCollider;
		public TilemapCollider2D rightCollider;
		
		public Hitbox leftEnter;
		public Hitbox topSwitch;
		public Hitbox rightEnter;
		
		bool rightSideOn;
		
		void Start()
		{
			player = GameObject.FindWithTag("Player").GetComponent<Player>();
		}
		
		void Update()
		{
			if(leftEnter.OverlappingWith(playerHitbox))
			{
				rightSideOn = true;
			}
			
			if(topSwitch.OverlappingWith(playerHitbox))
			{
				if(player.Grounded)
				{
					if(player.GroundSpeed > 0)
					{
						rightSideOn = false;
					}
					else
					{
						rightSideOn = true;
					}
				}
			}
			
			if(rightEnter.OverlappingWith(playerHitbox))
			{
				rightSideOn = false; 
			}
			
			if(rightSideOn)
			{
				leftCollider.enabled = false;
				rightCollider.enabled = true;
			}
			else
			{
				leftCollider.enabled = true;
				rightCollider.enabled = false;
			}
		}
	}
}

