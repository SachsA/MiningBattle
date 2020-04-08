using System.Collections.Generic;
using UnityEngine;

namespace FlowPathfinding
{
    public class MultiLevelSectorManager
    {
        #region PublicVariables

        public readonly int MaxGateSize = 500; // in reality its always 1 bigger (maxGateSize + 1)

        public readonly LowLevelSectorManager LowLevel = new LowLevelSectorManager();
        
        public HighLevelSectorManager HighLevel = new HighLevelSectorManager();

        public WorldData WorldData;

        #endregion

        #region PrivateMethods

        // create look up table to easily find all the sectors within,  a sector on a higher level
        private void FillInLookUpLowerSectors(WorldArea worldArea)
        {
            for (int i = 0; i < worldArea.LookUpLowerSectors.Length; i++)
            {
                int level = i + 1;

                foreach (MultiLevelSector sector in worldArea.SectorGrid[level])
                {
                    int lowerLevelX = sector.GridX * WorldData.Pathfinder.levelScaling;
                    int lowerLevelY = sector.GridY * WorldData.Pathfinder.levelScaling;

                    // get lower sector in the top left corner
                    int lowerIndex = (lowerLevelY * worldArea.LevelDimensions[level - 1][2]) + lowerLevelX;

                    int width = GetSectorGridWidthAtLevel(worldArea, sector.Level - 1) - lowerLevelX;
                    int arrayWidth = Mathf.Min(width, WorldData.Pathfinder.levelScaling);
                    worldArea.LookUpLowerSectors[level - 1][sector.Id] = new int[arrayWidth][];

                    int height = GetSectorGridHeightAtLevel(worldArea, sector.Level - 1) - lowerLevelY;
                    int arrayHeight = Mathf.Min(height, WorldData.Pathfinder.levelScaling);
                    for (int j = 0; j < worldArea.LookUpLowerSectors[level - 1][sector.Id].Length; j++)
                        worldArea.LookUpLowerSectors[level - 1][sector.Id][j] = new int[arrayHeight];

                    // get surrounding sectors
                    for (int x = 0; x < arrayWidth; x++)
                    {
                        for (int y = 0; y < arrayHeight; y++)
                            worldArea.LookUpLowerSectors[level - 1][sector.Id][x][y] =
                                lowerIndex + x + (y * GetSectorGridWidthAtLevel(worldArea, sector.Level - 1));
                    }
                }
            }
        }

        private AbstractNode CreateAbstractNodeOnSectorEdge(MultiLevelSector sector, int edgeIndex, Tile tile,
            WorldArea area)
        {
            AbstractNode sectorNode = CreateAbstractNodeInSector(sector, tile, area);
            sector.SectorNodesOnEdge[edgeIndex].Add(sectorNode);

            return sectorNode;
        }

        private Vector2 DirectionBetweenSectorsVector2(MultiLevelSector a, MultiLevelSector b)
        {
            int deltaX = b.Left - a.Left;
            int deltaY = b.Top - a.Top;

            if (deltaX > 1)
                deltaX = 1;

            if (deltaX < -1)
                deltaX = -1;

            if (deltaY > 1)
                deltaY = 1;

            if (deltaY < -1)
                deltaY = -1;

            return new Vector2(deltaX, deltaY);
        }

        private List<Tile> GetNodesOnSectorRowNoBlocked(Vector2 start, Vector2 direction, WorldArea area)
        {
            List<Tile> returnList = new List<Tile>();

            for (int i = 0; i < GetSectorWidthAtLevel(area, 0); i++)
            {
                var x = (int) start.x + (int) direction.x * i;
                var y = (int) start.y + (int) direction.y * i;

                if (x > -1 && x < area.GridWidth && y > -1 && y < area.GridLength)
                {
                    Tile tile = area.TileGrid[x][y];
                    if (tile != null && !tile.Blocked)
                        returnList.Add(tile);
                }
            }

            return returnList;
        }

