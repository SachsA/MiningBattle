using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;

public class ActualSpaceships : MonoBehaviour
{
    [Header("Available Sprites")]
    public Sprite[] AttackerSprites = new Sprite[4];
    public Sprite[] MinerSprites = new Sprite[4];
    public Sprite[] DefenderSprites = new Sprite[4];

    [Header("Images")]
    public Image AttackerImage;
    public Image MinerImage;
    public Image DefenderImage;

    private void Start()
    {
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
                AttackerImage.sprite = attacker;
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
                MinerImage.sprite = miner;
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
                DefenderImage.sprite = defender;
                break;
            }
        }
    }
}
