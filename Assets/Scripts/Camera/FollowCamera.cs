using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public Transform target = null;
    public float lookDamp = 0.3f;
    public float followDamp = 0.5f;
    public Vector3 offset = new Vector3(0,2,-10);
    public float followGraceDistanceUp = 5;
    public Vector2 screenContaintSize = new Vector2(10, 10);

    Vector2 followVelocity = Vector2.zero;
    Vector2 desiredPosition = Vector2.zero;

    float baseY = 0;

    Camera cam = null;

    bool snap = true;
    float smoothTime = 0;
    float smoothTimeVel = 0;

    public void SnapToTarget() { snap = true; }

    // Use this for initialization
    void Awake () 
    {
        if (target)
        {
            desiredPosition = target.position;
            baseY = target.position.y;
        }
        cam = GetComponent<Camera>();
        smoothTime = followDamp;
    }
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if(target == null && GameManager.Instance.currentPlayer)
            target = GameManager.Instance.currentPlayer.transform;

        var player = GameManager.Instance.currentPlayer;

        Vector2 viewSize = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        Rect worldCameraBounds = GameManager.Instance.GetMapBounds(viewSize);

        float targetSmoothTime = followDamp;

        if (target != null && player && !player.levelComplete)
        {
            desiredPosition = target.position + offset;

            if ((player.grounder.IsGrounded() && Mathf.Abs(player.rigidbody2D.velocity.y) <= 0.1f)
                || player.GetComponent<Health>().GetIsDead())
            {
                baseY = target.position.y + offset.y;
            }
            desiredPosition.y = baseY;

            //Constrain distance from player unless we're clamping to the edge of the world
            Rect screenContraintArea = new Rect((Vector2)target.position - screenContaintSize * 0.5f, screenContaintSize);
            if (!screenContraintArea.Contains(desiredPosition))
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, screenContraintArea.xMin+0.01f, screenContraintArea.xMax- 0.01f);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, screenContraintArea.yMin+ 0.01f, screenContraintArea.yMax- 0.01f);
                baseY = desiredPosition.y;
                targetSmoothTime = 0f;
            }
        }
        else
        {
            //desiredPosition = transform.position;
            //snap = true;
        }

        desiredPosition = Vector2.Min(Vector2.Max(desiredPosition, worldCameraBounds.min), worldCameraBounds.max);

        Vector2 newPos = desiredPosition;
        if (snap)
        {
            followVelocity = Vector3.zero;
            snap = false;
        }
        else
        {
            smoothTime = Mathf.SmoothDamp(smoothTime, targetSmoothTime, ref smoothTimeVel, 0.3f);
            newPos = Vector2.SmoothDamp(transform.position, desiredPosition, ref followVelocity, smoothTime, float.MaxValue, Time.smoothDeltaTime);
        }

        newPos = Vector2.Min(Vector2.Max(newPos, worldCameraBounds.min), worldCameraBounds.max);

        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
	}

    void OnDrawGizmos()
    {
        if(!cam) cam = GetComponent<Camera>();

        Vector2 viewSize = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        Rect worldCameraBounds = GameManager.Instance.GetMapBounds(viewSize);
        //Vector2 mapSize = GameManager.Instance.GetMapSize();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldCameraBounds.center, worldCameraBounds.size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector3)desiredPosition - Vector3.forward, 0.4f);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + Vector3.forward, 0.4f);
    }
}