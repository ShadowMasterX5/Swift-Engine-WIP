using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlintInputSystem
{
	public class InputManager : MonoBehaviour
	{
		public static InputManager instance;
		
		[SerializeField] private Actions keyBinds;
		
		public List<KeyCode> GetActionKeys(string actionName)
		{
			List<KeyCode> keyCodes = new List<KeyCode>(0);
			
			foreach (Actions.ActionButton actionButton in keyBinds.ActionButtons)
			{
				if(actionName == actionButton.ActionName)
				{
					foreach(string key in actionButton.positiveKeyCodes)
					{
						keyCodes.Add(StringToKey(key));
					}
				} 
			}
			
			if(keyCodes.Count == 0)
			{
				Debug.LogError("Cannot find action button with name: " + actionName, instance); 
			}
			
			return keyCodes;
		}
		
		public float GetActionAxis(string actionAxisName)
		{
			List<KeyCode> positiveKeyCodes = new List<KeyCode>(0);
			List<KeyCode> negativeKeyCodes = new List<KeyCode>(0);

			foreach (Actions.ActionButton actionButton in keyBinds.ActionButtons)
			{
				if(actionAxisName == actionButton.ActionName)
				{
					foreach(string key in actionButton.positiveKeyCodes)
					{
						positiveKeyCodes.Add(StringToKey(key));
					}
					
					foreach(string key in actionButton.negativeKeyCodes)
					{
						negativeKeyCodes.Add(StringToKey(key));
					}
				} 
			}
			
			if(positiveKeyCodes.Count == 0 && negativeKeyCodes.Count == 0)
			{
				Debug.LogError("Cannot find action axis with name: " + actionAxisName, instance); 
			}
			else
			{
				if(positiveKeyCodes.Count == 0)
				{
					positiveKeyCodes.Add(KeyCode.None);
				}
				if(negativeKeyCodes.Count == 0)
				{
					negativeKeyCodes.Add(KeyCode.None);
				}
			}
			
			float axis = 0f;
			
			if(GetKeyList(positiveKeyCodes) && !GetKeyList(negativeKeyCodes)) axis = 1f;
			
			if(!GetKeyList(positiveKeyCodes) && GetKeyList(negativeKeyCodes)) axis = -1f;
			
			return axis;
		}
		
		// Returns true the frame the key is pressed.
		public bool GetActionDown(string action) 
		{
			List<KeyCode> keys = GetActionKeys(action);

			return GetKeyListDown(keys);
		}
		
		// Returns true the frame the key is released.
		public bool GetActionUp(string action) 
		{
			List<KeyCode> keys = GetActionKeys(action);

			return GetKeyListUp(keys);
		}
		
		// Returns true while the key is down.
		public bool GetAction(string action)
		{
			List<KeyCode> keys = GetActionKeys(action);
			
			return GetKeyList(keys);
		}
		
		public bool GetKeyListDown(List<KeyCode> keys)
		{
			bool pressed = false;
			
			foreach(KeyCode key in keys)
			{
				if(Input.GetKeyDown(key)) pressed = true;
			}
			
			return pressed;
		}
		
		public bool GetKeyListUp(List<KeyCode> keys)
		{
			bool pressed = false;
			
			foreach(KeyCode key in keys)
			{
				if(Input.GetKeyUp(key)) pressed = true;
			}
			
			return pressed;
		}
		
		public bool GetKeyList(List<KeyCode> keys)
		{
			bool pressed = false;
			
			foreach(KeyCode key in keys)
			{
				if(Input.GetKey(key)) pressed = true;
			}
			
			return pressed;
		}
		
		public KeyCode StringToKey(string keyString)
		{
			return (KeyCode) System.Enum.Parse(typeof(KeyCode), keyString);
		}
	}	
}