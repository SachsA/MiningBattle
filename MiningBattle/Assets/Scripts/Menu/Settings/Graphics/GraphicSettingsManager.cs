using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GraphicSettingsManager : MonoBehaviour
{
    public Toggle fullscreenToggle;

    public Dropdown textureDropdown;
    public Dropdown antiAliasingDropdown;
    public Dropdown vSyncDropdown;

    //public Dropdown resolutionsDropdown;
    //public Resolution[] resolutions;

    public GraphicSettings graphicSettings;

    public static GraphicSettingsManager Instance;

    private void Awake()
    {
        if (Instance != null)
            return;

        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        graphicSettings = new GraphicSettings();

        fullscreenToggle.onValueChanged.AddListener(delegate { OnFullscreenToggle(); });
        //resolutionsDropdown.onValueChanged.AddListener(delegate { OnResolutionChange(); });
        
        textureDropdown.onValueChanged.AddListener(delegate { OnTextureChange(); });
        antiAliasingDropdown.onValueChanged.AddListener(delegate { OnAntiAliasingChange(); });
        vSyncDropdown.onValueChanged.AddListener(delegate { OnVSyncChange(); });

/*
        resolutions = Screen.resolutions;

        foreach (Resolution resolution in resolutions)
            resolutionsDropdown.options.Add(new Dropdown.OptionData(resolution.ToString()));
*/

        LoadSettings();
    }

    public void SaveSettings()
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Settings")))
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Settings"));

        graphicSettings.AntiAliasing = antiAliasingDropdown.value;
        graphicSettings.FullScreen = Screen.fullScreen = fullscreenToggle.isOn;
        graphicSettings.Texture = textureDropdown.value;
        //graphicSettings.ResolutionIndex = resolutionsDropdown.value;
        graphicSettings.VSync = vSyncDropdown.value;

        var path = Path.Combine(Application.persistentDataPath, "Settings", "GraphicSettings.json");
        File.WriteAllText(path, JsonUtility.ToJson(graphicSettings, true));
    }

    private void LoadSettings()
    {
        var path = Path.Combine(Application.persistentDataPath, "Settings", "GraphicSettings.json");

        if (File.Exists(path))
        {
            graphicSettings = JsonUtility.FromJson<GraphicSettings>(File.ReadAllText(path));

            antiAliasingDropdown.value = graphicSettings.AntiAliasing;
            fullscreenToggle.isOn = graphicSettings.FullScreen;
            textureDropdown.value = graphicSettings.Texture;
            //resolutionsDropdown.value = graphicSettings.ResolutionIndex;
            vSyncDropdown.value = graphicSettings.VSync;

            //resolutionsDropdown.RefreshShownValue();
        }
    }

    private void OnFullscreenToggle()
    {
        graphicSettings.FullScreen = fullscreenToggle.isOn;
        Screen.fullScreen = graphicSettings.FullScreen;
    }

/*
    public void OnResolutionChange()
    {
        var value = resolutionsDropdown.value;

        Screen.SetResolution(resolutions[value].width, resolutions[value].height, Screen.fullScreen);
        graphicSettings.ResolutionIndex = resolutionsDropdown.value;
    }
*/

    private void OnTextureChange()
    {
        graphicSettings.Texture = textureDropdown.value;
        QualitySettings.masterTextureLimit = graphicSettings.Texture;
    }

    private void OnAntiAliasingChange()
    {
        graphicSettings.AntiAliasing = antiAliasingDropdown.value;
        QualitySettings.antiAliasing = (int) Mathf.Pow(2f, graphicSettings.AntiAliasing);
    }

    private void OnVSyncChange()
    {
        graphicSettings.VSync = vSyncDropdown.value;
        QualitySettings.vSyncCount = graphicSettings.VSync;
    }
}