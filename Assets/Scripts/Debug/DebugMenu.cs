using UnityEngine;
using System.Collections;

namespace SonicFramework
{
	public class DebugMenu : MonoBehaviour
	{
		public Player player;
		
		private float count;
		
		private Vector2 refRes = new Vector2(400, 224);
		
		private Vector2 screenScale;
		
		private IEnumerator Start()
		{
			GUI.depth = 2;
			while (true)
			{
				count = 1f / Time.unscaledDeltaTime;
				yield return new WaitForSeconds(0.1f);
			}
		}
		
		private void OnEnable()
		{
			StartCoroutine(Start());
		}
		
		private void OnGUI()
		{
			screenScale = new Vector2(Screen.width, Screen.height) / refRes;
			
			GUIStyle style = new GUIStyle(GUI.skin.label);			
			style.fontSize = Mathf.FloorToInt(screenScale.y * 8);
			GUIUtilis.DrawQuad(new Rect(2 * screenScale.x,  2 * screenScale.y, 75 * screenScale.x, 71 * screenScale.y), Color.black/2f);
			
			// Fps
			style.normal.textColor = Color.white;
			GUI.Label(new Rect(5 * screenScale.x,  5 * screenScale.y, 75 * screenScale.x, 25 * screenScale.y), 
				"FPS: " + Mathf.Round(count), style);
			
			// Xsp
			style.normal.textColor = Color.red;
			GUI.Label(new Rect(5 * screenScale.x, 20 * screenScale.y, 75 * screenScale.x, 25 * screenScale.y), 
				"XSP: " + (Mathf.Sign(player.Velocity.x)  == 1 ? " " : "") + Mathf.Round(player.Velocity.x  * 10f) / 10f, style);
			
			// Ysp	
			style.normal.textColor = Color.green;
			GUI.Label(new Rect(5 * screenScale.x, 30 * screenScale.y, 75 * screenScale.x, 25 * screenScale.y), 
				"YSP: " + (Mathf.Sign(player.Velocity.y)  == 1 ? " " : "") + Mathf.Round(player.Velocity.y  * 10f) / 10f, style);
				
			// Gsp
			style.normal.textColor = Color.blue;
			GUI.Label(new Rect(5 * screenScale.x, 40 * screenScale.y, 75 * screenScale.x, 25 * screenScale.y), 
				"GSP: " + (Mathf.Sign(player.GroundSpeed) == 1 ? " " : "") + Mathf.Round(player.GroundSpeed * 10f) / 10f, style);
				
			// Rings
			style.normal.textColor = Color.yellow;
			GUI.Label(new Rect(5 * screenScale.x, 50 * screenScale.y, 75 * screenScale.x, 25 * screenScale.y), 
				"Rings: " + PhysicsManager.Instance.rings, style);
		}
	}
}