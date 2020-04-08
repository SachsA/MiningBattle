using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class CreateNewRoom : MonoBehaviourPunCallbacks
{
    #region Private Fields

    #region Serialized Fields

    [SerializeField]
    private Text buttonText = null;

    [SerializeField]
    private Button button = null;

    [SerializeField]
    private InputField inputRoomName = null;

    [SerializeField]
    private Toggle _privateGame = null;

    [SerializeField]
    private GameObject _pwd = null;

    [SerializeField]
    private Text _password = null;

    public Text _roomName;

    #endregion

    #endregion

    #region Monobehaviour Callbacks

    private void Start()
    {
        _privateGame.isOn = false;
        _pwd.SetActive(false);
        inputRoomName.text = "";
    }

    #endregion

    #region Public Methods

    public void OnClick_CreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        button.interactable = false;
        inputRoomName.interactable = false;
        buttonText.text = "In progress...";

        if (_privateGame.isOn)
            _pwd.SetActive(true);
        else
            Validate();
    }

    public void Validate()
    {
        if (_privateGame.isOn && _password.text == "")
        {
            StartCoroutine("ShowMessage");
            return;
        }
        _pwd.SetActive(false);

        Hashtable RoomCustomProps = new Hashtable();
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 4, PublishUserId = true };
        RoomCustomProps["Locked"] = false;
        string[] optionsToPass = new string[1];
        optionsToPass[0] = "Locked";

        if (_privateGame.isOn)
        {
            optionsToPass = new string[2];
            RoomCustomProps["Locked"] = true;
            RoomCustomProps["Password"] = _password.text;
            optionsToPass[0] = "Locked";
            optionsToPass[1] = "Password";
        }
        roomOptions.CustomRoomPropertiesForLobby = optionsToPass;
        roomOptions.CustomRoomProperties = RoomCustomProps;
        if (inputRoomName.text.Equals(""))
            inputRoomName.text = PhotonNetwork.LocalPlayer.NickName + "'s Game";
        PhotonNetwork.CreateRoom(_roomName.text, roomOptions, TypedLobby.Default);
    }

    public void Cancel()
    {
        _privateGame.isOn = false;
        button.interactable = true;
        inputRoomName.interactable = true;
        buttonText.text = "Create Room";
        _pwd.SetActive(false);
    }

    #endregion

    #region PUN Callbacks

    public override void OnCreatedRoom()
    {
        PhotonNetwork.LoadLevel("Room");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Cannot create room: " + message);
    }

    #endregion

    #region ErrorMessage

    [SerializeField]
    private Text _error;

    System.Collections.IEnumerator ShowMessage()
    {
        _error.text = "Cannot be empty.";
        yield return new WaitForSeconds(3f);
        _error.text = "";
    }

    #endregion
}