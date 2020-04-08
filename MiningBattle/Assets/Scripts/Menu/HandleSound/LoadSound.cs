using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class LoadSound : MonoBehaviour
{
    public AudioMixer masterMixer;
    
    private void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "Settings", "SoundSettings.json");

        if (File.Exists(path) && masterMixer)
        {
            SoundSettings settings = JsonUtility.FromJson<SoundSettings>(File.ReadAllText(path));
            
            SetVolume(settings.MasterIsOn, "MasterVolume", settings.MasterDecibel);
            SetVolume(settings.MusicIsOn, "MusicVolume", settings.MusicDecibel);
            SetVolume(settings.EffectsIsOn, "EffectsVolume", settings.EffectsDecibel);
        }
    }

    private void SetVolume(bool isOn, string volumeName, float value)
    {
        if (isOn)
            masterMixer.SetFloat(volumeName, Mathf.Log10(value) * 20);
        else
            masterMixer.SetFloat(volumeName, -80);
    }
}
