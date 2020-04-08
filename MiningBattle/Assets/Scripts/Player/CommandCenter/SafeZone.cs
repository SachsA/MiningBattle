using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SafeZone : MonoBehaviour, IPunObservable
{
    private byte EventCodeTouched = 69;

    private PhotonView _photonView;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    private void Awake()
    {
        _photonView = PhotonView.Get(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemySpaceship"))
        {
            if (PhotonNetwork.IsConnected)
            {
                int nb = 0;
                NetworkIdentity identity = collision.GetComponent<NetworkIdentity>();
                for (int i = 0; i < identity.Life; i++, nb += 1)
                    PhotonNetwork.RaiseEvent(EventCodeTouched, identity._id, new RaiseEventOptions { TargetActors = new[] { collision.GetComponent<PhotonView>().OwnerActorNr } }, new SendOptions { Reliability = true });
                identity.Life -= nb;
                if (identity.Life <= 0)
                    WinCondition.Instance.myAnalytics.spaceshipDestroyed += 1;
            }
        }
    }
}
