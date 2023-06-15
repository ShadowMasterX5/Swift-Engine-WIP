using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using FlintInputSystem;

namespace SonicFramework
{
	public class AiInput : MonoBehaviour
	{
		[SerializeField] private Player ai;
		[SerializeField] private Player player;
		
		public bool close;
		public bool xClose;
		public bool yClose;
		public bool far;
		
		bool spindashing;
		
		private void Awake()
		{
			spindashing = false;
			if (ai == null || player == null)
			{
				enabled = false;
				return;
			}
		}
		
		void Update()
		{
			//If on the ground, check Sonic's position. If below, jump up to Sonic's position. 
				//If away, walk toward sonic. If far away, spin dash toward sonic.
			float distance = (ai.Position - player.Position).magnitude;
			float xDistance = Mathf.Abs(ai.Position.x - player.Position.x);
			float yDistance = Mathf.Abs(ai.Position.y - player.Position.y);
			close = (distance < 2);
			xClose = (xDistance < 2);
			yClose = (yDistance < 2);
			far = (distance >= 8);
				
			if(!xClose)
			{
				if((ai.Grounded && !far) || (!ai.Grounded))
				{
					if(ai.Position.x < player.Position.x)
					{
						ai.InputRight = true;
					}
					else
					{
						ai.InputRight = false;
					}
					
					if(ai.Position.x > player.Position.x)
					{
						ai.InputLeft = true;
					}
					else
					{
						ai.InputLeft = false;
					}
				}	
				else
				{
					ai.InputRight = false;
					ai.InputLeft = false;
				}
			}
			else
			{
				if(xClose)
				{
					if(ai.GroundSpeed < -1)
					{
						ai.InputRight = true;
					}
					else
					{
						ai.InputRight = false;
					}
					
					if(ai.GroundSpeed > 1)
					{
						ai.InputLeft = true;
					}
					else
					{
						ai.InputLeft = false;
					}
				}
			}
			
			if(ai.Grounded)
			{
				if(far && (Mathf.Abs(ai.GroundAngle) < 22.5f))
				{
					if(Mathf.Abs(ai.GroundSpeed) < 2)
					{
						if(ai.GroundSpeed < -0.25f) 
							StartCoroutine(InputDelay(0, "right", 0.1f));
						if(ai.GroundSpeed >  0.25f) 
							StartCoroutine(InputDelay(0, "left", 0.1f));
						
						if(Mathf.Abs(ai.GroundSpeed) < 0.1f)
						{
							if(!spindashing)
								StartCoroutine(SpinDash());
						}
					}
				}
			}
			
			//Close? Check to see what sonic does. If he jumps, you jump. If he ducks, you duck.
				//If he spins, you spin, when he releases, you release.
				
			if(close)
			{
				if(ai.Grounded && player.Jumped)
				{
					StartCoroutine(InputDelay(0.5f, "jump", 0.1f));
				}
			}

			//The second grouping most likely only happens if the first one passes and simply responds to keypresses/button events just like sonic, 
				//except with an artificial delay.

			//In object oriented programming, which wasn't used in classic sonic games, but surely used in sonic mania, 
				//ground checks are probably actor level methods so the player character and tails (and the enemies) would all inherit it. 
				//Checking distance between tails on sonic, especially since it's only one axis at the time, is super cheap.

			//In sonic mania, you can also make him carry you which, in sonic 3, required a second player/controller. 
				//In mania, this is probably a clever extension of the if close enough, do what I do...
				//You press jump twice, sonic just does the air spin, but tails starts flying. Once he does that, 
				//then the -go to pick up sonic- would happen. After you hook up, 
				//you're basically just controlling tails until you jump from his hands or land.

			/*
			ai.InputJump = inputManager.GetAction("Action");
			ai.InputRight = inputManager.GetAction("Right");
			ai.InputLeft = inputManager.GetAction("Left");
			ai.InputUp = inputManager.GetAction("Up");
			ai.InputDown = inputManager.GetAction("Down");
			*/
		}
		
		IEnumerator SpinDash()
		{
			if(spindashing) yield return null;
			spindashing = true;
			ai.GroundSpeed = 0;
			
			if(ai.Position.x < player.Position.x)
			{
				while(!ai.lookingRight)
				{
					ai.InputRight = true;
					yield return new WaitForSeconds(0.1f);
				}
				ai.InputRight = false;
			}
			
			if(ai.Position.x > player.Position.x)
			{
				while(ai.lookingRight)
				{
					ai.InputLeft = true;
					yield return new WaitForSeconds(0.1f);
				}
				ai.InputLeft = false;
			}
			
			yield return new WaitForSeconds(0.1f);
			
			ai.InputDown = true;
			
			yield return new WaitForSeconds(0.1f);
			for(int i = 0; i < 5; i++) 
			{
				Debug.Log("rev");
				ai.InputJump = true;
				yield return new WaitForSeconds(0.1f);
				ai.InputJump = false;
				yield return new WaitForSeconds(0.1f);
			}
			
			yield return new WaitForSeconds(0.2f);
			ai.InputJump = false;
			ai.InputDown = false;
			yield return new WaitForSeconds(0.5f);
			spindashing = false;
		}
		
		IEnumerator InputDelay(float delay, string input, float holdTime = 0.5f)
		{
			yield return new WaitForSeconds(delay);
			
			switch (input) {
				case "left":
					ai.InputLeft = true;
					break;
				case "right":
					ai.InputRight = true;
					break;
				case "up":
					ai.InputUp = true;
					break;
				case "down":
					ai.InputDown = true;
					break;
				case "jump":
					ai.InputJump = true;
					break;
				default :
					
					break;
			}
			
			yield return new WaitForSeconds(holdTime);
			
			switch (input) {
				case "left":
					ai.InputLeft = false;
					break;
				case "right":
					ai.InputRight = false;
					break;
				case "up":
					ai.InputUp = false;
					break;
				case "down":
					ai.InputDown = false;
					break;
				case "jump":
					ai.InputJump = false;
					break;
				default :
					
					break;
			}
			
		}
	}
}