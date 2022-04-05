using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    public BoxCollider2D boxCollider;

    public string targetLevel = "Title";


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(LevelComplete_Routine());
        }
    }

    IEnumerator LevelComplete_Routine()
    {
        var player = GameManager.Instance.currentPlayer;
        if(player)
        {
            player.levelComplete = true;
        }

        yield return new WaitForSeconds(3.0f);

        LevelTransition.Instance.TransitionToLevel(targetLevel);
    }

    private void OnDrawGizmos()
    {
        if (boxCollider)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, boxCollider.size * transform.lossyScale);
        }
    }
}
