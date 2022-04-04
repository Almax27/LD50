using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimPickup : MonoBehaviour
{
    public float pickupDestroyDelay = 1.0f;
    public FAFAudioSFXSetup pickupSFX = null;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var playerHealth = GameManager.Instance.currentPlayer.GetComponent<Health>();
            playerHealth.Heal(1);

            var animator = GetComponent<Animator>();
            if (animator)
            {
                animator.SetTrigger("onPickup");
            }

            Destroy(gameObject, pickupDestroyDelay);

            pickupSFX?.Play(transform.position);
        }
    }
}
