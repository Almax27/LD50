using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimPickup : MonoBehaviour
{
    public float pickupDestroyDelay = 1.0f;
    public FAFAudioSFXSetup pickupSFX = null;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(enabled && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var player = collision.GetComponentInChildren<PlayerController>();
            var health = collision.GetComponentInChildren<Health>();
            var animator = GetComponentInChildren<Animator>();

            if(player && health && animator && !health.GetIsDead() && !player.isSleeping)
            {
                enabled = false;

                player.sleepTimer = 0;
                player.lastStimTime = Time.time;

                animator.SetTrigger("onPickup");

                Destroy(gameObject, pickupDestroyDelay);

                pickupSFX?.Play(transform.position);
            }            
        }
    }
}