        private List<MultiLevelSector> GetNeighboursWithinWorldArea(MultiLevelSector sector, WorldArea area)
        {
            List<MultiLevelSector> neighbours = new List<MultiLevelSector>();

            int sectorsGridHeight = GetSectorGridHeightAtLevel(area, sector.Level);
            int sectorsGridWidth = GetSectorGridWidthAtLevel(area, sector.Level);

            int checkX = sector.GridX + 1;
            int checkY = sector.GridY;
            if (checkX >= 0 && checkX < sectorsGridWidth)
                neighbours.Add(area.SectorGrid[sector.Level][(checkY * sectorsGridWidth) + checkX]);

            checkX = sector.GridX - 1;
            checkY = sector.GridY;
            if (checkX >= 0 && checkX < sectorsGridWidth)
                neighbours.Add(area.SectorGrid[sector.Level][(checkY * sectorsGridWidth) + checkX]);

            checkX = sector.GridX;
            checkY = sector.GridY - 1;
            if (checkY >= 0 && checkY < sectorsGridHeight)
                neighbours.Add(area.SectorGrid[sector.Level][(checkY * sectorsGridWidth) + checkX]);

            checkX = sector.GridX;
            checkY = sector.GridY + 1;
            if (checkY >= 0 && checkY < sectorsGridHeight)
                neighbours.Add(area.SectorGrid[sector.Level][(checkY * sectorsGridWidth) + checkX]);

            return neighbours;
        }

        #endregion

        #region PublicMethods

