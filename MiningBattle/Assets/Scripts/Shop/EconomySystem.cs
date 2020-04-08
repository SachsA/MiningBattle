using Photon.Pun;
using System.Collections;
using UnityEngine;

public class EconomySystem : MonoBehaviour
{
    public Transform spaceshipManager;

    public int baseIncome; // Amount of money earned every second
    public int malusIncomePerShip;

    public int startMoney;

    public static EconomySystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            BeginGame();
    }

    public void BeginGame()
    {
        PlayerInventory.EarnMoney(startMoney); // Earn 300$ at the beginning of game
        StartCoroutine(GenerateBaseIncome()); // Earn money over time
    }

    private IEnumerator GenerateBaseIncome()
    {
        while (gameObject != null)
        {
            PlayerInventory.EarnMoney(GetBaseIncome());
            yield return new WaitForSeconds(1f);
        }
    }

    private int GetBaseIncome()
    {
        int finalIncome = baseIncome;
        int shipNumber = spaceshipManager.childCount;

        finalIncome -= Mathf.Clamp(malusIncomePerShip * shipNumber, 0, baseIncome);
        return finalIncome;
    }
}
