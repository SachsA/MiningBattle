using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundSettingsManager : MonoBehaviour
{
    public AudioMixer masterMixer;

    public Toggle masterToggle;
    public Toggle musicToggle;
    public Toggle effectsToggle;

    public Text masterText;
    public Text musicText;
    public Text effectsText;

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider effectsSlider;

    public SoundSettings soundSettings;

    public static SoundSettingsManager Instance;

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
        soundSettings = new SoundSettings();

        masterSlider.onValueChanged.AddListener(delegate { OnVolumeMasterChange(); });
        musicSlider.onValueChanged.AddListener(delegate { OnVolumeMusicChange(); });
        effectsSlider.onValueChanged.AddListener(delegate { OnVolumeEffectsChange(); });

        masterToggle.onValueChanged.AddListener(delegate { OnMasterVolumeToggle(); });
        musicToggle.onValueChanged.AddListener(delegate { OnMusicVolumeToggle(); });
        effectsToggle.onValueChanged.AddListener(delegate { OnEffectsVolumeToggle(); });

        LoadSettings();
    }

    public void SaveSettings()
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Settings")))
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Settings"));

        soundSettings.MasterDecibel = masterSlider.value;
        soundSettings.MusicDecibel = musicSlider.value;
        soundSettings.EffectsDecibel = effectsSlider.value;

        soundSettings.MasterIsOn = masterToggle.isOn;
        soundSettings.MusicIsOn = musicToggle.isOn;
        soundSettings.EffectsIsOn = effectsToggle.isOn;

        var path = Path.Combine(Application.persistentDataPath, "Settings", "SoundSettings.json");
        File.WriteAllText(path, JsonUtility.ToJson(soundSettings, true));
    }

    private void LoadSettings()
    {
        string path = Path.Combine(Application.persistentDataPath, "Settings", "SoundSettings.json");

        if (File.Exists(path))
        {
            soundSettings = JsonUtility.FromJson<SoundSettings>(File.ReadAllText(path));

            masterToggle.isOn = soundSettings.MasterIsOn;
            masterSlider.value = soundSettings.MasterDecibel;

            musicToggle.isOn = soundSettings.MusicIsOn;
            musicSlider.value = soundSettings.MusicDecibel;

            effectsToggle.isOn = soundSettings.EffectsIsOn;
            effectsSlider.value = soundSettings.EffectsDecibel;
        }
    }

    private void SetVolumeToggle(bool isOn, string volumeName, Slider slider)
    {
        if (isOn)
        {
            masterMixer.SetFloat(volumeName, Mathf.Log10(slider.value) * 20);
            slider.interactable = true;
        }
        else
        {
            masterMixer.SetFloat(volumeName, -80);
            slider.interactable = false;
        }
    }

    private void OnMasterVolumeToggle()
    {
        var isOn = masterToggle.isOn;
        
        soundSettings.MasterIsOn = isOn;
        SetVolumeToggle(isOn, "MasterVolume", masterSlider);
    }

    private void OnMusicVolumeToggle()
    {
        var isOn = musicToggle.isOn;
        
        soundSettings.MusicIsOn = isOn;
        SetVolumeToggle(isOn, "MusicVolume", musicSlider);
    }

    private void OnEffectsVolumeToggle()
    {
        var isOn = effectsToggle.isOn;
        
        soundSettings.EffectsIsOn = isOn;
        SetVolumeToggle(isOn, "EffectsVolume", effectsSlider);
    }

    public void OnVolumeMasterChange()
    {
        soundSettings.MasterDecibel = masterSlider.value;
        var value = Mathf.Log10(soundSettings.MasterDecibel) * 20;

        masterText.text = "Master Volume:   " + (masterSlider.value * 100).ToString("F0") + " %";
        masterMixer.SetFloat("MasterVolume", value);
    }

    public void OnVolumeMusicChange()
    {
        soundSettings.MusicDecibel = musicSlider.value;
        var value = Mathf.Log10(soundSettings.MusicDecibel) * 20;

        musicText.text = "Music Volume:     " + (musicSlider.value * 100).ToString("F0") + " %";
        masterMixer.SetFloat("MusicVolume", value);
    }

    public void OnVolumeEffectsChange()
    {
        soundSettings.EffectsDecibel = effectsSlider.value;
        var value = Mathf.Log10(soundSettings.EffectsDecibel) * 20;

        effectsText.text = "Effects Volume:   " + (effectsSlider.value * 100).ToString("F0") + " %";
        masterMixer.SetFloat("EffectsVolume", value);
    }
}