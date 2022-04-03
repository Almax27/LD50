using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public Transform target = null;
    public float lookDamp = 0.3f;
    public float followDamp = 0.5f;
    public Vector3 offset = new Vector3(0,2,-10);
    public Vector2 worldSize = new Vector2(100, 100);

    Vector3 followVelocity = Vector3.zero;
    Vector3 desiredPosition = Vector3.zero;

    Camera cam = null;

    bool snap = true;


	// Use this for initialization
	void Start () 
    {
        if (target)
        {
            desiredPosition = target.position;
        }
        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if (target != null)
        {
            desiredPosition = target.position;
            if (snap)
            {
                transform.position = desiredPosition + offset;
                followVelocity = Vector3.zero;
                snap = false;
            }
            else
            {
                if (followDamp > 0)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition + offset, ref followVelocity, followDamp, float.MaxValue, Time.smoothDeltaTime);
                }
                else if (followDamp == 0)
                {
                    transform.position = desiredPosition + offset;
                }
            }
        }
        else
        {
            snap = true;
        }

        //clamp to world size
        var pos = transform.position;
        Vector2 viewSize = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        float x = Mathf.Max(0, worldSize.x*0.5f - viewSize.x);
        float y = Mathf.Max(0, worldSize.y*0.5f - viewSize.y);
        pos.x = Mathf.Clamp(pos.x, -x, x);
        pos.y = Mathf.Clamp(pos.y, -y, y);
        transform.position = pos;
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, worldSize);
    }
}