using UnityEngine;
using System.Collections;

public class Grounder : MonoBehaviour {

    public LayerMask groundMask = new LayerMask();

    public Vector2 size = Vector2.one;

    public float groundedTick = 0.0f;

    bool isGrounded = false;

    private void Start()
    {
        UpdateGrounded();
    }

    void Update()
    {
        if(UpdateGrounded() && isGrounded)
        {
            SendMessageUpwards("OnGrounded", SendMessageOptions.DontRequireReceiver);
        }
        groundedTick += Time.fixedDeltaTime;
    }

    public bool IsGrounded(float grace = 0.1f)
    {
        return isGrounded && groundedTick >= grace;
    }

    bool UpdateGrounded()
    {
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
        Vector3 wScale = transform.lossyScale;
        var ground = GetGroundHit();
        bool wasGrounded = isGrounded;
        bool newGrounded = ground;
        if (wasGrounded != newGrounded)
        {
            groundedTick = 0;
            isGrounded = newGrounded;
            return true;
        }
        return false;
    }

    public RaycastHit2D GetGroundHit()
    {
        var hits = Physics2D.BoxCastAll(transform.parent.position, size, 0, Vector2.down, Mathf.Abs(transform.localPosition.y), groundMask);
        foreach (var hit in hits)
        {
            if (hit && hit.collider && !hit.collider.isTrigger)
            {
                return hit;
            }
        }
        return new RaycastHit2D();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 wScale = transform.lossyScale;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(size.x * wScale.x, size.y * wScale.y, 0));
    }
}
