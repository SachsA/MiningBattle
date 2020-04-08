using UnityEngine;

public class SwitchSettingsPanels : MonoBehaviour
{
    [Header("Panels contained in the Settings/Graphics menu.")]
    public GameObject graphicsPanel;

    [Header("Panels contained in the Settings/Sound menu.")]
    public GameObject soundPanel;

    [Header("Panels contained in the Settings/Controls menu.")]
    public GameObject firstPageControlsPanel;
    public GameObject secondPageControlsPanel;

    #region MonoBehaviour

    private void OnEnable()
    {
        SetPanelsToInactive();
        graphicsPanel.SetActive(true);
    }

    #endregion

    #region PrivateMethods

    private void SetPanelsToInactive()
    {
        graphicsPanel.SetActive(false);
        soundPanel.SetActive(false);
        firstPageControlsPanel.SetActive(false);
        secondPageControlsPanel.SetActive(false);
    }

    #endregion

    #region PublicMethods

    public void SwitchPanel(string panelName)
    {
        SetPanelsToInactive();

        switch (panelName)
        {
            //Graphics panel
            case "GraphicsPanel":
                graphicsPanel.SetActive(true);
                break;

            //Sound panel
            case "SoundPanel":
                soundPanel.SetActive(true);
                break;

            //Controls panel
            case "FirstPageControlsPanel":
                firstPageControlsPanel.SetActive(true);
                break;
            case "SecondPageControlsPanel":
                secondPageControlsPanel.SetActive(true);
                break;
        }
    }

    #endregion
}