        public void SetupSectorsWorldArea(WorldArea worldArea)
        {
            int sectorWidth = WorldData.Pathfinder.sectorSize;
            int sectorHeight = WorldData.Pathfinder.sectorSize;
            if (WorldData.Pathfinder.maxLevelAmount == 0)
            {
                sectorWidth = worldArea.GridWidth;
                sectorHeight = worldArea.GridLength;
                worldArea.LevelDimensions = new int[1][];
                worldArea.LevelDimensions[0] = new int[4];
                worldArea.LevelDimensions[0][0] = sectorWidth;
                worldArea.LevelDimensions[0][1] = sectorHeight;
                worldArea.LevelDimensions[0][2] = 1;
                worldArea.LevelDimensions[0][3] = 1;
                worldArea.SectorGrid = new MultiLevelSector[1][];
                worldArea.SectorGrid[0] =
                    new MultiLevelSector[worldArea.LevelDimensions[0][2] * worldArea.LevelDimensions[0][3]];

                int j = 0;
                int i = 0;
                int level = 0;
                int index = 0;
                worldArea.TileSectorNodeConnections = new Dictionary<Tile, List<AbstractNode>>[1];
                worldArea.TileSectorNodeConnections[level] = new Dictionary<Tile, List<AbstractNode>>();
                worldArea.SectorGrid[level][index] = new MultiLevelSector
                {
                    GridX = j,
                    GridY = i,
                    Id = index,
                    Level = level,
                    Top = i * worldArea.LevelDimensions[level][0],
                    Bottom = i * worldArea.LevelDimensions[level][0] + worldArea.LevelDimensions[level][0] - 1,
                    Left = j * worldArea.LevelDimensions[level][0],
                    Right = j * worldArea.LevelDimensions[level][0] + worldArea.LevelDimensions[level][0] - 1
                };
                worldArea.SectorGrid[level][index].TilesInWidth = Mathf.Min(
                    worldArea.GridWidth - worldArea.SectorGrid[level][index].Left, worldArea.LevelDimensions[level][0]);
                worldArea.SectorGrid[level][index].TilesInHeight = Mathf.Min(
                    worldArea.GridLength - worldArea.SectorGrid[level][index].Top, worldArea.LevelDimensions[level][1]);
                worldArea.SectorGrid[level][index].WorldAreaIndex = worldArea.Index;
                worldArea.SectorGrid[level][index].Setup();
            }
            else
            {
                worldArea.TileSectorNodeConnections =
                    new Dictionary<Tile, List<AbstractNode>>[WorldData.Pathfinder.maxLevelAmount];

                worldArea.LookUpLowerSectors = new int[WorldData.Pathfinder.maxLevelAmount - 1][][][];
                worldArea.LevelDimensions = new int[WorldData.Pathfinder.maxLevelAmount][];
                worldArea.SectorGrid = new MultiLevelSector[WorldData.Pathfinder.maxLevelAmount][];
            }

            for (int level = 0; level < WorldData.Pathfinder.maxLevelAmount; level++)
            {
                worldArea.TileSectorNodeConnections[level] = new Dictionary<Tile, List<AbstractNode>>();

                worldArea.LevelDimensions[level] = new int[4];
                worldArea.LevelDimensions[level][0] = sectorWidth;
                worldArea.LevelDimensions[level][1] = sectorHeight;
                worldArea.LevelDimensions[level][2] =
                    Mathf.FloorToInt((worldArea.GridWidth / (float) sectorWidth));
                //Math.CeilToInt(worldLayer.gridWidth / (float)sectorWidth);
                worldArea.LevelDimensions[level][3] =
                    Mathf.FloorToInt((worldArea.GridLength / (float) sectorHeight));
                //Math.CeilToInt(worldLayer.gridHeight / (float)sectorHeight);

                worldArea.SectorGrid[level] = new MultiLevelSector[worldArea.LevelDimensions[level][2] * worldArea.LevelDimensions[level][3]];

                for (int i = 0; i < worldArea.LevelDimensions[level][3]; i++)
                {
                    for (int j = 0; j < worldArea.LevelDimensions[level][2]; j++)
                    {
                        int index = (i * worldArea.LevelDimensions[level][2]) + j;
                        worldArea.SectorGrid[level][index] = new MultiLevelSector
                        {
                            GridX = j,
                            GridY = i,
                            Id = index,
                            Level = level,
                            Top = i * worldArea.LevelDimensions[level][0],
                            Bottom =
                                i * worldArea.LevelDimensions[level][0] + worldArea.LevelDimensions[level][0] - 1,
                            Left = j * worldArea.LevelDimensions[level][0],
                            Right = j * worldArea.LevelDimensions[level][0] + worldArea.LevelDimensions[level][0] -
                                    1
                        };
                        worldArea.SectorGrid[level][index].TilesInWidth = Mathf.Min(
                            worldArea.GridWidth - worldArea.SectorGrid[level][index].Left,
                            worldArea.LevelDimensions[level][0]);
                        worldArea.SectorGrid[level][index].TilesInHeight = Mathf.Min(
                            worldArea.GridLength - worldArea.SectorGrid[level][index].Top,
                            worldArea.LevelDimensions[level][1]);
                        worldArea.SectorGrid[level][index].WorldAreaIndex = worldArea.Index;
                        worldArea.SectorGrid[level][index].Setup();
                    }
                }

                sectorWidth *= WorldData.Pathfinder.levelScaling;
                sectorHeight *= WorldData.Pathfinder.levelScaling;

                if (level != 0)
                    worldArea.LookUpLowerSectors[level - 1] = new int[worldArea.SectorGrid[level].Length][][];
            }

            if (WorldData.Pathfinder.maxLevelAmount != 0)
                FillInLookUpLowerSectors(worldArea);
        }

        public void SetupSectorConnections(WorldArea worldArea)
        {
            LowLevel.SetupSectorConnections(worldArea);
            HighLevel.SetupSectorConnections(worldArea);

            foreach (MultiLevelSector[] list in worldArea.SectorGrid)
            {
                foreach (MultiLevelSector sector in list)
                    sector.SearchConnections();
            }
        }

