using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ScrollingTexture : MonoBehaviour
{
	SpriteRenderer spriteRenderer;
	public float b;
	public float g;
	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	void Update()
	{
		spriteRenderer.material.SetVector("_spriteScale", new Vector2(
												(spriteRenderer.sprite.rect.width / spriteRenderer.sprite.texture.width), 
												-(spriteRenderer.sprite.rect.height / spriteRenderer.sprite.texture.height)
												));
		spriteRenderer.material.SetVector("_spritePos", new Vector2(
												-(b + (spriteRenderer.sprite.textureRect.x / spriteRenderer.sprite.texture.width)), 
												-(g + (spriteRenderer.sprite.textureRect.y / spriteRenderer.sprite.texture.height))
												));
	}
}
