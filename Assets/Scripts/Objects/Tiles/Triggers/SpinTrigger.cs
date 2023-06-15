using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace SonicFramework
{
	[CreateAssetMenu(menuName = "sonicFramework/SpinTrigger", fileName = "SpinTrigger")]
	public class SpinTrigger : TriggerBase
	{
		public override void Trigger(Player player)
		{
			if(player.inTunnel) return;
			
			
			bool wasGrounded = true;
			if(!player.Grounded)
			{
				wasGrounded = false;
				if(!player.Rolling)
				{
					player.Grounded = true;
					if(player.CheckSensor(player.Sensors[6], 0f))
					{	
						player.groundMode = GroundMode.leftWall;
					}
					else if(player.CheckSensor(player.Sensors[7], 0f))
					{
						player.groundMode = GroundMode.rightWall;
					}
				}
				else
				{
					player.Grounded = true;
					if(player.CheckSensor(player.Sensors[6], 0.4f))
					{	
						player.groundMode = GroundMode.leftWall;
					}
					else if(player.CheckSensor(player.Sensors[7], 0.4f))
					{
						player.groundMode = GroundMode.rightWall;
					}
				}
			}

			float boostSpeed = 6f;
			float maxBoostSpeed = 14f;
			
			if(!player.Rolling)
			{
				player.Position -= 6/16f * player.GroundModeUp;
			}
			player.Rolling = true;
			
			if(wasGrounded)
			{
				if(Mathf.Abs(player.GroundSpeed) < maxBoostSpeed) {
					if(player.GroundSpeed != 0) player.GroundSpeed += Mathf.Sign(player.GroundSpeed) * boostSpeed;
					player.GroundSpeed = Mathf.Clamp(player.GroundSpeed, -maxBoostSpeed, maxBoostSpeed);
				}
			}
			else
			{
				if(player.groundMode == GroundMode.leftWall)
				{
					player.GroundAngle = -90f;
					player.GroundSpeed = -player.Velocity.y - (Mathf.Sign(player.Velocity.y) * boostSpeed);
					Debug.Log("left" + player.GroundSpeed);
					//if(Mathf.Abs(player.Velocity.y) < boostSpeed && (player.Velocity.y != 0)) player.GroundSpeed += Mathf.Sign(player.Velocity.y) * boostSpeed;
				}
				else if(player.groundMode == GroundMode.rightWall)
				{
					player.GroundAngle = 90f;
					player.GroundSpeed = player.Velocity.y + (Mathf.Sign(player.Velocity.y) * boostSpeed);
					Debug.Log("right" + player.GroundSpeed);
					//if(Mathf.Abs(player.Velocity.y) < boostSpeed && (player.Velocity.y != 0)) player.GroundSpeed += Mathf.Sign(player.Velocity.y) * boostSpeed;
				}
			}
			player.inTunnel = true;
			if(player.GroundSpeed == 0) player.GroundSpeed = player.lookingRight? boostSpeed : -boostSpeed;
		}
	}
}