        public static void RemoveAllConnectionsWithinSector(MultiLevelSector sector)
        {
            foreach (List<AbstractNode> list in sector.SectorNodesOnEdge)
            {
                foreach (AbstractNode node in list)
                {
                    node.Connections.Clear();

                    if (node.NodeConnectionToOtherSector != null)
                        node.Connections.Add(node.NodeConnectionToOtherSector, 1);
                }
            }

            foreach (AbstractNode node in sector.WorldAreaNodes.Keys)
            {
                node.Connections.Clear();

                if (node.NodeConnectionToOtherSector != null)
                    node.Connections.Add(node.NodeConnectionToOtherSector, 1);
            }
        }

        public void RemoveAllConnectionsOfWorldAreaNode(AbstractNode node)
        {
            foreach (AbstractNode nodeConnected in node.Connections.Keys)
                nodeConnected.Connections.Remove(node);

            node.Connections.Clear();

            if (node.NodeConnectionToOtherSector != null)
                node.Connections.Add(node.NodeConnectionToOtherSector, 1);
        }

        public void RemoveAllAbstractNodesInSectorEdges(MultiLevelSector sector)
        {
            RemoveAllConnectionsWithinSector(sector);

            foreach (var t in sector.SectorNodesOnEdge)
                t.Clear();
        }

        public void ConnectNodeInSector(MultiLevelSector sector, AbstractNode sectorNode, WorldArea area)
        {
            if (sector.Level == 0)
                LowLevel.ConnectNodeInSector(sector, sectorNode, area);
            else
                HighLevel.ConnectNodeInSector(sector, sectorNode, area);
        }

        public void ConnectWorldAreaNodesToSectorNodes(MultiLevelSector sector, WorldArea area)
        {
            if (sector.Level == 0)
                LowLevel.ConnectWorldAreaNodes(sector, area);
            else
                HighLevel.ConnectWorldAreaNodes(sector, area);
        }

        public void SetSearchFields(int areaIndex, List<int> sectors, bool value)
        {
            WorldArea worldArea = WorldData.WorldAreas[areaIndex];
            foreach (var t in sectors)
            {
                MultiLevelSector sector = worldArea.SectorGrid[0][t];
                for (int x = 0; x < sector.TilesInWidth; x++)
                {
                    for (int y = 0; y < sector.TilesInHeight; y++)
                        worldArea.SearchField[sector.Left + x][sector.Top + y] = value;
                }
            }
        }

        // set which slots/tiles we can expand on.
        public void SetSearchFields(List<int> sectors, WorldArea area, bool value)
        {
            foreach (var t in sectors)
            {
                MultiLevelSector sector = area.SectorGrid[0][t];
                for (int x = 0; x < sector.TilesInWidth; x++)
                {
                    for (int y = 0; y < sector.TilesInHeight; y++)
                        area.SearchField[sector.Left + x][sector.Top + y] = value;
                }
            }
        }

        public void CreateSectorNodes(MultiLevelSector sector, int neighbourId, int edgeIndex,
            int edgeIndexNeighbourSector, Tile inSectorNode, Tile inNeighbourSectorNode, WorldArea area)
        {
            AbstractNode node = CreateAbstractNodeOnSectorEdge(sector, edgeIndex, inSectorNode, area);
            AbstractNode neighbourNode = CreateAbstractNodeOnSectorEdge(area.SectorGrid[sector.Level][neighbourId],
                edgeIndexNeighbourSector, inNeighbourSectorNode, area);

            node.NodeConnectionToOtherSector = neighbourNode;
            neighbourNode.NodeConnectionToOtherSector = node;

            ConnectSectorNodes(node, neighbourNode, 1);
        }

        public AbstractNode CreateAbstractNodeInSector(MultiLevelSector sector, Tile tile, WorldArea area)
        {
            AbstractNode sectorNode = new AbstractNode {TileConnection = tile};
            tile.HasAbstractNodeConnection = true;
            sectorNode.Sector = sector.Id;
            sectorNode.WorldAreaIndex = area.Index;


            if (area.TileSectorNodeConnections[sector.Level].ContainsKey(tile))
                area.TileSectorNodeConnections[sector.Level][tile].Add(sectorNode);
            else
                area.TileSectorNodeConnections[sector.Level].Add(tile, new List<AbstractNode> {sectorNode});

            return sectorNode;
        }

