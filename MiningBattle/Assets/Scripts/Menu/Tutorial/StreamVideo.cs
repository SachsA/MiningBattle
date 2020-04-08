using UnityEngine;
using UnityEngine.Video;

public class StreamVideo : MonoBehaviour
{
    #region PublicVariables

    public VideoPlayer videoPlayer;
    public VideoClip videoClip;

    #endregion

    #region PublicMethods

    public void PlayVideo()
    {
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
    }

    #endregion
}
