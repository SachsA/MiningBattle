using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChangeKeyScript : MonoBehaviour
{
    #region PrivateVariables

    private bool _waitingForKey;
    
    private Text _buttonText;

    private Event _keyEvent;

    private KeyCode _key;

    #endregion

    #region PublicVariables

    public Text cameraForwardKeyText;
    public Text cameraBackwardKeyText;
    public Text cameraLeftKeyText;
    public Text cameraRightKeyText;
    public Text cameraToBaseKeyText;

    public Text shopKeyText;
    public Text buyMinerKeyText;
    public Text buyAttackerKeyText;
    public Text buyDefenceKeyText;

    public Text selectionModeKeyText;
    
    public Text automationKeyText;
    public Text destroySpaceshipKeyText;
    
    public Text attackerSelectionKeyText;
    public Text defenceSelectionKeyText;
    public Text minerSelectionKeyText;
    
    public Text pauseKeyText;

    #endregion
    
    private void OnEnable()
    {
        _waitingForKey = false;
        
        cameraForwardKeyText.text = PlayerInputsManager.Instance.CameraForwardKey.ToString();
        cameraBackwardKeyText.text = PlayerInputsManager.Instance.CameraBackwardKey.ToString();
        cameraLeftKeyText.text = PlayerInputsManager.Instance.CameraLeftKey.ToString();
        cameraRightKeyText.text = PlayerInputsManager.Instance.CameraRightKey.ToString();
        cameraToBaseKeyText.text = PlayerInputsManager.Instance.CameraToBaseKey.ToString();
        
        shopKeyText.text = PlayerInputsManager.Instance.ShopKey.ToString();
        buyMinerKeyText.text = PlayerInputsManager.Instance.BuyMinerKey.ToString();
        buyAttackerKeyText.text = PlayerInputsManager.Instance.BuyAttackerKey.ToString();
        buyDefenceKeyText.text = PlayerInputsManager.Instance.BuyDefenceKey.ToString();

        selectionModeKeyText.text = PlayerInputsManager.Instance.SelectionModeKey.ToString();
        
        automationKeyText.text = PlayerInputsManager.Instance.AutomationKey.ToString();
        destroySpaceshipKeyText.text = PlayerInputsManager.Instance.DestroySpaceshipKey.ToString();
        
        minerSelectionKeyText.text = PlayerInputsManager.Instance.MinerSelectionKey.ToString();
        attackerSelectionKeyText.text = PlayerInputsManager.Instance.AttackerSelectionKey.ToString();
        defenceSelectionKeyText.text = PlayerInputsManager.Instance.DefenceSelectionKey.ToString();

        pauseKeyText.text = PlayerInputsManager.Instance.PauseKey.ToString();
    }

    private void OnGUI()
    {
        _keyEvent = Event.current;

        if (_keyEvent.isKey && _waitingForKey)
        {
            _key = _keyEvent.keyCode;
            _waitingForKey = false;
        }
    }

    public void StartAssignment(string keyName)
    {
        if (!_waitingForKey)
            StartCoroutine(AssignKey(keyName));
    }

    public void SendText(Text text)
    {
        _buttonText = text;
    }

    private IEnumerator WaitForKey()
    {
        while (!_keyEvent.isKey)
        {
            yield return null;
        }
    }

    private bool CheckValidity()
    {
        bool check = true;

        if (_key == PlayerInputsManager.Instance.CameraForwardKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.CameraBackwardKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.CameraLeftKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.CameraRightKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.CameraToBaseKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.ShopKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.BuyMinerKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.BuyAttackerKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.BuyDefenceKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.SelectionModeKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.AutomationKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.DestroySpaceshipKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.AttackerSelectionKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.DefenceSelectionKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.MinerSelectionKey)
            check = false;
        else if (_key == PlayerInputsManager.Instance.PauseKey)
            check = false;
        return check;
    }

    private IEnumerator AssignKey(string inputName)
    {
        _waitingForKey = true;

        yield return WaitForKey();

        if (!CheckValidity())
            yield break;

        switch (inputName)
        {
            case "CameraForward":
                PlayerInputsManager.Instance.CameraForwardKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.CameraForwardKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.cameraForwardKeyName, PlayerInputsManager.Instance.CameraForwardKey.ToString());
                break;
            case "CameraBackward":
                PlayerInputsManager.Instance.CameraBackwardKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.CameraBackwardKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.cameraBackwardKeyName, PlayerInputsManager.Instance.CameraBackwardKey.ToString());
                break;
            case "CameraLeft":
                PlayerInputsManager.Instance.CameraLeftKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.CameraLeftKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.cameraLeftKeyName, PlayerInputsManager.Instance.CameraLeftKey.ToString());
                break;
            case "CameraRight":
                PlayerInputsManager.Instance.CameraRightKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.CameraRightKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.cameraRightKeyName, PlayerInputsManager.Instance.CameraRightKey.ToString());
                break;
            case "CameraToBase":
                PlayerInputsManager.Instance.CameraToBaseKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.CameraToBaseKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.cameraToBaseKeyName, PlayerInputsManager.Instance.CameraToBaseKey.ToString());
                break;
            case "Shop":
                PlayerInputsManager.Instance.ShopKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.ShopKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.shopKeyName, PlayerInputsManager.Instance.ShopKey.ToString());
                break;
            case "BuyMiner":
                PlayerInputsManager.Instance.BuyMinerKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.BuyMinerKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.buyMinerKeyName, PlayerInputsManager.Instance.BuyMinerKey.ToString());
                break;
            case "BuyAttacker":
                PlayerInputsManager.Instance.BuyAttackerKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.BuyAttackerKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.buyAttackerKeyName, PlayerInputsManager.Instance.BuyAttackerKey.ToString());
                break;
            case "BuyDefence":
                PlayerInputsManager.Instance.BuyDefenceKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.BuyDefenceKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.buyDefenceKeyName, PlayerInputsManager.Instance.BuyDefenceKey.ToString());
                break;
            case "SelectionMode":
                PlayerInputsManager.Instance.SelectionModeKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.SelectionModeKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.selectionModeKeyName, PlayerInputsManager.Instance.SelectionModeKey.ToString());
                break;
            case "Automation":
                PlayerInputsManager.Instance.AutomationKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.AutomationKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.automationKeyName, PlayerInputsManager.Instance.AutomationKey.ToString());
                break;
            case "DestroySpaceship":
                PlayerInputsManager.Instance.DestroySpaceshipKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.DestroySpaceshipKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.destroySpaceshipKeyName, PlayerInputsManager.Instance.DestroySpaceshipKey.ToString());
                break;
            case "AttackerSelection":
                PlayerInputsManager.Instance.AttackerSelectionKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.AttackerSelectionKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.attackerSelectionKeyName, PlayerInputsManager.Instance.AttackerSelectionKey.ToString());
                break;
            case "DefenceSelection":
                PlayerInputsManager.Instance.DefenceSelectionKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.DefenceSelectionKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.defenceSelectionKeyName, PlayerInputsManager.Instance.DefenceSelectionKey.ToString());
                break;
            case "MinerSelection":
                PlayerInputsManager.Instance.MinerSelectionKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.MinerSelectionKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.minerSelectionKeyName, PlayerInputsManager.Instance.MinerSelectionKey.ToString());
                break;
            case "Pause":
                PlayerInputsManager.Instance.PauseKey = _key;
                _buttonText.text = PlayerInputsManager.Instance.PauseKey.ToString();
                PlayerPrefs.SetString(PlayerInputsManager.Instance.pauseKeyName, PlayerInputsManager.Instance.PauseKey.ToString());
                break;
        }
        yield return null;
    }

}