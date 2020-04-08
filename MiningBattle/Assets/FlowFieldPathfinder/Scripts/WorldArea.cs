using System.Collections.Generic;
using UnityEngine;

namespace FlowPathfinding
{
    public class WorldArea
    {
        #region PrivateVariables

        private WorldData _worldData;

        #endregion

        #region PublicVariables

        public int Index;

        // left and top tile offset from worldStart. for example: our left is 5 tiles away from worldStart.x  or  our top is 8 tiles away from worldStart.z
        public int LeftOffset;
        public int TopOffset;
        public int GridWidth;
        public int GridLength;
        public int AnglePositive = 1;

        public int[][] LevelDimensions; // per level of abstraction we store sector width/height/grid width/grid height

        // lookUpLowerSectors[sectorLevel-1][hih level sectorID]
        // get sectors on a lower layer of abstraction as a 2D array, that are inside the higher level sector id
        public int[][][][] LookUpLowerSectors;

        public float Angle;
        public float AngledAreaResolutionDifference = 1;

        public bool FlatArea;
        public bool AngleDirectionX;

        public bool[][] SearchField; // matches tile grid decides what tiles to expand during flow field generation

        public Vector3 Origin;

        public readonly List<int> WorldAreasConnectedIndexes = new List<int>();

        // tileSectorNodeConnections[sector level][Tile]
        // get list of abstractNodes on this position
        public Dictionary<Tile, List<AbstractNode>>[] TileSectorNodeConnections;

        // worldAreaTileConnections[IntVector2(area index, sector index)]
        // list of tile positions V3.x, V3.y : V3.z is the index of the worldArea its in (never our own)
        public Dictionary<IntVector2, List<IntVector3>> WorldAreaTileConnections;

        //groupsInSector[intVector2(connectedWorldAreaIndex, currentTile.sectorIndex)]
        // list of groups of tiles per sector
        public readonly Dictionary<IntVector2, List<List<IntVector2>>> GroupsInSectors =
            new Dictionary<IntVector2, List<List<IntVector2>>>();


        public Tile[][] TileGrid;

        public MultiLevelSector[][] SectorGrid = null;

        public enum Side
        {
            Top = 0,
            Down = 1,
            Left = 2,
            Right = 3
        }

        #endregion

        #region PrivateMethods

        private void SetValues(IntVector2 offSet, int width, int length, int yLayer, WorldData worldData)
        {
            IntVectorComparer comparer = new IntVectorComparer();
            WorldAreaTileConnections = new Dictionary<IntVector2, List<IntVector3>>(comparer);
            _worldData = worldData;
            LeftOffset = offSet.X;
            TopOffset = offSet.Y;

            Origin = _worldData.TileManager.GetWorldPosition(LeftOffset, TopOffset);
            Origin.y = _worldData.Pathfinder.worldStart.y + yLayer * _worldData.Pathfinder.characterHeight + _worldData.Pathfinder.characterHeight * 0.5f;

            GridWidth = width;
            GridLength = length;

            SearchField = new bool[GridWidth][];
            TileGrid = new Tile[GridWidth][];
            for (int x = 0; x < GridWidth; x++)
            {
                TileGrid[x] = new Tile[GridLength];
                SearchField[x] = new bool[GridLength];
            }
        }

        private void SetValues(int left, int right, int top, int bot, int yLayer, WorldData worldData)
        {
            IntVectorComparer comparer = new IntVectorComparer();
            WorldAreaTileConnections = new Dictionary<IntVector2, List<IntVector3>>(comparer);
            _worldData = worldData;
            LeftOffset = left;
            TopOffset = top;

            Origin = _worldData.TileManager.GetWorldPosition(LeftOffset, TopOffset);
            Origin.y = _worldData.Pathfinder.worldStart.y + yLayer * _worldData.Pathfinder.characterHeight +
                       _worldData.Pathfinder.characterHeight * 0.5f;

            GridWidth = 1 + right - LeftOffset;
            GridLength = 1 + bot - TopOffset;

            SearchField = new bool[GridWidth][];
            TileGrid = new Tile[GridWidth][];
            for (int x = 0; x < GridWidth; x++)
            {
                TileGrid[x] = new Tile[GridLength];
                SearchField[x] = new bool[GridLength];
            }
        }

        #endregion

        #region PublicMethods

