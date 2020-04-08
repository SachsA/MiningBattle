using UnityEngine;
using UnityEngine.EventSystems;

public class GameInformations : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Private Fields

    #region Serialized Fields

    [SerializeField] private GameObject _textInfos;

    #endregion

    #endregion

    #region Monobehaviour Callbacks

    private void Start()
    {
        _textInfos.SetActive(false);
    }

    #endregion

    #region IPointer(Enter&Exit)Handler

    public void OnPointerEnter(PointerEventData eventData)
    {
        _textInfos.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _textInfos.SetActive(false);
    }

    #endregion
}