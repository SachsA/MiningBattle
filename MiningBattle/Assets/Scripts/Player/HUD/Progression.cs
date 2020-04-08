using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Progression : MonoBehaviour, IPunObservable
{
    public static Progression Instance;

    public float refreshFrequency = 1;
    public GameObject[] cursors = new GameObject[4];

    private float _lastUpdate = 0;
    private PhotonView _photonView;

    private Vector3 _startPosition = new Vector3(-250, 35, 0);
    private Vector3 _endPosition = new Vector3(250, 35, 0);
    private float _progressionLength = 0;
    private float _maxDistance = 0;

    private string _color = "";
    private float _bestDistance;

    private void Awake()
    {
        Instance = this;
        _photonView = GetComponent<PhotonView>();
        _progressionLength = Vector3.Magnitude(_startPosition - _endPosition);
        if (PhotonNetwork.IsConnected)
        {
            for (int i = PhotonNetwork.CurrentRoom.PlayerCount; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
                cursors[i].SetActive(false);
        }
        _color = (string)PhotonNetwork.LocalPlayer.CustomProperties["Color"];
        foreach (GameObject cursor in cursors)
            cursor.GetComponent<RectTransform>().localPosition = _startPosition;
    }

    private void Start()
    {
        _maxDistance = World.Instance.DistancePBtoCA - World.Instance.GetAsteroidRadius();
        _bestDistance = _maxDistance;
    }

    private void FixedUpdate()
    {
        _lastUpdate += Time.fixedDeltaTime;
        if (_lastUpdate < refreshFrequency)
            return;
        _lastUpdate = 0;

        if (PhotonNetwork.IsConnected)
        {
            float distance = SpaceshipManager.Instance.GetClosestMinerFromDaegunium();
            if (distance > 0 && distance < _maxDistance && distance < _bestDistance)
            {
                _bestDistance = distance;
                object[] data = new object[3];
                data[0] = _color;
                data[1] = distance;
                _photonView.RPC("SetDistance", RpcTarget.All, data);
            }
        }
    }

    [PunRPC]
    public void SetDistance(object[] data)
    {
        string color = (string)data[0];
        float distance = (float)data[1];

        foreach (GameObject cursor in cursors)
        {
            if (cursor.name == "Cursor_" + color)
            {
                float x = distance == 0 ? x = _startPosition.x : _endPosition.x - (distance * _progressionLength / _maxDistance);
                cursor.GetComponent<RectTransform>().localPosition = new Vector3(x, 35, 0);
                break;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
