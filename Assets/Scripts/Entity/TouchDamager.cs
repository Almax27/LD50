using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDamager : MonoBehaviour
{
    public float damage = 1;
    public float gracePeriod = 0.5f;
    public LayerMask layerMask;

    float lastDamageTime = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if((layerMask & 1<<collision.gameObject.layer) != 0 && Time.time - lastDamageTime > gracePeriod)
        {
            lastDamageTime = Time.time;
            var damageObj = new Damage(damage, gameObject);
            collision.gameObject.SendMessageUpwards("OnDamage", damageObj, SendMessageOptions.DontRequireReceiver);
            SendMessageUpwards("OnAttackHit", damageObj, SendMessageOptions.DontRequireReceiver);
        }
    }
}
