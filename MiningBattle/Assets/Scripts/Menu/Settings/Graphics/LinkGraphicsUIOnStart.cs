using UnityEngine;
using UnityEngine.UI;

public class LinkGraphicsUIOnStart : MonoBehaviour
{
    private void Start()
    {
        var toggles = Resources.FindObjectsOfTypeAll<Toggle>();
        foreach (var toggle in toggles)
        {
            switch (toggle.name)
            {
                case "FullScreenToggle":
                    GraphicSettingsManager.Instance.fullscreenToggle = toggle;
                    break;
            }
        }

        var dropdowns = Resources.FindObjectsOfTypeAll<Dropdown>();
        foreach (var dropdown in dropdowns)
        {
            switch (dropdown.name)
            {
                case "TexturesDropdown":
                    GraphicSettingsManager.Instance.textureDropdown = dropdown;
                    break;
                case "AntiAliasingDropdown":
                    GraphicSettingsManager.Instance.antiAliasingDropdown = dropdown;
                    break;
                case "VSyncDropdown":
                    GraphicSettingsManager.Instance.vSyncDropdown = dropdown;
                    break;
            }
        }
    }
}
