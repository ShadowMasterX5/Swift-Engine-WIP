using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public struct DebugRect 
	{
		//Variable declaration
		//Note: I'm explicitly declaring them as public, but they are public by default. You can use private if you choose.
		public Rect rect;
		public Color color;
	   
		//Constructor (not necessary, but helpful)
		public DebugRect(Rect rect, Color color) 
		{
			this.rect = rect;
			this.color = color;
		}
	}


	public class GuiDebug : MonoBehaviour
	{
		Camera cam;
		
		Vector2 refRes = new Vector2(400, 224);
		
		private List<DebugRect> debugRects = new List<DebugRect>();
		
		void Awake()
		{
			cam = Camera.main;
		}
		
		void Update()
		{
			
		}
		
		private void OnGUI()
		{
			Vector2 screenScale = new Vector2(Screen.width, Screen.height) / refRes;
			DrawRect(new Rect(0, 0, 100, 100), Color.red);
			foreach(DebugRect debugRect in debugRects)
			{
				GUI.DrawTexture(debugRect.rect, Utilis.FillColorAlpha(new Texture2D(1,1), debugRect.color), ScaleMode.StretchToFill);
			}
			
			debugRects.Clear();
		}
		
		public void DrawRect(Rect rect, Color? color = null)
		{
			Color col = color?? Color.white;
			
			debugRects.Add(new DebugRect(rect, col));
		}
	}
}