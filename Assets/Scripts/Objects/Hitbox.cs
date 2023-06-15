using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SonicFramework
{
	[ExecuteAlways]
	public class Hitbox : MonoBehaviour
	{	
		public bool colliding;
		
		private Player player;
		
		public Direction direction;
		public Vector2 scale = Vector2.one;
		public Vector2 offset;
		
		public Rect hitboxRect;
		
		Vector2 position;
		Vector2 size;
		Camera cam;
		
		PhysicsManager physicsManger;
		
		void Start()
		{
			cam = Camera.main;
			physicsManger = GameObject.FindWithTag("PhysicsManager").GetComponent<PhysicsManager>();
		}
		
		void Update()
		{
			position = (Vector2)transform.position + offset;
			Vector2 localScale = scale * (Vector2)transform.localScale;
			
			float roundAngle = Utilis.WrapAngleFromNegative180To180(Mathf.Round(transform.eulerAngles.z / 45f) * 45);
			
			switch(roundAngle) {
				case 0:
					direction = Direction.up;
					break;
				case 90:
					direction = Direction.left;
					break;
				case 180:
					direction = Direction.down;
					break;
				case -90:
					direction = Direction.right;
					break;
				default :
					direction = Direction.up;
					break;
			}
			
			switch(direction){
				case Direction.up:
					size = localScale;
					break;
				case Direction.down:
					size = localScale;
					break;
				case Direction.right:
					size = new Vector2(localScale.y, localScale.x);
					break;
				case Direction.left:
					size = new Vector2(localScale.y, localScale.x);
					break;
			}
			
			size.x = Mathf.Clamp(size.x, 0, Mathf.Infinity);
			size.y = Mathf.Clamp(size.y, 0, Mathf.Infinity);
			
			hitboxRect = new Rect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y);
			
			if(Application.isPlaying)
			{
				Runtime();
			}
			else
			{
				Editor();
			}
		}
		
		void Runtime()
		{
			
		}
		
		void Editor()
		{
#if UNITY_EDITOR
			if(Selection.Contains (this.gameObject)) // show outline
			{
				DebugUtilis.DrawRect(new Rect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y), Color.green);
			}
#endif	
		}
	
		void OnGUI()
		{
			if(physicsManger.debugMode)
			{
				float ScreenScale = (Screen.height / cam.orthographicSize) / 2;
				Vector2 screenPos = cam.WorldToScreenPoint(position);
				Vector2 screenRectSize = size * ScreenScale;
				screenPos.y = Screen.height - screenPos.y;
				Color color = Color.magenta;
				color.a = 0.2f;
				GUIUtilis.DrawQuad(new Rect(screenPos.x - screenRectSize.x / 2, screenPos.y - screenRectSize.y / 2, screenRectSize.x, screenRectSize.y), color);
			}
		}
		
		public bool OverlappingWith(Hitbox otherHitbox)
		{
			return hitboxRect.Overlaps(otherHitbox.hitboxRect);
		}
	}
}