        public void SlopedAreaCopyTiles(float angle, int left, int right, int top, int bot,
            List<List<IntVector2>> tileList, List<TemporaryWorldArea> tempWorldAreas, int yLayer, WorldData worldData)
        {
            Angle = angle;
            SetValues(left, right, top, bot, yLayer, worldData);
            _worldData.MultiLevelSectorManager.SetupSectorsWorldArea(this);

            Tile highestYTile = null;
            Tile lowestYTile = null;

            int i = 0;
            foreach (List<IntVector2> vec2List in tileList)
            {
                Tile[][] grid = tempWorldAreas[yLayer + i].TileGrid;
                foreach (IntVector2 vec2 in vec2List)
                {
                    int x = vec2.X - LeftOffset;
                    int y = vec2.Y - TopOffset;

                    TileGrid[x][y] = grid[vec2.X][vec2.Y];
                    TileGrid[x][y].GridPos = new IntVector2(x, y);
                    TileGrid[x][y].IntegrationValue = TileManager.TileResetIntegrationValue;
                    TileGrid[x][y].WorldAreaIndex = Index;

                    int sectorX = Mathf.FloorToInt(x / (float) LevelDimensions[0][0]); // sectorWidth
                    int sectorY = Mathf.FloorToInt(y / (float) LevelDimensions[0][1]); // sectorHeight

                    if (_worldData.Pathfinder.maxLevelAmount == 0)
                        TileGrid[x][y].SectorIndex = 0;
                    else
                        TileGrid[x][y].SectorIndex = (sectorY * LevelDimensions[0][2]) + sectorX; // *sectorGridWidth

                    MultiLevelSector sector = SectorGrid[0][TileGrid[x][y].SectorIndex];
                    int deltaX = x - sector.Left;
                    int deltaY = y - sector.Top;
                    TileGrid[x][y].IndexWithinSector = (deltaY * sector.TilesInWidth) + deltaX;

                    if (lowestYTile == null || TileGrid[vec2.X - LeftOffset][vec2.Y - TopOffset].YWorldPos <
                        lowestYTile.YWorldPos)
                        lowestYTile = TileGrid[vec2.X - LeftOffset][vec2.Y - TopOffset];

                    if (highestYTile == null || TileGrid[vec2.X - LeftOffset][vec2.Y - TopOffset].YWorldPos >
                        highestYTile.YWorldPos)
                        highestYTile = TileGrid[vec2.X - LeftOffset][vec2.Y - TopOffset];
                }

                i++;
            }

            if (highestYTile != null)
            {
                Origin.y = highestYTile.YWorldPos - ((highestYTile.YWorldPos - lowestYTile.YWorldPos) * 0.5f);

                // X or Z  direction slope
                Vector3 rayStartingPoint =
                    new Vector3(0, lowestYTile.YWorldPos + _worldData.Pathfinder.characterHeight * 0.4f, 0);
                float tileOffset = (_worldData.Pathfinder.tileSize * 0.5f);

                rayStartingPoint.x = _worldData.Pathfinder.worldStart.x +
                                     ((lowestYTile.GridPos.X + LeftOffset) * _worldData.Pathfinder.tileSize) +
                                     tileOffset;
                rayStartingPoint.z = _worldData.Pathfinder.worldStart.z -
                                     ((lowestYTile.GridPos.Y + TopOffset) * _worldData.Pathfinder.tileSize) -
                                     tileOffset;

                if (Physics.Raycast(rayStartingPoint, Vector3.down, out var hit,
                    _worldData.Pathfinder.characterHeight * 0.6f,
                    1 << _worldData.Pathfinder.groundLayer))
                {
                    if (Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.z))
                    {
                        AngleDirectionX = true;

                        if (hit.normal.x > 0)
                            AnglePositive = -1;
                        else
                            AnglePositive = 1;
                    }
                    else
                    {
                        AngleDirectionX = false;
                        if (hit.normal.z > 0)
                            AnglePositive = 1;
                        else
                            AnglePositive = -1;
                    }
                }

                float length; //highestYTile.yWorldPos - lowestYTile.yWorldPos
                if (AngleDirectionX)
                    length = (GridWidth - 1) * _worldData.Pathfinder.tileSize;
                else
                    length = (GridLength - 1) * _worldData.Pathfinder.tileSize;

                if (length != 0)
                {
                    AngledAreaResolutionDifference =
                        Vector3.Distance(_worldData.TileManager.GetTileWorldPosition(lowestYTile, this),
                            _worldData.TileManager.GetTileWorldPosition(highestYTile, this)) / length;
                    //Debug.Log("angledAreaResolutionDifference  " + angledAreaResolutionDifference);
                }
            }
        }

