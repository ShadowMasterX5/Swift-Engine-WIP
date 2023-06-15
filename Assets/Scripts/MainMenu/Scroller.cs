using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroller : MonoBehaviour
{
	public float width;
	public float height;
	
	public Vector2 speed;
	
	public float x;
	public float y;

	public float parallaxX;
	public float parallaxY;
	
	Vector2 startPos;
	public Vector2 offset;
	// Start is called before the first frame update
	void Start()
	{
		startPos = (Vector2)transform.position;
	}

	// Update is called once per frame
	void Update()
	{
		x -= speed.x * Time.deltaTime;
		y -= speed.y * Time.deltaTime;
		
		if(x <= -width) x = x + width;
		if(x >=  width) x = x - width;
		
		if(y <= -height) y = y + height;
		if(y >=  height) y = y - height;
		
		transform.position = startPos + (x * (Vector2)transform.right) + (y * (Vector2)transform.up) + offset;
	}
}
