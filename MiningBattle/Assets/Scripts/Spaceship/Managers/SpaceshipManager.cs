using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipManager : MonoBehaviour
{
    #region PrivateVariables

    private List<GameObject> _mySpaceships;
    private List<GameObject> _myAttackSpaceships;
    private List<GameObject> _myDefenceSpaceships;
    private List<GameObject> _myMiningSpaceships;

    private SelectSpaceshipManager _selectSpaceshipManager;

    private FlowPathfinding.Pathfinder _pathfinder;

    private List<Miner> _miners = new List<Miner>();

    private List<GameObject> _minersLast = new List<GameObject>();
    private List<Vector2> _minersLastPositions = new List<Vector2>();

    private Vector3 _daeguniumPosition = new Vector3(0.5f, 0.5f, 0);

    private AudioSource _audioSource;

    #endregion

    #region PublicVariables

    public static SpaceshipManager Instance;

    public GameObject attackSpaceship;
    public GameObject defenceSpaceship;
    public GameObject miningSpaceship;

    public GameObject playerBase;
    public float spawnRadius = 3.0f;

    #endregion

    #region PrivateMethods

    private void Awake()
    {
        Instance = this;
        //_photonView = PhotonView.Get(this);
        //_currentSpaceshipIndex = 0;
        _mySpaceships = new List<GameObject>();
        _myAttackSpaceships = new List<GameObject>();
        _myDefenceSpaceships = new List<GameObject>();
        _myMiningSpaceships = new List<GameObject>();

        _selectSpaceshipManager = GetComponent<SelectSpaceshipManager>();
        _pathfinder = GameObject.Find("Pathfinder2D").GetComponent<FlowPathfinding.Pathfinder>();

        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (PauseMenu.GameIsPaused)
            return;

        if (Input.GetKeyDown(PlayerInputsManager.Instance.AutomationKey))
            SwitchMinersAutomation();
    }

    private static Vector3 RandomCircle(Vector3 center, float radius)
    {
        Vector3 pos;
        var ang = Random.value * 360;

        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;

        return pos;
    }

    #endregion

    #region PublicMethods

    public List<GameObject> GetMySpaceships()
    {
        return _mySpaceships;
    }

    public List<GameObject> GetMyAttackSpaceships()
    {
        return _myAttackSpaceships;
    }

    public List<GameObject> GetMyDefenceSpaceships()
    {
        return _myDefenceSpaceships;
    }

    public List<GameObject> GetMyMiningSpaceships()
    {
        return _myMiningSpaceships;
    }

    public void AddSpaceship(SpaceshipType.Types type)
    {
        GameObject spaceship;

        var center = playerBase.transform.position;
        var position = RandomCircle(center, spawnRadius);
        var rotation = Quaternion.identity;

        switch (type)
        {
            case SpaceshipType.Types.Attack:
                if (PhotonNetwork.IsConnected)
                {
                    spaceship = PhotonNetwork.Instantiate("AttackSpaceship", position, rotation);
                    spaceship.transform.SetParent(transform);
                }
                else
                {
                    spaceship = Instantiate(attackSpaceship, position, rotation, transform);
                }
                _mySpaceships.Add(spaceship);
                _myAttackSpaceships.Add(spaceship);
                WinCondition.Instance.myAnalytics.attackBuilt += 1;
                break;
            case SpaceshipType.Types.Defence:
                if (PhotonNetwork.IsConnected)
                {
                    spaceship = PhotonNetwork.Instantiate("DefenceSpaceship", position, rotation);
                    spaceship.transform.SetParent(transform);
                }
                else
                {
                    spaceship = Instantiate(defenceSpaceship, position, rotation, transform);
                }
                _mySpaceships.Add(spaceship);
                _myDefenceSpaceships.Add(spaceship);
                WinCondition.Instance.myAnalytics.defenceBuilt += 1;
                break;
            case SpaceshipType.Types.Mining:
                if (PhotonNetwork.IsConnected)
                {
                    spaceship = PhotonNetwork.Instantiate("MiningSpaceship", position, rotation);
                    spaceship.transform.SetParent(transform);
                }
                else
                {
                    spaceship = Instantiate(miningSpaceship, position, rotation, transform);
                }
                _mySpaceships.Add(spaceship);
                _myMiningSpaceships.Add(spaceship);
                WinCondition.Instance.myAnalytics.minerBuilt += 1;
                break;
        }
    }

    public void RemoveSpaceship(GameObject spaceship)
    {
        var type = spaceship.GetComponent<Spaceship>().type;
        Spaceship seeker = spaceship.GetComponent<Spaceship>();
        _mySpaceships.Remove(spaceship);
        _pathfinder.seekerManager.RemoveSeeker(seeker);

        switch (type)
        {
            case SpaceshipType.Types.Attack:
                _myAttackSpaceships.Remove(spaceship);
                break;
            case SpaceshipType.Types.Defence:
                _myDefenceSpaceships.Remove(spaceship);
                break;
            case SpaceshipType.Types.Mining:
                _myMiningSpaceships.Remove(spaceship);
                break;
        }

        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Destroy(spaceship);
        else
            Destroy(spaceship);
        WinCondition.Instance.myAnalytics.spaceshipGotDestroyed += 1;
    }

    public void SetPlayerBase(GameObject _playerBase)
    {
        playerBase = _playerBase;
    }

    public List<Vector2> GetLastMinersPosition()
    {
        return _minersLastPositions;
    }

    public void AddLastMinerPosition(GameObject miner, Vector2 position)
    {
        int index = _minersLast.IndexOf(miner);

        if (index >= 0)
        {
            _minersLastPositions.RemoveAt(index);
            _minersLastPositions.Insert(index, position);
        }
        else
        {
            _minersLast.Add(miner);
            _minersLastPositions.Add(position);
        }
    }

    public void RemoveLastMinerPosition(Vector2 position)
    {
        int index = _minersLastPositions.IndexOf(position);

        if (index >= 0)
        {
            _minersLast.RemoveAt(index);
            _minersLastPositions.RemoveAt(index);
        }
    }

    public void SwitchMinersAutomation()
    {
        List<Spaceship> spaceships = _selectSpaceshipManager.GetSelectedSpaceships();
        bool automation = true;

        _miners.Clear();

        foreach (Spaceship spaceship in spaceships)
        {
            Miner miner = spaceship.GetComponent<Miner>();
            if (miner != null)
            {
                _miners.Add(miner);
                if (miner.GetAutomation())
                    automation = false;
            }
        }

        foreach (Miner miner in _miners)
        {
            miner.SetAutomation(automation);
        }
    }

    public float GetClosestMinerFromDaegunium()
    {
        _myMiningSpaceships.Sort((v1, v2) => (v1.transform.position - _daeguniumPosition).sqrMagnitude.CompareTo((v2.transform.position - _daeguniumPosition).sqrMagnitude));

        if (_myMiningSpaceships.Count > 0)
            return (_myMiningSpaceships[0].transform.position - _daeguniumPosition).magnitude;
        return 0;
    }

    #endregion
}