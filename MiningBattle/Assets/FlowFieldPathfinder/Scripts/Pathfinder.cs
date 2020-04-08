using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlowPathfinding
{
    public class Pathfinder : MonoBehaviour
    {
        #region PrivateVariables

        private delegate void RemappingCompleted();

        private bool _aJobIsRunning;
        private bool _regenerating;
        private bool _isCameraNotNull;

        private readonly List<ThreadedJob> _pathJobs = new List<ThreadedJob>();
        private readonly List<FlowFieldPath> _drawQue = new List<FlowFieldPath>();
        private readonly List<IntVector3> _pathAdjustmentsInProgress = new List<IntVector3>();

        private List<Tile[][]> _tileGrids;
        private readonly List<IntVector2> _tileGridsOffset = new List<IntVector2>();

        private Camera _camera;

        private GameObject _flowFieldHolder;
        private GameObject _integrationFieldHolder;

        private RemappingCompleted _onRemappingCompleted;

        private readonly Dictionary<int, List<Seeker>> _charactersInFlowPath = new Dictionary<int, List<Seeker>>();

        #endregion

        #region PublicVariables

        public int drawSectorLevel;
        public int maxCostValue = 20;
        public int brushSize = 3;
        public int brushStrength = 4;
        public int brushFallOff = 1;
        public int sectorSize = 10;
        public int levelScaling = 3; // how many sectors fit in level above (3x3)
        public int maxLevelAmount = 2; // amount of levels of abstraction
        public int obstacleLayer;
        public int groundLayer;

        public bool drawSectorNetwork;
        public bool drawSectors;
        public bool drawTiles = true;
        public bool drawTree;
        public bool worldIsMultiLayered;
        public bool twoDimensionalMode;
        public bool showIntegrationField;
        public bool showFlowField;

        public float worldWidth = 10;
        public float worldLength = 10;
        public float tileSize = 1;
        public float characterHeight = 2;
        public float worldHeight = 10;
        public float generationClimbHeight = 0.8f;
        public float invalidYValue;

        public Vector3 worldStart;

        public WorldData worldData = new WorldData(null);

        public SeekerMovementManager seekerManager;

        public CostFieldManager costManager;

        public static Pathfinder Instance;

        #endregion

        #region PrivateMethods

        private void Update()
        {
            if (!_aJobIsRunning)
            {
                if (worldData.WorldManager.TilesBlockedAdjusted.Count > 0 ||
                    worldData.WorldManager.TilesCostAdjusted.Count > 0)
                    worldData.WorldManager.InputChanges();

                if (_pathJobs.Count > 0)
                {
                    _aJobIsRunning = true;

                    _pathJobs[0].Start();
                }
            }
            else
            {
                if (_regenerating)
                    return;

                if (_pathJobs.Count > 0 && _pathJobs[0].Update())
                {
                    // job is done
                    _aJobIsRunning = false;

                    _pathJobs[0] = null;
                    _pathJobs.Remove(_pathJobs[0]);
                }
            }

            if (_drawQue.Count > 0)
            {
                foreach (var t in _drawQue)
                {
                    DrawFlowField(t.FlowField);

                    DrawIntegrationField(t.IntegrationField);
                }

                _drawQue.Clear();
            }
        }

        private void LateUpdate()
        {
            if (_regenerating && !_aJobIsRunning)
            {
                _regenerating = false;
                GenerateMapping();
            }
        }

        private void GenerateMapping()
        {
            RemapWorldSetup(false);

            _onRemappingCompleted?.Invoke();
        }

        private void RemapWorldSetup(bool automatic)
        {
            if (automatic)
                GenerateWorld(true, true);
            else if (World.Instance)
                GenerateWorldMiningBattle();

            seekerManager = GetComponent<SeekerMovementManager>();
            seekerManager.Setup(this, worldData);

            foreach (Transform field in transform.GetChild(0).transform)
                field.gameObject.SetActive(false);
        }

        private void GenerateWorldMiningBattle()
        {
            int nbAreas = 1;

            int gridWidth = (int) worldWidth / 1;
            int gridHeight = (int) worldHeight / 1;

            _tileGrids = new List<Tile[][]>();

            for (int i = 0; i < nbAreas; i++)
            {
                Tile[][] tileGrid = new Tile[gridWidth][];

                for (int j = 0; j < gridWidth; j++)
                    tileGrid[j] = new Tile[gridHeight];

                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                        tileGrid[x][y] = new Tile {GridPos = new IntVector2(x, y)};
                }

                _tileGrids.Add(tileGrid);
            }

            GenerateWorldManually(_tileGrids, _tileGridsOffset, true);

            worldData.WorldBuilder.ConnectWorldAreas();

            BlockAsteroid(gridWidth * 1, gridHeight * 1);
            BlockSpawnBases();
        }

        private void BlockAsteroid(int gridWidth, int gridHeight)
        {
            Vector3 offset = new Vector3(0.5f, 0.5f, 0);
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3Int pos = World.Instance.getTilePosition(x, y);
                    World.BlockType type = World.Instance.GetBlockType((Vector3) pos);
                    if (type != World.BlockType.VOID && type != World.BlockType.DAEGUNIUM)
                    {
                        Tile tile = worldData.TileManager.GetTileFromPosition(pos + offset);
                        worldData.WorldManager.BlockTile(tile);
                    }
                }
            }
        }

        private void BlockSpawnBases()
        {
            var spawnPositions = World.Instance.GetSpawnPositions();
            foreach (var spawnPosition in spawnPositions)
            {
                for (int x = -4; x < 4; x++)
                {
                    for (int y = -4; y < 4; y++)
                    {
                        Vector2 pos = new Vector2 {x = spawnPosition.x + x + 0.5f, y = spawnPosition.y + y + 0.5f};
                        Tile tile = worldData.TileManager.GetTileFromPosition(pos);
                        worldData.WorldManager.BlockTile(tile);
                    }
                }
            }
        }

        public void BlockSpawnBases(Vector2 position)
        {
            for (int x = -4; x < 4; x++)
            {
                for (int y = -4; y < 4; y++)
                {
                    Vector2 pos = new Vector2 { x = position.x + x + 0.5f, y = position.y + y + 0.5f };
                    Tile tile = worldData.TileManager.GetTileFromPosition(pos);
                    worldData.WorldManager.BlockTile(tile);
                }
            }
        }

        private void GenerateWorldManually(List<Tile[][]> tileGrids, List<IntVector2> tileGridOffset,
            bool autoConnectWorldAreas)
        {
            worldData.GenerateWorldManually(this, tileGrids, tileGridOffset, autoConnectWorldAreas);
            worldData.Setup();
        }

        private void OnDrawGizmos()
        {
            // draw world bounding box
            Gizmos.color = Color.blue;

            var matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(-90, 0, 0), new Vector3(1, 1, 1));
            Gizmos.matrix = matrix;

            Gizmos.DrawCube(worldStart, Vector3.zero);

            //Bottom
            Gizmos.DrawLine(worldStart, worldStart + new Vector3(worldWidth, 0, 0));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, 0, 0),
                worldStart + new Vector3(worldWidth, 0, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, 0, -worldLength),
                worldStart + new Vector3(0, 0, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(0, 0, -worldLength), worldStart);

            //Top 
            Gizmos.DrawLine(worldStart + new Vector3(0, worldHeight, 0),
                worldStart + new Vector3(worldWidth, worldHeight, 0));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, worldHeight, 0),
                worldStart + new Vector3(worldWidth, worldHeight, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, worldHeight, -worldLength),
                worldStart + new Vector3(0, worldHeight, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(0, worldHeight, -worldLength),
                worldStart + new Vector3(0, worldHeight, 0));

            //Sides
            Gizmos.DrawLine(worldStart, worldStart + new Vector3(0, worldHeight, 0));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, 0, 0),
                worldStart + new Vector3(worldWidth, worldHeight, 0));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, 0, -worldLength),
                worldStart + new Vector3(worldWidth, worldHeight, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(0, 0, -worldLength),
                worldStart + new Vector3(0, worldHeight, -worldLength));

            if (worldData.Pathfinder != null)
                worldData.DrawGizmos();
        }

        private void DrawFlowField(FlowField flowField)
        {
            if (_flowFieldHolder != null)
                Destroy(_flowFieldHolder);

            GameObject arrow = Resources.Load("Prefab/FlowArrow") as GameObject;

            if (worldData.Pathfinder.showFlowField)
            {
                _flowFieldHolder = new GameObject();
                for (int x = 0; x < worldData.WorldAreas.Count; x++)
                {
                    for (int i = 0; i < worldData.WorldAreas[x].SectorGrid[0].Length; i++)
                    {
                        if (flowField.Field.ContainsKey(new IntVector2(x, i)))
                        {
                            MultiLevelSector sector = worldData.WorldAreas[x].SectorGrid[0][i];
                            Vector2 sectorPos = new Vector2(sector.Left, sector.Top);

                            for (int j = 0; j < sector.TilesInWidth * sector.TilesInHeight; j++)
                            {
                                int y = Mathf.FloorToInt((float) j / sector.TilesInWidth);

                                Vector2 node = sectorPos + new Vector2(j - (sector.TilesInWidth * y), y);

                                if (worldData.WorldAreas[x].TileGrid[(int) node.x][(int) node.y] != null)
                                {
                                    GameObject b = Instantiate(arrow,
                                        worldData.TileManager.GetTileWorldPosition(
                                            worldData.WorldAreas[x].TileGrid[(int) node.x][(int) node.y],
                                            worldData.WorldAreas[x]) + new Vector3(0, 0.2f, 0), Quaternion.identity);

                                    Vector2 flow =
                                        worldData.FlowFieldManager.DirToVector(
                                            flowField.Field[new IntVector2(x, i)][j]);
                                    b.transform.LookAt(b.transform.position + new Vector3(flow.x, 0, flow.y));

                                    b.transform.parent = _flowFieldHolder.transform;
                                    b.transform.localScale = new Vector3(worldData.Pathfinder.tileSize,
                                        worldData.Pathfinder.tileSize, worldData.Pathfinder.tileSize);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawIntegrationField(IntegrationField integrationField)
        {
            if (_integrationFieldHolder != null)
                Destroy(_integrationFieldHolder);

            GameObject integrationTile = Resources.Load("Prefab/IntegrationTile") as GameObject;

            if (worldData.Pathfinder.showIntegrationField)
            {
                _integrationFieldHolder = new GameObject();
                for (int x = 0; x < worldData.WorldAreas.Count; x++)
                {
                    for (int i = 0; i < worldData.WorldAreas[x].SectorGrid[0].Length; i++)
                    {
                        if (integrationField.Field.ContainsKey(new IntVector2(x, i)))
                        {
                            MultiLevelSector sector = worldData.WorldAreas[x].SectorGrid[0][i];
                            Vector2 sectorPos = new Vector2(sector.Left, sector.Top);

                            for (int j = 0; j < sector.TilesInWidth * sector.TilesInHeight; j++)
                            {
                                int y = Mathf.FloorToInt((float) j / sector.TilesInWidth);

                                Vector2 node = sectorPos + new Vector2(j - (sector.TilesInWidth * y), y);

                                if (worldData.WorldAreas[x].TileGrid[(int) node.x][(int) node.y] != null)
                                {
                                    GameObject b = Instantiate(integrationTile,
                                        worldData.TileManager.GetTileWorldPosition(
                                            worldData.WorldAreas[x].TileGrid[(int) node.x][(int) node.y],
                                            worldData.WorldAreas[x]), Quaternion.identity);

                                    int value = integrationField.Field[new IntVector2(x, i)][j];

                                    if (value * 3 >= worldData.ColorLists.PathCostColors.Length - 2)
                                        b.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color =
                                            worldData.ColorLists.PathCostColors[
                                                worldData.ColorLists.PathCostColors.Length - 2];
                                    else
                                    {
                                        if (value < worldData.ColorLists.PathCostColors.Length - 2)
                                            b.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material
                                                .color = worldData.ColorLists.PathCostColors[value * 3];
                                    }

                                    b.transform.position += Vector3.up * 0.15f;
                                    b.transform.parent = _integrationFieldHolder.transform;
                                    b.transform.localScale = new Vector3(worldData.Pathfinder.tileSize,
                                        worldData.Pathfinder.tileSize, worldData.Pathfinder.tileSize);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SearchPath(Tile destinationNode, List<Seeker> units)
        {
            _pathJobs.Add(new SearchPathJob(destinationNode, units, this));
        }

        // send to corresponding units
        private void SendOutPath(int key, FlowFieldPath path, bool edit)
        {
            foreach (Seeker seeker in _charactersInFlowPath[key])
                seeker.SetFlowField(path, edit);
        }

        #endregion

        #region PublicMethods

        public void Awake()
        {
            _camera = Camera.main;
            _isCameraNotNull = _camera != null;

            Instance = this;

            _tileGridsOffset.Add(new IntVector2(0, 0));

            //GenerateMapping();
        }

        public void Start()
        {
            invalidYValue = worldStart.y + worldHeight + 1;
            worldData.Pathfinder = this;
        }

        public void GenerateWorld(bool generateWhileInPlayMode, bool loadCostField)
        {
            if (generateWhileInPlayMode)
            {
                GetComponent<SaveLoad>().LoadLevel();
                worldData.GenerateWorld(this, true, loadCostField);
                worldData.Setup();
            }
            else
            {
                if (loadCostField)
                {
                    GetComponent<SaveLoad>().LoadLevel();
                    worldData.GenerateWorld(this, false, true);
                }
                else
                {
                    worldData.CostFields.Clear();
                    worldData.GenerateWorld(this, false, false);
                }
            }
        }

        public void FindPath(Tile destinationNode, List<Seeker> units)
        {
            if (destinationNode != null)
            {
                SearchPath(destinationNode, units);
            }
        }

        public void RegenerateMapping()
        {
            _regenerating = true;
        }

        public void UnblockTileMined(Vector3 tilePosition)
        {
            tilePosition.y += 1;
            Tile tile = worldData.TileManager.GetTileFromPosition(tilePosition);

            worldData.WorldManager.UnBlockTile(tile);
        }

        public Vector3 GetMousePosition()
        {
            if (_isCameraNotNull)
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                var worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);

                var hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                if (hit.collider)
                {
                    return hit.point;
                }
            }

            return new Vector3(0, invalidYValue, 0);
        }

        public void AddSectorInPath(WorldArea area, Tile tile, FlowFieldPath path)
        {
            _pathAdjustmentsInProgress.Add(new IntVector3(path.Key, area.Index, tile.SectorIndex));
            _pathJobs.Add(new AddToPathJob(area, tile, path, this));
            //_Jobs.Add(new AddToPathJob(area, tile, path, this), true);
        }

        public bool WorkingOnPathAdjustment(int pathIndex, int areaIndex, int tileSector)
        {
            if (_pathAdjustmentsInProgress.Contains(new IntVector3(pathIndex, areaIndex, tileSector)))
                return true;
            return false;
        }

        // path finished, send it to the right characters, and draw it 
        public void PathCreated(FlowFieldPath path, int key, bool edit)
        {
            if (path != null)
            {
                SendOutPath(key, path, edit);
                _drawQue.Add(path);
            }
        }

        // Add seeker to already calculated flowPath, for example, there is 1 flow field path for all characters in a tower defence game
        // and you want to add characters over time. 
        public void AddSeekerToExistingFlowField(int flowFieldPathKey, Seeker seeker)
        {
            _charactersInFlowPath[flowFieldPathKey].Add(seeker);
            seeker.SetFlowField(worldData.FlowFieldManager.FlowFieldPaths[flowFieldPathKey], false);
        }

        public void KeepTrackOfUnitsInPaths(List<Seeker> units)
        {
            Dictionary<int, FlowFieldPath> checkedKeys = new Dictionary<int, FlowFieldPath>();

            foreach (Seeker seeker in units)
            {
                if (seeker.flowFieldPath != null)
                {
                    if (_charactersInFlowPath.ContainsKey(seeker.flowFieldPath.Key))
                    {
                        _charactersInFlowPath[seeker.flowFieldPath.Key].Remove(seeker);

                        if (!checkedKeys.ContainsKey(seeker.flowFieldPath.Key))
                            checkedKeys.Add(seeker.flowFieldPath.Key, seeker.flowFieldPath);
                    }
                }
            }

            foreach (int key in checkedKeys.Keys)
            {
                if (_charactersInFlowPath[key].Count == 0)
                {
                    _charactersInFlowPath.Remove(key);
                    worldData.FlowFieldManager.FlowFieldPaths.Remove(checkedKeys[key]);
                }
            }
        }

        public int GenerateKey(List<Seeker> units)
        {
            int i = 0;
            while (_charactersInFlowPath.ContainsKey(i))
                i++;

            _charactersInFlowPath.Add(i, units);
            return i;
        }

        public void PathAdjusted(FlowFieldPath path, WorldArea area, Tile tile)
        {
            _pathAdjustmentsInProgress.Remove(new IntVector3(path.Key, area.Index, tile.SectorIndex));

            DrawFlowField(path.FlowField);

            DrawIntegrationField(path.IntegrationField);
        }

        public void AddToPath(WorldArea area, Tile startingPoint, FlowFieldPath path)
        {
            Dictionary<IntVector2, Tile> startingPoints = new Dictionary<IntVector2, Tile>();
            IntVector2 pointKey = new IntVector2(area.Index, startingPoint.SectorIndex);
            startingPoints.Add(pointKey, startingPoint);

            List<List<int>> areasAndSectorsPath = worldData.HierarchicalPathfinder.FindPaths(startingPoints,
                path.Destination, worldData.WorldAreas[path.Destination.WorldAreaIndex]);

            
            worldData.IntegrationFieldManager.StartIntegrationFieldCreation(path.Destination, areasAndSectorsPath,
                path, null, path.Key, true);
        }

        public void WorldHasBeenChanged(List<IntVector2> changedAreasAndSectors)
        {
            List<FlowFieldPath> changedPaths = new List<FlowFieldPath>();

            foreach (var t1 in worldData.FlowFieldManager.FlowFieldPaths)
            {
                foreach (var t in changedAreasAndSectors)
                {
                    if (t1.FlowField.Field.ContainsKey(t))
                    {
                        if (!changedPaths.Contains(t1))
                            changedPaths.Add(t1);
                        break;
                    }
                }
            }

            foreach (var t in changedPaths)
                FindPath(t.Destination, _charactersInFlowPath[t.Key]);
        }

        #endregion
    }
}