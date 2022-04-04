using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDamager : MonoBehaviour
{
    public LayerMask layerMask;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if((layerMask.value | collision.gameObject.layer) != 0)
        {
            collision.gameObject.SendMessageUpwards("OnDamage", new Damage(1, gameObject), SendMessageOptions.DontRequireReceiver);
        }
    }
}
