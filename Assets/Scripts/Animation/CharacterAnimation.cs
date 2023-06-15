using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class CharacterAnimation : MonoBehaviour
	{
		public SpriteAnimations animations;
		
		public SpriteRenderer spriteRenderer;
		
		public new string animation;
		
		public string currentAnimation;
		
		public SpriteAnimation currentSpriteAnimation;
		
		public int currentFrame;
		
		public float animSpeed;
		
		float t;
		
		public int currentFrameRate = 24;
		
		void FixedUpdate()
		{
			float d = 1f;
			
			if(currentFrameRate < 1)
			{
				currentFrameRate = 1;
			}
			
			if(animSpeed < 0.1f)
			{
				animSpeed = 0.1f;
			}
			
			else
			{
				if(animation!=currentAnimation)
				{	
					d = 1f / (currentFrameRate);
				}
				else
				{	
					d = 1f / (currentFrameRate * animSpeed);
				}
			}
			
			t += 1/60f;
			
			while(t >= d)
			{
				t -= d;
				if(animation != null)
				{
					if(animations.GetAnim(animation) != null)
					{
						OnSubImageChange();
					}
					
				}
			}
		}
		
		public void PlayAnim(string name, float speed, float time = 0)
		{
			float t = Mathf.Clamp(time, 0f, 1f);
			if(name != animation)
			{
				OnSubImageChange();
				t = 0;
				animation = name;
				currentFrame = Mathf.FloorToInt(animations.GetAnim(animation).frames.Count * t);
			}			
			else if(t != 0)
			{
				currentFrame = Mathf.FloorToInt(animations.GetAnim(animation).frames.Count * t);
			}
			
			animSpeed = speed;
			
		}
		
		void OnSubImageChange()
		{
			if(currentAnimation != animation)
			{
				currentAnimation = animation;
			}
			UpdateCurrentAnimation();
		}
		
		void UpdateCurrentAnimation()
		{
			currentSpriteAnimation = animations.GetAnim(currentAnimation);
			
			if(currentFrame < currentSpriteAnimation.frames.Count)
			{
				spriteRenderer.sprite = currentSpriteAnimation.frames[currentFrame];
			}
			
			if(currentFrame >= (currentSpriteAnimation.frames.Count - 1))
			{
				if(currentSpriteAnimation.looping == true)
				{
					currentFrame = 0;
				}
			}
			else
			{
				currentFrame++;
			}
			
		}
	}
}

