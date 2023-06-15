using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace SonicFramework
{
	[ExecuteAlways]
	public class PhysicsManager : MonoBehaviour
	{
		public static PhysicsManager Instance { get; private set; }

		public string levelSong;
		public bool debugMode;
		[HideInInspector] public static bool debugFrame;
		public int subSteps;
		public Player[] playerArray;
		public int rings;
		public LayerMask groundMask;
		public LayerMask layerAMask;
		public LayerMask layerBMask;

		public bool enableLevelBounds;
		public Rect levelBounds;
		
		public SonicObject[] objectArray;
		
		void OnEnable()
		{
			Application.targetFrameRate = 60;
			InitializeInstance();
			playerArray = FindObjectsOfType<Player>();
			System.Array.Reverse(playerArray);
			
			objectArray = FindObjectsOfType<SonicObject>();
		}
		
		void InitializeInstance()
		{
			if (Instance != null && Instance != this) 
			{ 
				Destroy(this); 
			} 
			else 
			{ 
				Instance = this; 
			} 
		}
		
		void Start()
		{
			if(Application.isPlaying)
			{
				SoundManager.Instance.PlayMusic(Resources.Load<AudioClip>("Audio/StageMusic/" + levelSong));
			}	
		}

		void Editor()
		{
#if UNITY_EDITOR
			if(Instance != null)
			{
				InitializeInstance();
			}
			if(Selection.Contains (this.gameObject)) // show outline
			{
				DebugUtilis.DrawRect(levelBounds, Color.green);
			}
#endif	
		}
		
		void Update()
		{
			if(Application.isPlaying)
			{
				RuntimeUpdate();
			}
			else
			{
				Editor();
			}
		}
		
		void RuntimeUpdate()
		{	
			float stepDelta;
			
			HandlePausing();
			
			if(!paused)
			{
				foreach(Player player in playerArray)
				{
					subSteps = Mathf.FloorToInt(Mathf.Min(16, Mathf.Max(4, player.Velocity.magnitude/2f + 2f)));
					
					for(int i = 0; i < subSteps; i++) 
					{
						if(subSteps > 0)
						{
							// Stop step delta from reaching infinity when dividing by 0.
							stepDelta = (1f / (float)subSteps) * Time.deltaTime * 60f;
						}
						else 
						{
							// Stop step delta from reaching infinity when dividing by 0.
							stepDelta = 0;
						}
						
						bool LastStep = i == (subSteps - 1) ? true : false;

						player.PlayerUpdate(stepDelta);

						if(LastStep)
						{
							player.DebugUpdate();
							player.cam.CameraUpdate(player);
						}
						
						if(player == playerArray[0])
						{
							foreach(SonicObject obj in objectArray)
							{
								obj.ObjectUpdate(stepDelta);
							}
						}
					}
				}
			}
		}

		bool paused;
		
		void HandlePausing()
		{
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				paused = !paused;
			}
			
			if(paused) Time.timeScale = 0;
			else Time.timeScale = 1;
			
			if(paused)
			{
				if(Input.GetKeyDown(KeyCode.M))
				{
					paused = false;
					SceneLoader.LoadScene("MainMenu");
				}
			}
		}
	}
}

