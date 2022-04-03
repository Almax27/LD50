using UnityEngine;
using System.Collections;

public class PixelCameraHelper : MonoBehaviour
{
    public float pixelsPerUnit = 16;
    public int pixelScale = 1;
    public float smoothTime = 0.2f;
    new Camera camera = null;

    float velocity = 0;

    // Use this for initialization
    void Start()
    {
        camera = GetComponent<Camera>();
        camera.orthographicSize = CalculateOrthSize();
    }
	
    // Update is called once per frame
    void Update()
    {
        camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, CalculateOrthSize(), ref velocity, smoothTime);
    }

    float CalculateOrthSize()
    {
        float orthographicSize = 0.5f * (Screen.height / pixelsPerUnit);
        orthographicSize /= pixelScale;
        return orthographicSize;
    }
}

