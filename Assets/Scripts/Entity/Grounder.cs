using UnityEngine;
using System.Collections;

public class Grounder : MonoBehaviour {

    public bool isGrounded = false;
    public LayerMask groundMask = new LayerMask();

    public Vector2 size = Vector2.one;

    void FixedUpdate()
    {
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
        Vector3 wScale = transform.lossyScale;
        var ground = Physics2D.OverlapArea(pos2D - (size*0.5f) * wScale.x, pos2D + (size*0.5f) * wScale.y, groundMask);
        bool wasGrounded = isGrounded;
        isGrounded = ground != null && ground.gameObject != gameObject;
        if (!wasGrounded && isGrounded)
        {
            SendMessageUpwards("OnGrounded", SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 wScale = transform.lossyScale;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(size.x * wScale.x, size.y * wScale.y, 0));
    }
}
