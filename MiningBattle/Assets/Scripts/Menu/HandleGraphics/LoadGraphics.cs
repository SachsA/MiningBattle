using System.IO;
using UnityEngine;

public class LoadGraphics : MonoBehaviour
{
    private void Start()
    {
        var path = Path.Combine(Application.persistentDataPath, "Settings", "GraphicSettings.json");

        if (File.Exists(path))
        {
            var graphicSettings = JsonUtility.FromJson<GraphicSettings>(File.ReadAllText(path));
         
            Screen.fullScreen = graphicSettings.FullScreen;
            QualitySettings.masterTextureLimit = graphicSettings.Texture;
            QualitySettings.antiAliasing = (int) Mathf.Pow(2f, graphicSettings.AntiAliasing);
            QualitySettings.vSyncCount = graphicSettings.VSync;
        }
    }
}