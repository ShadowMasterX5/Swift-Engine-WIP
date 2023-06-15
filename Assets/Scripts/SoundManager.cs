using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager Instance;
	
	public AudioSource music,
					// Player SoundEffects
					jump,
					skid,
					spindashCharge,
					spindashRelease,
					dropdash,
					roll,
					spring,
					menuBleep,
					menuAccept,
					menuWoosh,
					ring;
	
	void Awake()
	{
		if(Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	
	public void PlayMusic(AudioClip audioClip)
	{
		music.clip = audioClip;
		music.Play();
	}
	
	public void Jump()
	{
		jump.Play();
	}
	
	public void Skid()
	{
		skid.Play();
	}
	
	public void SpindashCharge()
	{
		spindashCharge.Play();
	}
	
	public void SpindashRelease()
	{
		spindashRelease.Play();
	}
	
	public void Dropdash()
	{
		dropdash.Play();
	}
	
	public void Roll()
	{
		roll.Play();
	}
	
	public void Spring()
	{
		spring.Play();
	}
}
