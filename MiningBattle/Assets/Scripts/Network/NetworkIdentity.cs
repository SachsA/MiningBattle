using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkIdentity : MonoBehaviour
{
    public string _id = "";
    public bool _isLocal = false;

    public bool IsVisible = false;
    public int SeenBy = 0;

    public int Life = 0;
}
