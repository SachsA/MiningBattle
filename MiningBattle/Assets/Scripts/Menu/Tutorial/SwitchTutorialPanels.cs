using UnityEngine;

public class SwitchTutorialPanels : MonoBehaviour
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
        CanvasManager.Instance.isFurtherInTutorialGameplay = false;
        CanvasManager.Instance.isFurtherInTutorialControls = false;
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
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                introductionPanel.SetActive(true);
                break;
            case "GoalPanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                goalPanel.SetActive(true);
                break;
            case "MapPanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                mapPanel.SetActive(true);
                break;
            case "BasePanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                basePanel.SetActive(true);
                break;
            case "AsteroidPanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                asteroidPanel.SetActive(true);
                break;
            case "FOGPanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                fogPanel.SetActive(true);
                break;
            case "EconomyPanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                economyPanel.SetActive(true);
                break;
            case "MiningModePanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                miningModePanel.SetActive(true);
                break;
            case "SelectSpaceshipModePanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                selectSpaceshipModePanel.SetActive(true);
                break;
            case "MiningSpaceshipPanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                miningSpaceshipPanel.SetActive(true);
                break;
            case "AttackSpaceshipPanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
                attackSpaceshipPanel.SetActive(true);
                break;
            case "DefenceSpaceshipPanel":
                CanvasManager.Instance.isFurtherInTutorialGameplay = true;
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
                CanvasManager.Instance.isFurtherInTutorialControls = true;
                cameraControlsPanel.SetActive(true);
                break;
            case "ShopControlsPanel":
                CanvasManager.Instance.isFurtherInTutorialControls = true;
                shopControlsPanel.SetActive(true);
                break;
            case "SelectionModeControlsPanel":
                CanvasManager.Instance.isFurtherInTutorialControls = true;
                selectionModeControlsPanel.SetActive(true);
                break;
            case "UIControlsPanel":
                CanvasManager.Instance.isFurtherInTutorialControls = true;
                uIControlsPanel.SetActive(true);
                break;
            case "SpaceshipsControlsPanel":
                CanvasManager.Instance.isFurtherInTutorialControls = true;
                spaceshipsControlsPanel.SetActive(true);
                break;
        }
    }

    #endregion
}