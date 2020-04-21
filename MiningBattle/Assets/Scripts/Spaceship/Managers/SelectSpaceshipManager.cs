using System.Collections.Generic;
using FlowPathfinding;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectSpaceshipManager : MonoBehaviour
{
    #region PrivateVariables

    private enum KeyPressedSelection
    {
        Default,
        AttackerSelection,
        DefenceSelection,
        MinerSelection
    }

    private KeyPressedSelection _keyPressedSelection;

    private float _sizeX;
    private float _sizeY;

    private bool _isCameraNotNull;
    private bool _gameHasStarted;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _startMouse;

    private List<Spaceship> _spaceships;
    private List<Spaceship> _selectedSpaceships;
    private List<Seeker> _selectedSeekers;

    private Camera _camera;

    private SpaceshipManager _spaceshipManager;

    private bool _active;
    private bool _overUI = false;

    #endregion

    #region PublicVariables

    public static SelectSpaceshipManager Instance;

    public RectTransform selectSquareImage;

    public GameObject AutomationButton;

    public GameObject DeleteButton;

    #endregion

    #region PrivateMethods

    private void Awake()
    {
        Instance = this;
        _camera = Camera.main;
        _isCameraNotNull = _camera != null;

        _spaceshipManager = GetComponentInParent<SpaceshipManager>();

        _spaceships = new List<Spaceship>();
        _selectedSpaceships = new List<Spaceship>();
        _selectedSeekers = new List<Seeker>();

        _active = true;
        AutomationButton.SetActive(false);
        DeleteButton.SetActive(false);
    }

    private void Update()
    {
        if (_gameHasStarted && _active)
        {
            Inputs();
            MinersAutomationButton();
            DestroySpaceshipsButton();
        }
        else
            _gameHasStarted = true;
    }

    private void Inputs()
    {
        if (PauseMenu.GameIsPaused)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            _overUI = false;
            if (EventSystem.current.IsPointerOverGameObject())
            {
                _overUI = true;
                return;
            }

            ClearSelectedSpaceships();

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            _startPos = ray.GetPoint(-ray.origin.z / ray.direction.z);

            if (Input.GetKey(PlayerInputsManager.Instance.AttackerSelectionKey))
                _keyPressedSelection = KeyPressedSelection.AttackerSelection;
            else if (Input.GetKey(PlayerInputsManager.Instance.DefenceSelectionKey))
                _keyPressedSelection = KeyPressedSelection.DefenceSelection;
            else if (Input.GetKey(PlayerInputsManager.Instance.MinerSelectionKey))
                _keyPressedSelection = KeyPressedSelection.MinerSelection;
            else
                _keyPressedSelection = KeyPressedSelection.Default;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selectSquareImage.sizeDelta.magnitude < 0.001)
                SelectOneSpaceship();
            else
            {
                if (_keyPressedSelection == KeyPressedSelection.AttackerSelection)
                    SelectMultipleSpaceshipsType(SpaceshipType.Types.Attack);
                else if (_keyPressedSelection == KeyPressedSelection.DefenceSelection)
                    SelectMultipleSpaceshipsType(SpaceshipType.Types.Defence);
                else if (_keyPressedSelection == KeyPressedSelection.MinerSelection)
                    SelectMultipleSpaceshipsType(SpaceshipType.Types.Mining);
                else
                    SelectMultipleSpaceships();
            }

            selectSquareImage.sizeDelta = Vector2.zero;
        }

        if (Input.GetMouseButton(0) && !_overUI)
            DrawSelectionBox();

        if (Input.GetKeyDown(PlayerInputsManager.Instance.DestroySpaceshipKey))
            RemoveSelectedSpaceships();
    }

    private void ClearSelectedSpaceships()
    {
        if (_selectedSpaceships.Count <= 0) return;

        foreach (var obj in _selectedSpaceships)
            obj.SetIsSelected(false);

        _selectedSpaceships.Clear();
        _selectedSeekers.Clear();
    }

    private void SelectMultipleSpaceships()
    {
        var rectBox = new Rect(_startMouse.x, _startMouse.y, _endPos.x - _startMouse.x, _endPos.y - _startMouse.y);

        UpdateListOfSpaceships();

        foreach (var spaceship in _spaceships)
        {
            if (_isCameraNotNull &&
                rectBox.Contains(_camera.WorldToScreenPoint(spaceship.gameObject.transform.position), true))
            {
                if (_selectedSpaceships.Contains(spaceship)) continue;

                _selectedSpaceships.Add(spaceship);
                _selectedSeekers.Add(spaceship.GetComponent<Seeker>());

                spaceship.SetIsSelected(true);
            }
            else if (_selectedSpaceships.Contains(spaceship))
            {
                _selectedSpaceships.Remove(spaceship);
                _selectedSeekers.Remove(spaceship.GetComponent<Seeker>());

                spaceship.SetIsSelected(false);
            }
        }
    }

    private void SelectMultipleSpaceshipsType(SpaceshipType.Types type)
    {
        var rectBox = new Rect(_startMouse.x, _startMouse.y, _endPos.x - _startMouse.x, _endPos.y - _startMouse.y);

        UpdateListOfSpaceships();

        foreach (var spaceship in _spaceships)
        {
            if (_isCameraNotNull &&
                rectBox.Contains(_camera.WorldToScreenPoint(spaceship.gameObject.transform.position), true) &&
                spaceship.type == type)
            {
                if (_selectedSpaceships.Contains(spaceship)) continue;

                _selectedSpaceships.Add(spaceship);
                _selectedSeekers.Add(spaceship.GetComponent<Seeker>());
                spaceship.SetIsSelected(true);
            }
            else if (_selectedSpaceships.Contains(spaceship))
            {
                _selectedSpaceships.Remove(spaceship);
                _selectedSeekers.Remove(spaceship.GetComponent<Seeker>());
                spaceship.SetIsSelected(false);
            }
        }
    }

    private void SelectOneSpaceship()
    {
        if (!_isCameraNotNull) return;

        var entityHit = Physics2D.Raycast(_startPos, Vector2.zero);

        if (!entityHit) return;

        var spaceshipHit = entityHit.collider.gameObject.GetComponent<Spaceship>();

        if (!spaceshipHit) return;

        foreach (var spaceship in _spaceships)
            spaceship.SetIsSelected(false);

        _selectedSpaceships.Add(spaceshipHit);
        _selectedSeekers.Add(spaceshipHit.GetComponent<Seeker>());
        spaceshipHit.SetIsSelected(true);
    }

    private void DrawSelectionBox()
    {
        _endPos = Input.mousePosition;

        _startMouse = _camera.WorldToScreenPoint(_startPos);

        var centre = (_startMouse + _endPos) / 2;

        selectSquareImage.position = centre;

        _sizeX = Mathf.Abs(_startMouse.x - _endPos.x);
        _sizeY = Mathf.Abs(_startMouse.y - _endPos.y);

        selectSquareImage.sizeDelta = new Vector2(_sizeX, _sizeY);
    }

    private void UpdateListOfSpaceships()
    {
        var spaceshipsGo = _spaceshipManager.GetMySpaceships();

        _spaceships.Clear();
        foreach (var obj in spaceshipsGo)
            _spaceships.Add(obj.GetComponent<Spaceship>());
    }

    private void MinersAutomationButton()
    {
        bool hasMiners = false;

        foreach (Spaceship selectedSpaceship in _selectedSpaceships)
        {
            if (selectedSpaceship != null && selectedSpaceship.gameObject != null &&
                selectedSpaceship.gameObject.GetComponent<Miner>() != null)
            {
                hasMiners = true;
                break;
            }
        }

        if (AutomationButton.activeInHierarchy != hasMiners)
            AutomationButton.SetActive(hasMiners);
    }

    private void DestroySpaceshipsButton()
    {
        bool hasSpaceships = _selectedSpaceships.Count > 0 ? true : false;

        if (DeleteButton.activeInHierarchy != hasSpaceships)
            DeleteButton.SetActive(hasSpaceships);
    }

    #endregion

    #region PublicMethods

    public List<Spaceship> GetSelectedSpaceships()
    {
        return _selectedSpaceships;
    }

    public List<Seeker> GetSelectedSeekers()
    {
        return _selectedSeekers;
    }

    public void SetActive(bool active)
    {
        _active = active;
    }

    public bool GetActive()
    {
        return _active;
    }

    public void RemoveSelectedSpaceships()
    {
        List<Spaceship> toRemove = new List<Spaceship>();

        foreach (Spaceship spaceship in _selectedSpaceships)
        {
            if (_spaceships.Contains(spaceship))
            {
                _spaceships.Remove(spaceship);
                switch (spaceship.type)
                {
                    case SpaceshipType.Types.Attack:
                        PlayerInventory.EarnMoney((int) (BuySpaceships.Instance.attackPrice * 0.5f));
                        break;
                    case SpaceshipType.Types.Defence:
                        PlayerInventory.EarnMoney((int) (BuySpaceships.Instance.defencePrice * 0.5f));
                        break;
                    case SpaceshipType.Types.Mining:
                        PlayerInventory.EarnMoney((int) (BuySpaceships.Instance.minerPrice * 0.5f));
                        break;
                }

                _spaceshipManager.RemoveSpaceship(spaceship.gameObject);
                toRemove.Add(spaceship);
            }
        }

        foreach (Spaceship rm in toRemove)
        {
            _selectedSpaceships.Remove(rm);
            _selectedSeekers.Remove(rm);
        }

        toRemove.Clear();

        ClearSelectedSpaceships();
    }

    public void RemoveSelectedSpaceship(Spaceship spaceship)
    {
        if (_selectedSpaceships.Contains(spaceship))
        {
            _selectedSpaceships.Remove(spaceship);
            _selectedSeekers.Remove(spaceship);
        }
    }

    #endregion
}