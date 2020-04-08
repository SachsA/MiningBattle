using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private Camera _mainCamera;
    private AudioSource _audioSpaceshipDestroy;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _audioSpaceshipDestroy = GetComponent<AudioSource>();
        Instance = this;
    }

    public void PlaySpaceshipDestructionSound(bool isMine, Vector3 position, bool isVisible)
    {
        if (isMine)
            _audioSpaceshipDestroy.Play();
        //else if (isVisible)
        //{
        //    Vector3 screenPoint = Camera.main.WorldToViewportPoint(position);
        //    if (screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
        //        _audioSpaceshipDestroy.Play();
        //}
    }
}
