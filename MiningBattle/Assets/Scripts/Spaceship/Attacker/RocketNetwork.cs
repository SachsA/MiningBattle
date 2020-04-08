using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class RocketNetwork : MonoBehaviour, IPunInstantiateMagicCallback
{
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Hashtable hashPlayer = info.Sender.CustomProperties;
        int currentId = (int)hashPlayer["CurrentId"];

        NetworkIdentity identity = GetComponent<NetworkIdentity>();
        identity._id = info.Sender.UserId + "_" + currentId;

        if (!info.Sender.IsLocal)
            gameObject.tag = "EnemyRocket";
        else
        {
            Hashtable hash = new Hashtable();
            hash["CurrentId"] = currentId + 1;
            info.Sender.SetCustomProperties(hash);
        }
    }
}
