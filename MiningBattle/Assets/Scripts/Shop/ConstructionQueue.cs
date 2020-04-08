using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionQueue : MonoBehaviour
{
    public float buildTime;
    public int queueSizeLimit;
    public SpaceshipManager spaceshipManager;
    public BuySpaceships buySpaceships;

    public static ConstructionQueue Instance;

    [HideInInspector] // Is meant to used by other classes but not set up in inspector
    public List<ShopQueue> buildQueues;

    private bool isBuildCoroutineRunning;
    private DisplayQueues displayQueues;

    public struct ShopQueue
    {
        public SpaceshipType.Types spaceshipType;
        public int nbrInQueue;
    };


    private void Start()
    {
        Instance = this;
        isBuildCoroutineRunning = false;
        displayQueues = GetComponent<DisplayQueues>();
        buildQueues = new List<ShopQueue>();
    }

    #region ListManagement
    public List<ShopQueue> GetBuildQueues()
    {
        return buildQueues;
    }

    public void AddToQueue(SpaceshipType.Types spaceshipType, int numberToAdd)
    {
        int lastQueue = GetLastQueue(spaceshipType);
        ShopQueue newQueue;

        if (lastQueue == -1)
        {
            // Add queue in list if last queue is not of same type
            newQueue.nbrInQueue = numberToAdd;
            newQueue.spaceshipType = spaceshipType;
            buildQueues.Insert(buildQueues.Count, newQueue);
            displayQueues.AddQueue();
            // Starts building spaceships with buildTime if not already
            if (!isBuildCoroutineRunning)
                StartCoroutine(BuildCoroutine());
        }
        else if (lastQueue != -1)
        {
            // Add to last queue if of same type
            newQueue = buildQueues[lastQueue];
            newQueue.nbrInQueue += numberToAdd;
            buildQueues[lastQueue] = newQueue;
        }
    }

    public void RemoveFromQueue(SpaceshipType.Types spaceshipType, int numberToRemove)
    {
        ShopQueue newQueue;
        int index = 0;

        foreach (ShopQueue actualQueueValue in buildQueues)
        {
            if (actualQueueValue.spaceshipType == spaceshipType)
            {
                // If value WILL be 0, remove queue from list
                if (actualQueueValue.nbrInQueue - numberToRemove <= 0)
                {
                    buildQueues.Remove(actualQueueValue);
                    displayQueues.DeleteQueue(0, "0");
                }
                else
                {
                    // If value will NOT be 0, substract value from queue
                    newQueue = actualQueueValue;
                    newQueue.nbrInQueue -= numberToRemove;
                    buildQueues[index] = newQueue;
                }
                break;
            }
            index++;
        }
    }

    public void RemoveQueueAtIndex(int queueIndex)
    {
        if (buildQueues[queueIndex].spaceshipType == SpaceshipType.Types.Attack)
            PlayerInventory.EarnMoney(buildQueues[queueIndex].nbrInQueue * buySpaceships.attackPrice);
        else if (buildQueues[queueIndex].spaceshipType == SpaceshipType.Types.Mining)
            PlayerInventory.EarnMoney(buildQueues[queueIndex].nbrInQueue * buySpaceships.minerPrice);
        else if (buildQueues[queueIndex].spaceshipType == SpaceshipType.Types.Defence)
            PlayerInventory.EarnMoney(buildQueues[queueIndex].nbrInQueue * buySpaceships.defencePrice);
        buildQueues.RemoveAt(queueIndex);
        displayQueues.DeleteQueue(queueIndex, "Cancel");
        StopAllCoroutines();
        StartCoroutine(BuildCoroutine());
    }

    public void DeleteSpaceshipFromQueueAtIndex(int queueIndex)
    {
        if (buildQueues[queueIndex].spaceshipType == SpaceshipType.Types.Attack)
            PlayerInventory.EarnMoney(buySpaceships.attackPrice);
        else if (buildQueues[queueIndex].spaceshipType == SpaceshipType.Types.Mining)
            PlayerInventory.EarnMoney(buySpaceships.minerPrice);
        else if (buildQueues[queueIndex].spaceshipType == SpaceshipType.Types.Defence)
            PlayerInventory.EarnMoney(buySpaceships.defencePrice);
        RemoveFromQueue(buildQueues[queueIndex].spaceshipType, 1);
        StopAllCoroutines();
        StartCoroutine(BuildCoroutine());
    }

    private int GetLastQueue(SpaceshipType.Types spaceshipType)
    {
        int lastIndex = buildQueues.Count;

        if (lastIndex == 0 || buildQueues[lastIndex - 1].spaceshipType != spaceshipType)
            return -1;
        else
            return lastIndex - 1;
    }
    #endregion ListManagement

    public bool CanBuySpaceship(SpaceshipType.Types spaceshipType)
    {
        if (buildQueues.Count >= queueSizeLimit)
        {
            if (buildQueues[buildQueues.Count - 1].spaceshipType == spaceshipType)
                return true;
            return false;
        }
        return true;
    }

    private IEnumerator BuildCoroutine()
    {
        isBuildCoroutineRunning = true;
        while (buildQueues.Count > 0)
        {
            // Animate building
            displayQueues.AnimateBuildTimeFirstQueue(buildTime);
            yield return new WaitForSeconds(buildTime);
            // Instantiate spaceship and remove 1 nbr_spaceship from first queue
            spaceshipManager.AddSpaceship(buildQueues[0].spaceshipType);
            RemoveFromQueue(buildQueues[0].spaceshipType, 1);
        }
        isBuildCoroutineRunning = false;
    }
}
