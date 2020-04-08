using UnityEngine;
using UnityEngine.EventSystems;

public class MineZoneSelection : MonoBehaviour
{
    #region PublicVariables

    public RectTransform selectSquareImage;

    #endregion

    #region PrivateVariables

    [SerializeField]
    private Grid grid = null;


    private bool _onSelection;
    private bool _onUnselection;
    
    private Vector2 _endMousePos;

    private Vector2 _startZonePos;
    private Vector2 _endZonePos;

    private Vector2 _startMousePosWorld;

    private float _sizeX;
    private float _sizeY;

    private bool _active;
    
    private Camera _camera;
    private bool _isCameraNotNull;

    #endregion

    private void Awake()
    {
        _camera = Camera.main;
        _isCameraNotNull = _camera != null;
    }

    private void Start()
    {
        selectSquareImage.sizeDelta = new Vector2(0, 0);
        _onSelection = false;
        _onUnselection = false;
        _active = false;
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused || !_active)
            return;

        if (!_onUnselection)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                _onSelection = true;
                _startZonePos = (Vector2Int)GetSelectionPositionAtClick();
            }
            else if (_onSelection)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    _onSelection = false;
                    selectSquareImage.sizeDelta = new Vector2(0, 0);

                    _endZonePos = (Vector2Int)GetSelectionPositionAtClick();
                    World.Instance.SelectMineableZone(_startZonePos, _endZonePos);
                } else
                    DrawSelectionBox();
            }
        }
        if (!_onSelection)
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                _onUnselection = true;
                _startZonePos = (Vector2Int)GetSelectionPositionAtClick();
            }
            else if (_onUnselection)
            {
                if (Input.GetMouseButtonUp(1))
                {
                    _onUnselection = false;
                    selectSquareImage.sizeDelta = new Vector2(0, 0);

                    _endZonePos = (Vector2Int)GetSelectionPositionAtClick();
                    World.Instance.UnselectMineableZone(_startZonePos, _endZonePos);
                } else 
                    DrawSelectionBox();
            }
        }
    }

    private Vector3Int GetSelectionPositionAtClick()
    {
        if (_isCameraNotNull)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            _startMousePosWorld = ray.GetPoint(-ray.origin.z / ray.direction.z);
        }

        return grid.WorldToCell(_startMousePosWorld);
    }

    private void DrawSelectionBox()
    {
        _endMousePos = Input.mousePosition;

        Vector2 startMousePosScreen = _camera.WorldToScreenPoint(_startMousePosWorld);
        
        Vector2 centre = (startMousePosScreen + _endMousePos) / 2;
        selectSquareImage.position = centre;

        _sizeX = Mathf.Abs(startMousePosScreen.x - _endMousePos.x);
        _sizeY = Mathf.Abs(startMousePosScreen.y - _endMousePos.y);

        selectSquareImage.sizeDelta = new Vector2(_sizeX, _sizeY);
    }

    public void SetActive(bool active)
    {
        _active = active;
    }

    public bool GetActive()
    {
        return _active;
    }
}
