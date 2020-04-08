using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstructionQueue;

public class BuySpaceships : MonoBehaviour
{
    public static BuySpaceships Instance;

    public DisplayShop shopMenu;

    /* Spaceships prices */
    public int attackPrice;
    public int defencePrice;
    public int minerPrice;

    public int maxSpaceships;

    private ConstructionQueue constructionQueue;

    #region MonoBehaviour

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        constructionQueue = GetComponent<ConstructionQueue>();
    }

    private void Update()
    {
        if (PauseMenu.GameIsPaused)
            return;

        if (Input.GetKeyDown(PlayerInputsManager.Instance.ShopKey))
            ShowHideShopMenu();
        if (shopMenu.isOpen)
        {
            if (Input.GetKeyDown(PlayerInputsManager.Instance.BuyMinerKey))
                BuyMiner();
            else if (Input.GetKeyDown(PlayerInputsManager.Instance.BuyAttackerKey))
                BuyAttack();
            else if (Input.GetKeyDown(PlayerInputsManager.Instance.BuyDefenceKey))
                BuyDefence();
        }
    }

    #endregion MonoBehaviour

    public void ShowHideShopMenu()
    {
        if (shopMenu.isOpen)
            shopMenu.CloseShopMenu();
        else
            shopMenu.OpenShopMenu();
    }

    #region Miner

    public void BuyMiner()
    {
        int nbMinerInQueue = 0;
        int nbSpaceshipsInQueue = 0;
        List<ShopQueue> shopQueues = constructionQueue.GetBuildQueues();
        foreach (var shopQueue in shopQueues)
        {
            if (shopQueue.spaceshipType == SpaceshipType.Types.Mining)
                nbMinerInQueue += shopQueue.nbrInQueue;
            nbSpaceshipsInQueue += shopQueue.nbrInQueue;
        }

        // If construction queue if not full (or last queue is of same type) and player has money
        if (constructionQueue.CanBuySpaceship(SpaceshipType.Types.Mining) &&
            PlayerInventory.GetActualMoney() >= minerPrice && SpaceshipManager.Instance.GetMyMiningSpaceships().Count + nbMinerInQueue < 50 &&
            SpaceshipManager.Instance.GetMySpaceships().Count + nbSpaceshipsInQueue < maxSpaceships)
        {
            shopMenu.StartAnimation("BuyMiner");
            PlayerInventory.LooseMoney(minerPrice);
            constructionQueue.AddToQueue(SpaceshipType.Types.Mining, 1);
        }
        else
            shopMenu.StartAnimation("CantBuyMiner");
    }

    #endregion Miner

    #region Attack

    public void BuyAttack()
    {
        int nbSpaceshipsInQueue = 0;
        List<ShopQueue> shopQueues = constructionQueue.GetBuildQueues();
        foreach (var shopQueue in shopQueues)
            nbSpaceshipsInQueue += shopQueue.nbrInQueue;

        // If construction queue if not full (or last queue is of same type) and player has money
        if (constructionQueue.CanBuySpaceship(SpaceshipType.Types.Attack) &&
            PlayerInventory.GetActualMoney() >= attackPrice &&
            SpaceshipManager.Instance.GetMySpaceships().Count + nbSpaceshipsInQueue < maxSpaceships)
        {
            shopMenu.StartAnimation("BuyAttack");
            PlayerInventory.LooseMoney(attackPrice);
            constructionQueue.AddToQueue(SpaceshipType.Types.Attack, 1);
        }
        else
            shopMenu.StartAnimation("CantBuyAttack");
    }

    #endregion Attack

    #region Defence

    public void BuyDefence()
    {
        int nbSpaceshipsInQueue = 0;
        List<ShopQueue> shopQueues = constructionQueue.GetBuildQueues();
        foreach (var shopQueue in shopQueues)
            nbSpaceshipsInQueue += shopQueue.nbrInQueue;

        // If construction queue if not full (or last queue is of same type) and player has money
        if (constructionQueue.CanBuySpaceship(SpaceshipType.Types.Defence) &&
            PlayerInventory.GetActualMoney() >= defencePrice &&
            SpaceshipManager.Instance.GetMySpaceships().Count + nbSpaceshipsInQueue < maxSpaceships)
        {
            shopMenu.StartAnimation("BuyDefence");
            PlayerInventory.LooseMoney(defencePrice);
            constructionQueue.AddToQueue(SpaceshipType.Types.Defence, 1);
        }
        else
            shopMenu.StartAnimation("CantBuyDefence");
    }

    #endregion Defence
}