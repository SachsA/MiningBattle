using System.Collections.Generic;
using UnityEngine;

public class FieldOfAttack : MonoBehaviour
{
    #region PrivateVariables

    private int _count;
    
    private List<GameObject> _enemies;
    
    private Attacker _attacker;

    #endregion

    #region PublicVariables

    public Transform spaceship;

    public float DistanceToShoot;
    
    #endregion
    
    #region PrivateMethods

    private void Awake()
    {
        _count = 0;
        _attacker = GetComponentInParent<Attacker>();
        _enemies = new List<GameObject>();
    }

    private void FixedUpdate()
    {
        if (_enemies.Count > 0)
        {
            foreach (var enemy in _enemies)
            {
                if (enemy == null)
                {
                    DeleteEnemy(enemy);
                    return;
                }
                if (Vector3.Distance(spaceship.position, enemy.transform.position) <= DistanceToShoot)
                {
                    if (!_attacker.ContainsEnemy(enemy))
                        _attacker.AddEnemy(enemy);
                }
                else if (_attacker.ContainsEnemy(enemy))
                    _attacker.DeleteEnemy(enemy);
            }
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
        _attacker.DeleteEnemy(enemy);
        _enemies.Remove(enemy);
    }

    #endregion
}