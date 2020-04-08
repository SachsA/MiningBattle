using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    #region PrivateVariables

    private bool _isShooting;

    private GameObject _target;
    private List<GameObject> _enemies;

    private Spaceship _spaceship;

    private Coroutine _lastRoutine;

    private List<GameObject> _nullObj;

    #endregion

    #region PublicVariables

    public GameObject Rocket = null;

    public float fireSpeed = 1.0f;

    public bool lookAtEnemy;

    #endregion
    
    #region PrivateMethods

    private void Awake()
    {
        _spaceship = GetComponent<Spaceship>();
        _enemies = new List<GameObject>();
        _nullObj = new List<GameObject>();
    }

    private void FixedUpdate()
    {
        if (_target != null)
        {
            Vector3 dir = _target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        }

        if (_enemies.Count > 0)
        {
            var closest = ClosestEnemy();

            if (closest != null)
            {
                if (closest == _target) return;

                _target = closest;
                lookAtEnemy = true;
                Shot();
            }
            else
                lookAtEnemy = false;
        }
        else if (_isShooting)
        {
            StopCoroutine(_lastRoutine);
            _isShooting = false;
            lookAtEnemy = false;
        }
        else
        {
            lookAtEnemy = false;
            _target = null;
        }
    }

    private GameObject ClosestEnemy()
    {
        foreach (GameObject enemy in _enemies)
        {
            if (enemy == null)
                _nullObj.Add(enemy);
        }
        foreach (GameObject obj in _nullObj)
            _enemies.Remove(obj);
        _nullObj.Clear();
        _enemies.Sort((v1, v2) => ((Vector2)v1.transform.position - (Vector2)transform.position).sqrMagnitude.CompareTo(((Vector2)v2.transform.position - (Vector2)transform.position).sqrMagnitude));

        if (_enemies.Count > 0)
            return _enemies[0];
        return null;
    }

    private void Shot()
    {
        if (!_isShooting)
        {
            _lastRoutine = StartCoroutine(Fire());
            _isShooting = true;
        }
        else
        {
            StopCoroutine(_lastRoutine);
            _lastRoutine = StartCoroutine(Fire());
        }
    }

    private IEnumerator Fire()
    {
        while (_target != null)
        {
            if (PhotonNetwork.IsConnected)
            {
                GameObject currentRocket = PhotonNetwork.Instantiate("Rocket", transform.position, Quaternion.identity);
                currentRocket.GetComponent<Rocket>().Target = _target.transform;
            }
            else
            {
                GameObject currentRocket = Instantiate(Rocket, this.transform.position, Quaternion.identity);
                currentRocket.GetComponent<Rocket>().Target = _target.transform;
            }
            yield return new WaitForSeconds(fireSpeed);
        }
    }

    #endregion
    
    #region PublicMethods

    public void AddEnemy(GameObject enemy)
    {
        _enemies.Add(enemy);
    }

    public void DeleteEnemy(GameObject enemy)
    {
        if (_enemies.Contains(enemy))
            _enemies.Remove(enemy);
    }

    public bool ContainsEnemy(GameObject enemy)
    {
        return _enemies.Contains(enemy);
    }

    #endregion
}