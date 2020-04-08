using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;

public class RoomManager : MonoBehaviour, IPunObservable
{
    #region Public Fields

    public static RoomManager Instance;

    public const string CountdownStartTime = "StartTime";
    public const string CountdownStop = "Stop";

    public bool timerHasStarted = false;

    public GameObject Loading;

    #endregion

    #region Private Fields

    #region Serialized Fields

    [SerializeField]
    private Text roomName = null;

    [SerializeField]
    private GameObject readyText = null;

    [SerializeField]
    private GameObject readyCheck = null;

    [SerializeField]
    private Transform[] PlayersPosition = new Transform[4];

    #endregion

    private List<GameObject> players = new List<GameObject>();

    private Dictionary<int, string> PlayerColors = new Dictionary<int, string>();

    private int PlayerIndex = 0;
    #endregion

    #region Monobehaviour Callbacks

    private void Awake()
    {
        Instance = this;
        timerHasStarted = false;
        PlayerColors.Add(0, "Blue");
        PlayerColors.Add(1, "Red");
        PlayerColors.Add(2, "Green");
        PlayerColors.Add(3, "Yellow");
        PlayerIndex = 0;
        Loading.SetActive(false);
    }

    private void Start()
    {
        if (roomName)
            roomName.text = PhotonNetwork.CurrentRoom.Name;
        if (PhotonNetwork.IsMasterClient)
            updatePlayersInRoom();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && players.Count != PhotonNetwork.CurrentRoom.PlayerCount)
        {
            readyText.SetActive(true);
            readyCheck.SetActive(false);
            Hashtable RoomCustomProps = new Hashtable();
            RoomCustomProps[CountdownStop] = (float)PhotonNetwork.Time;
            PhotonNetwork.CurrentRoom.SetCustomProperties(RoomCustomProps);
            PhotonNetwork.RaiseEvent(EventRoomLoading, false, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
            updatePlayersInRoom();
        }
        if (PhotonNetwork.IsMasterClient && !timerHasStarted)
            PlayersReady();
    }

    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    #endregion

    #region Public Methods

    public void LoadGame()
    {
        if (GameObject.Find("MusicMenu") != null)
            GameObject.Find("MusicMenu").GetComponent<AudioSource>().Stop();

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }

    public void LeaveRoom()
    {
        CrossSceneInformation.isPlayerConnected = true;
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Menu");
    }

    public void Ready()
    {
        if (PhotonNetwork.IsMasterClient && players.Count <= 1)
            return;
        GameObject[] ps = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in ps)
        {
            if (player.GetComponent<PlayerBox>().GetPlayer().UserId == PhotonNetwork.LocalPlayer.UserId)
            {
                player.GetComponent<PlayerBox>().setStatus(!player.GetComponent<PlayerBox>()._isReady);
                ChangeReadyButton(player.GetComponent<PlayerBox>()._isReady);
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("ChangePlayerStatus", RpcTarget.All, player.GetComponent<PlayerBox>().GetPlayer().UserId, player.GetComponent<PlayerBox>()._isReady);
                break;
            }
        }
    }

    #endregion

    #region Private Methods

    private void updatePlayersInRoom()
    {
        Dictionary<int, Player> players = PhotonNetwork.CurrentRoom.Players;
        Dictionary<int, bool> playersReady = new Dictionary<int, bool>();
        GameObject[] playersObj = GameObject.FindGameObjectsWithTag("Player");
        int index = 0;

        foreach (GameObject playerObj in playersObj)
        {
            if (players.ContainsValue(playerObj.GetComponent<PlayerBox>().GetPlayer()))
            {
                playersReady.Add(index, playerObj.GetComponent<PlayerBox>()._isReady);
                index += 1;
            }
        }

        RemovePlayers();
        PlayerIndex = 0;

        var keysPlayers = players.Keys.ToList();
        keysPlayers.Sort();
        var keysReady = playersReady.Keys.ToList();
        keysReady.Sort();

        index = 0;
        for (index = 0; index < PhotonNetwork.CurrentRoom.Players.Count; index++)
        {
            bool isReady = false;
            if (playersReady.Count > 0 && playersReady.ContainsKey(index))
                isReady = playersReady[index];
            AddNewPlayerBox(PhotonNetwork.CurrentRoom.Players[keysPlayers[index]], index, isReady);
        }
    }

    private void AddNewPlayerBox(Player player, int index, bool isReady)
    {
        object[] data = new object[3];
        data[0] = PlayersPosition[index].position;
        data[1] = player;
        data[2] = isReady;
        string prefab = "Player" + (index + 1);
        GameObject playerObj = PhotonNetwork.Instantiate(prefab, Vector3.zero, Quaternion.identity, 0, data);

        Hashtable hashPlayer = new Hashtable();
        hashPlayer["Color"] = PlayerColors[PlayerIndex];
        hashPlayer["CurrentId"] = 0;
        player.SetCustomProperties(hashPlayer);

        Hashtable hash = new Hashtable();
        hash[player.UserId] = PlayerColors[PlayerIndex];
        PlayerIndex += 1;
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        players.Add(playerObj);
        object[] content = new object[2];
        content[0] = player.UserId;
        content[1] = isReady;
        StartCoroutine("WaitForChangePlayerStatus", content);
    }

    System.Collections.IEnumerator WaitForChangePlayerStatus(object[] content)
    {
        yield return new WaitForSeconds(1f);
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ChangePlayerStatus", RpcTarget.All, (string)content[0], (bool)content[1]);
    }

    private void RemovePlayers()
    {
        foreach (GameObject player in players)
            PhotonNetwork.Destroy(player);
        players.Clear();
    }

    private void ChangeReadyButton(bool isRdy)
    {
        if (isRdy)
        {
            readyText.SetActive(false);
            readyCheck.SetActive(true);
        } else
        {
            readyText.SetActive(true);
            readyCheck.SetActive(false);
            Hashtable RoomCustomProps = new Hashtable();
            RoomCustomProps[CountdownStop] = (float)PhotonNetwork.Time;
            PhotonNetwork.CurrentRoom.SetCustomProperties(RoomCustomProps);

            PhotonNetwork.RaiseEvent(EventRoomLoading, false, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
        }
    }

    private void PlayersReady()
    {
        int nbReady = 0;
        foreach (GameObject player in players)
            if (player.GetComponent<PlayerBox>()._isReady)
                nbReady += 1;

        if (!timerHasStarted && players.Count > 1 && nbReady == players.Count)
        {
            Hashtable RoomCustomProps = new Hashtable();
            RoomCustomProps[CountdownStartTime] = (float)PhotonNetwork.Time;
            PhotonNetwork.CurrentRoom.SetCustomProperties(RoomCustomProps);

            PhotonNetwork.RaiseEvent(EventRoomLoading, true, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
        }
    }

    #endregion

    #region Photon

    [PunRPC]
    public void ChangePlayerStatus(string userId, bool newStatus)
    {
        GameObject[] PlayerBox = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in PlayerBox)
        {
            if (player.GetComponent<PlayerBox>().GetPlayer().UserId == userId)
            {
                player.GetComponent<PlayerBox>().setStatus(newStatus);
                break;
            }
        }
    }

    private byte EventRoomLoading = 5;

    public void OnEvent(EventData photonEvent)
    {
        byte code = photonEvent.Code;

        if (code == EventRoomLoading)
            Loading.SetActive((bool)photonEvent.CustomData);
    }

    #endregion

    #region IPunObservable Interface

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    #endregion
}