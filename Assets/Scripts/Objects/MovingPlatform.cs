using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class MovingPlatform : MonoBehaviour
	{	
		public float duration;
		
		public float waitTime;
		
		[SerializeField] AnimationCurve curve;
		
		public Transform startPos;

		public Vector3 position;
		
		public Transform endPos;
		
		[HideInInspector] public Vector2 velocity;
		
		[HideInInspector] public List<Player> AttachedPlayers;
		
		private float t;
		
		private float easing;
		
		private bool moveToStart;
		
		private bool moveToEnd = true;
		
		private bool moving;

		void Start()
		{
			position = startPos.position; 
		}
		
		void OnEnable()
		{
			moving = false;
		}
		
		IEnumerator waitCoroutine(float t, bool goEnd)
		{
			moving = true;
			yield return new WaitForSeconds(t);
			
			if(goEnd)
			{
				moveToEnd = true;
				moveToStart = false;
			}
			else
			{
				moveToEnd = false;
				moveToStart = true;
			}
			
			moving = false;
		}
		// Update is called once per frame
		public void PlatformMovement(float stepDelta, SonicObject obj)
		{
			Vector2 prevPos = (Vector2)position;
			if(moveToEnd)
			{
				t = Mathf.MoveTowards(t, 1, (1 / duration) * stepDelta / 16);
				easing = curve.Evaluate(t);
				position = Vector2.Lerp(startPos.position, endPos.position, easing);
				if(position == endPos.position)
				{
					if(moving == false)
					StartCoroutine(waitCoroutine(waitTime, false));
				}
			}
			
			if(moveToStart)
			{
				t = Mathf.MoveTowards(t, 0, (1 / duration) * stepDelta / 16);
				easing = curve.Evaluate(t);
				position = Vector2.Lerp(startPos.position, endPos.position, easing);
				if(position == startPos.position)
				{
					if(moving == false)
					StartCoroutine(waitCoroutine(waitTime, true));
				}
			}
			
			Vector2 movement = (Vector2)position - prevPos;
			velocity = (movement / stepDelta) * 16f;
			
			if(AttachedPlayers.Count > 0)
			{
				foreach(Player player in AttachedPlayers)
				{
					if(!player.Grounded) continue;
					
					player.movingPlatformVelocity += movement;
				}
				
				AttachedPlayers.Clear();	
			}

			transform.position = position;
		}
		
		public void AttachPlayerToPlatform(Player player)
		{
			if(player == null) return;
			
			if(AttachedPlayers.Contains(player)) return;
			
			AttachedPlayers.Add(player);
		}
	}	
}

