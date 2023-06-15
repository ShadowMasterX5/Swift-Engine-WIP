using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using FlintInputSystem;

namespace SonicFramework
{
	public class PlayerInput : MonoBehaviour
	{
		[SerializeField] private Player player;
		public InputManager inputManager;
		
		private void Awake()
		{
			if (player == null)
			{
				enabled = false;
				return;
			}
		}
		
		void Update()
		{
			player.InputJump = inputManager.GetAction("Action");
			player.InputRight = inputManager.GetAction("Right");
			player.InputLeft = inputManager.GetAction("Left");
			player.InputUp = inputManager.GetAction("Up");
			player.InputDown = inputManager.GetAction("Down");
		}
	}	
}

