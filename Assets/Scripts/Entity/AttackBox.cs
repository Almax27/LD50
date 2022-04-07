using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public Vector2 size = new Vector2(1,1);
    public LayerMask layerMask;

    public float lastHitTime { get; private set; }

    void DealDamage_Internal(Damage damage)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, size, 0, layerMask);
        if (colliders != null)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject == gameObject || collider.gameObject == damage.owner)
                    continue;
                    
                if (!damage.hitObjects.Contains(collider.gameObject))
                {
                    damage.hitObjects.Add(collider.gameObject);
                    collider.gameObject.SendMessageUpwards("OnDamage", damage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        if(damage.hitObjects.Count > 0)
        {
            damage.hitSFX?.Play(transform.position);
            lastHitTime = Time.time;
            SendMessageUpwards("OnAttackHit", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    IEnumerator DealDamage_Routine(Damage damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        DealDamage_Internal(damage);
    }

    public void DealDamage(Damage damage, float delay = 0)
    {
        StartCoroutine(DealDamage_Routine(damage, delay));
    }

    public bool IsTargetInRange(Vector2 sizeAdjustment)
    {
        Vector2 adjustedSize = Vector2.Max(Vector2.zero, size + sizeAdjustment);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, adjustedSize, 0, layerMask);
        Debug.DrawLine(transform.position, (Vector2)transform.position + adjustedSize * 0.5f, Color.white, 3.0f);
        if (colliders != null)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject == gameObject || collider.gameObject == transform.root.gameObject)
                    continue;

                return true;
            }
        }
        return false;
    }
   

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
