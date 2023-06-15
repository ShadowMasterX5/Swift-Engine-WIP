using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	public Animator transition;
	public Canvas loadingCanvas;
	public float transitionTime;
	
	CanvasGroup canvasGroup;
	
	void OnEnable()
	{
		loadingCanvas.gameObject.SetActive(false);
		StartCoroutine(startIEnum());
	}
	
	IEnumerator startIEnum()
	{
		canvasGroup = transition.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 1f;
		
		yield return new WaitForSeconds(0.05f);
		
		transition.SetTrigger("end");
	}
	
	public static void LoadScene(string sceneName)
	{
		SceneLoader loader = (SceneLoader)FindObjectOfType(typeof(SceneLoader));
		
		loader.StartCoroutine(loader.LoadSceneWithTransition(sceneName));
	}
	
	IEnumerator LoadSceneWithTransition(string sceneName)
	{
		transition.SetTrigger("start");
		
		yield return new WaitForSecondsRealtime(transitionTime);
		
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		
		while (!asyncLoad.isDone)
		{
			loadingCanvas.gameObject.SetActive(true);
			yield return null;
		}
		loadingCanvas.gameObject.SetActive(false);
	}
}
