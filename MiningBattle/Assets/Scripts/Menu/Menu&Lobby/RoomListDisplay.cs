using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class RoomListDisplay : MonoBehaviourPunCallbacks
{
    #region Private Fields

    #region Serialized Fields

    //[SerializeField]
    //private GameObject _roomPrefab = null;
    //private GameObject RoomPrefab
    //{
    //    get { return _roomPrefab; }
    //}

    #endregion

    //private List<RoomList> _roomListButtons = new List<RoomList>();
    //private List<RoomList> RoomListButtons
    //{
    //    get { return _roomListButtons; }
    //}

    #endregion

    #region Public Methods

    //public List<RoomList> GetRoomList()
    //{
    //    return _roomListButtons;
    //}

    #endregion

    //#region Pun Callbacks

    //public override void OnRoomListUpdate(List<RoomInfo> roomList)
    //{
    //    Debug.Log("Here");
    //    foreach (RoomInfo room in roomList)
    //    {
    //        RoomReceived(room);
    //    }
    //    RemoveOldRooms();
    //}

    //#endregion

    //#region Public Methods

    //public void RoomReceived(RoomInfo room)
    //{
    //    int index = RoomListButtons.FindIndex(x => x.RoomName == room.Name);

    //    if (index == -1)
    //    {
    //        if (room.IsVisible && room.PlayerCount < room.MaxPlayers)
    //        {
    //            GameObject roomListingObj = Instantiate(RoomPrefab);
    //            roomListingObj.transform.SetParent(transform, false);

    //            RoomList roomListing = roomListingObj.GetComponent<RoomList>();
    //            RoomListButtons.Add(roomListing);

    //            index = (RoomListButtons.Count - 1);

    //        }
    //    }

    //    if (index != -1)
    //    {
    //        RoomList roomListing = RoomListButtons[index];
    //        roomListing.SetRoomNameText(room.Name);
    //        roomListing.SetRoomNbPlayers(room.PlayerCount);
    //        roomListing.SetLocked((bool)room.CustomProperties["Locked"]);
    //        if ((bool)room.CustomProperties["Locked"])
    //        {
    //            roomListing.SetPassword((string)room.CustomProperties["Password"]);
    //        }
    //        roomListing.Updated = true;
    //    }
    //}

    //#endregion 

    //#region Pun RPC

    //[PunRPC]
    //public void RemoveOldRooms()
    //{
    //    List<RoomList> removeRooms = new List<RoomList>();

    //    foreach (RoomList roomListing in RoomListButtons)
    //    {
    //        if (roomListing.RoomNbPlayers == 0)
    //        {
    //            removeRooms.Add(roomListing);
    //        }
    //    }

    //    foreach (RoomList roomListing in removeRooms)
    //    {
    //        GameObject roomListingObj = roomListing.gameObject;
    //        RoomListButtons.Remove(roomListing);
    //        Destroy(roomListingObj);
    //    }
    //}

    //#endregion
}
