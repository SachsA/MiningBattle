using System.Collections.Generic;
using FlowPathfinding;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveSpaceshipManager : MonoBehaviour
{
    #region PrivateVariables

    private List<Seeker> _selectedSeekers;

    private SelectSpaceshipManager _selectSpaceshipManager;

    private bool _gameHasStarted;

    #endregion

    public Pathfinder pathfinder;
    
    #region PrivateMethods

    private void Awake()
    {
        _selectSpaceshipManager = GetComponent<SelectSpaceshipManager>();
        _selectedSeekers = new List<Seeker>();
    }

    private void Update()
    {
        if (PauseMenu.GameIsPaused)
            return;

        if (_gameHasStarted)
            Inputs();
        else
            _gameHasStarted = true;
    }

    private void Inputs()
    {
        if (!_selectSpaceshipManager.GetActive())
            return;
        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            var mousePosition = pathfinder.GetMousePosition();

            Tile tile = pathfinder.worldData.TileManager.GetTileFromPosition(mousePosition);
            if (tile != null)
            {
                _selectedSeekers = _selectSpaceshipManager.GetSelectedSeekers();

                var index = 0;
                foreach (var selectedSeeker in _selectedSeekers)
                {
                    selectedSeeker.SetIndexSeeker(index);
                    
                    Miner miner = selectedSeeker.gameObject.GetComponent<Miner>();
                    if (miner != null)
                    {
                        miner.ManualMove(tile);
                    }

                    
                    index++;
                }
                pathfinder.FindPath(tile, _selectedSeekers);
            }
        }
    }

    #endregion
}