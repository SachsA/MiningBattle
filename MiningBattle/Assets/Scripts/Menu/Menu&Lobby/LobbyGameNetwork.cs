using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyGameNetwork : MonoBehaviourPunCallbacks
{
    #region Public Fields

    public static LobbyGameNetwork Instance;

    #endregion

    #region Private Fields

    #region Serialized Fields

    [SerializeField]
    private GameObject connected = null;

    [SerializeField]
    private GameObject loading = null;

    #endregion

    #endregion

    #region Monobehaviour Callbacks

    private void Awake()
    {
        Instance = this;
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.GameVersion = "1";
    }

    #endregion

    #region Public Methods

    public void StartConnection()
    {
        connected.SetActive(false);
        loading.SetActive(true);
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            connected.SetActive(true);
            loading.SetActive(false);
        }
    }

    #endregion

    #region PUN Callbacks

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");

        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        Hashtable PlayerCustomProps = new Hashtable();
        PlayerCustomProps["Ping"] = PhotonNetwork.GetPing();
        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerCustomProps);

        connected.SetActive(true);
        loading.SetActive(false);
        PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        if (!PhotonNetwork.InRoom)
        {
           CanvasManager.Instance.lobbyFunction.transform.SetAsLastSibling();
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Cannot join room: " + message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        CrossSceneInformation.isPlayerConnected = false;
    }

    #endregion
}
