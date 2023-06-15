using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class Ring : MonoBehaviour
	{
		public Player player;
		public Hitbox playerHitbox;
		public Hitbox ringHitbox;
		
		public CharacterAnimation animator1;
		public CharacterAnimation animator2;
		
		bool beenCollected = false;
		bool start = false;
		
		public void Start()
		{
			player = GameObject.FindWithTag("Player").GetComponent<Player>();
			playerHitbox = player.hitboxRect.GetComponent<Hitbox>();
			ringHitbox = this.gameObject.GetComponent<Hitbox>();
		}
		
		public void RingUpdate(float stepDelta)
		{	
			if(ringHitbox != null && playerHitbox != null)
			{
				if(ringHitbox.OverlappingWith(playerHitbox) && !beenCollected)
				{	
					OnCollect();
					beenCollected = true;
				}
				
				if(animator2.currentAnimation == "ringSparkle2")
				{
					if((animator2.currentFrame) == (animator2.currentSpriteAnimation.frames.Count - 1))
					{
						Destroy(this.gameObject);
					}
				}
			}
		}
		
		void OnCollect()
		{
			PhysicsManager.Instance.rings ++;
			animator1.animation = "ringSparkle1";
			animator2.animation = "ringSparkle2";
			
			SoundManager.Instance.ring.Stop();
			SoundManager.Instance.ring.Play();
		}
	}
}