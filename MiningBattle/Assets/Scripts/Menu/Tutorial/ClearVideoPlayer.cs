using UnityEngine;
using UnityEngine.Video;

public class ClearVideoPlayer : MonoBehaviour
{
    #region PublicVariables

    public VideoPlayer videoPlayer;

    #endregion

    #region MonoBehaviour

    private void OnEnable()
    {
        videoPlayer.targetTexture.Release();
    }

    private void OnDisable()
    {
        videoPlayer.targetTexture.Release();
    }

    #endregion
}
