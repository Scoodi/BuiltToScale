using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreathingAnimation : MonoBehaviour
{
    public float AnimationRate = 1f;
    public float randomVariation = 0.2f;
    public Sprite[] animationSprites;
    public Image display;
    private int currentFrame = 0;
    void Start()
    {
        StartCoroutine(BreatheLoop(Random.Range(0f,AnimationRate)));
    }

    IEnumerator BreatheLoop(float time)
    {
        display.sprite = animationSprites[currentFrame];
        if (currentFrame < animationSprites.Length - 1 )
        {
            currentFrame++;
        } else
        {
            currentFrame = 0;
        }

        yield return new WaitForSeconds(time);
        StartCoroutine(BreatheLoop(AnimationRate + Random.Range(-randomVariation,randomVariation)));
    }
}
