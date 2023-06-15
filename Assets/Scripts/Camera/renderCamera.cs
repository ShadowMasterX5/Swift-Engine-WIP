using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class renderCamera : MonoBehaviour
{
    public RenderTexture rt;

    Camera cam;
    Rect cameraViewRect;

    void Start()
    {
        cam = GetComponent<Camera>();
        cameraViewRect = new Rect(cam.rect.xMin * Screen.width, Screen.height - cam.rect.yMax * Screen.height, cam.pixelWidth, cam.pixelHeight);
    }

    void OnGUI()
    {
        //
    }

    void OnPostRender()
    {
        
        Graphics.DrawTexture(cameraViewRect, rt);
    }
}


