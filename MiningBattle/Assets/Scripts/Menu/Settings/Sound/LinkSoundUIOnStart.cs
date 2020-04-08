using UnityEngine;
using UnityEngine.UI;

public class LinkSoundUIOnStart : MonoBehaviour
{
    private void Start()
    {
        var toggles = Resources.FindObjectsOfTypeAll<Toggle>();
        foreach (var toggle in toggles)
        {
            switch (toggle.name)
            {
                case "MasterVolumeToggle":
                    SoundSettingsManager.Instance.masterToggle = toggle;
                    break;
                case "MusicVolumeToggle":
                    SoundSettingsManager.Instance.musicToggle = toggle;
                    break;
                case "EffectsVolumeToggle":
                    SoundSettingsManager.Instance.effectsToggle = toggle;
                    break;
            }
        }

        var texts = Resources.FindObjectsOfTypeAll<Text>();
        foreach (var text in texts)
        {
            switch (text.name)
            {
                case "MasterVolumeText":
                    SoundSettingsManager.Instance.masterText = text;
                    break;
                case "MusicVolumeText":
                    SoundSettingsManager.Instance.musicText = text;
                    break;
                case "EffectsVolumeText":
                    SoundSettingsManager.Instance.effectsText = text;
                    break;
            }
        }

        var sliders = Resources.FindObjectsOfTypeAll<Slider>();
        foreach (var slider in sliders)
        {
            switch (slider.name)
            {
                case "MasterVolumeSlider":
                    SoundSettingsManager.Instance.masterSlider = slider;
                    break;
                case "MusicVolumeSlider":
                    SoundSettingsManager.Instance.musicSlider = slider;
                    break;
                case "EffectsVolumeSlider":
                    SoundSettingsManager.Instance.effectsSlider = slider;
                    break;
            }
        }
    }
}
