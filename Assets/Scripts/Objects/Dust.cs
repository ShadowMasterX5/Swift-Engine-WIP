using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	[RequireComponent(typeof(CharacterAnimation))]
	public class Dust : MonoBehaviour
	{
		public Player player;
		public string currentAnim;
		
		SpriteRenderer spriteRenderer;
		CharacterAnimation animator;
		
		Vector2 offset;
		float rotation;
		
		public Dust(Player _player, string _currentAnim)
		{
			player = _player;
			currentAnim = _currentAnim;
		}
		
		void Awake()
		{
			animator = GetComponent<CharacterAnimation>();	
			spriteRenderer = GetComponent<SpriteRenderer>();	
		}
		
		public void instantiate()
		{
			UpdateDust();
			UpdateDustPosition();
		}

		// Update is called once per frame
		void UpdateDust()
		{
			animator.animation = currentAnim;
			animator.currentAnimation = currentAnim;
			offset = Vector2.zero;
			rotation = 0;
			if(currentAnim == "spindashDust")
			{
				spriteRenderer.flipX = !player.lookingRight;
				offset.y += 3.5f/16f;
				offset.x += -2f + (2/16f);
				if(spriteRenderer.flipX) offset.x = -offset.x;
			}
			
			if(currentAnim == "dropdashDust")
			{
				spriteRenderer.flipX = !player.lookingRight;
				offset.y += 1 + 9.5f/16f;
				offset.x += -2f + (2/16f);
				if(spriteRenderer.flipX) offset.x = -offset.x;
			}
			
			if(currentAnim == "skiddingDust")
			{
				spriteRenderer.flipX = !player.lookingRight;
				offset.y += -0.5f;
				offset.x += 0.4f;
				if(spriteRenderer.flipX) offset.x = -offset.x;
			}
		}
		
		void Update()
		{
			if(currentAnim == "spindashDust")
			{
				UpdateDust();
			}
			
			if(currentAnim == "dropdashDust" || currentAnim == "skiddingDust")
			{
				if((animator.currentFrame) == (animator.currentSpriteAnimation.frames.Count - 1))
				{
					Destroy(this.gameObject);
				}
			}
		}
		
		void UpdateDustPosition()
		{
			transform.position = player.Position + offset;
			transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);	
		}
	}
}