using UnityEngine;
using System.Collections;

public class Grounder : MonoBehaviour {

    public bool isGrounded = false;
    public LayerMask groundMask = new LayerMask();

    public Vector2 size = Vector2.one;

    public float groundedTick = 0.0f;

    void FixedUpdate()
    {
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
        Vector3 wScale = transform.lossyScale;
        var ground = Physics2D.OverlapArea(pos2D - (size*0.5f) * wScale.x, pos2D + (size*0.5f) * wScale.y, groundMask);
        bool wasGrounded = isGrounded;
        bool newGrounded = ground != null && !ground.isTrigger && ground.gameObject != gameObject;
        if(wasGrounded != newGrounded)
        {
            if (newGrounded)
            {
                groundedTick = 0;
                isGrounded = newGrounded;
                SendMessageUpwards("OnGrounded", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                if(groundedTick > 0.1f) //grace period
                {
                    isGrounded = newGrounded; //left the ground
                }
            }
        }
        groundedTick += Time.fixedDeltaTime;
    }

    public RaycastHit2D GetGroundHit()
    {
        return Physics2D.CircleCast(transform.parent.position, Mathf.Max(size.x, size.y), Vector2.down, Mathf.Abs(transform.localPosition.y + size.y), groundMask);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 wScale = transform.lossyScale;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(size.x * wScale.x, size.y * wScale.y, 0));
    }
}
