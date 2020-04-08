using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Rocket : MonoBehaviour, IPunObservable
{
    public Transform Target = null;
    public float Speed;

    private byte EventCodeTouched = 69;
    private PhotonView _photonView;

    private byte EventCodeDestroy = 70;

    private void Awake()
    {
        _photonView = PhotonView.Get(this);
    }

    private void Start()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            GetComponent<AudioSource>().Play();
    }

    void Update()
    {
        if (!_photonView.IsMine)
            return;
        if (Target != null)
        {
            Vector3 dir = Target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            transform.position = Vector2.MoveTowards(transform.position, Target.position, Speed * Time.deltaTime);
        }
        else
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Destroy(this.gameObject);
            else
                Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_photonView.IsMine)
            return;
        if (collision.CompareTag("EnemySpaceship"))
        {
            if (PhotonNetwork.IsConnected)
            {
                NetworkIdentity identity = collision.GetComponent<NetworkIdentity>();
                identity.Life -= 1;
                if (identity.Life <= 0)
                    WinCondition.Instance.myAnalytics.spaceshipDestroyed += 1;
                PhotonNetwork.RaiseEvent(EventCodeTouched, identity._id, new RaiseEventOptions { TargetActors = new[] { collision.GetComponent<PhotonView>().OwnerActorNr } }, new SendOptions { Reliability = true });
                PhotonNetwork.Destroy(this.gameObject);
            }
            else
            {
                collision.GetComponent<Spaceship>().Touched();
                Destroy(this.gameObject);
            }
        }
        if (collision.CompareTag("CommandCenter") && !collision.GetComponent<PhotonView>().IsMine)
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Destroy(this.gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
