using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class PlayerSprite : MonoBehaviour
	{
		public Player player;
		public GameObject spriteObject;
		Vector2 playerPosition;
		Vector2 spritePosition;
		public float spriteAngle;
		public float tempAngle;
		Vector2 spriteOffset;
		public float spriteSmoothing;
		float smoothedAngle;
		float smoothAngleVelocity;
		bool lookingR;
		public float angleDifference;
		
		bool oldGrounded;
		
		public new SpriteRenderer renderer;
		
		public CharacterAnimation characterAnimator;
		
		public Object dustPrefab;

		// Start is called before the first frame update
		void Start()
		{
			dustPrefab = Resources.Load<Object>("Prefabs/Dust");
		}

		// Update is called once per frame
		void LateUpdate()
		{
			UpdateSpriteTransform();
			
			UpdateAnimations();
			
			spriteObject.transform.position = spritePosition;
			spriteObject.transform.rotation = Quaternion.AngleAxis(tempAngle, Vector3.forward);	
		}
		
		public static Vector2 RotateVector(Vector2 v, float angle)
		{
			var ca = Mathf.Cos(angle * Mathf.Deg2Rad);
			var sa = Mathf.Sin(angle * Mathf.Deg2Rad);
			
			float yOff = 0;
			if(angle!= 0) yOff = 1/32f;
			return new Vector2(ca*v.x - sa*v.y, sa*v.x + ca*v.y + yOff);
		}
		
		void UpdateAnimations()
		{
			if(SpindashAnimCheck()) return;
			if(SpringAnimCheck()) return;
			if(DropDashAnimCheck()) return;
			if(RollingAnimCheck()) return;
			if(CrouchedCheck()) return;
			if(SkidAnimCheck()) return;
			if(PushingCheck()) return;
			if(RunningAnimsCheck()) return;
		}
		
		private GameObject spindashDustObj;
		
		bool SpindashAnimCheck()
		{
			if(player.spinDashing)
			{	
				if(spindashDustObj == null)
				{
					spindashDustObj = Instantiate(dustPrefab) as GameObject;
					Dust spindashDust = spindashDustObj.GetComponent<Dust>();
					spindashDust.currentAnim = "spindashDust";
					spindashDust.player = player;
					spindashDust.instantiate();
				}
					
				characterAnimator.PlayAnim("spindash", 1);
				return true;
			}
			else if(spindashDustObj != null)
			{
				Destroy(spindashDustObj);
				spindashDustObj = null;
			}
			return false;
		}
		
		bool springAnimDone = false;
		bool springDiagonalOnce = false;
		float diagonalSpringTimer;
		bool bouncedHorizontal;
		
		bool SpringAnimCheck()
		{
			if(player.Grounded)
			{
				springDiagonalOnce = false;
				diagonalSpringTimer = 0;
				springAnimDone = false;
				bouncedHorizontal = false;
			} 
			
			if(player.bouncedOffSpring)
			{	
				if(player.springBounceStart)
				{
					springAnimDone = false;
					springDiagonalOnce = false;
					
					if(player.springType == SpringType.horizontal)
					{
						bouncedHorizontal = true;
					}
				}
				
				player.springBounceStart = false;
				if(diagonalSpringTimer > 0)
				{
					diagonalSpringTimer -= Time.deltaTime * 60f;
					if(diagonalSpringTimer < 0)
					{
						diagonalSpringTimer = 0;
					}
				}	
				
				if(player.springType == SpringType.horizontal)
				{
					if(bouncedHorizontal)
					{
						characterAnimator.PlayAnim(characterAnimator.currentAnimation, 1);
						bouncedHorizontal = false;
						player.bouncedOffSpring = false;
						return true;
					}
					else return false;	
				}
				
				if(!springAnimDone)
				{
					if(player.springType == SpringType.vertical)
					{
						characterAnimator.PlayAnim("springCorkskrew", 1);
						
						if((characterAnimator.currentFrame) == (characterAnimator.currentSpriteAnimation.frames.Count - 1))
						{
							springAnimDone = true;
							characterAnimator.PlayAnim("walk", 1);
							return false;
						}	
						return true;
					}
					
					if(player.springType == SpringType.diagonal)
					{
						characterAnimator.PlayAnim("springDiagonal", 1);
						
						if(!springDiagonalOnce)
						{
							diagonalSpringTimer = 48;
						}
						
						springDiagonalOnce = true;
						
						if(diagonalSpringTimer == 0)
						{
							springAnimDone = true;
							characterAnimator.PlayAnim("walk", 1);
							return false;
						}
						return true;
					}
				}
			}
			return false;
		}
		
		bool DropDashAnimCheck()
		{
			float dropAnimSpeed = (Mathf.Abs(player.drpspd / 6f)) + 1f;
			if(player.drpspd >= 8f)
			{
				characterAnimator.PlayAnim("dropdash", dropAnimSpeed);
				return true;
			}
			return false;
		}
		
		bool RollingAnimCheck()
		{
			float rollingAnimSpeed = (Mathf.Abs(player.GroundSpeed / 5f)) + 1f;

			if(player.IsBall)
			{
				characterAnimator.PlayAnim("roll", rollingAnimSpeed);
				return true;
			}
			return false;
		}
		
		bool CrouchedCheck()
		{
			if(player.crouched)
			{
				if(characterAnimator.currentAnimation == "spindash")
				{
					characterAnimator.PlayAnim("crouch", 1, 0.9f);
				}
				characterAnimator.PlayAnim("crouch", 1);
				return true;
			}
			else
			{
				if(characterAnimator.animation == "crouch")
				{
					characterAnimator.PlayAnim("uncrouch", 1);	
				}
				
				if(characterAnimator.animation == "uncrouch")
				{
					if((characterAnimator.currentFrame) == (characterAnimator.currentSpriteAnimation.frames.Count - 1))
					{
						return false;
					}	
					return true;
				}
			}
			return false;
		}
		
		bool SkidAnimCheck()
		{
			float skidAnimSpeed = (Mathf.Abs(player.GroundSpeed / 6f)) + 0.5f;
			
			if(!player.Grounded) return false;
		
			if(player.skidding)
			{
				if(!player.Grounded)
				{
					characterAnimator.PlayAnim("walk", 1f);
					return false;
				}
			
				if(!(characterAnimator.currentAnimation == "skidStart") && !(characterAnimator.currentAnimation == "skidLoop"))
				{
					lookingR = player.lookingRight;
					characterAnimator.PlayAnim("skidStart", 1);
				}	
				
				if(characterAnimator.animation == "skidStart")
				{		
					if((characterAnimator.currentFrame) == (characterAnimator.currentSpriteAnimation.frames.Count - 1))
					{
						characterAnimator.PlayAnim("skidLoop", skidAnimSpeed);
					}	
					return true;
				}
				else
				{
					characterAnimator.PlayAnim("skidLoop", skidAnimSpeed);
				}
				
				return true;
			}
			else
			{
				if(characterAnimator.currentAnimation == "skidLoop" || characterAnimator.currentAnimation == "skidStart")
				{
					if((player.InputLeft && player.lookingRight) || (player.InputRight && !player.lookingRight))
					{
						characterAnimator.PlayAnim("skidTurn", 1f);	
					}
					else 
					{
						characterAnimator.PlayAnim("idle", 1);	
						return false;
					}
				}
				if(characterAnimator.animation == "skidTurn")
				{
					if(characterAnimator.currentAnimation == "skidTurn")
						player.lookingRight = !lookingR;
					
					if((characterAnimator.currentFrame) == (characterAnimator.currentSpriteAnimation.frames.Count - 1))
					{
						return false;
					}	
					return true;
				}
			}
			return false;
		}
		
		bool PushingCheck()
		{
			if(player.Grounded)
			{
				float pushingAnimSpeed = ((Mathf.Abs(player.GroundSpeed / 6f)* 4) + 0.25f);
				if((player.Sensors[4].info.collided && player.InputLeft && (player.Sensors[4].info.flagged)) || (player.Sensors[5].info.collided && player.InputRight && (player.Sensors[5].info.flagged)))
				{
					characterAnimator.PlayAnim("push", pushingAnimSpeed);
					return true;
				}
			}
			return false;
		}
		
		float minJogSpeed = 4;
		float minRunSpeed = 6;
		float minDashSpeed = 12;
		bool RunningAnimsCheck()
		{
			float groundAnimSpeed = (Mathf.Abs(player.GroundSpeed / 6f)) + 0.25f;

			if(Mathf.Abs(player.GroundSpeed) == 0)
			{
				if(player.Grounded) characterAnimator.PlayAnim("idle", 1);
				else characterAnimator.PlayAnim("walk", groundAnimSpeed);
				return true;
			}
			if(Mathf.Abs(player.GroundSpeed) < minJogSpeed)
			{
				characterAnimator.PlayAnim("walk", groundAnimSpeed);
				minJogSpeed = 4f;
				return true;
			}
			if(Mathf.Abs(player.GroundSpeed) < minRunSpeed)
			{
				if(characterAnimator.currentAnimation != "walk" || characterAnimator.currentFrame == 3)
					characterAnimator.PlayAnim("jog", groundAnimSpeed);
				minJogSpeed = 3.5f;
				minRunSpeed = 6f;
				return true;
			}
			if(Mathf.Abs(player.GroundSpeed) < minDashSpeed)
			{
				characterAnimator.PlayAnim("run", groundAnimSpeed);
				minRunSpeed = 5.5f;
				minDashSpeed = 12f;
				return true;
			}
			else
			{
				characterAnimator.PlayAnim("dash", groundAnimSpeed);
				minDashSpeed = 11.5f;
				return true;
			}
			
			return false;
		}
		
		float angleSmoothDamp;
		float skidDustEmissionTimer;

		void UpdateSpriteTransform()
		{
			spriteOffset = Vector2.zero;
			
			if(player.Grounded)
			{	
				float targetRotation = player.GroundAngle;
				targetRotation = player.GroundAngle;

				if(!oldGrounded && (Mathf.Abs(player.GroundAngle) < 5f)){
					spriteAngle = 0;
					Debug.Log("odl");
				}

				float rotMult = 0.8f;

				if (Mathf.Abs(player.GroundAngle) <= 26.5f && Mathf.Abs(spriteAngle) <= 26.5f){
					targetRotation = 0;
					rotMult = 0.75f;
				}

				float rotate = (player.GroundAngle - spriteAngle) * rotMult;
				float shift  = ((Mathf.Abs(player.GroundSpeed * rotMult) <= 5)? 5.25f : Mathf.Abs(player.GroundSpeed * rotMult * 0.6f) + 0.25f);
				float rotateSpeed = (Mathf.Abs(rotate * Mathf.Pow(2, shift))) * Time.deltaTime * rotMult;

				spriteAngle = Mathf.MoveTowardsAngle(spriteAngle, targetRotation, rotateSpeed);

				oldGrounded = true;
			}
			else
			{
				spriteAngle = Mathf.MoveTowardsAngle(spriteAngle, 0, 4 * Time.deltaTime * 60f);	

				oldGrounded = false;
			}
			
			bool looking = (player.lookingRight);
			
			if(characterAnimator.currentSpriteAnimation.name == "skidTurn")
			{
				looking = !looking;
			}
			
			if(looking)
			{
				spriteOffset.x += -1.5f;
				renderer.flipX = false;
			}
			else
			{
				spriteOffset.x +=  1.5f;
				renderer.flipX = true;
			}

			tempAngle = spriteAngle;
			if(characterAnimator.currentSpriteAnimation.name == "roll" || characterAnimator.currentSpriteAnimation.name == "dropdash")
			{
				spriteOffset.y -= 5/16f;
				tempAngle = 0;
			}
			
			if(characterAnimator.currentSpriteAnimation.name == "spindash" || characterAnimator.currentSpriteAnimation.name == "skidStart" || characterAnimator.currentSpriteAnimation.name == "skidLoop" || characterAnimator.currentSpriteAnimation.name == "skidTurn")
			{
				tempAngle = 0;
			}
			
			if(skidDustEmissionTimer > 0)
			{
				skidDustEmissionTimer -= Time.deltaTime * 60;
				if(skidDustEmissionTimer <= 0)
				{
					skidDustEmissionTimer = 0;
				}
			}
			
			if(player.skidding)
			{
				if(skidDustEmissionTimer == 0)
				{
					GameObject skidDustObj = Instantiate(dustPrefab) as GameObject;
					Dust skidDust = skidDustObj.GetComponent<Dust>();
					skidDust.currentAnim = "skiddingDust";
					skidDust.player = player;
					skidDust.instantiate();
					
					skidDustEmissionTimer = 3f;
				}
			}
			if(player.Grounded)
			{
				spriteOffset += new Vector2(0, (3 - player.sizeRead.y / 2));
			}
			else
			{
				spriteOffset += new Vector2(0, (3 - player.sizeRead.y / 2) - 1/32f);
			}
			
			Vector2 pos = (Vector2)player.transform.position + RotateVector(spriteOffset, tempAngle);
			if(player.Grounded)
			{
				spritePosition = Utilis.Vector2RoundToPixel(new Vector2(pos.x, pos.y - 1/32f), 16f);
			}
			else
			{
				spritePosition = Utilis.Vector2RoundToPixel(new Vector2(pos.x, pos.y), 16f);
			}
			
		}
	}
}

