using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillTrigger : MonoBehaviour
{
    public BoxCollider2D boxCollider;

    public Vector2 damageDirection = Vector2.up;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var body = collision.GetComponent<Rigidbody2D>();
        if(body && damageDirection != Vector2.zero && Vector2.Angle(-body.velocity, transform.rotation * damageDirection) >= 90)
        {
            return;
        }

        var health = collision.GetComponent<Health>();
        if(health)
        {
            health.Kill(false, true);
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider)
        {
            Gizmos.color = new Color(1,0,0,0.5f);
            Gizmos.DrawCube(transform.position, boxCollider.size * transform.lossyScale);

            if(damageDirection != Vector2.zero)
            {
                Gizmos.color = Color.white;
                Vector2 dir = transform.rotation * damageDirection.normalized;
                Vector2 start = transform.position;
                Vector2 end = start + dir;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawLine(end, end + (Vector2)(Quaternion.Euler(0, 0, 150) * dir) * 0.5f);
                Gizmos.DrawLine(end, end + (Vector2)(Quaternion.Euler(0, 0, -150) * dir) * 0.5f);
            }
        }
    }
}
