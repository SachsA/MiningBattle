using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using FlowPathfinding;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using Tile = UnityEngine.Tilemaps.Tile;

[RequireComponent(typeof(PhotonView))]
public class World : MonoBehaviour, IPunObservable
{
    #region Nested Objects

    public enum BlockType
    {
        VOID = 0,
        ROCK = 1,
        IRON = 2,
        COPPER = 3,
        GOLD = 4,
        DIAMOND = 5,
        DAEGUNIUM = 6
    }

    #endregion

    #region Public Fields

    public static World Instance;

    [Tooltip("Distance from the center of the asteroid to the player base.")]
    public int DistancePBtoCA;

    #endregion

    #region Private Fields

    #region SerializeFields

    [Header("World Size")]
    [SerializeField]
    public Vector2Int WorldSize;

    [Header("Radius")]
    [SerializeField]
    private int RockRadius = 119;
    [SerializeField]
    private int DiamondRadius = 19;
    [SerializeField]
    private int DaeguniumRadius = 5;
    
    [Header("Smooth Radius")]
    [SerializeField]
    private float smooth = 0.9f;

    [Header("Loading Game")]
    [SerializeField]
    private GameObject Background;

    [Header("Player")]
    [SerializeField]
    private CameraControl CameraControl;
    [SerializeField]
    private SpaceshipManager SpaceshipManager;

    #region Tiles

    [Header("Tiles")]

    [SerializeField]
    private RockTile RockTile;

    [SerializeField]
    private MineableTile MineableTile;

    [SerializeField]
    private Tile DiamondTile;

    [SerializeField]
    private Tile DaeguniumTile;

    [SerializeField]
    private OreTile IronTile;

    [SerializeField]
    private Tile GoldTile;

    [SerializeField]
    private Tile CopperTile;

    [SerializeField]
    private Tile MiniMapTile;

    #endregion

    #region TileMap

    [Header("TileMaps")]

    [SerializeField]
    private Tilemap AstroRock;

    [SerializeField]
    private Tilemap AstroOre;

    [SerializeField]
    private Tilemap AstroMineable;

    [SerializeField]
    private Tilemap AstroMiniMap;

    #endregion

    #endregion

    private Dictionary<BlockType, List<Vector2Int>> oreDictionary;

    private Vector2Int Origin;

    private CustomTile[,] Map;

    private CustomTile[,] Print;

    private PhotonView PhotonView;

    private byte EventReady = 11;

    private byte EventBase = 12;

    private byte EventStart = 13;

    private int nbPlayersReady;

    private int nbPlayersGenerate;

    private List<Vector2> spawnPositions;

    private GameObject ProgressCanvas;

    private LoadingGame loadingGame;

    private int[] ids = new int[4];
    
    private Camera mainCamera;

    private AudioSource audioSource;

    #region BlockType

    private static CustomTile VoidType = new CustomTile(0, BlockType.VOID, 0);

    private static CustomTile RockType = new CustomTile(20, BlockType.ROCK, 10);

    private static CustomTile IronType = new CustomTile(40, BlockType.IRON, 30);

    private static CustomTile CopperType = new CustomTile(60, BlockType.COPPER, 80);

    private static CustomTile GoldType = new CustomTile(80, BlockType.GOLD, 150);

    private static CustomTile DiamondType = new CustomTile(160, BlockType.DIAMOND, 400);

    private static CustomTile DaeguniumType = new CustomTile(-1, BlockType.DAEGUNIUM, 0);

    #endregion

    #endregion

    #region Monobehaviour Callbacks

    private void Awake()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();

        PhotonView = PhotonView.Get(this);
        PhotonPeer.RegisterType(typeof(CustomTile), (byte)'A', SerializedCustomTile.Serialize, SerializedCustomTile.Deserialize);

        Instance = this;
        Origin = new Vector2Int(-WorldSize.x / 2, -WorldSize.y / 2);

