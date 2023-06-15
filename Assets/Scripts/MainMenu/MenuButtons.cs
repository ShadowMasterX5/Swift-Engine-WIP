using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtons : MonoBehaviour
{
	public string MenuName;
	public List<GameObject> buttons;
	public GameObject selectionCursor;
	
	private int selectedButton = 0;
	
	public Vector2 topButtonPosition;
	public Vector2 bottomButtonPosition;
	private Vector2 selectionCursorOffset;
	private Vector2 buttonOffset;
	
	public float buttonSmoothing = 0.125f;
	private Vector2 cursorSmoothVelocity;
	private Vector2 buttonSmoothVelocity;
	
	private List<SpriteRenderer> spriteRendererCache = new List<SpriteRenderer>(1);
	private List<SpriteRenderer> backSpriteRendererCache = new List<SpriteRenderer>(1);
	private List<Color> buttonBackColors = new List<Color>(1);
	
	private List<Vector2> buttonSmoothVelocities = new List<Vector2>(1);
	private List<Vector2> selectedButtonOffsets  = new List<Vector2>(1);
	
	private List<float> buttonAngleSmoothVelocities = new List<float>(1);
	private List<float> selectedButtonAngleOffsets  = new List<float>(1);
	
	private float t = 0;
	
	private float selectTime = 0;
	private float verticalSpacing;
	
	bool lastSelectedUp;
	
	bool onSelect;
	
	bool flickering;
	bool activate;
	
	private MenuManager menuManager;
	
	void Start()
	{
		menuManager = GameObject.FindWithTag("menuManager").GetComponent<MenuManager>();

		buttonBackColors.Clear(); 
		backSpriteRendererCache.Clear();
		
		for(int i = 0; i < buttons.Count; i++) 
		{
			OptionButton optionButton = buttons[i].GetComponent<OptionButton>();
			if(optionButton != null)
			{
				 optionButton.SetSpriteOption(PlayerPrefs.GetInt(optionButton.name));
			}
			buttonBackColors.Add(Color.black);
			spriteRendererCache.Add(buttons[i].GetComponent<SpriteRenderer>());
			
			backSpriteRendererCache.Add(default(SpriteRenderer));
			backSpriteRendererCache.Add(default(SpriteRenderer));
			
			backSpriteRendererCache[i * 2    ] = buttons[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
			backSpriteRendererCache[i * 2 + 1] = buttons[i].transform.GetChild(1).GetComponent<SpriteRenderer>();
		}
	}
	
	void Update()
	{
		ResizeButtonSmoothingArrays();
		
		t += Time.deltaTime;
		selectTime += Time.deltaTime;
		
		if(!flickering)
		{
			if(menuManager.currentMenu == this.gameObject) 
			{
				if(Input.GetKeyDown(KeyCode.DownArrow))
				{
					if(selectedButton < buttons.Count - 1)
					{
						SoundManager.Instance.menuBleep.Stop();
						SoundManager.Instance.menuBleep.Play();
						selectedButton++;
						onSelect = true;
						lastSelectedUp = true;
						selectTime = 0f;
					}
				}
				
				if(Input.GetKeyDown(KeyCode.UpArrow))
				{
					if(selectedButton > 0)
					{
						SoundManager.Instance.menuBleep.Stop();
						SoundManager.Instance.menuBleep.Play();
						selectedButton--;
						onSelect = true;
						lastSelectedUp = false;
						selectTime = 0f;
					}
				}
			}
			else
			{
				onSelect = true;
				selectTime = 0f;
				selectedButton = 0;
			}
		}
		
		selectedButton = Mathf.Clamp(selectedButton, 0, buttons.Count - 1);
			
		if(menuManager.currentMenu == this.gameObject) 
		{
			if((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Return)) && !flickering)
			{
				ButtonConfirm(selectedButton);
			}

			if((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) && !flickering)
			{
				SoundManager.Instance.menuBleep.Stop();
				SoundManager.Instance.menuBleep.Play();
				OptionButtonChange(selectedButton, Input.GetKeyDown(KeyCode.RightArrow));
			}
		}
		
		verticalSpacing = Mathf.Abs(topButtonPosition.y - bottomButtonPosition.y) / (buttons.Count - 1f);
		Vector2 buttonOffsetUnsmoothed;
		buttonOffsetUnsmoothed.x = 0;
		buttonOffsetUnsmoothed.y = (selectedButton * (verticalSpacing)) - (((float)selectedButton / ((float)buttons.Count - 1f)) * -(bottomButtonPosition.y - topButtonPosition.y));
		buttonOffset = Vector2.SmoothDamp(buttonOffset, buttonOffsetUnsmoothed, ref buttonSmoothVelocity, buttonSmoothing);
		
		for(int i = 0; i < buttons.Count; i++) 
		{
			Vector2 pos = (Vector2)buttons[i].transform.localPosition;
			Vector2 offset = Vector2.zero;
			
			if(i != selectedButton)
			{
				offset.x = Mathf.Sin((t + i) * 2) * 0.125f;
			}
			
			Vector2 selectedOffset = Vector2.zero;
			if(i == selectedButton)
			{
				selectedOffset = new Vector2(0, Mathf.Clamp((-selectTime * 8) + 1f, 0, Mathf.Infinity));
			
				if(menuManager.currentMenu == this.gameObject)
				{
					Color backCol = (Color.red * 0.75f) + (Color.white * ((Mathf.Sin((selectTime) * 12) + 1f) / 2f));
					backCol.a = 1;
					buttonBackColors[i] = backCol;
					
					backSpriteRendererCache[i * 2    ].enabled = true;
					backSpriteRendererCache[i * 2 + 1].enabled = true;
				}
				selectedOffset.x += 0.5f;
				selectedOffset.y += Mathf.Sin(selectTime * 3) * 0.1f;
			}
			else
			{
				if(menuManager.currentMenu == this.gameObject)
				{
					buttonBackColors[i] = Color.black;
					backSpriteRendererCache[i * 2    ].enabled = false;
					backSpriteRendererCache[i * 2 + 1].enabled = false;
				}
			}
			
			backSpriteRendererCache[i * 2    ].color = buttonBackColors[i];
			backSpriteRendererCache[i * 2 + 1].color = buttonBackColors[i];
			
			Vector2 tempVel = buttonSmoothVelocities[i];
			
			selectedButtonOffsets[i] = Vector2.SmoothDamp(selectedButtonOffsets[i], selectedOffset, ref tempVel, buttonSmoothing);
			buttonSmoothVelocities[i] = tempVel;

			pos = GetButtonPos(i, topButtonPosition, buttonOffset) + offset + selectedButtonOffsets[i];
			buttons[i].transform.localPosition = pos;
			
			onSelect = false;
		}
		
		selectionCursorOffset.x = Mathf.Sin(selectTime * 3) * 0.1f;
		Vector2 cursorPositionUnsmoothed = GetButtonPos(selectedButton, topButtonPosition, buttonOffset) + new Vector2(-1f, 0) + selectionCursorOffset;
		
		selectionCursor.transform.localPosition = Vector2.SmoothDamp((Vector2)selectionCursor.transform.localPosition, cursorPositionUnsmoothed, ref cursorSmoothVelocity, buttonSmoothing / 2f);
	}
	
	private void ResizeButtonSmoothingArrays()
	{
		if(buttonSmoothVelocities.Count != buttons.Count)
		{
			buttonSmoothVelocities = ResizeList<Vector2>(buttonSmoothVelocities, buttons.Count);
		}
		
		if(selectedButtonOffsets.Count != buttons.Count)
		{
			selectedButtonOffsets = ResizeList<Vector2>(selectedButtonOffsets, buttons.Count);
		}
		
		if(buttonAngleSmoothVelocities.Count != buttons.Count)
		{
			buttonAngleSmoothVelocities = ResizeList<float>(buttonAngleSmoothVelocities, buttons.Count);
		}
		
		if(selectedButtonAngleOffsets.Count != buttons.Count)
		{
			selectedButtonAngleOffsets = ResizeList<float>(selectedButtonAngleOffsets, buttons.Count);
		}
	}
	
	IEnumerator ButtonFlicker(GameObject button)
	{
		SpriteRenderer spriteRenderer = button.GetComponent<SpriteRenderer>();
		flickering = true;
		for(int i = 0; i < 4; i++) 
		{
			spriteRenderer.enabled = false;
			yield return new WaitForSeconds(1/25f);
			
			spriteRenderer.enabled = true;
			yield return new WaitForSeconds(1/25f);
			
			spriteRenderer.enabled = false;
			yield return new WaitForSeconds(1/25f);
			
			spriteRenderer.enabled = true;
			yield return new WaitForSeconds(1/25f);
		}
		
		flickering = false;
	}
	
	IEnumerator ButtonActivateDelay(int buttonIndex, float t)
	{
		yield return new WaitForSeconds(t);
		
		switch (MenuName) {
			case "MainMenu":
				if(selectedButton == 0) SceneLoader.LoadScene("StardustSpeedway");
				if(selectedButton == 1) menuManager.currentMenuIndex = 1;
				if(selectedButton == 2) Debug.Log("Extras");
				if(selectedButton == 3) Application.Quit();
				
				break;
			case "Options":
				
				break;
			default :
				break;
		}
		
	}
	
	private List<T> ResizeList<T>(List<T> list, int newCount) 
	{
		List<T> resizedList = list;
		if(newCount <= 0) 
		{
			resizedList.Clear();
		} 
		else 
		{
			while(resizedList.Count > newCount) resizedList.RemoveAt(resizedList.Count - 1);
			while(resizedList.Count < newCount) resizedList.Add(default(T));
		}
		
		return resizedList;
	}
	
	private Vector2 GetButtonPos(int buttonIndex, Vector2 _topButtonPos, Vector2 _buttonOffset)
	{
		Vector2 pos = Vector2.zero;
		
		pos.x = _topButtonPos.x;
		pos.y = _topButtonPos.y - buttonIndex * verticalSpacing;
		
		pos += _buttonOffset;
		
		return pos;
	}
	
	public void ButtonConfirm(int selectedButton)
	{
		OptionButton optionButton = buttons[selectedButton].GetComponent<OptionButton>();
		if(optionButton != null) return;
		SoundManager.Instance.menuAccept.Stop();
		SoundManager.Instance.menuAccept.Play();
		switch (MenuName) {
			case "MainMenu":
				break;
			case "Options":
				if(selectedButton == 0)
				{
					if(Options.Instance.windowSize >= 3)
					{
						Options.Instance.windowSize = 0;
					}
					else
					{
						Options.Instance.windowSize++;
					}
					
					Debug.Log("windowSize" + Options.Instance.windowSize);
				}
				break;
			default :
				break;
		}
		StartCoroutine(ButtonFlicker(buttons[selectedButton]));
		StartCoroutine(ButtonActivateDelay(selectedButton, 0.2f));
	}
	
	public void OptionButtonChange(int selectedButton, bool right)
	{
		OptionButton optionButton = buttons[selectedButton].GetComponent<OptionButton>();
		switch (MenuName) {
			case "MainMenu":
				break;
			case "Options":
				if(selectedButton == 0)
				{
					if(right)
					{
						if(Options.Instance.windowSize >= 3)
						{
							Options.Instance.windowSize = 0;
						}
						else
						{
							Options.Instance.windowSize++;
						}
					}
					else
					{
						if(Options.Instance.windowSize <= 0)
						{
							Options.Instance.windowSize = 3;
						}
						else
						{
							Options.Instance.windowSize--;
						}
					}
					if(optionButton != null) optionButton.SetSpriteOption(Options.Instance.windowSize);
					Debug.Log("windowSize" + Options.Instance.windowSize);
				}
				
				if(selectedButton == 1)
				{
					if(right)
					{
						if(Options.Instance.fullScreen == 1) Options.Instance.fullScreen = 0;
						else Options.Instance.fullScreen = 1;
					}
					else
					{
						if(Options.Instance.fullScreen == 1) Options.Instance.fullScreen = 0;
						else Options.Instance.fullScreen = 1;
					}
					if(optionButton != null) optionButton.SetSpriteOption(Options.Instance.fullScreen);
					Debug.Log("fullScreen" + Options.Instance.fullScreen);
				}
				break;
			default :
				break;
		}
	}
}
