using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class SpaceshipNetwork : MonoBehaviour, IPunInstantiateMagicCallback
{
    public Sprite[] AttackerSprites = new Sprite[4];

    public Sprite[] DefenderSprites = new Sprite[4];

    public Sprite[] MinerSprites = new Sprite[4];

    public Sprite LightMask;

    public Sprite[] MiniMapIcons = new Sprite[4];

    public SpriteRenderer IconRenderer;
    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Hashtable hashPlayer = info.Sender.CustomProperties;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.enabled = false;

        NetworkIdentity identity = GetComponent<NetworkIdentity>();

        string color = (string)hashPlayer["Color"];
        int currentId = (int)hashPlayer["CurrentId"];
        identity._id = info.Sender.UserId + "_" + currentId;

        SpaceshipType.Types type = GetComponent<Spaceship>().type;

        switch (type)
        {
            case SpaceshipType.Types.Attack:
                DrawAttackSpaceship(renderer, color);
                identity.Life = 1;
                break;
            case SpaceshipType.Types.Mining:
                DrawMiningSpaceship(renderer, color);
                identity.Life = 1;
                break;
            case SpaceshipType.Types.Defence:
                DrawDefenceSpaceship(renderer, color);
                identity.Life = 3;
                break;
            case SpaceshipType.Types.None:
                break;
        }
        
        foreach (Sprite icon in MiniMapIcons)
        {
            if (icon.name == "Spaceship_" + color)
            {
                IconRenderer.sprite = icon;
                break;
            }
        }
        
        if (info.Sender.IsLocal)
        {
            renderer.enabled = true;

            Hashtable hash = new Hashtable();
            hash["CurrentId"] = currentId + 1;
            info.Sender.SetCustomProperties(hash);

            gameObject.tag = "Spaceship";
            gameObject.layer = 9;

            identity._isLocal = true;
            GetComponentInChildren<SpriteMask>().sprite = LightMask;
            SpriteRenderer[] childs = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in childs)
            {
                if (child.gameObject.CompareTag("MinimapIcon"))
                {
                    child.enabled = true;
                    break;
                }
            }
        } else
        {
            switch (type)
            {
                case SpaceshipType.Types.Attack:
                    Destroy(GetComponent<Attacker>());
                    break;
                case SpaceshipType.Types.Mining:
                    Destroy(GetComponent<Miner>());
                    break;
                case SpaceshipType.Types.Defence:
                    //Destroy(GetComponent<Defender>());
                    break;
                case SpaceshipType.Types.None:
                    break;
            }
            identity._isLocal = false;
            gameObject.tag = "EnemySpaceship";
            gameObject.layer = 18;

            Transform[] childs = GetComponentsInChildren<Transform>();
            foreach (Transform child in childs)
            {
                if (child.gameObject.CompareTag("EnemySpaceship"))
                    continue;
                if (child.gameObject.CompareTag("MinimapIcon"))
                    continue;
                Destroy(child.gameObject);
            }
            Destroy(GetComponent<Spaceship>());
            Destroy(GetComponent<CharacterController2D>());
        }
    }

    private void DrawAttackSpaceship(SpriteRenderer renderer, string color)
    {
        foreach (Sprite attacker in AttackerSprites)
        {
            if (attacker.name == "Attacker_" + color)
            {
                renderer.sprite = attacker;
                break;
            }
        }
    }

    private void DrawMiningSpaceship(SpriteRenderer renderer, string color)
    {
        foreach (Sprite miner in MinerSprites)
        {
            if (miner.name == "Miner_" + color)
            {
                renderer.sprite = miner;
                break;
            }
        }
    }

    private void DrawDefenceSpaceship(SpriteRenderer renderer, string color)
    {
        foreach (Sprite defender in DefenderSprites)
        {
            if (defender.name == "Defender_" + color)
            {
                renderer.sprite = defender;
                break;
            }
        }
    }

    private AudioClip test;

    private void OnDestroy()
    {
        if (PhotonNetwork.IsConnected)
            SoundManager.Instance.PlaySpaceshipDestructionSound(PhotonView.Get(this).IsMine, transform.position, GetComponent<NetworkIdentity>().IsVisible);
    }
}
