using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlowPathfinding;
using Photon.Pun;

public class Miner : MonoBehaviour
{
    #region Enums

    public enum Status
    {
        Idle = 0,
        Ability,
        Move
    }

    public enum Ability
    {
        MoveToBase = 0,
        MoveFromBase,
        MoveToTile,
        Mine
    }

    public enum Move
    {
        Start = 0,
        Moving,
        End
    }

    #endregion

    #region Private Fields

    private bool _automation;

    private Status _status;
    private Ability _ability;
    private Move _move;

    private Seeker _seeker;
    private Spaceship _spaceship;

    private Vector3 _basePosition, _savedPosition, _tilePosition;
    
    private Vector2Int[] _tilesMineable = null;

    private float _timeFromLastTick;

    private Tile _tileTarget;
    
    private int _storage;

    #endregion

    #region Public Fields

    [Tooltip("How long did it take to mine one time.")]
    public float AbilitySpeed = 0.10f;

    [Tooltip("The storage capacity.")] public int MaxStorage = 5;

    [Tooltip("The spaceship script.")] public Spaceship Spaceship;

    [Tooltip("The script used to play the waiting icon animation.")]
    public WaitingAnimation WaitingAnimation;

    [Tooltip("The script used to play the storage icon animation.")]
    public StorageAnimation StorageAnimation;

    #endregion

