using UnityEngine;

public class SwitchTutorialPanelsInPauseMenu : MonoBehaviour
{
    [Header("Panels contained in the Tutorial/Gameplay menu.")]
    public GameObject summaryGameplayPanel;
    public GameObject introductionPanel;
    public GameObject goalPanel;
    public GameObject mapPanel;
    public GameObject basePanel;
    public GameObject asteroidPanel;
    public GameObject fogPanel;
    public GameObject economyPanel;
    public GameObject miningModePanel;
    public GameObject selectSpaceshipModePanel;
    public GameObject miningSpaceshipPanel;
    public GameObject attackSpaceshipPanel;
    public GameObject defenceSpaceshipPanel;

    [Header("Panels contained in the Tutorial/UI menu.")]
    public GameObject uiPanel;

    [Header("Panels contained in the Tutorial/Controls menu.")]
    public GameObject summaryControlsPanel;
    public GameObject cameraControlsPanel;
    public GameObject shopControlsPanel;
    public GameObject selectionModeControlsPanel;
    public GameObject uIControlsPanel;
    public GameObject spaceshipsControlsPanel;

    #region MonoBehaviour

    private void OnEnable()
    {
        SetPanelsToInactive();
        summaryGameplayPanel.SetActive(true);
    }

    #endregion

    #region PrivateMethods

    private void SetPanelsToInactive()
    {
        //Gameplay panel
        summaryGameplayPanel.SetActive(false);
        introductionPanel.SetActive(false);
        goalPanel.SetActive(false);
        mapPanel.SetActive(false);
        basePanel.SetActive(false);
        asteroidPanel.SetActive(false);
        fogPanel.SetActive(false);
        economyPanel.SetActive(false);
        miningModePanel.SetActive(false);
        selectSpaceshipModePanel.SetActive(false);
        miningModePanel.SetActive(false);
        miningSpaceshipPanel.SetActive(false);
        attackSpaceshipPanel.SetActive(false);
        defenceSpaceshipPanel.SetActive(false);

        //UI Panel
        uiPanel.SetActive(false);

        //Controls Panel
        summaryControlsPanel.SetActive(false);
        cameraControlsPanel.SetActive(false);
        shopControlsPanel.SetActive(false);
        selectionModeControlsPanel.SetActive(false);
        uIControlsPanel.SetActive(false);
        spaceshipsControlsPanel.SetActive(false);
        
        //Set boolean telling that we are not at the root of gameplay/controls to false
        PauseMenu.Instance.isFurtherInTutorialGameplay = false;
        PauseMenu.Instance.isFurtherInTutorialControls = false;
    }

    #endregion

    #region PublicMethods

    public void SwitchPanel(string panelName)
    {
        SetPanelsToInactive();

        switch (panelName)
        {
            //Gameplay panel
            case "SummaryGameplayPanel":
                summaryGameplayPanel.SetActive(true);
                break;
            case "IntroductionPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                introductionPanel.SetActive(true);
                break;
            case "GoalPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                goalPanel.SetActive(true);
                break;
            case "MapPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                mapPanel.SetActive(true);
                break;
            case "BasePanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                basePanel.SetActive(true);
                break;
            case "AsteroidPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                asteroidPanel.SetActive(true);
                break;
            case "FOGPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                fogPanel.SetActive(true);
                break;
            case "EconomyPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                economyPanel.SetActive(true);
                break;
            case "MiningModePanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                miningModePanel.SetActive(true);
                break;
            case "SelectSpaceshipModePanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                selectSpaceshipModePanel.SetActive(true);
                break;
            case "MiningSpaceshipPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                miningSpaceshipPanel.SetActive(true);
                break;
            case "AttackSpaceshipPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                attackSpaceshipPanel.SetActive(true);
                break;
            case "DefenceSpaceshipPanel":
                PauseMenu.Instance.isFurtherInTutorialGameplay = true;
                defenceSpaceshipPanel.SetActive(true);
                break;

            //UI panel
            case "UIPanel":
                uiPanel.SetActive(true);
                break;

            //Controls panel
            case "SummaryControlsPanel":
                summaryControlsPanel.SetActive(true);
                break;
            case "CameraControlsPanel":
                PauseMenu.Instance.isFurtherInTutorialControls = true;
                cameraControlsPanel.SetActive(true);
                break;
            case "ShopControlsPanel":
                PauseMenu.Instance.isFurtherInTutorialControls = true;
                shopControlsPanel.SetActive(true);
                break;
            case "SelectionModeControlsPanel":
                PauseMenu.Instance.isFurtherInTutorialControls = true;
                selectionModeControlsPanel.SetActive(true);
                break;
            case "UIControlsPanel":
                PauseMenu.Instance.isFurtherInTutorialControls = true;
                uIControlsPanel.SetActive(true);
                break;
            case "SpaceshipsControlsPanel":
                PauseMenu.Instance.isFurtherInTutorialControls = true;
                spaceshipsControlsPanel.SetActive(true);
                break;
        }
    }

    #endregion
}