using UnityEngine;
using UnityEngine.UI;

public class LoadingCircle : MonoBehaviour
{
    #region Public Fields

    public float rotateSpeed = 350f;

    #endregion

    #region Monobehaviour Callbacks

    private void Update()
    {
        transform.Rotate(rotateSpeed * Vector3.forward * Time.deltaTime);
    }

    #endregion
}