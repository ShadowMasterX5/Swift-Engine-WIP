using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlintInputSystem
{
	[CreateAssetMenu(fileName = "KeyBinds", menuName = "sonicFramework/KeyBinds")]
	public class Actions : ScriptableObject 
	{
		[System.Serializable]
		public class ActionButton
		{
			public string ActionName;
			public List<string> positiveKeyCodes = new List<string>(1);
			public List<string> negativeKeyCodes = new List<string>(1);
		}
		
		public List<ActionButton> ActionButtons;
	}
}