        public void ConnectSectorNodes(AbstractNode sectorNode, List<AbstractNode> sectorNodes, int distance)
        {
            if (sectorNodes.Count > 0)
            {
                foreach (AbstractNode node in sectorNodes)
                    ConnectSectorNodes(sectorNode, node, distance);
            }
        }

        public void ConnectSectorNodes(AbstractNode sectorNode, AbstractNode sectorNode2, int distance)
        {
            if (sectorNode.Connections.ContainsKey(sectorNode2))
                sectorNode.Connections[sectorNode2] = distance;
            else
                sectorNode.Connections.Add(sectorNode2, distance);


            if (sectorNode2.Connections.ContainsKey(sectorNode))
                sectorNode2.Connections[sectorNode] = distance;
            else
                sectorNode2.Connections.Add(sectorNode, distance);
        }

        public void RemoveAllAbstractNodesOnSectorEdge(MultiLevelSector sector, int edgeIndex)
        {
            WorldArea area = WorldData.WorldAreas[sector.WorldAreaIndex];

            // remove the connections from other sectorNodes to the sectorNodes we will remove now
            foreach (AbstractNode sectorNode in sector.SectorNodesOnEdge[edgeIndex])
            {
                foreach (AbstractNode nodeConnected in sectorNode.Connections.Keys)
                    nodeConnected.Connections.Remove(sectorNode);
            }

            // remove 
            foreach (AbstractNode sectorNode in sector.SectorNodesOnEdge[edgeIndex])
            {
                if (area.TileSectorNodeConnections[sector.Level][sectorNode.TileConnection].Count > 1)
                    area.TileSectorNodeConnections[sector.Level][sectorNode.TileConnection].Remove(sectorNode);
                else
                {
                    area.TileSectorNodeConnections[sector.Level].Remove(sectorNode.TileConnection);
                    sectorNode.TileConnection.HasAbstractNodeConnection = false;
                }
            }

            // remove entire edge
            sector.SectorNodesOnEdge[edgeIndex].Clear();
        }

        public void RemoveAbstractNode(int level, AbstractNode sectorNode)
        {
            if (sectorNode != null)
            {
                List<AbstractNode> keys = new List<AbstractNode>(sectorNode.Connections.Keys);
                foreach (AbstractNode node in keys)
                    node.Connections.Remove(sectorNode);


                WorldData.WorldAreas[sectorNode.WorldAreaIndex].TileSectorNodeConnections[level][
                    sectorNode.TileConnection].Remove(sectorNode);

                if (WorldData.WorldAreas[sectorNode.WorldAreaIndex].TileSectorNodeConnections[level][
                        sectorNode.TileConnection].Count == 0)
                {
                    WorldData.WorldAreas[sectorNode.WorldAreaIndex].TileSectorNodeConnections[level]
                        .Remove(sectorNode.TileConnection);

                    //if (level == 0)
                    sectorNode.TileConnection.HasAbstractNodeConnection = false;
                }
            }
        }

        // get list of nodes that are on the border of start sector, with next sector 
        public List<Tile> RowBetweenSectorsWithinWorldArea(MultiLevelSector sectorStart, MultiLevelSector sectorNext,
            WorldArea area)
        {
            Vector2 dir = DirectionBetweenSectorsVector2(sectorStart, sectorNext);

            if (dir == -Vector2.up)
                return GetNodesOnSectorRowNoBlocked(new Vector2(sectorStart.Left, sectorStart.Top), Vector2.right,
                    area);
            if (dir == Vector2.up)
                return GetNodesOnSectorRowNoBlocked(new Vector2(sectorStart.Left, sectorStart.Bottom), Vector2.right,
                    area);
            if (dir == -Vector2.right)
                return GetNodesOnSectorRowNoBlocked(new Vector2(sectorStart.Left, sectorStart.Top), Vector2.up, area);
            if (dir == Vector2.right)
                return GetNodesOnSectorRowNoBlocked(new Vector2(sectorStart.Right, sectorStart.Top), Vector2.up, area);

            return null;
        }

