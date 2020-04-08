using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    public SelectSpaceshipManager selectSpaceshipManager;
    public MineZoneSelection mineZoneSelection;
    public Animator digButtonAnimator;

    void Update()
    {
        if (PauseMenu.GameIsPaused)
            return;

        if (Input.GetKeyDown(PlayerInputsManager.Instance.SelectionModeKey))
            SwitchBetweenSelection();
    }

    public void SwitchBetweenSelection()
    {
        if (selectSpaceshipManager.GetActive() == true)
            digButtonAnimator.SetTrigger("EnableDig");
        else
            digButtonAnimator.SetTrigger("DisableDig");
        selectSpaceshipManager.SetActive(!selectSpaceshipManager.GetActive());
        mineZoneSelection.SetActive(!mineZoneSelection.GetActive());
    }
}
