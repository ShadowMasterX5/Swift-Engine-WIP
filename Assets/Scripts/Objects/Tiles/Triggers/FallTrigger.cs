using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	[CreateAssetMenu(menuName = "sonicFramework/FallTrigger", fileName = "FallTrigger")]
	public class FallTrigger : TriggerBase
	{
		public override void Trigger(Player player)
		{
			Debug.Log("fallTriggered");
			if(player.Grounded)
			{
				player.Grounded = false;
				player.Position += 0.05f * player.GroundModeUp;
			}
		}
	}
}
