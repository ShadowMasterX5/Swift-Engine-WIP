using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class SpinningCircle : MonoBehaviour
	{
		public LineRenderer lineRend;
		
		public float width;
		public float radius;
		public float rotation;
		float r;
		
		public float speed = 1;
		public float timeOffset = 0;
		public float amplitude = 1;
		public float frequency = 1;
		
		public GameObject circle1;
		public GameObject circle2;
		
		float time;
		float size;
		
		public bool openEnd;
		public float openAmount = 0;
		public float openEndRotationOffset = 0;
		
		void Update()
		{
			time += Time.deltaTime;
			float t = time + timeOffset;
			size = radius + (Mathf.Sin(t * frequency) + 1f) * amplitude;
			rotation = Utilis.WrapAngleFromNegative180To180((t * 360 * speed) / (Mathf.PI * 2));
			
			//openAmount = Mathf.Clamp(openAmount, 0, 1);
			int vertexNum = 60;
			if(openEnd) vertexNum = 150;
			float rot = Utilis.WrapAngleFromNegative180To180(-rotation );
			if(rot < 0) rot += 360;
			
			DrawPolygon(lineRend, vertexNum, size, transform.position, width, width, rot * Mathf.Deg2Rad, ((openAmount/2) / (Mathf.PI * 2 * size)) * 180 * Mathf.Deg2Rad);
			
			if(circle1 != null)
			{
				Vector2 circle1Pos = Vector2.zero;
				circle1Pos.x = -Mathf.Sin(r);
				circle1Pos.y =  Mathf.Cos(r);
				circle1.transform.position = (Vector3)(circle1Pos * size) + transform.position;
			} 
			
			if(circle2 != null)
			{
				Vector2 circle2Pos = Vector2.zero;
				circle2Pos.x = -Mathf.Sin(r - Mathf.PI);
				circle2Pos.y =  Mathf.Cos(r - Mathf.PI);
				circle2.transform.position = (Vector3)(circle2Pos * size) + transform.position;
			}
		}
		
		void DrawPolygon(LineRenderer lineRenderer, int vertexNumber, float _radius, Vector3 centerPos, float startWidth, float endWidth, float rot, float endOpeningAmount)
		{
			float _s = openEnd? ((endOpeningAmount * (float)vertexNumber) / 2f): 0;
			int s = Mathf.FloorToInt(_s);
			
			float rotation = rot;
			if(openEnd) Debug.Log(endOpeningAmount);
			lineRenderer.startWidth = startWidth;
			lineRenderer.endWidth = endWidth;
			lineRenderer.loop = !openEnd;
			float angle = (2f * Mathf.PI / vertexNumber);
			int iterations = Mathf.RoundToInt(vertexNumber - _s);
			int g = 0;
			if(iterations % 2 == 0 && openEnd) g = 1;
			lineRenderer.positionCount = iterations + g - Mathf.RoundToInt(_s * 2) + (openEnd? 2 : 0);	
			
			for (int j = Mathf.RoundToInt(_s * 2); j < iterations + g + (openEnd? 2 : 0); j++)
			{
				int i = j;// - Mathf.RoundToInt(_s / 2);
				if(j == Mathf.RoundToInt(_s * 2)) r = (angle * (float)i + rotation) + openEndRotationOffset;
				Matrix4x4 rotationMatrix = new Matrix4x4(new Vector4(    Mathf.Cos(angle * (float)i + rotation), Mathf.Sin(angle * (float)i + rotation), 0, 0),
														new Vector4(-1 * Mathf.Sin(angle * (float)i + rotation), Mathf.Cos(angle * (float)i + rotation), 0, 0),
														new Vector4(0, 0, 1, 0),
														new Vector4(0, 0, 0, 1));
				Vector3 initialRelativePosition = new Vector3(0, _radius, 1f);
				lineRenderer.SetPosition(j - Mathf.RoundToInt(_s * 2), centerPos + rotationMatrix.MultiplyPoint(initialRelativePosition));
			}
		}
	}
}