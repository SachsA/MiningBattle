using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Trailer : MonoBehaviour
{
    private bool _isFollowing = false;
    private bool _unzoom = false;

    private Transform _first;

    public int Speed = 1;

    private void Update()
    {
        if (!_isFollowing && SpaceshipManager.Instance.GetMyMiningSpaceships().Count == 1)
        {
            _isFollowing = true;
            _first = SpaceshipManager.Instance.GetMyMiningSpaceships()[0].gameObject.transform;
        }

        if (Input.GetKeyDown(KeyCode.H))
            GetComponent<CinemachineVirtualCamera>().Follow = _first;

        if (Input.GetKeyDown(KeyCode.J))
        {
            GetComponent<CinemachineVirtualCamera>().Follow = null;
            _unzoom = true;
        }
        if (_unzoom && transform.position.z > -50)
            transform.Translate(new Vector3(0, 0, Time.deltaTime * Speed));
    }
}
