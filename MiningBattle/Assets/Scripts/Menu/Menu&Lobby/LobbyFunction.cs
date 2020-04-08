using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyFunction : MonoBehaviourPunCallbacks
{
    #region Private Fields

    #region Serialized Fields

    [SerializeField]
    private GameObject _passwordObj = null;

    [SerializeField]
    private Text _inputPwd = null;

    private List<RoomList> _roomList = new List<RoomList>();

    [SerializeField]
    private GameObject _roomPrefab = null;

    [SerializeField]
    private GameObject _roomContent = null;

    #endregion

    private RoomList _room = null;

    #endregion

    #region Monobehaviour Callbacks

    private void Start()
    {
        _passwordObj.SetActive(false);
    }

    #endregion

    #region Public Methods

    public void OnClickRoom(string roomName)
    {
        foreach (RoomList room in _roomList)
        {
            if (room.RoomName == roomName)
            {
                _room = room;
                if (_room.GetLocked())
                {
                    _passwordObj.SetActive(true);
                }
                else
                {
                    Validate();
                }
                break;
            }
        }
    }

    public void Validate()
    {
        if (_room.GetLocked())
        {
            if (_room.GetPassword() != _inputPwd.text)
            {
                StartCoroutine("ShowMessage");
                return;
            }
        }
        PhotonNetwork.JoinRoom(_room.RoomName);
    }

    public void Cancel()
    {
        _passwordObj.SetActive(false);
    }

    #endregion

    #region ErrorMessage

    [SerializeField]
    private Text _error;

    System.Collections.IEnumerator ShowMessage()
    {
        _error.text = "Wrong password.";
        yield return new WaitForSeconds(3f);
        _error.text = "";
    }

    #endregion

    #region Pun Callbacks

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
            RoomReceived(room);
        RemoveOldRooms();
    }

    #endregion

    #region Private Methods

    private void RoomReceived(RoomInfo room)
    {
        int index = _roomList.FindIndex(x => x.RoomName == room.Name);

        if (index == -1)
        {
            if (room.IsVisible && room.PlayerCount < room.MaxPlayers)
            {
                GameObject roomListingObj = Instantiate(_roomPrefab);
                roomListingObj.transform.SetParent(_roomContent.transform, false);

                RoomList roomListing = roomListingObj.GetComponent<RoomList>();
                _roomList.Add(roomListing);

                index = (_roomList.Count - 1);

            }
        }

        if (index != -1)
        {
            RoomList roomListing = _roomList[index];
            roomListing.SetRoomNameText(room.Name);
            roomListing.SetRoomNbPlayers(room.PlayerCount);
            if (room.PlayerCount != 0)
            {
                roomListing.SetLocked((bool)room.CustomProperties["Locked"]);
                if ((bool)room.CustomProperties["Locked"])
                    roomListing.SetPassword((string)room.CustomProperties["Password"]);
            }
            roomListing.Updated = true;
        }
    }

    #endregion 

    #region Pun RPC

    [PunRPC]
    public void RemoveOldRooms()
    {
        List<RoomList> removeRooms = new List<RoomList>();

        foreach (RoomList roomListing in _roomList)
        {
            if (roomListing.RoomNbPlayers == 0)
            {
                removeRooms.Add(roomListing);
            }
        }

        foreach (RoomList roomListing in removeRooms)
        {
            GameObject roomListingObj = roomListing.gameObject;
            _roomList.Remove(roomListing);
            Destroy(roomListingObj);
        }
    }

    #endregion
}