    #region Monobehaviour Callbacks

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected || GetComponent<PhotonView>().IsMine)
        {
            _seeker = GetComponent<Seeker>();
            _spaceship = GetComponent<Spaceship>();

            _status = Status.Idle;
            _storage = 0;
            _savedPosition = _tilePosition = transform.position;
            _basePosition = SpaceshipManager.Instance.playerBase.GetComponent<PlayerBaseNetwork>().StoragePosition;
            _automation = true;
           
            WaitingAnimation.Active(false);
            StorageAnimation.Active(false);
        }
    }

    private void Update()
    {
        if (_automation)
        {
            switch (_status)
            {
                case Status.Ability:
                    UseAbility();
                    break;
                case Status.Move:
                    MoveToTarget();
                    break;
                case Status.Idle:
                    if (_storage >= MaxStorage)
                    {
                        _status = Status.Ability;
                        _ability = Ability.MoveToBase;
                        _move = Move.Start;
                        return;
                    }
                    _spaceship.LookAround();
                    if (FindMineableTile())
                    {
                        _status = Status.Ability;
                        _ability = Ability.MoveToTile;
                        _move = Move.Start;
                    }
                    else if (UpdateSavedPositions())
                    {
                        _status = Status.Ability;
                        _ability = Ability.MoveFromBase;
                        _move = Move.Start; 
                    }
                    break;
            }
        }
        else
        {
            MoveToTarget();
        }
    }

    #endregion

    #region Private Methods

    private void UseAbility()
    {
        switch (_ability)
        {
            case Ability.MoveFromBase:
                MoveFromBase();
                break;
            case Ability.MoveToBase:
                MoveToBase();
                break;
            case Ability.MoveToTile:
                MoveToTile();
                break;
            case Ability.Mine:
                Mine();
                break;
        }
    }

    private void MoveToBase()
    {
        switch (_move)
        {
            case Move.Start:
                StartMoving(_basePosition);
                break;
            case Move.Moving:
                break;
            case Move.End:
                StorageAnimation.Active(false);
                _storage = 0;
                _ability = Ability.MoveFromBase;
                _move = Move.Start;
                break;
        }
    }

    private void MoveFromBase()
    {
        switch (_move)
        {
            case Move.Start:
                StartMoving(_savedPosition);
                break;
            case Move.Moving:
                break;
            case Move.End:
                _status = Status.Idle;
                _move = Move.Start;
                break;
        }
    }
    
    private void MoveToTarget()
    {
        switch (_move)
        {
            case Move.Start:
            case Move.Moving:
                break;
            case Move.End:
                _status = Status.Idle;
                _move = Move.Start;
                break;
        }
    }
    
    private void MoveToTile()
    {
        switch (_move)
        {
            case Move.Start:
                StartMoving(_savedPosition);
                break;
            case Move.Moving:
                break;
            case Move.End: 
                _ability = Ability.Mine;
                break;
        }
    }

    private void StartMoving(Vector3 destination)
    {
        Tile moveTarget = Pathfinder.Instance.worldData.TileManager.GetTileFromPosition(destination);
        Tile actualPosition = Pathfinder.Instance.worldData.TileManager.GetTileFromPosition(transform.position + _offset);
        if (moveTarget != null)
        {
            if (moveTarget == actualPosition)
            {
                _move = Move.End;
                return;
            }
            _seeker.SetIndexSeeker(0);
            _seeker._mode = Seeker.Mode.Automatic;
            Pathfinder.Instance.FindPath(moveTarget, new List<Seeker>(GetComponents<Seeker>()));
            _move = Move.Moving;
        }
    }

    private bool FindMineableTile()
    {
        if (_tilesMineable != null && _tilesMineable[0] != null)
        {
            if (DefineTileTarget())
            {
                return true;
            }
        }
        return false;
    }

    private bool UpdateSavedPositions()
    {
        if (SpaceshipManager.Instance.GetLastMinersPosition().Count == 0)
            return false;
        Vector2 destination = SpaceshipManager.Instance.GetLastMinersPosition()[0];
        SpaceshipManager.Instance.RemoveLastMinerPosition(destination);

        _savedPosition = destination;
        return true;
    }

    private List<Tile> _neighbours = null;
    private Vector3 _offset = new Vector3(0.5f, 0.5f, 0);

    private bool DefineTileTarget()
    {
        bool foundTile = false;
        _neighbours = null;

        foreach (Vector2 tileMineable in _tilesMineable)
        {
            if (tileMineable == Vector2.zero)
                continue;
            
            if (World.Instance.LockTile(tileMineable))
            {
                _tilePosition = tileMineable;
                _tileTarget = Pathfinder.Instance.worldData.TileManager.GetTileFromPosition(_tilePosition + _offset);
                _neighbours = Pathfinder.Instance.worldData.TileManager.GetStraightNeighbours(
                    _tileTarget.GridPos.X,
                    _tileTarget.GridPos.Y,
                    Pathfinder.Instance.worldData.WorldAreas[_tileTarget.WorldAreaIndex]);
                
                foreach (Tile neighbour in _neighbours)
                {
                    if (neighbour.Blocked == false)
                    {
                        foundTile = true;
                        break;
                    }
                }

                if (foundTile == false)
                {
                    World.Instance.UnlockTile(tileMineable);
                    continue;
                }
                _neighbours.Sort((v1, v2) =>
                    (Pathfinder.Instance.worldData.TileManager.GetTileWorldPosition(v1,
                         Pathfinder.Instance.worldData.WorldAreas[v1.WorldAreaIndex]) - transform.position).magnitude
                    .CompareTo((Pathfinder.Instance.worldData.TileManager.GetTileWorldPosition(v2,
                                    Pathfinder.Instance.worldData.WorldAreas[v2.WorldAreaIndex]) - transform.position)
                        .magnitude));
                break;
            }
        }

        if (!foundTile)
            return false;

        foreach (Tile neighbour in _neighbours)
        {
            if (neighbour.Blocked == false)
            {
                _savedPosition.x = Pathfinder.Instance.worldData.TileManager.GetTileWorldPosition(neighbour,
                    Pathfinder.Instance.worldData.WorldAreas[neighbour.WorldAreaIndex]).x;
                _savedPosition.y = Pathfinder.Instance.worldData.TileManager.GetTileWorldPosition(neighbour,
                    Pathfinder.Instance.worldData.WorldAreas[neighbour.WorldAreaIndex]).z;
                return true;
            }
        }
        World.Instance.UnlockTile(_tilePosition);
        return false;
    }

    public int IndexTiles = 0;

    private void FindClosestMineableTile(Vector2Int tilePos)
    {
        if (World.Instance.IsTileMineable(tilePos) && !TilesMineableContains(tilePos))
        {
            _tilesMineable[IndexTiles] = tilePos;
            IndexTiles += 1;
        }
    }

    private bool TilesMineableContains(Vector2Int tilePos)
    {
        for (int i = 0; i < IndexTiles + 1; i++)
        {
            if (_tilesMineable[i] == tilePos)
                return true;
        }
        return false;
    }

    private void Mine()
    {
        _timeFromLastTick += Time.deltaTime;
        if (_timeFromLastTick >= AbilitySpeed)
        {
            _timeFromLastTick -= AbilitySpeed;
            int amount = World.Instance.MineTile(_tilePosition);
            if (amount != 0)
            {
                _status = Status.Idle;
                if (amount > 0)
                {
                    _storage += 1;
                    PlayerInventory.EarnMoney(amount);
                    if (_storage >= MaxStorage)
                    {
                        StorageAnimation.Active(true);
                        StorageAnimation.Full();
                        _status = Status.Ability;
                        _ability = Ability.MoveToBase;
                        SpaceshipManager.Instance.AddLastMinerPosition(this.gameObject, _savedPosition);
                        _move = Move.Start;
                    }
                }
            }
        }
    }

    private void StopAutomation()
    {
        WaitingAnimation.Active(true);
        WaitingAnimation.StartWait();

        _automation = false;
    }

    private void StartAutomation()
    {
        WaitingAnimation.StopWait();
        WaitingAnimation.Active(false);

        _automation = true;
    }

    #endregion

    #region Public Methods

    public void SetAutomation(bool automation)
    {
        if (automation)
            StartAutomation();
        else
            StopAutomation();
    }

    public bool GetAutomation()
    {
        return _automation;
    }

    public void ManualMove(Tile tileDestination)
    {
        World.Instance.UnlockTile(_tilePosition);

        _status = Status.Move;
        _move = Move.Moving;
    }

    public void DestinationIsReached()
    {
        _move = Move.End;
    }

    public void setIdle()
    {
        _status = Status.Idle;
    }

    public void ViewTile(Vector3 tilePos, bool updateOre)
    {
        if (updateOre)
            World.Instance.DiscoverOre(tilePos);

        FindClosestMineableTile(Vector2Int.FloorToInt(tilePos));
    }

    public void InitializeTilesMineable(int nb)
    {
        _tilesMineable = new Vector2Int[nb];
    }

    #endregion
}