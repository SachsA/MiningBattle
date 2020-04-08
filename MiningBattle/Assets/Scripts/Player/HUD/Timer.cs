using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public static Timer Instance;

    public Text TimerText;

    private float Chrono;
    private bool GameHasStarted;

    private void Awake()
    {
        Instance = this;
        TimerText.text = "00:00:00";
        Chrono = 0;
        GameHasStarted = false;
    }

    void Update()
    {
        if (GameHasStarted)
        {
            Chrono += Time.deltaTime;
            string hours = Mathf.Floor((Chrono % 216000) / 3600).ToString("00");
            string minutes = Mathf.Floor((Chrono % 3600) / 60).ToString("00");
            string seconds = (Chrono % 60).ToString("00");
            TimerText.text = hours + ":" + minutes + ":" + seconds;
        }
    }

    public void BeginGame()
    {
        GameHasStarted = true;
    }
}
