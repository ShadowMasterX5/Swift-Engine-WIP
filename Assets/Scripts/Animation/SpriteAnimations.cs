using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	[CreateAssetMenu(fileName = "SpriteAnimations", menuName = "sonicFramework/SpriteAnimations", order = 0)]
	public class SpriteAnimations : ScriptableObject 
	{
		public List<SpriteAnimation> spriteAnimations;
		
		public SpriteAnimation GetAnim(string name)
		{
			List<SpriteAnimation> anims = new List<SpriteAnimation>();
			
			foreach(SpriteAnimation anim in spriteAnimations)
			{
				if(anim.name == name) anims.Add(anim);
			}
			
			if(anims.Count == 0)
			{
				return null;
			}
			else
			{
				return anims[0];
			}
		}
	}
	
	[System.Serializable]
	public class SpriteAnimation
	{
		public string name;
		
		public bool looping = true;
		
		public List<Sprite> frames;
	}
}