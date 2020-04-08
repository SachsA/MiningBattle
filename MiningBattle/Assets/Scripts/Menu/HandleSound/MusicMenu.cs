using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicMenu : MonoBehaviour
{
    public static MusicMenu Instance = null;

    void Awake()
    {
        if (Instance != null)
            return;
        Instance = this;
        DontDestroyOnLoad(this);
    }
}
