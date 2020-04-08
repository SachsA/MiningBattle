using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMoney : MonoBehaviour
{

    private Text moneyText;

    // Start is called before the first frame update
    void Start()
    {
        moneyText = GetComponent<Text>();
    }

    void OnGUI()
    {
        moneyText.text = PlayerInventory.GetActualMoney().ToString() + "$";
    }
}
