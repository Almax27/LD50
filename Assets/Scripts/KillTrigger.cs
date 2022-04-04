using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillTrigger : MonoBehaviour
{
    public BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var health = collision.GetComponent<Health>();
        if(health)
        {
            health.Kill();
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, boxCollider.size * transform.lossyScale);
        }
    }
}
