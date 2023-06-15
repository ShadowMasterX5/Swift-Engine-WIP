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
	public class TriggerBase : Tile
	{
		public TriggerBase()
		{
			flags = TileFlags.None;
			colliderType = Tile.ColliderType.Grid;
		}
		
		public virtual void Trigger(Player player){}
	}
}

