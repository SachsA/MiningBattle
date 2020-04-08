using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class PauseMenu : MonoBehaviour
{
    #region PrivateVariables

    private const string MenuScene = "Menu";

    [SerializeField] private GameObject pauseMenuUi;
    
    [SerializeField] private GameObject menuSettings;
    private bool _settingsIsDisplayed;
    
    [SerializeField] private GameObject menuTutorial;
    private bool _tutorialIsDisplayed;

    #endregion

    #region PublicVariables

    [HideInInspector] public bool isFurtherInTutorialGameplay;
    [HideInInspector] public bool isFurtherInTutorialControls;

    public static PauseMenu Instance;
    
    public static bool GameIsPaused;

    #endregion

    #region PrivateMethods

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pauseMenuUi.SetActive(false);
        menuSettings.SetActive(false);
        menuTutorial.SetActive(false);
    }

    private void Update()
    {
        if (GameIsPaused && _settingsIsDisplayed && Input.GetKeyDown(KeyCode.Escape))
            CloseSettings();
        else if (GameIsPaused && _tutorialIsDisplayed && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isFurtherInTutorialGameplay)
            {
                menuTutorial.SetActive(false);
                menuTutorial.SetActive(true);
            }
            else if (isFurtherInTutorialControls)
            {
                menuTutorial.SetActive(false);
                menuTutorial.SetActive(true);
                menuTutorial.GetComponent<SwitchTutorialPanelsInPauseMenu>().summaryControlsPanel.SetActive(true);
            }
            else
            {
                CloseTutorial();
            }
        }
        else if (Input.GetKeyDown(PlayerInputsManager.Instance.PauseKey))
            DisplayPauseMenu();
    }

    private void Pause()
    {
        GameIsPaused = true;

        pauseMenuUi.SetActive(true);
    }

    #endregion

    #region PublicMethods

    public void OpenSettings()
    {
        _settingsIsDisplayed = true;

        menuSettings.SetActive(true);
    }

    public void CloseSettings()
    {
        GraphicSettingsManager.Instance.SaveSettings();
        SoundSettingsManager.Instance.SaveSettings();

        _settingsIsDisplayed = false;
        menuSettings.SetActive(false);
    }

    public void OpenTutorial()
    {
        _tutorialIsDisplayed = true;

        menuTutorial.SetActive(true);
    }

    public void CloseTutorial()
    {
        _tutorialIsDisplayed = false;

        menuTutorial.SetActive(false);
    }
    
    public void DisplayPauseMenu()
    {
        if (GameIsPaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        GameIsPaused = false;

        pauseMenuUi.SetActive(false);
    }

    public void ReturnMenu()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;
        PlayerInventory.actualMoney = 0;
        GameIsPaused = false;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene(MenuScene);
        }
    }

    public void QuitGame()
    {
        PlayerInventory.actualMoney = 0;
        GameIsPaused = false;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        else
        {
            Application.Quit();
        }
    }

    #endregion
}