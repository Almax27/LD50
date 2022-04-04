using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayerSorter : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance && GameManager.Instance.gameLayer)
        {
            foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.sortingOrder = GameManager.Instance.gameLayer.m_SortOrder;
            }
        }
    }
}
