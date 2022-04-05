using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public Transform target = null;
    public float lookDamp = 0.3f;
    public float followDamp = 0.5f;
    public Vector3 offset = new Vector3(0,2,-10);
    public float followGraceDistanceUp = 5;
    public float maxDistanceFromTarget = 5;

    Vector3 followVelocity = Vector3.zero;
    Vector3 desiredPosition = Vector3.zero;



    float baseY = 0;

    Camera cam = null;

    bool snap = true;

	// Use this for initialization
	void Start () 
    {
        if (target)
        {
            desiredPosition = target.position;
            baseY = target.position.y;
        }
        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if(target == null && GameManager.Instance.currentPlayer)
            target = GameManager.Instance.currentPlayer.transform;

        var player = GameManager.Instance.currentPlayer;

        if (target != null && player)
        {
            desiredPosition = target.position + offset;

            if (player.grounder.isGrounded && player.grounder.groundedTick > 0.1f && Mathf.Abs(player.rigidbody2D.velocity.y) <= 0.1f)
            {
                baseY = target.position.y + offset.y;
            }
            desiredPosition.y = baseY;

            if (snap)
            {
                transform.position = desiredPosition;
                followVelocity = Vector3.zero;
                snap = false;
            }
            else
            {
                if (followDamp > 0)
                {
                    var newPos = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, followDamp, float.MaxValue, Time.smoothDeltaTime);
                    var relPos = newPos - target.position;
                    var clampedPos = Vector2.ClampMagnitude(newPos - target.position, maxDistanceFromTarget);
                    relPos.x = clampedPos.x;
                    relPos.y = clampedPos.y;
                    transform.position = target.position + relPos;
                }
                else if (followDamp == 0)
                {
                    transform.position = desiredPosition;
                }
            }
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, followDamp, float.MaxValue, Time.smoothDeltaTime);
            snap = true;
        }

        //clamp to world size
        var pos = transform.position;
        Vector2 viewSize = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);

        //Clamp from top left
        Vector2 mapSize = GameManager.Instance.GetMapSize();
        pos.x = Mathf.Clamp(pos.x, viewSize.x, mapSize.x - viewSize.x);
        pos.y = Mathf.Clamp(pos.y, -mapSize.y + viewSize.y, -viewSize.y);
        transform.position = pos;
	}

    void OnDrawGizmos()
    {
        Vector2 mapSize = GameManager.Instance.GetMapSize();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(mapSize.x * 0.5f, -mapSize.y * 0.5f), mapSize);
    }
}