using UnityEngine;

public class PlayerInputsManager : MonoBehaviour
{
    public static PlayerInputsManager Instance;

    //Camera inputs
    [HideInInspector]
    public string cameraForwardKeyName = "CameraForward";
    public KeyCode CameraForwardKey { get; set; }
    
    [HideInInspector]
    public string cameraBackwardKeyName = "CameraBackward";
    public KeyCode CameraBackwardKey { get; set; }
    
    [HideInInspector]
    public string cameraLeftKeyName = "CameraLeft";
    public KeyCode CameraLeftKey { get; set; }
    
    [HideInInspector]
    public string cameraRightKeyName = "CameraRight";
    public KeyCode CameraRightKey { get; set; }
    
    [HideInInspector]
    public string cameraToBaseKeyName = "CameraToBase";
    public KeyCode CameraToBaseKey { get; set; }
    
    //Shop inputs
    [HideInInspector]
    public string shopKeyName = "Shop";
    public KeyCode ShopKey { get; set; }
    
    [HideInInspector]
    public string buyMinerKeyName = "BuyMiner";
    public KeyCode BuyMinerKey { get; set; }

    [HideInInspector]
    public string buyAttackerKeyName = "BuyAttacker";
    public KeyCode BuyAttackerKey { get; set; }
    
    [HideInInspector]
    public string buyDefenceKeyName = "BuyDefence";
    public KeyCode BuyDefenceKey { get; set; }
    
    //Mouse inputs
    [HideInInspector]
    public string selectionModeKeyName = "SelectionMode";
    public KeyCode SelectionModeKey { get; set; }
    
    //Spaceships inputs
    [HideInInspector]
    public string automationKeyName = "Automation";
    public KeyCode AutomationKey { get; set; }
    
    [HideInInspector]
    public string destroySpaceshipKeyName = "DestroySpaceship";
    public KeyCode DestroySpaceshipKey { get; set; }

    [HideInInspector]
    public string attackerSelectionKeyName = "AttackerSelection";
    public KeyCode AttackerSelectionKey { get; set; }
    
    [HideInInspector]
    public string defenceSelectionKeyName = "DefenceSelection";
    public KeyCode DefenceSelectionKey { get; set; }

    [HideInInspector]
    public string minerSelectionKeyName = "MinerSelection";
    public KeyCode MinerSelectionKey { get; set; }

    //UI inputs
    [HideInInspector]
    public string pauseKeyName = "Pause";
    public KeyCode PauseKey { get; set; }
    
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

        //Camera inputs
        CameraForwardKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(cameraForwardKeyName, "W"));
        CameraBackwardKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(cameraBackwardKeyName, "S"));
        CameraLeftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(cameraLeftKeyName, "A"));
        CameraRightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(cameraRightKeyName, "D"));
        CameraToBaseKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(cameraToBaseKeyName, "Space"));
        
        //Shop inputs
        ShopKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(shopKeyName, "B"));
        BuyMinerKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(buyMinerKeyName, "Alpha1"));
        BuyAttackerKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(buyAttackerKeyName, "Alpha2"));
        BuyDefenceKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(buyDefenceKeyName, "Alpha3"));

        //Mouse inputs
        SelectionModeKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(selectionModeKeyName, "G"));
        
        //Spaceships inputs
        AutomationKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(automationKeyName, "R"));
        DestroySpaceshipKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(destroySpaceshipKeyName, "Delete"));
        
        AttackerSelectionKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(attackerSelectionKeyName, "LeftShift"));
        DefenceSelectionKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(defenceSelectionKeyName, "LeftControl"));
        MinerSelectionKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(minerSelectionKeyName, "LeftAlt"));
        
        //UI inputs
        PauseKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(pauseKeyName, "Escape"));
    }
}