        public void FlatAreaCopyTiles(IntVector2 offset, Tile[][] tileGrid, int yLayer, WorldData worldData)
        {
            FlatArea = true;
            SetValues(offset, tileGrid.Length, tileGrid[0].Length, yLayer, worldData);
            _worldData.MultiLevelSectorManager.SetupSectorsWorldArea(this);
            bool firstTile = true;

            for (int x = 0; x < tileGrid.Length; x++)
            {
                for (int y = 0; y < tileGrid[0].Length; y++)
                {
                    if (tileGrid[x][y] == null)
                        continue;
                    TileGrid[x][y] = tileGrid[x][y];

                    // manually
                    TileGrid[x][y].YWorldPos = worldData.Pathfinder.worldStart.y; // 0f; // forced y = 0.  2D worldAreas
                    _worldData.LayerWorldAreaIndexes[yLayer][x + LeftOffset][y + TopOffset] = Index; // set world Index

                    TileGrid[x][y].GridPos = new IntVector2(x, y);
                    TileGrid[x][y].IntegrationValue = TileManager.TileResetIntegrationValue;
                    TileGrid[x][y].WorldAreaIndex = Index;

                    int sectorX = Mathf.FloorToInt(x / (float) LevelDimensions[0][0]); // sectorWidth
                    int sectorY = Mathf.FloorToInt(y / (float) LevelDimensions[0][1]); // sectorHeight

                    if (_worldData.Pathfinder.maxLevelAmount == 0)
                        TileGrid[x][y].SectorIndex = 0;
                    else
                        TileGrid[x][y].SectorIndex = (sectorY * LevelDimensions[0][2]) + sectorX;

                    MultiLevelSector sector = SectorGrid[0][TileGrid[x][y].SectorIndex];
                    int deltaX = x - sector.Left;
                    int deltaY = y - sector.Top;
                    TileGrid[x][y].IndexWithinSector = (deltaY * sector.TilesInWidth) + deltaX;

                    if (firstTile)
                    {
                        Origin.y = TileGrid[x][y].YWorldPos;
                        firstTile = false;
                    }
                }
            }
        }

        public void FlatAreaCopyTiles(int left, int right, int top, int bot, List<IntVector2> tileList, Tile[][] grid,
            int yLayer, WorldData worldData)
        {
            FlatArea = true;
            SetValues(left, right, top, bot, yLayer, worldData);
            _worldData.MultiLevelSectorManager.SetupSectorsWorldArea(this);
            bool firstTile = true;

            foreach (IntVector2 vec2 in tileList)
            {
                int x = vec2.X - LeftOffset;
                int y = vec2.Y - TopOffset;

                TileGrid[x][y] = grid[vec2.X][vec2.Y];
                TileGrid[x][y].GridPos = new IntVector2(x, y);
                TileGrid[x][y].IntegrationValue = TileManager.TileResetIntegrationValue;
                TileGrid[x][y].WorldAreaIndex = Index;

                int sectorX = Mathf.FloorToInt(x / (float) LevelDimensions[0][0]); // sectorWidth
                int sectorY = Mathf.FloorToInt(y / (float) LevelDimensions[0][1]); // sectorHeight

                if (_worldData.Pathfinder.maxLevelAmount == 0)
                    TileGrid[x][y].SectorIndex = 0;
                else
                    TileGrid[x][y].SectorIndex = (sectorY * LevelDimensions[0][2]) + sectorX;

                MultiLevelSector sector = SectorGrid[0][TileGrid[x][y].SectorIndex];
                int deltaX = x - sector.Left;
                int deltaY = y - sector.Top;
                TileGrid[x][y].IndexWithinSector = (deltaY * sector.TilesInWidth) + deltaX;

                if (firstTile)
                {
                    Origin.y = TileGrid[x][y].YWorldPos;

                    firstTile = false;
                }
            }
        }

        public void SetValuesFlatWorld(int left, int right, int top, int bot, WorldData worldData)
        {
            _worldData = worldData;
            LeftOffset = left;
            TopOffset = top;

            Origin = _worldData.TileManager.GetWorldPosition(LeftOffset, TopOffset);

            GridWidth = 1 + right - LeftOffset;
            GridLength = 1 + bot - TopOffset;

            SearchField = new bool[GridWidth][];
            TileGrid = new Tile[GridWidth][];
            for (int x = 0; x < GridWidth; x++)
            {
                TileGrid[x] = new Tile[GridLength];
                SearchField[x] = new bool[GridLength];
            }
        }

        public Tile GetTileFromGrid(int x, int y)
        {
            if (x < 0 || y < 0 || x > GridWidth - 1 || y > GridLength - 1)
            {
                Debug.Log("sampling outside of grid range");
                return null;
            }

            return TileGrid[x][y];
        }

        #endregion
    }
}