using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public static class Utilis
	{
		public static Vector3 Round(Vector3 value, int digits)
		{
			float mult = Mathf.Pow(10.0f, (float)digits);
			return new Vector3(
				Mathf.Round(value.x * mult) / mult,
				Mathf.Round(value.y * mult) / mult,
				Mathf.Round(value.z * mult) / mult
			);
		}
		
		public static Vector3 Vector3Round(Vector3 vect)
		{
			float x = Mathf.Round(vect.x);
			float y = Mathf.Round(vect.y);
			float z = Mathf.Round(vect.z);
			
			return new Vector3(x,y,z);
		}
		
		public static Vector2 Vector2Round(Vector2 vect)
		{
			float x = Mathf.Round(vect.x);
			float y = Mathf.Round(vect.y);
			
			return new Vector2(x,y);
		}

		public static Vector2 PixelPerfectRound(Vector2 vect) // rounds x and floors y
		{
			float x = Mathf.Round(vect.x + 0.005f);
			float y = Mathf.Round(vect.y + 0.005f);
			
			return new Vector2(x,y);
		}

		public static Vector2 Vector2FloorRound(Vector2 vect) // floors x and rounds y
		{
			float x = Mathf.Round(vect.x);
			float y = Mathf.Floor(vect.y);
			
			return new Vector2(x,y);
		}
		
		public static Vector3Int Vector3FloorToInt(Vector3 vect)
		{
			int x = Mathf.FloorToInt(vect.x);
			int y = Mathf.FloorToInt(vect.y);
			int z = Mathf.FloorToInt(vect.z);
			
			return new Vector3Int(x,y,z);
		}
		
		public static Vector3 Vector3Floor(Vector3 vect)
		{
			float x = Mathf.Floor(vect.x);
			float y = Mathf.Floor(vect.y);
			float z = Mathf.Floor(vect.z);
			
			return new Vector3(x,y,z);
		}
		
		public static Vector2 Vector2Floor(Vector2 vect)
		{
			float x = Mathf.Floor(vect.x);
			float y = Mathf.Floor(vect.y);
			
			return new Vector2(x,y);
		}
		
		public static Vector2Int Vector2FloorInt(Vector2 vect)
		{
			int x = Mathf.FloorToInt(vect.x);
			int y = Mathf.FloorToInt(vect.y);
			
			return new Vector2Int(x,y);
		}
		
		public static Vector2 Vector2FloorToPixel(Vector2 vect,float pixlesPerUnit)
		{
			return Utilis.Vector2Floor(vect * pixlesPerUnit) / pixlesPerUnit;
		}
		
		public static Vector2 Vector2RoundToPixel(Vector2 vect,float pixlesPerUnit)
		{
			return Utilis.Vector2Round(vect * pixlesPerUnit) / pixlesPerUnit;
		}
		
		public static Vector3 Vector3FloorToPixel(Vector3 vect,float pixlesPerUnit)
		{
			return Utilis.Vector3Floor(vect * pixlesPerUnit) / pixlesPerUnit;
		}
		
		public static Vector3 Vector3RoundToPixel(Vector3 vect,float pixlesPerUnit)
		{
			return Utilis.Vector3Round(vect * pixlesPerUnit) / pixlesPerUnit;
		}
		
		public static int AxisBool(bool b1, bool b2)
		{
			// returns -1 or 1 depending on each bool.
			// if both are false or both are true, returns 0.
			if((!b1 && !b2) || (b1 && b2)) return 0;
			else return b1 ? -1 : 1;
		}
		
		public static bool intToBool(int x)
		{
			return x == 1 ? true : false;
		}
		
		public static int boolToInt(bool b)
		{
			return b ? 1 : 0;
		}
		
		public static bool inRange(float x, float min, float max, bool isEqualTo = false, bool firstEqual = false)
		{
			if(isEqualTo) 
				return (x >= min && x <= max) ? true : false;
			else if(firstEqual)
				return (x >= min && x < max) ? true : false;
			else
				return (x > min && x < max) ? true : false;
		}
		
		public static Vector2 Vector2ToAngle(float angle, Vector2 referenceAngle)
		{
			return (Vector2)(Quaternion.Euler(0, 0, angle) * referenceAngle);
		}
		
		public static float WrapAngleFromNegative180To180(float angle)
		{
			float a = angle % 360;
			
			if (a > 180)
			{
				a -= 360;
			}
			if (a < -180)
			{
				a += 360;
			}
			return a;
		}
		
		public static Texture2D FillColorAlpha(Texture2D tex2D, Color? fillColor = null)
		{   
			Texture2D tex = new Texture2D(tex2D.width, tex2D.height);
			Color[] fillPixels = new Color[tex2D.width * tex2D.height];
			for(int i = 0; i < fillPixels.Length; i++)
			{
				fillPixels[i] = fillColor ?? Color.clear;
			}
			tex.SetPixels(fillPixels);
			tex.filterMode = FilterMode.Point;
			tex.Apply();
			return tex;
		}
	}
}