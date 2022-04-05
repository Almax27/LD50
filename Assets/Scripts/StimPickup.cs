using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimPickup : MonoBehaviour
{
    public float pickupDestroyDelay = 1.0f;
    public FAFAudioSFXSetup pickupSFX = null;

    public bool autoUse = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var animator = GetComponent<Animator>();
            if (animator)
            {
                animator.SetTrigger("onPickup");
            }

            Destroy(gameObject, pickupDestroyDelay);

            pickupSFX?.Play(transform.position);

            if(autoUse)
            {
                GameManager.Instance.currentPlayer.sleepTimer = GameManager.Instance.currentPlayer.timeToSleep;
            }
            else
            {
                var playerHealth = GameManager.Instance.currentPlayer.GetComponent<Health>();
                playerHealth.Heal(1);
            }
        }
    }
}
