using System.Collections.Generic;
using UnityEngine;

public class FieldOfUpdate : MonoBehaviour
{
    public FieldOfAttack fieldOfAttack = null;

    private List<NetworkIdentity> _visibles = new List<NetworkIdentity>();
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemySpaceship"))
        {
            NetworkIdentity spaceshipIdentity = collision.gameObject.GetComponent<NetworkIdentity>();
            spaceshipIdentity.SeenBy += 1;
            _visibles.Add(spaceshipIdentity);
            if (!spaceshipIdentity.IsVisible)
            {
                SpriteRenderer[] childs = collision.gameObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer child in childs)
                    child.enabled = true;
                spaceshipIdentity.IsVisible = true;
            }

            if (fieldOfAttack != null)
                fieldOfAttack.AddEnemy(collision.gameObject);
        }
        if (collision.CompareTag("CommandCenter") && !collision.gameObject.GetComponent<NetworkIdentity>()._isLocal)
        {
            SpriteRenderer[] childs = collision.gameObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in childs)
                child.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemySpaceship"))
        {
            NetworkIdentity spaceshipIdentity = collision.gameObject.GetComponent<NetworkIdentity>();
            spaceshipIdentity.SeenBy -= 1;
            _visibles.Remove(spaceshipIdentity);
            if (spaceshipIdentity.SeenBy <= 0)
            {
                SpriteRenderer[] childs = collision.gameObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer child in childs)
                    child.enabled = false;

                spaceshipIdentity.IsVisible = false;
            }

            if (fieldOfAttack != null)
                fieldOfAttack.DeleteEnemy(collision.gameObject);
        }
    }

    private void OnDestroy()
    {
        foreach (NetworkIdentity visible in _visibles)
        {
            visible.SeenBy -= 1;
            if (visible.SeenBy <= 0)
            {
                SpriteRenderer[] childs = visible.gameObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer child in childs)
                    child.enabled = false;
                visible.IsVisible = false;
            }
        }
    }
}