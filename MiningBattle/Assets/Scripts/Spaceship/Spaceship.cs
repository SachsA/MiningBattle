using System;
using ExitGames.Client.Photon;
using FlowPathfinding;
using Photon.Pun;
using UnityEngine;

public class Spaceship : Seeker
{
    #region PrivateVariables

    private bool _isMoving;
    private bool _isSelected;

    private Vector2 _moveTo;

    private Attacker _attacker;

    private Rigidbody2D _rb2D;

    private Vector2[] _seenTiles = null;

    private Vector2 _spaceshipPosition = new Vector2();

    private byte EventCodeTouched = 69;

    private Tile _lastTile;

    private float _lastTime = 0;

    #endregion

    #region PublicVariables

    public SpriteRenderer circleSelection;

    public SpaceshipType.Types type;

    public SpaceshipState.State state;

    public int FieldOfView;

    public int Life;

    #endregion

    #region PrivateMethods

    private void Awake()
    {
        _miner = GetComponent<Miner>();
        circleSelection.enabled = false;
        state = SpaceshipState.State.Idle;
        _attacker = GetComponentInChildren<Attacker>();
        _rb2D = GetComponent<Rigidbody2D>();
        GetNbTilesInFOV();
        _lastTile = Pathfinder.Instance.worldData.TileManager.GetTileFromPosition(transform.position);
    }


    private float _lastCheck = 0;

    private void Update()
    {
        _spaceshipPosition.x = transform.position.x;
        _spaceshipPosition.y = transform.position.y;

        Vector2Int positionIndex = World.Instance.getTileIndex(transform.position.x, transform.position.y);
        Vector3 positionInWorld = World.Instance.getTilePosition(positionIndex.x, positionIndex.y);
        if (_lastTile != currentTile && World.Instance.GetBlockType(positionInWorld) == World.BlockType.DAEGUNIUM)
        {
            WinCondition.Instance.TriggerWin();
        }

        bool updateSeen = false;
        _lastCheck += Time.deltaTime;
        if (_lastCheck > 0.5)
        {
            updateSeen = true;
            _lastCheck = 0;
        }

        if (_lastTile != currentTile || updateSeen)
        {
            FOG.Instance.UnSeeTiles(_seenTiles);
            FindTilesInFOV();

            if (_miner != null)
                _miner.IndexTiles = 0;

            foreach (Vector2 seenTile in _seenTiles)
            {
                Vector2Int tileIndex = World.Instance.getTileIndex(seenTile.x, seenTile.y);
                Vector3 tilePos = World.Instance.getTilePosition(tileIndex.x, tileIndex.y);

                FOG.Instance.SeeTile(tilePos, tileIndex);

                if (World.Instance.GetBlockType(tilePos) != World.BlockType.VOID)
                {
                    World.Instance.DiscoverRock(tilePos);
                    if (_miner != null)
                        _miner.ViewTile(tilePos, currentTile != _lastTile);
                }
            }
        }

        _lastTile = currentTile;
    }

    public void LookAround()
    {
        if (_miner != null)
            _miner.IndexTiles = 0;
        foreach (Vector2 seenTile in _seenTiles)
        {
            Vector2Int tileIndex = World.Instance.getTileIndex(seenTile.x, seenTile.y);
            Vector3 tilePos = World.Instance.getTilePosition(tileIndex.x, tileIndex.y);

            if (World.Instance.GetBlockType(tilePos) != World.BlockType.VOID)
            {
                World.Instance.DiscoverRock(tilePos);
                if (_miner != null)
                    _miner.ViewTile(tilePos, false);
            }
        }
    }

    private void FixedUpdate()
    {
        if (_attacker)
        {
            if (!_attacker.lookAtEnemy)
            {
                if (_isMoving)
                {
                    LookAt(_moveTo);
                }
                else
                    _rb2D.angularVelocity = 0;
            }
            else
                _rb2D.angularVelocity = 0;
        }
        else if (_isMoving)
            LookAt(_moveTo);
        else
            _rb2D.angularVelocity = 0;
    }

    private void GetNbTilesInFOV()
    {
        int nbTiles = 0;

        int radius = FieldOfView;
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                if (x * x + y * y < Math.Pow(radius, 2))
                {
                    nbTiles++;
                }
            }
        }

        _seenTiles = new Vector2[nbTiles];
        if (_miner != null)
            _miner.InitializeTilesMineable(nbTiles);
    }

    private Vector2Int tmpTile = new Vector2Int(0, 0);

    private void FindTilesInFOV()
    {
        int radius = FieldOfView;
        int i = 0;
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                if (x * x + y * y < Math.Pow(radius, 2))
                {
                    tmpTile.x = Mathf.FloorToInt(_spaceshipPosition.x + x);
                    tmpTile.y = Mathf.FloorToInt(_spaceshipPosition.y + y);
                    _seenTiles[i] = tmpTile;
                    ++i;
                }
            }
        }

        Array.Sort(_seenTiles, compareToTarget);
    }

    private int compareToTarget(Vector2 a, Vector2 b)
    {
        float da = (a - (Vector2) transform.position).magnitude;
        float db = (b - (Vector2) transform.position).magnitude;

        if (da < db)
            return -1;
        else if (db < da)
            return 1;
        return 0;
    }

    private void OnDestroy()
    {
        FOG.Instance.UnSeeTiles(_seenTiles);
    }

    #endregion

    #region PublicMethods

    public void LookAt(Vector3 target)
    {
        var angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg - 90;

        var diffAngle = angle - _rb2D.rotation;
        if (diffAngle > 180)
            diffAngle -= 360;
        else if (diffAngle < -180)
            diffAngle += 360;

        _rb2D.angularVelocity = diffAngle * Time.deltaTime * 300;
    }

    public void SetIsSelected(bool select)
    {
        _isSelected = select;
        circleSelection.enabled = _isSelected;
    }

    public bool GetIsSelected()
    {
        return _isSelected;
    }

    public void SetMoveTo(Vector2 moveTo)
    {
        _moveTo = moveTo;
    }

    public void SetIsMoving(bool isMoving)
    {
        _isMoving = isMoving;
    }

    public Vector2[] GetSeenTiles()
    {
        return _seenTiles;
    }

    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEventTouched;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventTouched;
    }

    public void OnEventTouched(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == EventCodeTouched)
        {
            string spaceshipId = (string) photonEvent.CustomData;
            if (GetComponent<NetworkIdentity>()._id != spaceshipId)
                return;
            Touched();
        }
    }

    public void Touched()
    {
        Life -= 1;
        if (Life <= 0)
        {
            SelectSpaceshipManager.Instance.RemoveSelectedSpaceship(this);
            SpaceshipManager.Instance.RemoveSpaceship(this.gameObject);
        }
    }

    #endregion
}