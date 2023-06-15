using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	[RequireComponent(typeof(SonicObject))]
	[RequireComponent(typeof(CharacterAnimation))]
	[RequireComponent(typeof(BoxCollider2D))]
	[ExecuteAlways]
	[System.Serializable]
	public class Spring : MonoBehaviour
	{
		public enum SpringMode
		{
			yellow = 0,
			red = 1,
		}
		
		public enum SpringDirection
		{
			up = 0,
			upRight = 1,
			right = 2,
			downRight = 3,
			down = 4,
			downLeft = 5,
			left = 6,
			upLeft = 7,
		}

		private Player player;
		private SonicObject obj;
		public SpringMode mode = SpringMode.yellow;
		public SpriteAnimations yellowAnimations;
		public SpriteAnimations redAnimations;
		public SpringDirection direction = SpringDirection.up;
		public CharacterAnimation characterAnimator;
		public new BoxCollider2D collider;
		public SpriteRenderer spriteRenderer;
		bool activated;
		Player activatedPlayer;
		
		[HideInInspector] public List<Player> AttachedPlayers;
		
		public void Awake()
		{
			obj = GetComponent<SonicObject>();
			characterAnimator = GetComponent<CharacterAnimation>();
			collider = GetComponent<BoxCollider2D>();
		}
		
		private SpringDirection directionFromAngle(float ang)
		{
			var angle = Utilis.WrapAngleFromNegative180To180(ang);
			
			if(Mathf.Abs(angle) <= 22.5f) return SpringDirection.up;
			if(Mathf.Abs(angle) > 157.5f) return SpringDirection.down;
			if(angle <= 67.5f && Mathf.Sign(angle) ==  1) return SpringDirection.upLeft;
			if(angle > -67.5f && Mathf.Sign(angle) == -1) return SpringDirection.upRight;
			if(angle >  67.5f && angle <=  112.5f) return SpringDirection.left;
			if(angle < -67.5f && angle >= -112.5f) return SpringDirection.right;
			if(Mathf.Abs(angle) > 112.5f && Mathf.Sign(angle) ==  1) return SpringDirection.downLeft;
			if(Mathf.Abs(angle) > 112.5f && Mathf.Sign(angle) == -1) return SpringDirection.downRight;
			
			return SpringDirection.up;
		}
		
		public void Update()
		{
			direction = directionFromAngle(transform.eulerAngles.z);
			
			if(mode == SpringMode.yellow) characterAnimator.animations = yellowAnimations;
			else characterAnimator.animations = redAnimations;
			
			spriteRenderer = characterAnimator.spriteRenderer;
			if(!Application.isPlaying)
			{		
				spriteRenderer.sprite = characterAnimator.animations.GetAnim("springIdle").frames[0];
			}
		}
		
		public void SpringUpdate(float stepDelta)
		{	
			if(obj.collidedPlayers.Count > 0)
			{
				for(int i = 0; i < obj.collidedPlayers.Count; i++) 
				{
					AttachPlayerToSpring(obj.collidedPlayers[i], obj.collidedSensors[i]);
				}
			}
			
			if(AttachedPlayers.Count > 0)
			{
				Activate();
			}
			
			if(activated)
			{
				characterAnimator.PlayAnim("springActivate", 1f);
			}
			
			if(characterAnimator.currentAnimation == "springActivate")
			{
				if(characterAnimator.currentFrame >= 2)
				{
					collider.enabled = true;
				}
				
				if((characterAnimator.currentFrame) == (characterAnimator.currentSpriteAnimation.frames.Count - 1))
				{
					characterAnimator.PlayAnim("springIdle", 1);
					
					activated = false;
				}
			}
		}

		void Activate()
		{
			float springForce = mode == SpringMode.yellow? 10f : 16f;
			activated = true;
			characterAnimator.PlayAnim("springActivate", 1f, 0.1f);
			SoundManager.Instance.spring.Stop();
			SoundManager.Instance.Spring();
			foreach(Player player in AttachedPlayers)
			{
				player.bouncedOffSpring = true;
				player.springBounceStart = true;
				player.cam.camDelay = 1;
				bool startedRoll = player.Rolling;
				if(direction == SpringDirection.up || direction == SpringDirection.down)
				{
					player.Rolling = false;
					player.Velocity = new Vector2(player.Velocity.x, springForce * transform.up.y);
					player.Grounded = false;
					player.springType = SpringType.vertical;
					player.Position = new Vector2(player.Position.x, ((Vector2)transform.position + (Vector2)(transform.up * player.heightRadius) + (Vector2)(transform.up * 0.25f)).y);
				}
				else if(direction == SpringDirection.left || direction == SpringDirection.right)
				{
					Debug.Log(player.Rolling);
					if(player.Grounded)
					{
						if(Mathf.Abs(player.GroundAngle) > 90f)
						{
							player.GroundSpeed = -springForce * transform.up.x;
							player.groundMode = GroundMode.ceiling;
							player.lookingRight = Mathf.Sign(springForce * transform.up.x) == 1? false : true;
						}
						else
						{
							player.lookingRight = Mathf.Sign(springForce * transform.up.x) == 1? true : false;
							player.GroundSpeed = springForce * transform.up.x;
						}
						player.oldGroundSpeed = player.GroundSpeed;
					}
					else
					{
						player.Velocity = new Vector2(springForce * transform.up.x, player.Velocity.y);
						player.Grounded = false;
					}
					if(startedRoll) player.Rolling = true;
					player.springType = SpringType.horizontal;	
					player.controlLockTimer = 16f;
					player.Position = new Vector2(((Vector2)transform.position + (Vector2)(transform.up * player.heightRadius) + (Vector2)(transform.up * 0.25f)).x, player.Position.y);
				}
				else
				{
					player.Rolling = false;
					player.springType = SpringType.diagonal;	
					player.Velocity = springForce * transform.up;
					player.Grounded = false;
				}
				
				collider.enabled = false;
				activatedPlayer = player;
			}
			
			AttachedPlayers.Clear();
		}
		
		public void AttachPlayerToSpring(Player player, Sensor sensor)
		{
			if(player == null) return;
			
			if(AttachedPlayers.Contains(player)) return;
			
			if(direction == SpringDirection.up || direction == SpringDirection.upLeft || direction == SpringDirection.upRight)
			{
				if(sensor.dir.normalized != -Vector2.up)
				return;
			}
			
			if(direction == SpringDirection.down || direction == SpringDirection.downLeft || direction == SpringDirection.downRight)
			{
				if(sensor.dir.normalized != Vector2.up)
				return;
			}
			
			if(direction == SpringDirection.left)
			{
				if(sensor.dir.normalized != Vector2.right)
				return;
			}
			
			if(direction == SpringDirection.right)
			{
				if(sensor.dir.normalized != -Vector2.right)
				return;
			}
			
			AttachedPlayers.Add(player);
		}
	}
}

