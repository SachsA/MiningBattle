using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;

public class DisplayQueues : MonoBehaviour
{
    [SerializeField]
    private Transform queuesParent = null;
    [SerializeField]
    private Transform destroyingQueuesParent = null;
    [SerializeField]
    private GameObject queueDisplayPrefab = null;

    [Header("Available Sprites")]
    public Sprite[] AttackerSprites = new Sprite[4];
    public Sprite[] MinerSprites = new Sprite[4];
    public Sprite[] DefenderSprites = new Sprite[4];

    [Header("Used Sprites")]
    public Sprite AttackerSprite;
    public Sprite MinerSprite;
    public Sprite DefenderSprite;

    private Text[] queuesDisplay;
    private ConstructionQueue constrQueue;
    private List<ConstructionQueue.ShopQueue> shopQueue;

    void Start()
    {
        constrQueue = GetComponent<ConstructionQueue>();
        string color = "";
        if (PhotonNetwork.IsConnected)
        {
            Hashtable hashPlayer = PhotonNetwork.LocalPlayer.CustomProperties;
            color = (string)hashPlayer["Color"];
        }
        else
            color = "Blue";
        DrawAttackSpaceship(color);
        DrawDefenceSpaceship(color);
        DrawMiningSpaceship(color);
    }

    private void DrawAttackSpaceship(string color)
    {
        foreach (Sprite attacker in AttackerSprites)
        {
            if (attacker.name == "Attacker_" + color)
            {
                AttackerSprite = attacker;
                break;
            }
        }
    }

    private void DrawMiningSpaceship(string color)
    {
        foreach (Sprite miner in MinerSprites)
        {
            if (miner.name == "Miner_" + color)
            {
                MinerSprite = miner;
                break;
            }
        }
    }

    private void DrawDefenceSpaceship(string color)
    {
        foreach (Sprite defender in DefenderSprites)
        {
            if (defender.name == "Defender_" + color)
            {
                DefenderSprite = defender;
                break;
            }
        }
    }

    #region DisplayQueues

    private void OnGUI()
    {
        int displayIndex = 0;

        shopQueue = constrQueue.GetBuildQueues();
        queuesDisplay = GetDisplayQueues();
        foreach (ConstructionQueue.ShopQueue actualQueueValue in shopQueue)
        {
            queuesDisplay[displayIndex].text = actualQueueValue.nbrInQueue.ToString();
            if (actualQueueValue.spaceshipType != SpaceshipType.Types.None)
            {
                switch (actualQueueValue.spaceshipType)
                {
                    case SpaceshipType.Types.Attack:
                        queuesDisplay[displayIndex].transform.GetChild(0).GetComponent<Image>().sprite = AttackerSprite;
                        break;
                    case SpaceshipType.Types.Mining:
                        queuesDisplay[displayIndex].transform.GetChild(0).GetComponent<Image>().sprite = MinerSprite;
                        break;
                    case SpaceshipType.Types.Defence:
                        queuesDisplay[displayIndex].transform.GetChild(0).GetComponent<Image>().sprite = DefenderSprite;
                        break;
                    case SpaceshipType.Types.None:
                        break;
                }
            }
            else
                queuesDisplay[displayIndex].transform.GetChild(0).GetComponent<Image>().sprite = null;
            displayIndex++;
        }
    }

    private Text[] GetDisplayQueues()
    {
        Text[] displays = new Text[constrQueue.queueSizeLimit];
        int index = 0;

        foreach (Transform child in queuesParent)
        {
            displays[index] = child.GetComponent<Text>();
            index++;
            if (index > constrQueue.queueSizeLimit)
                break;
        }
        return displays;
    }
    #endregion DisplayQueues

    #region GameobjectQueueManagement
    public void AddQueue()
    {
        GameObject queue;
        Vector2 posToSpawn = new Vector3(25f, -25f);

        posToSpawn.x += 125f * queuesParent.childCount;
        queue = Instantiate(queueDisplayPrefab, queuesParent);
        queue.GetComponent<BuildQueue>().queueIndex = queuesParent.childCount - 1;
        queue.GetComponent<RectTransform>().anchoredPosition = posToSpawn;
        queue.GetComponent<Animator>().SetTrigger("Appear");
    }

    public void DeleteQueue(int index, string destroyedText)
    {
        GameObject queue = queuesParent.GetChild(index).gameObject;
        BuildQueue queueDestroyed;

        queue.transform.SetParent(destroyingQueuesParent);
        queueDestroyed = queue.GetComponent<BuildQueue>();
        queueDestroyed.SetDestroyed(destroyedText);
        MoveAllQueuesLeft(index);
        SetQueuesIndex();
    }

    private void MoveAllQueuesLeft(int startIndex)
    {
        int childIndex = 0;

        foreach (Transform child in queuesParent)
        {
            if (childIndex >= startIndex)
                child.GetComponent<BuildQueue>().MoveLeft(25f + (125f * childIndex));
            childIndex++;
        }
    }

    public void AnimateBuildTimeFirstQueue(float buildTime)
    {
        BuildQueue firstQueue = queuesParent.GetChild(0).GetComponent<BuildQueue>();

        firstQueue.AnimateBuildTime(buildTime);
    }

    private void SetQueuesIndex()
    {
        int index = 0;

        foreach (Transform child in queuesParent)
        {
            child.GetComponent<BuildQueue>().queueIndex = index;
            index++;
        }
    }
    #endregion GameobjectQueueManagement
}
