using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerBox : MonoBehaviour, IPunObservable
{
    #region Private Fields

    private Player _player = null;

    #region SerializedFields

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private TextMesh _playerName = null;

    #endregion

    #endregion

    #region Public Fields

    public bool _isReady = false;

    #endregion

    private void Start()
    {
        _isReady = false;
        object[] data = GetComponent<PhotonView>().InstantiationData;
        transform.position = (Vector3)data[0];
        transform.localRotation = new Quaternion(0, 180f, 0, 0);
        _player = (Player)data[1];
        _isReady = (bool)data[2];
        _animator.SetBool("Ready", _isReady);
        _playerName.text = _player.NickName;
    }

    public Player GetPlayer()
    {
        return _player;
    }

    public void setStatus(bool newStatus)
    {
        _isReady = newStatus;
        _animator.SetBool("Ready", _isReady);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
