using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageAnimation : MonoBehaviour
{
    private Vector3 FirstPosition = new Vector3(0.4f, 0.5f, 0);
    private Vector3 SecondPosition = new Vector3(0.6f, 0.5f, 0);

    public SpriteRenderer SpriteRenderer;

    private Color FirstColor = new Color(1, 1, 1, 1);
    private Color SecondColor = new Color(1, 1, 1, 0.5f);

    private bool LastWaitingState = false;

    public WaitingAnimation WaitingAnimation;

    private void Awake()
    {
        transform.localPosition = FirstPosition;
        LastWaitingState = WaitingAnimation.GetIsStarted();
    }

    private void Update()
    {
        if (LastWaitingState != WaitingAnimation.GetIsStarted())
        {
            LastWaitingState = WaitingAnimation.GetIsStarted();
            if (!LastWaitingState)
            {
                SpriteRenderer.color = FirstColor;
                transform.localPosition = FirstPosition;
            }
            else
            {
                SpriteRenderer.color = SecondColor;
                transform.localPosition = SecondPosition;
            }
        }
    }

    public void Full()
    {
        LastWaitingState = WaitingAnimation.GetIsStarted();
        if (!LastWaitingState)
        {
            SpriteRenderer.color = FirstColor;
            transform.localPosition = FirstPosition;
        } else
        {
            SpriteRenderer.color = SecondColor;
            transform.localPosition = SecondPosition;
        }
    }

    public void Active(bool _active)
    {
        SpriteRenderer.enabled = _active;
    }
}
