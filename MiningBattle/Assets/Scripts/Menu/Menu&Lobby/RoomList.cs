using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomList : MonoBehaviour
{
    #region Private Fields

    #region Serialized Fields

    [SerializeField]
    private Text _roomNameText = null;
    private Text RoomNameText
    {
        get { return _roomNameText; }
    }

    [SerializeField]
    private Text _roomNbPlayersText = null;
    private Text RoomNbPlayersText
    {
        get { return _roomNbPlayersText; }
    }

    [SerializeField]
    private GameObject _locked = null;

    #endregion

    private string _password = "";

    #endregion

    #region Monobehaviour Callbacks

    private void Start()
    {
        GameObject lobbyGO = GameObject.Find("Lobby");
        if (lobbyGO != null)
        {
            Button button = GetComponent<Button>();
            button.onClick.AddListener(() => lobbyGO.GetComponent<LobbyFunction>().OnClickRoom(RoomNameText.text));
        }
    }

    private void OnDestroy()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
    }

    #endregion

    #region Public Methods

    public int RoomNbPlayers { get; private set; }
    public void SetRoomNbPlayers(int nbPlayers)
    {
        RoomNbPlayers = nbPlayers;
        RoomNbPlayersText.text = RoomNbPlayers.ToString() + "/4";
    }

    public string RoomName { get; private set; }
    public void SetRoomNameText(string text)
    {
        RoomName = text;
        RoomNameText.text = RoomName;
    }

    public bool Updated { get; set; }

    public void SetLocked(bool locked)
    {
        _locked.SetActive(locked);
    }

    public bool GetLocked()
    {
        return _locked.activeSelf;
    }

    public void SetPassword(string pwd)
    {
        _password = pwd;
    }

    public string GetPassword()
    {
        return _password;
    }

    #endregion
}