        oreDictionary = new Dictionary<BlockType, List<Vector2Int>>
        {
            { BlockType.IRON, new List<Vector2Int>() },
            { BlockType.COPPER, new List<Vector2Int>() },
            { BlockType.GOLD, new List<Vector2Int>() }
        };

        AstroRock.origin = new Vector3Int(0, 0, 0);
        AstroOre.origin = new Vector3Int(0, 0, 0);
        AstroMineable.origin = new Vector3Int(0, 0, 0);

        Map = new CustomTile[WorldSize.x, WorldSize.y];
        Print = new CustomTile[WorldSize.x, WorldSize.y];

        nbPlayersReady = 0;
        nbPlayersGenerate = 0;
        spawnPositions = new List<Vector2>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            spawnPositions = SpawnPositions.Generate(4, DistancePBtoCA, 40);
        }
        if (PhotonNetwork.IsConnected)
        {
            PhotonView.RPC("ReadyToPlay", RpcTarget.MasterClient);
        } else
        {
            GenerateAsteroid();
            expandOres();
            spawnPositions = SpawnPositions.Generate(4, DistancePBtoCA, 40);
            Array.Copy(Map, Print, WorldSize.x * WorldSize.y);
            Pathfinder.Instance.RegenerateMapping();
            UpdateBaseFog(new Vector3Int(-16, -141, 0));
            
        }
    }

    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    #endregion

    #region Public Methods

    #region Getters

    public float GetAsteroidRadius()
    {
        return RockRadius;
    }

    public Vector2Int Size()
    {
        return WorldSize;
    }

    public List<Vector2> GetSpawnPositions()
    {
        return spawnPositions;
    }
    
    public bool IsTileMineable(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if (Map[tileIndex.x, tileIndex.y].type == BlockType.VOID)
        {
            return false;
        }
        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            return Map[tileIndex.x, tileIndex.y].isMineable;
        }
        return false;
    }

    public bool IsTileMined(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            return Map[tileIndex.x, tileIndex.y].isMined;
        }
        return false;
    }

    public BlockType GetBlockType(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            return Map[tileIndex.x, tileIndex.y].type;
        }
        return BlockType.VOID;
    }

    public bool isTileDiscovered(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            return Map[tileIndex.x, tileIndex.y].isDiscovered;
        }
        return false;
    }

    private Vector3Int TilePosition = new Vector3Int(0, 0, 0);

    public Vector3Int getTilePosition(int x, int y)
    {
        TilePosition.x = x + Origin.x;
        TilePosition.y = y + Origin.y;
        TilePosition.z = 0;

        return TilePosition;
    }

    private Vector2Int TileIndex = new Vector2Int(0, 0);

    private int inc = 0;
    public Vector2Int getTileIndex(float x, float y)
    {
        TileIndex.x = Mathf.FloorToInt(x) - Origin.x;
        TileIndex.y = Mathf.FloorToInt(y) - Origin.y;
        
        return TileIndex;
    }

    #endregion

    #region Setters

    public void SelectMineableZone(Vector2 topLeft, Vector2 botRight)
    {
        updateMineableTiles(topLeft, botRight, true);
    }

    public void UnselectMineableZone(Vector2 topLeft, Vector2 botRight)
    {
        updateMineableTiles(topLeft, botRight, false);
    }

    public void DiscoverTile(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            Map[tileIndex.x, tileIndex.y].isDiscovered = true;
            Vector3Int position = new Vector3Int((int)tilePosition.x, (int)tilePosition.y, 0);

            AstroOre.RefreshTile(position);
            AstroMiniMap.RefreshTile(position);

            if (PhotonNetwork.IsConnected && Map[tileIndex.x, tileIndex.y].type != Print[tileIndex.x, tileIndex.y].type || Map[tileIndex.x, tileIndex.y].isMined != Print[tileIndex.x, tileIndex.y].isMined)
            {
                PrintToMap(tileIndex);
            }
        }
    }

    private Vector3Int TilePosition2 = new Vector3Int(0, 0, 0);

    public void DiscoverOre(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            if (!Map[tileIndex.x, tileIndex.y].isDiscovered)
            {
                Map[tileIndex.x, tileIndex.y].isDiscovered = true;
                TilePosition2.x = (int)tilePosition.x;
                TilePosition2.y = (int)tilePosition.y;
                TilePosition2.z = 0;

                AstroOre.RefreshTile(TilePosition2);
                AstroMiniMap.RefreshTile(TilePosition2);
            }
        }
    }

    public void DiscoverRock(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            if (PhotonNetwork.IsConnected && Map[tileIndex.x, tileIndex.y].type != Print[tileIndex.x, tileIndex.y].type)
                PrintToMap(tileIndex);
        }
    }

    public void ShineTile(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            //Animation de brillance des "ores" qui ne sont ni VOID ni ROCKS ni DAEGUNIUM
        }
    }

    public bool LockTile(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            if (Map[tileIndex.x, tileIndex.y].isMineable &&
            !Map[tileIndex.x, tileIndex.y].isMined)
            {
                Map[tileIndex.x, tileIndex.y].isMined = true;
                if (PhotonNetwork.IsConnected)
                    PhotonView.RPC("UpdateTile", RpcTarget.All, Map[tileIndex.x, tileIndex.y], tileIndex.x, tileIndex.y);
                return true;
            }
        }
        return false;
    }

    public void UnlockTile(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y))
        {
            if (Map[tileIndex.x, tileIndex.y].isMineable &&
            Map[tileIndex.x, tileIndex.y].isMined)
            {
                Map[tileIndex.x, tileIndex.y].isMined = false;
                if (PhotonNetwork.IsConnected)
                    PhotonView.RPC("UpdateTile", RpcTarget.All, Map[tileIndex.x, tileIndex.y], tileIndex.x, tileIndex.y);
            }
        }
    }

    public int MineTile(Vector2 tilePosition)
    {
        Vector2Int tileIndex = getTileIndex(tilePosition.x, tilePosition.y);

        if ((tileIndex.x >= 0 && tileIndex.x < WorldSize.x) &&
            (tileIndex.y >= 0 && tileIndex.y < WorldSize.y) &&
            Map[tileIndex.x, tileIndex.y].type != BlockType.VOID)
        {
            if (Map[tileIndex.x, tileIndex.y].life > 0)
            {
                Map[tileIndex.x, tileIndex.y].life -= 1;
                if (Map[tileIndex.x, tileIndex.y].life == 0)
                {
                    int amount = Map[tileIndex.x, tileIndex.y].amount;
                    DestroyMineral(tileIndex);
                    return amount;
                }
            }
            return 0;
        }
        return -1;
    }

    #endregion

    #region Photon

    [PunRPC]
    public void UpdateTile(CustomTile _tile, int x, int y)
    {
        Print[x, y] = _tile;
        Print[x, y].isMineable = Map[x, y].isMineable;
    }

    [PunRPC]
    public void ReadyToPlay()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            nbPlayersReady += 1;
            if (nbPlayersReady == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                ProgressCanvas = PhotonNetwork.Instantiate("Progress_Canvas", new Vector3(0, 0, 0), Quaternion.identity);
                loadingGame = ProgressCanvas.GetComponent<LoadingGame>();

                PhotonNetwork.RaiseEvent(EventReady, DateTime.Now.Millisecond, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
                loadingGame.IncreaseProgress(0.22f);
            }
        }
    }

    [PunRPC]
    public void WorldIsGenerated(int _playerId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ids[nbPlayersGenerate] = _playerId;
            nbPlayersGenerate += 1;
            if (nbPlayersGenerate == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                loadingGame.IncreaseProgress(0.59f);
                for (int index = 0; index < nbPlayersGenerate; index++)
                {
                    PhotonNetwork.RaiseEvent(EventBase, spawnPositions[index], new RaiseEventOptions { TargetActors = new[] { ids[index] } }, new SendOptions { Reliability = true });
                    PhotonNetwork.RaiseEvent(EventBases, spawnPositions[index], new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
                }
                nbPlayersGenerate = 0;
            }
        }
    }

    [PunRPC]
    public void BaseInPosition()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            nbPlayersGenerate += 1;
            if (nbPlayersGenerate == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                loadingGame.IncreaseProgress(1f);
                // PhotonNetwork.Time;
                PhotonNetwork.RaiseEvent(EventStart, DateTime.Now.Millisecond, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
            }
        }
    }

    private byte EventBases = 14;

    public void OnEvent(EventData photonEvent)
    {
        byte code = photonEvent.Code;

        if (code == EventReady)
        {
            Destroy(Background);
            int seed = (int)photonEvent.CustomData;
            Random.InitState(seed);
            StartCoroutine("GenerateAsync");
        }
        if (code == EventBases)
        {
            Vector2 position = (Vector2)photonEvent.CustomData;
            Pathfinder.Instance.BlockSpawnBases(position);
        }
        if (code == EventBase)
        {
            Vector2 position = (Vector2)photonEvent.CustomData;

            PhotonNetwork.Instantiate("CommandCenter", position, Quaternion.identity);

            PhotonView.RPC("BaseInPosition", RpcTarget.MasterClient);
            
            UpdateBaseFog(Vector3Int.FloorToInt(position));
        }
        if (code == EventStart)
        {
            int timeStarted = (int)photonEvent.CustomData;

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(ProgressCanvas);
            }
            // TODO: Start the miner automatic movement
            EconomySystem.Instance.BeginGame();
            // How to synchronize better ?
            Timer.Instance.BeginGame();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    #endregion

    #endregion

    #region Private Methods

    #region Generation Methods

    private void expandOres()
    {
        if (oreDictionary[BlockType.GOLD].Count > 0)
            expandOre(BlockType.GOLD, 15, 12);
        if (oreDictionary[BlockType.COPPER].Count > 0)
            expandOre(BlockType.COPPER, 10, 8);
        if (oreDictionary[BlockType.IRON].Count > 0)
            expandOre(BlockType.IRON, 8, 8);
    }

    private void expandOre(BlockType ore, int rarity, int currentCycle)
    {
        oreDictionary[ore] = shuffleList(oreDictionary[ore]);
        int count = oreDictionary[ore].Count;

        for (int tilePos = 0; tilePos < count; tilePos++)
        {
            Vector2Int position = oreDictionary[ore][0];
            Vector2Int newPosition = new Vector2Int(position.x, position.y);
            
            oreDictionary[ore].Remove(position);

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    newPosition.y = position.y + i;
                    newPosition.x = position.x + j;
                    if (Map[newPosition.x, newPosition.y].type == BlockType.ROCK)
                    {
                        if (Random.Range(0, rarity - currentCycle) != 0)
                            continue;
                        SetAstroTile(ore, newPosition);
                        oreDictionary[ore].Add(new Vector2Int(newPosition.x, newPosition.y));
                        oreDictionary[ore].Add(newPosition);

                        Vector3Int TilePosition = new Vector3Int(newPosition.x + Origin.x, newPosition.y + Origin.y, 0);
                        SetAstroTile(ore, newPosition);
                    }
                }
            }
        }
        if (oreDictionary[ore].Count > 0 && currentCycle > 0)
            expandOre(ore, rarity, currentCycle - 1);
    }

    private void GenerateAsteroid()
    {
        for (int y = -Origin.y -RockRadius; y < -Origin.y + RockRadius; y++)
        {
            for (int x = -Origin.x -RockRadius; x < -Origin.x + RockRadius; x++)
            {
                float power = (Origin.y + y) * (Origin.y + y) + (Origin.x + x) * (Origin.x + x);

                if (power <= DaeguniumRadius - smooth)
                {
                    SetAstroTile(BlockType.DAEGUNIUM, new Vector2Int(x, y));
                }
                else if (power <= DiamondRadius * DiamondRadius - smooth)
                {
                    SetAstroTile(BlockType.DIAMOND, new Vector2Int(x, y));
                }
                else if (power <= RockRadius * RockRadius - smooth)
                {
                    int distanceFromDaegunium = (int)(Mathf.Sqrt(power));

                    if (seedGold(power))
                    {
                        SetAstroTile(BlockType.GOLD, new Vector2Int(x, y));
                        oreDictionary[BlockType.GOLD].Add(new Vector2Int(x, y));
                    }
                    else if (seedCopper(distanceFromDaegunium))
                    {
                        SetAstroTile(BlockType.COPPER, new Vector2Int(x, y));
                        oreDictionary[BlockType.COPPER].Add(new Vector2Int(x, y));
                    }
                    else if (seedIron(distanceFromDaegunium))
                    {
                        SetAstroTile(BlockType.IRON, new Vector2Int(x, y));
                        oreDictionary[BlockType.IRON].Add(new Vector2Int(x, y));
                    }
                    else
                    {
                        SetAstroTile(BlockType.ROCK, new Vector2Int(x, y));
                    }

                }
                else
                {
                    SetAstroTile(BlockType.VOID, new Vector2Int(x, y));
                }
                
            }
        }

    }

    private void SetAstroTile(BlockType type, Vector2Int tileIndex)
    {
        if (type == BlockType.VOID)
        {
            Map[tileIndex.x, tileIndex.y] = VoidType;
            return;
        }

        Vector3Int tilePosition = getTilePosition(tileIndex.x, tileIndex.y);
        
        Map[tileIndex.x, tileIndex.y] = RockType;
        if (type != BlockType.DAEGUNIUM)
        {
            AstroRock.SetTile(tilePosition, RockTile);
            AstroMineable.SetTile(tilePosition, MineableTile);
        }

        switch (type)
        {
            case BlockType.IRON:
                Map[tileIndex.x, tileIndex.y] = IronType;
                AstroOre.SetTile(tilePosition, IronTile);
                break;
            case BlockType.COPPER:
                Map[tileIndex.x, tileIndex.y] = CopperType;
                AstroOre.SetTile(tilePosition, CopperTile);
                break;
            case BlockType.GOLD:
                Map[tileIndex.x, tileIndex.y] = GoldType;
                AstroOre.SetTile(tilePosition, GoldTile);
                break;
            case BlockType.DIAMOND:
                Map[tileIndex.x, tileIndex.y] = DiamondType;
                Map[tileIndex.x, tileIndex.y].isDiscovered = true;
                AstroOre.SetTile(tilePosition, DiamondTile);
                FOG.Instance.SeeTile((Vector3)tilePosition, tileIndex);
                break;
            case BlockType.DAEGUNIUM:
                Map[tileIndex.x, tileIndex.y] = DaeguniumType;
                Map[tileIndex.x, tileIndex.y].isDiscovered = true;
                AstroOre.SetTile(tilePosition, DaeguniumTile);
                FOG.Instance.SeeTile((Vector3)tilePosition, tileIndex);
                break;
        }
        AstroMiniMap.SetTile(tilePosition, MiniMapTile);
    }

    private bool seedGold(float power)
    {
        if (Random.Range(0, (int)(power * 15) - 5000) < 10)
            return true;
        return false;
    }

    private bool seedCopper(int distanceFromDaegunium)
    {
        if (Random.Range(0, distanceFromDaegunium * 300 - 5000) < 10)
            return true;
        return false;
    }

    private bool seedIron(int distanceFromDaegunium)
    {
        if (Random.Range(0, 10000) < 10)
            return true;
        return false;
    }

    #endregion

    private void UpdateBaseFog(Vector3Int playerBasePosition)
    {
        int radius = 30;
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                if (x * x + y * y < Math.Pow(radius, 2))
                {
                    int minusX = 0;
                    int minusY = 0;
                    FOG.Instance.SeeTile(new Vector2(playerBasePosition.x + x - minusX, playerBasePosition.y + y - minusY));
                }
            }        
        }
    }

    
    IEnumerator GenerateAsync()
    {
        GenerateAsteroid();
        expandOres();
        Array.Copy(Map, Print, WorldSize.x * WorldSize.y);
        Pathfinder.Instance.RegenerateMapping();
        PhotonView.RPC("WorldIsGenerated", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        yield return null;
    }

    private void PrintToMap(Vector2Int tileIndex)
    {
        if (Print[tileIndex.x, tileIndex.y].type == BlockType.VOID)
            DestroyMineral(tileIndex);
        else
            SetAstroTile(Print[tileIndex.x, tileIndex.y].type, tileIndex);
    }

    private List<Vector2Int> shuffleList(List<Vector2Int> alpha)
    {
        for (int i = 0; i < alpha.Count; i++)
        {
            Vector2Int temp = alpha[i];
            int randomIndex = Random.Range(i, alpha.Count);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
        return alpha;
    }

    private void updateMineableTiles(Vector2 topLeft, Vector2 botRight, bool select)
    {
        Vector3Int minPos = new Vector3Int(0, 0, 0);
        Vector3Int maxPos = new Vector3Int(0, 0, 0);
        Vector3Int tilePos = new Vector3Int(0, 0, 0);

        minPos.x = topLeft.x < botRight.x ? (int)topLeft.x : (int)botRight.x;
        minPos.y = topLeft.y < botRight.y ? (int)topLeft.y : (int)botRight.y;
        maxPos.x = topLeft.x < botRight.x ? (int)botRight.x : (int)topLeft.x;
        maxPos.y = topLeft.y < botRight.y ? (int)botRight.y : (int)topLeft.y;

        for (tilePos.y = minPos.y; tilePos.y < maxPos.y + 1; tilePos.y++)
        {
            for (tilePos.x = minPos.x; tilePos.x < maxPos.x + 1; tilePos.x++)
            {
                Vector2Int tileIndex = getTileIndex(tilePos.x, tilePos.y);

                if (Map[tileIndex.x, tileIndex.y].type != BlockType.VOID && Map[tileIndex.x, tileIndex.y].type != BlockType.DAEGUNIUM && Map[tileIndex.x, tileIndex.y].isMineable != select)
                {
                    Map[tileIndex.x, tileIndex.y].isMineable = select;
                }
                AstroMineable.RefreshTile(tilePos);
            }
        }
    }

    private void DestroyMineral(Vector2Int tileIndex)
    {
        Map[tileIndex.x, tileIndex.y] = new CustomTile(0, BlockType.VOID, 0);

        if (PhotonNetwork.IsConnected)
            PhotonView.RPC("UpdateTile", RpcTarget.All, Map[tileIndex.x, tileIndex.y], tileIndex.x, tileIndex.y);

        Vector3Int tilePosition = getTilePosition(tileIndex.x, tileIndex.y);
        AstroRock.SetTile(tilePosition, null);
        AstroOre.SetTile(tilePosition, null);
        AstroMineable.SetTile(tilePosition, null);
        AstroMiniMap.SetTile(tilePosition, null);

        AstroRock.RefreshTile(tilePosition);
        AstroOre.RefreshTile(tilePosition);
        AstroMineable.RefreshTile(tilePosition);

        //Update tile when mined
        Pathfinder.Instance.UnblockTileMined(tilePosition);

        Vector3 screenPoint = Camera.main.WorldToViewportPoint(tilePosition);
        if (screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            GetComponent<AudioSource>().Play();
    }
    
    #endregion
}
