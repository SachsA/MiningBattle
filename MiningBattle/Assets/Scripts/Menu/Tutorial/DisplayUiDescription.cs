using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayUiDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region PublicVariables

    public GameObject description;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        description.SetActive(false);
    }

    #endregion

    public void OnPointerEnter(PointerEventData eventData)
    {
        description.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        description.SetActive(false);
    }
}