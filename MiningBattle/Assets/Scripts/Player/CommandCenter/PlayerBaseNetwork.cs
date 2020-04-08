using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class PlayerBaseNetwork : MonoBehaviour, IPunInstantiateMagicCallback
{
    public Sprite[] CommandCenterVisuals = new Sprite[4];
    
    public Sprite[] MiniMapIcons = new Sprite[4];

    public SpriteRenderer MiniMapIcon;
    public SpriteRenderer SecurityRenderer;
    
    public Sprite SecurityVisual;

    public Vector3 StoragePosition;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Vector3 basePosition = this.transform.position;
            Vector3 normal = Vector3.Normalize(Vector3.zero - basePosition);
            StoragePosition = basePosition + (10 * normal);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Hashtable hashPlayer = info.Sender.CustomProperties;

        NetworkIdentity identity = GetComponent<NetworkIdentity>();

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.enabled = false;
        SecurityRenderer.enabled = false;

        string color = (string)hashPlayer["Color"];
        int currentId = (int)hashPlayer["CurrentId"];
        identity._id = info.Sender.UserId + "_" + currentId;

        foreach (Sprite visual in CommandCenterVisuals)
        {
            if (visual.name == "CommandCenter_" + color)
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = visual;
                break;
            }
        }

        foreach (Sprite visual in MiniMapIcons)
        {
            if (visual.name == "CommandCenter_" + color)
            {
                MiniMapIcon.sprite = visual;
                break;
            }
        }

        if (info.Sender.IsLocal)
        {
            identity._isLocal = true;
            renderer.enabled = true;
            MiniMapIcon.enabled = true;
            SecurityRenderer.enabled = true;

            Hashtable hash = new Hashtable();
            hash["CurrentId"] = currentId + 1;
            info.Sender.SetCustomProperties(hash);

            GameObject mainCamera = GameObject.Find("Main Camera");
            mainCamera.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -15);
            mainCamera.GetComponent<CameraControl>().SetPlayerBase(this.gameObject);

            SpaceshipManager manager = GameObject.Find("SpaceshipsManager").GetComponent<SpaceshipManager>();
            manager.SetPlayerBase(this.gameObject);

            Vector3 basePosition = this.transform.position;
            Vector3 normal = Vector3.Normalize(Vector3.zero - basePosition);
            StoragePosition = basePosition + (10 * normal);
        } else
        {
            identity._isLocal = false;

            Transform[] childs = GetComponentsInChildren<Transform>();
            foreach (Transform child in childs)
            {
                if (child.gameObject.CompareTag("CommandCenter"))
                {
                    Destroy(child.GetComponent<SafeZone>());
                    continue;
                }
                if (child.gameObject.name == "MinimapIcon" || child.gameObject.name == "SecurityZone")
                    continue;
                Destroy(child.gameObject);
            }
        }
    }
}
