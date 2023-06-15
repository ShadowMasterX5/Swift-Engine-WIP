using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	[CreateAssetMenu(menuName = "sonicFramework/LayerSwitcher", fileName = "LayerSwitcher")]
	public class LayerSwitcher : TriggerBase
	{
        public CollisionLayer collisionSwitch;
		public override void Trigger(Player player)
		{
			player.collisionLayer = collisionSwitch;
		}
	}
}
