using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	public class BackgroundParallax : MonoBehaviour
	{
		public List<Scroller> scrollers;
		public SonicCamera camera;
		public Camera cam;
		
		public Vector3 cameraDelta;
		
		public float currentHeight;
		public float normalizedHeight;
		
		void Awake()
		{
			cam = camera.GetComponent<Camera>();
		}
		
		public void Update()
		{
			float camHeight = 2f * cam.orthographicSize;
			float camWidth = camHeight * cam.aspect;

			currentHeight = (camera.transform.position.y - (PhysicsManager.Instance.levelBounds.y + camHeight / 2));
			normalizedHeight = currentHeight / (PhysicsManager.Instance.levelBounds.height - camHeight);
			
			cameraDelta = camera.transform.position - camera.oldPosition;
			scrollers[0].x += cameraDelta.x * scrollers[0].parallaxX * Time.deltaTime * 60f;

			float scrollerY = PhysicsManager.Instance.levelBounds.y + scrollers[0].height + normalizedHeight * (PhysicsManager.Instance.levelBounds.height - scrollers[0].height);
				
			for(int i = 0; i < scrollers.Count; i++) {
				Vector3 relativePosition = camera.transform.position - scrollers[i].transform.position;
			
				if(relativePosition.x <= 0)
				{
					scrollers[i].offset.x -= scrollers[i].width;
				}
				if(relativePosition.x >= scrollers[i].width)
				{
					scrollers[i].offset.x += scrollers[i].width;
				}

				scrollers[i].offset.y = Mathf.Floor(scrollerY*16f)/16f;
			
				if(i != 0){
					scrollers[i].x += cameraDelta.x * scrollers[i].parallaxX * Time.deltaTime * 60f;
				}
			}
		}
	}
}