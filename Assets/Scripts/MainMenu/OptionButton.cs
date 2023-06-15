using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionButton : MonoBehaviour
{
	public string name;
	public Sprite[] optionSprites;
	public SpriteRenderer spriteRenderer;

	public void SetSpriteOption(int index)
	{
		spriteRenderer.sprite = optionSprites[index];
	}
}