        public List<Tile> GetTilesInSector(MultiLevelSector sector, WorldArea area)
        {
            List<Tile> tiles = new List<Tile>();

            for (int x = 0; x < sector.TilesInWidth; x++)
            {
                for (int y = 0; y < sector.TilesInHeight; y++)
                {
                    Tile tile = area.TileGrid[sector.Left + x][sector.Top + y];
                    if (tile != null && !tile.Blocked)
                        tiles.Add(tile);
                }
            }

            return tiles;
        }

        public MultiLevelSector GetSectorOfTile(int level, Tile tile, WorldArea area)
        {
            int x = Mathf.FloorToInt(tile.GridPos.X / (float) GetSectorWidthAtLevel(area, level));
            int y = Mathf.FloorToInt(tile.GridPos.Y / (float) GetSectorHeightAtLevel(area, level));

            int index = (y * GetSectorGridWidthAtLevel(area, level)) + x;

            return area.SectorGrid[level][index];
        }

        public int GetSectorWidthAtLevel(WorldArea worldLayer, int level)
        {
            return worldLayer.LevelDimensions[level][0];
        }

        public int GetSectorHeightAtLevel(WorldArea worldLayer, int level)
        {
            return worldLayer.LevelDimensions[level][1];
        }

        public int GetSectorGridWidthAtLevel(WorldArea worldLayer, int level)
        {
            return worldLayer.LevelDimensions[level][2];
        }

        public int GetSectorGridHeightAtLevel(WorldArea worldLayer, int level)
        {
            return worldLayer.LevelDimensions[level][3];
        }

        public MultiLevelSector GetHigherSectorFromLower(int level, MultiLevelSector sector, WorldArea area)
        {
            int x = Mathf.FloorToInt(sector.Left / (float) GetSectorWidthAtLevel(area, level));
            int y = Mathf.FloorToInt(sector.Top / (float) GetSectorHeightAtLevel(area, level));

            int index = (y * GetSectorGridWidthAtLevel(area, level)) + x;
            return area.SectorGrid[level][index];
        }

        public int[][] GetLowerSectorsFromHigher(int level, int index, WorldArea area)
        {
            if (level > 0)
                return area.LookUpLowerSectors[level - 1][index];
            Debug.Log("Cant get sectors any lower");
            return null;
        }

        public static int FlipDirection(int dir)
        {
            switch (dir)
            {
                case 0:
                    return 1;
                case 1:
                    return 0;
                case 2:
                    return 3;
                case 3:
                    return 2;
                default:
                    return -1;
            }
        }

        public List<int> GetNeighboursIndexes(MultiLevelSector sector, WorldArea area)
        {
            List<int> neighbours = new List<int>();

            foreach (MultiLevelSector neighbourSector in GetNeighboursWithinWorldArea(sector, area))
                neighbours.Add(neighbourSector.Id);

            return neighbours;
        }

        public static Vector2 EdgeIndexToVector(int edge)
        {
            switch (edge)
            {
                case 0:
                    return -Vector2.up;
                case 1:
                    return Vector2.up;
                case 2:
                    return -Vector2.right;
                case 3:
                    return Vector2.right;
                default:
                    return Vector2.zero;
            }
        }

        public static bool LowerSectorEdgeMatchesHigher(MultiLevelSector highSector, MultiLevelSector lowSector,
            int edgeIndex)
        {
            switch (edgeIndex)
            {
                case 0 when highSector.Top == lowSector.Top:
                case 1 when highSector.Bottom == lowSector.Bottom:
                case 2 when highSector.Left == lowSector.Left:
                case 3 when highSector.Right == lowSector.Right:
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}