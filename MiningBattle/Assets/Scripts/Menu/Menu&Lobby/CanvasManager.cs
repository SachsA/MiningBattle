using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviourPunCallbacks
{
    #region PrivateVariables

    [SerializeField] private GameObject menu;

    [SerializeField] private GameObject lobby;

    [SerializeField] private GameObject menuSettings;

    [SerializeField] private GameObject menuTutorial;

    [SerializeField] private Text playerName;

    private bool _settingsIsDisplayed;
    private bool _tutorialIsDisplayed;

    #endregion

    #region PublicVariables

    [HideInInspector] public bool isFurtherInTutorialGameplay;
    [HideInInspector] public bool isFurtherInTutorialControls;

    public LobbyFunction lobbyFunction;

    public static CanvasManager Instance;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        menu.SetActive(true);
        lobby.SetActive(false);
        menuSettings.SetActive(false);
        menuTutorial.SetActive(false);
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    private void Update()
    {
        if (_settingsIsDisplayed && Input.GetKeyDown(KeyCode.Escape))
            CloseSettings();
        else if (_tutorialIsDisplayed && !isFurtherInTutorialGameplay && !isFurtherInTutorialControls && Input.GetKeyDown(KeyCode.Escape))
            CloseTutorial();
        else if (_tutorialIsDisplayed && isFurtherInTutorialGameplay && Input.GetKeyDown(KeyCode.Escape))
        {
            menuTutorial.SetActive(false);
            menuTutorial.SetActive(true);
        }
        else if (_tutorialIsDisplayed && isFurtherInTutorialControls && Input.GetKeyDown(KeyCode.Escape))
        {
            menuTutorial.SetActive(false);
            menuTutorial.SetActive(true);
            menuTutorial.GetComponent<SwitchTutorialPanels>().summaryControlsPanel.SetActive(true);
        }
    }

    #endregion

    #region PublicMethods

    public void MenuOrLobby(string type)
    {
        if (type == "Lobby" && (string.IsNullOrEmpty(playerName.text) || playerName.text[0] == ' '))
        {
            StartCoroutine(nameof(ShowMessage), string.IsNullOrEmpty(playerName.text));
            return;
        }

        if (playerName)
            PlayerPrefs.SetString("PlayerName", playerName.text);
        if (type == "Lobby")
        {
            menu.SetActive(false);
            lobby.SetActive(true);
            LobbyGameNetwork.Instance.StartConnection();
        }
        else
        {
            PhotonNetwork.Disconnect();
            menu.SetActive(true);
            lobby.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

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

    #endregion

    #region ErrorMessage

    [SerializeField] private Text error;

    IEnumerator ShowMessage(bool isEmpty)
    {
        error.text = !isEmpty ? "Really... A blank name ?" : "Give me your username!";
        yield return new WaitForSeconds(3f);
        error.text = "";
    }

    #endregion
}