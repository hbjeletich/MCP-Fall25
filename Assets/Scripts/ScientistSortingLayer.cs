using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScientistSortingLayer : MonoBehaviour
{
    public float targetZPos = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSortingOrder();
    }

    void Update()
    {
        UpdateSortingOrder();
    }

    void UpdateSortingOrder()
    {
        if (spriteRenderer != null)
        {
            if (transform.position.z < targetZPos)
            {
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 200;
            }
            else
            {
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 1;
            }
        }
    }
}