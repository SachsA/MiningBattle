using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTitle : MonoBehaviour
{
    #region Monobehaviour Callbacks

    void Update()
    {
        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            SceneManager.LoadScene(1);
    }

    public void LeaveGame()
    {
        Application.Quit();
    }

    #endregion
}