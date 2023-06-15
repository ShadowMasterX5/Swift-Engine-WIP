using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SonicFramework
{
	public enum HatchDirection
	{
		up = 0,
		upRight = 1,
		right = 2,
		downRight = 3,
		down = 4,
		downLeft = 5,
		left = 6,
		upLeft = 7,
	}
	public static class GuiUtilis
	{	
		public static void DrawRect(Rect rect, Color? color = null)
		{
			Color col = color ?? Color.black;
			Rect roundedRect = new Rect(Mathf.Floor(rect.x), Mathf.Floor(rect.y), Mathf.Round(rect.width), Mathf.Round(rect.height));
			
			EditorGUI.DrawRect(roundedRect, col);
		}
		
		// Draws lines for each pixel width to give the illusion of thickness.
		// Note: It will actually draw (fill factor) times more lines than the thickness to not leave any odd rare gaps.
		public static void DrawLine(Vector2 start, Vector2 end, Color? color = null, int thickness = 1, bool insetThickness = false, bool positiveInset = true)
		{
			Vector2 norm = new Vector2(end.y - start.y, start.x - end.x).normalized;
			
			float fillFactor = 1.5f;
			
			Color col = color ?? Color.black;
			
			GL.Color(Color.black);
			GL.Begin(GL.LINES);
			GL.Color(col);
			
			if(insetThickness)
			{
				int insetDirection = positiveInset ? 1 : -1;
				
				for(int i = 0; i < Mathf.FloorToInt(thickness * fillFactor + 1.5f); i++)
				{	
					Vector2 offset = (norm * i / fillFactor) * insetDirection;
					
					GL.Vertex((Vector3)Utilis.Vector2Floor(start + offset));
					GL.Vertex((Vector3)Utilis.Vector2Floor(end   + offset));
				}
			}
			else
			{
				for(int i = -Mathf.FloorToInt(thickness * fillFactor / 2f); i < Mathf.FloorToInt(thickness * fillFactor / 2f + 1.5f); i++)
				{	
					Vector2 offset = norm * i / fillFactor;
					
					GL.Vertex((Vector3)(start + offset));
					GL.Vertex((Vector3)(end   + offset));
				}
			}
			
			GL.End();
		}
		
		// Draws an outline of a rectangle with variable thickness, thickness inset towards the center of the rectangle.
		public static void DrawRectInline(Rect rect, Color? color = null, int thickness = 1)
		{	
			Color col = color ?? Color.black;
					
			DrawLine(new Vector2(rect.x             , rect.y              ), new Vector2(rect.x + rect.width, rect.y              ), col, thickness, true, false);
			DrawLine(new Vector2(rect.x + rect.width, rect.y              ), new Vector2(rect.x + rect.width, rect.y + rect.height), col, thickness, true, false);
			DrawLine(new Vector2(rect.x + rect.width, rect.y + rect.height), new Vector2(rect.x             , rect.y + rect.height), col, thickness, true, false);
			DrawLine(new Vector2(rect.x             , rect.y + rect.height), new Vector2(rect.x             , rect.y              ), col, thickness, true, false);
		}
		
		// Draws an outline that surounds a rect on all sides.
		public static void DrawRectOutline(Rect rect, Color? color = null, int thickness = 1)
		{	
			Color col = color ?? Color.black;
			
			Rect newRect = new Rect(rect.x - thickness, rect.y - thickness, rect.width + 2 * thickness, rect.height + 2 * thickness);
			
			DrawRectInline(newRect, col, thickness);
		}
		
		public static void DrawHatchLines(Rect rect, int hatchAmount = 1, Color? color = null, int thickness = 1)
		{
			Color col = color ?? Color.black;

			for(int i = 0; i < hatchAmount; i++) {
				float t = Mathf.Clamp((i - (hatchAmount - 1) / 2f) / (hatchAmount / 2f),  0, Mathf.Infinity);
				float g = Mathf.Clamp((i - (hatchAmount - 1) / 2f) / (hatchAmount / 2f), -Mathf.Infinity, 0);
				
				Vector2 hatchStart = new Vector2(t    , 1 + g);
				Vector2 hatchEnd   = new Vector2(g + 1, t    );
				
				GuiUtilis.DrawLine(
					(hatchStart * rect.size) + rect.position,
					(hatchEnd   * rect.size) + rect.position,
					col, thickness
				);
			}
		}
	}
}

