using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayAnalytics : MonoBehaviour
{
    public Image panelImage;
    public Sprite winSprite;
    public Text playerName;
    public Text minerBuilt;
    public Text attackBuilt;
    public Text defenceBuilt;
    public Text moneyEarned;
    public Text spaceshipDestroyed;
    public Text spaceshipGotDestroyed;

    public void DisplayAllAnalytics(string name, Analytics analytics)
    {
        if (CheckVariables())
        {
            playerName.text = name;
            if (analytics.hasWon)
                panelImage.sprite = winSprite;
            minerBuilt.text = analytics.minerBuilt.ToString();
            attackBuilt.text = analytics.attackBuilt.ToString();
            defenceBuilt.text = analytics.defenceBuilt.ToString();
            moneyEarned.text = analytics.moneyEarned.ToString();
            spaceshipDestroyed.text = analytics.spaceshipDestroyed.ToString();
            spaceshipGotDestroyed.text = analytics.spaceshipGotDestroyed.ToString();
}
    }

    private bool CheckVariables()
    {
        if (playerName == null || minerBuilt == null || attackBuilt == null || defenceBuilt == null ||
            moneyEarned == null || spaceshipDestroyed == null || spaceshipGotDestroyed == null)
            return false;
        return true;
    }
}
