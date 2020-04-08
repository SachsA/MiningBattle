using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingAnimation : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    public Sprite Dot;

    public SpriteRenderer DotRenderer1;
    public SpriteRenderer DotRenderer2;
    public SpriteRenderer DotRenderer3;

    private bool IsStarted = false;

    void Start()
    {
        DotRenderer1.enabled = false;
        DotRenderer2.enabled = false;
        DotRenderer3.enabled = false;
    }

    IEnumerator Waiting()
    {
        while (true)
        {
            DotRenderer1.enabled = true;
            yield return new WaitForSeconds(0.5f);
            DotRenderer2.enabled = true;
            yield return new WaitForSeconds(0.5f);
            DotRenderer3.enabled = true;
            yield return new WaitForSeconds(0.5f);
            DotRenderer1.enabled = false;
            DotRenderer2.enabled = false;
            DotRenderer3.enabled = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void StartWait()
    {
        IsStarted = true;
        StartCoroutine("Waiting");
    }

    public void StopWait()
    {
        IsStarted = false;
        StopCoroutine("Waiting");
        DotRenderer1.enabled = false;
        DotRenderer2.enabled = false;
        DotRenderer3.enabled = false;
    }

    public bool GetIsStarted()
    {
        return IsStarted;
    }

    public void Active(bool _active)
    {
        SpriteRenderer.enabled = _active;
    }
}
