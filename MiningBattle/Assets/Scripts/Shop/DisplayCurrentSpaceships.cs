using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCurrentSpaceships : MonoBehaviour
{
    private SpaceshipManager spaceshipManager;
    private BuySpaceships buySpaceships;

    public Text actualMiningText;
    public Text actualAttackText;
    public Text actualDefenceText;

    public Text actualSpaceshipsText;

    private void Start()
    {
        spaceshipManager = SpaceshipManager.Instance;
        buySpaceships = GetComponent<BuySpaceships>();
    }

    // Updates every frame but for UI
    private void OnGUI()
    {
        actualSpaceshipsText.text = spaceshipManager.GetMySpaceships().Count.ToString() + "/" + buySpaceships.maxSpaceships;
        actualMiningText.text = spaceshipManager.GetMyMiningSpaceships().Count.ToString();
        actualAttackText.text = spaceshipManager.GetMyAttackSpaceships().Count.ToString();
        actualDefenceText.text = spaceshipManager.GetMyDefenceSpaceships().Count.ToString();
    }
}
