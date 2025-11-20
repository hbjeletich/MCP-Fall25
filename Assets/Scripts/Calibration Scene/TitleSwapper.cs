using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleSwapper : MonoBehaviour
{
    [Header("Title Swapper Settings")]
    public Sprite[] titleSprites;
    public Image titleImage;
    public Vector2 swapTiming = new Vector2(2.0f,5.0f);
    
    [Header("Bounce Settings")]
    public float bounceScale = 0.2f;
    public float bounceSpeed = 2.0f;

    void Start()
    {
        int startingIndex = Random.Range(0, titleSprites.Length);
        titleImage.sprite=titleSprites[startingIndex];

        StartCoroutine(SwapRoutine());
        StartCoroutine(BounceRoutine());
    }

    void SwapSprite(int index)
    {
        if(index>=0 && index<titleSprites.Length)
        {
            titleImage.sprite=titleSprites[index];
        }
    }

    IEnumerator SwapRoutine()
    {
        while(true)
        {
            float waitTime = Random.Range(swapTiming.x, swapTiming.y);
            yield return new WaitForSeconds(waitTime);

            int newIndex = Random.Range(0, titleSprites.Length);
            SwapSprite(newIndex);
        }
    }

    IEnumerator BounceRoutine()
    {
        Vector3 originalScale = titleImage.transform.localScale;
        float timer = 0.0f;

        while(true)
        {
            timer += Time.deltaTime * bounceSpeed;
            float scaleFactor = 1.0f + Mathf.Sin(timer) * bounceScale;
            titleImage.transform.localScale = originalScale * scaleFactor;

            yield return null;
        }
    }
}
