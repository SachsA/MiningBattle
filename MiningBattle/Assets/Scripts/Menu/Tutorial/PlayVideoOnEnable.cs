using UnityEngine;
using UnityEngine.Video;

public class PlayVideoOnEnable : MonoBehaviour
{
    #region PublicVariables

    public VideoPlayer videoPlayer;
    public VideoClip videoClip;

    #endregion

    #region MonoBehaviour

    private void OnEnable()
    {
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
    }
    
    private void OnDisable()
    {
        videoPlayer.targetTexture.Release();
    }

    #endregion
}
