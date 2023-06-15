using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class PixelSnapping : MonoBehaviour
	{
		void LateUpdate()
		{
			Vector2 pixPos = Utilis.Vector2FloorToPixel((Vector2)transform.position + new Vector2(1/32f, 0), 16f);
			transform.position = new Vector3(pixPos.x, pixPos.y, transform.position.z);
		}
	}
}