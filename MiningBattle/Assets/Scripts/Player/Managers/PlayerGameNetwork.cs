using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGameNetwork : MonoBehaviourPunCallbacks
{
    public static PlayerGameNetwork Instance = null;

    private void Awake()
    {
        if (Instance != null)
            return;
        DontDestroyOnLoad(this);
        Instance = this;
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Menu");
    }

    public override void OnJoinedRoom()
    {
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (SceneManager.GetActiveScene().name == "Menu")
            return;
        Application.Quit();
    }
}
