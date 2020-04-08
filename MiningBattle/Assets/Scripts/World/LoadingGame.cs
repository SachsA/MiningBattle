using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingGame : MonoBehaviour, IPunObservable
{
    [SerializeField]
    private Text progressText;

    [SerializeField]
    private Slider progressSlider;

    private void Awake()
    {
        progressSlider.value = 0;
        progressText.text = "0 %";
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(progressSlider.value);
        }
        else
        {
            float nextValue = (float)stream.ReceiveNext();
            StartCoroutine("ProgressTheBar", nextValue);
        }
    }

    public void IncreaseProgress(float _toAdd)
    {
        float nextValue = _toAdd + progressSlider.value;
        if (_toAdd == 1f)
        {
            nextValue = 1f;
        }
        StartCoroutine("ProgressTheBar", nextValue);
    }

    IEnumerator ProgressTheBar(float _nextValue)
    {
        while (progressSlider.value <= _nextValue)
        {
            progressSlider.value += 0.05f;
            progressText.text = (int)(progressSlider.value * 100) + " %";
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDestroy()
    {
        progressSlider.value = 1f;
        progressText.text = "100 %";
        StopCoroutine("ProgressTheBar");
    }
}
