using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
	public List<GameObject> menus;
	public int currentMenuIndex;
	public GameObject currentMenu 
	{
		get
		{
			return menus[currentMenuIndex];
		}
	}

	void Start()
	{
		SoundManager.Instance.PlayMusic(Resources.Load<AudioClip>("Audio/StageMusic/MenuMusic"));
	}
	
	void Update()
	{
		for(int i = 0; i < menus.Count; i++)
		{
			Vector3 offscreenPos = new Vector3(15, 0, 0);
			if(menus[i] != currentMenu)
			{
				menus[i].transform.position = Vector3.MoveTowards(menus[i].transform.position, offscreenPos, 
					(offscreenPos.magnitude / 2f + 6 * (menus[i].transform.position - offscreenPos).magnitude) * Time.deltaTime);
			}
			else
			{
				menus[i].transform.position = Vector3.MoveTowards(menus[i].transform.position, Vector3.zero, 
					(offscreenPos.magnitude / 2f + 6 * (menus[i].transform.position - offscreenPos).magnitude) * Time.deltaTime);
			}
		}
		
		if(Input.GetKeyDown(KeyCode.S))
		{
			switch (currentMenuIndex) {
				case 0:
					break;
					
				case 1:
					currentMenuIndex = 0;
					break;
					
				default :
					break;
			}
		}
		
		if(Input.GetKeyDown(KeyCode.W))
		{
			switch (currentMenuIndex) {
				case 0:
					break;
					
				case 1:
					Options.Instance.Save();
					break;
					
				default :
					break;
			}
		}
	}
}
