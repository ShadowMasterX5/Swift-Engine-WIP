using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SonicFramework
{
	public class CrossingPaths : MonoBehaviour
	{
		public Player player;
		public Hitbox playerHitbox;
		
		public TilemapCollider2D firstPath;
		public TilemapCollider2D secondPath;
		
		public Hitbox firstEntrance1;
		public Hitbox firstEntrance2;
		public Hitbox secondEntrance1;
		public Hitbox secondEntrance2;
		
		bool firstPathOn;
		
		void Start()
		{
			player = GameObject.FindWithTag("Player").GetComponent<Player>();
		}
		
		void Update()
		{
			if(firstEntrance1.OverlappingWith(playerHitbox) || firstEntrance2.OverlappingWith(playerHitbox))
			{
				firstPathOn = true;
			}
			
			if(secondEntrance1.OverlappingWith(playerHitbox) || secondEntrance2.OverlappingWith(playerHitbox))
			{
				firstPathOn = false;
			}
			
			if(firstPathOn)
			{
				secondPath.enabled = false;
				firstPath.enabled = true;
			}
			else
			{
				secondPath.enabled = true;
				firstPath.enabled = false;
			}
		}
	}
}