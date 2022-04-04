using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public Vector2 size = new Vector2(1,1);
    public LayerMask layerMask;

    void DealDamage_Internal(Damage damage)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, size, 0, layerMask);
        List<GameObject> hitObjects = new List<GameObject>();
        if (colliders != null)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject == gameObject || collider.gameObject == damage.owner)
                    continue;
                    
                if (!hitObjects.Contains(collider.gameObject))
                {
                    hitObjects.Add(collider.gameObject);
                    collider.gameObject.SendMessageUpwards("OnDamage", damage);
                }
            }
        }

        if(hitObjects.Count > 0)
        {
            damage.hitSFX?.Play(transform.position);
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
   

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
