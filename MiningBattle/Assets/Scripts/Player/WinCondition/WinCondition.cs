using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class WinCondition : MonoBehaviour
{
    public static WinCondition Instance;
    public GameObject panelPrefab;
    public Transform panelParent;
    public GameObject[] playersPanels;
    public GameObject endGameCanvas;
    public GameObject winSpeech;
    public GameObject loseSpeech;
    public Analytics myAnalytics;
    public Text gameTime;

    /* Photon */
    private PhotonView photonView;
    private byte eventEndGame = 28;
    private byte eventAnalytics = 29;

    private bool _soloWin = false;

    #region MonoBehaviour
    private void Awake()
    {
        Instance = this;

        photonView = PhotonView.Get(this);
        PhotonPeer.RegisterType(typeof(Analytics), (byte)'B', SerializedAnalytics.Serialize, SerializedAnalytics.Deserialize);
        InitMyAnalytics();
    }

    private void Update()
    {
        if (!_soloWin && PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            TriggerWin();
            _soloWin = true;
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void OnEvent(EventData photonEvent)
    {
        byte code = photonEvent.Code;

        if (code == eventEndGame)
        {
            object[] playerInfos = new object[2];

            playerInfos[0] = PhotonNetwork.LocalPlayer.NickName;
            playerInfos[1] = myAnalytics;
            endGameCanvas.SetActive(true);
            gameTime.text = Timer.Instance.TimerText.text;
            PhotonNetwork.RaiseEvent(eventAnalytics, playerInfos, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
        }
        else if (code == eventAnalytics)
        {
            object[] playerInfos = (object[])photonEvent.CustomData;
            string playerName = (string)playerInfos[0];
            Analytics playerAnalytics = (Analytics)playerInfos[1];
            UpdatePanel(playerName, playerAnalytics);
            if (panelParent.childCount == PhotonNetwork.CurrentRoom.PlayerCount)
                Time.timeScale = 0;
        }
    }
    #endregion MonoBehaviour

    private void InitMyAnalytics()
    {
        myAnalytics.hasWon = false;
        myAnalytics.attackBuilt = 0;
        myAnalytics.minerBuilt = 0;
        myAnalytics.defenceBuilt = 0;
        myAnalytics.moneyEarned = 0;
        myAnalytics.spaceshipDestroyed = 0;
        myAnalytics.spaceshipGotDestroyed = 0;
    }

    public void TriggerWin()
    {
        loseSpeech.SetActive(false);
        winSpeech.SetActive(true);
        myAnalytics.hasWon = true;
        PhotonNetwork.RaiseEvent(eventEndGame, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
    }

    private void UpdatePanel(string playerName, Analytics analytics)
    {
        GameObject panel = Instantiate(panelPrefab, panelParent) as GameObject;
        DisplayAnalytics displayAnalytics = panel.GetComponent<DisplayAnalytics>();

        if (displayAnalytics != null)
            displayAnalytics.DisplayAllAnalytics(playerName, analytics);
        else
            Debug.LogError("Error: Panel number does not contain DisplayAnalytics script or has not been created correclty.");
    }
}
