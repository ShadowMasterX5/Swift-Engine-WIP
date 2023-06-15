using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Options : MonoBehaviour
{
	// This is really the only blurb of code you need to implement a Unity singleton
	private static Options _Instance;
	public static Options Instance
	{
		get
		{
			if (!_Instance)
			{
				_Instance = new GameObject().AddComponent<Options>();
				// name it for easy recognition
				_Instance.name = _Instance.GetType().ToString();
				// load saved data;
				_Instance.Load();
				// mark root as DontDestroyOnLoad();
				DontDestroyOnLoad(_Instance.gameObject);
			}
			return _Instance;
		}
	}
	
	public int windowSize = 1;
	public int fullScreen = 0;
	
	public void Save()
	{
		PlayerPrefs.SetInt("windowSize", windowSize);
		if(fullScreen == 0)
		{
			Screen.SetResolution(424 * (windowSize + 1), 240 * (windowSize + 1), false);
		}
		else
		{
			Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
		}
		
		PlayerPrefs.SetInt("fullScreen", fullScreen);
		Screen.fullScreen = fullScreen == 1 ? true : false;
	}
	
	public void Load()
	{
		windowSize = PlayerPrefs.GetInt("windowSize");
		fullScreen = PlayerPrefs.GetInt("fullScreen");
	}
}

