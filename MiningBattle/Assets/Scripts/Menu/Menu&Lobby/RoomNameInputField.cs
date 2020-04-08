using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class RoomNameInputField : MonoBehaviour
{
    #region Private Constants

    const string roomNamePrefKey = "RoomName";

    #endregion

    #region Public Methods

    public void SetRoomName(Text textInput)
    {
        if (string.IsNullOrEmpty(textInput.text))
        {
            Debug.LogError("Room Name is null or empty");
            return;
        }
        PlayerPrefs.SetString(roomNamePrefKey, textInput.text);
    }

    #endregion
}