using UnityEngine;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class LowLevelSectorManager
    {
        #region PrivateVariables

        private readonly List<Tile> _openSet = new List<Tile>();
        private readonly List<Tile> _closedSet = new List<Tile>();        

        #endregion

        #region PublicVariables

        public MultiLevelSectorManager Manager;

        #endregion

        #region PrivateMethods

                private void RebuildNodesOnSectorEdge(MultiLevelSector sector, int edgeIndex, int edgeIndexNeighbourSector,
            Vector2 startInSector, Vector2 startInNeighbourSector, Vector2 direction, WorldArea area)
        {
            // remove connections to sector nodes on edge + remove them and those directly linked on neighbour sector
            Manager.RemoveAllAbstractNodesOnSectorEdge(sector, edgeIndex);

            var maxStep = direction == Vector2.right ? sector.TilesInWidth : sector.TilesInHeight;

            int sec = -1;
            for (int i = 0; i < maxStep; i++)
            {
                Tile neighbour =
                    area.TileGrid[(int) startInNeighbourSector.x + (int) direction.x * i][
                        (int) startInNeighbourSector.y + (int) direction.y * i];
                if (neighbour != null)
                {
                    sec = neighbour.SectorIndex;
                    Manager.RemoveAllAbstractNodesOnSectorEdge(area.SectorGrid[0][sec], edgeIndexNeighbourSector);
                    break;
                }
            }

            if (sec != -1) // if we haven't found any tiles, no reason to try and build connections
            {
                // build nodes on edge
                bool sectorNodesOpen = false;
                int openLength = -1;
                int startNodeOfGroup = 0;

                for (int i = 0; i < maxStep; i++)
                {
                    var tile1 = area.TileGrid[(int) startInSector.x + (int) direction.x * i][
                        (int) startInSector.y + (int) direction.y * i];
                    var tile2 = area.TileGrid[(int) startInNeighbourSector.x + (int) direction.x * i][
                        (int) startInNeighbourSector.y + (int) direction.y * i];

                    if (tile1 != null && tile2 != null && !tile1.Blocked && !tile2.Blocked &&
                        Manager.WorldData.TileManager.TilesWithinRangeGeneration(tile1, tile2))
                    {
                        // starting point of a new connection/gate between sectors
                        if (!sectorNodesOpen)
                            sectorNodesOpen = true;

                        openLength++;
                    }
                    else
                    {
                        if (sectorNodesOpen) // if we have had a couple of open nodes couples
                        {
                            // small enough to represent with 1 transition
                            if (openLength < Manager.MaxGateSize)
                            {
                                int steps = Mathf.FloorToInt(openLength * 0.5f) + startNodeOfGroup;
                                Tile neighbourTile =
                                    area.TileGrid[(int) startInNeighbourSector.x + (int) direction.x * steps][
                                        (int) startInNeighbourSector.y + (int) direction.y * steps];
                                Manager.CreateSectorNodes(sector, neighbourTile.SectorIndex, edgeIndex,
                                    edgeIndexNeighbourSector,
                                    area.TileGrid[(int) startInSector.x + (int) direction.x * steps][
                                        (int) startInSector.y + (int) direction.y * steps], neighbourTile, area);
                            }
                            else
                            {
                                // to large, 2 transitions. on on each end
                                int multilayer = startNodeOfGroup;
                                Tile neighbourTile =
                                    area.TileGrid[(int) startInNeighbourSector.x + (int) direction.x * multilayer][
                                        (int) startInNeighbourSector.y + (int) direction.y * multilayer];
                                Manager.CreateSectorNodes(sector, neighbourTile.SectorIndex, edgeIndex,
                                    edgeIndexNeighbourSector,
                                    area.TileGrid[(int) startInSector.x + (int) direction.x * multilayer][
                                        (int) startInSector.y + (int) direction.y * multilayer], neighbourTile, area);

                                multilayer = (startNodeOfGroup + openLength);
                                neighbourTile =
                                    area.TileGrid[(int) startInNeighbourSector.x + (int) direction.x * multilayer][
                                        (int) startInNeighbourSector.y + (int) direction.y * multilayer];
                                Manager.CreateSectorNodes(sector, neighbourTile.SectorIndex, edgeIndex,
                                    edgeIndexNeighbourSector,
                                    area.TileGrid[(int) startInSector.x + (int) direction.x * multilayer][
                                        (int) startInSector.y + (int) direction.y * multilayer], neighbourTile, area);
                            }

                            openLength = -1;
                            sectorNodesOpen = false;
                        }

                        startNodeOfGroup = i + 1;
                    }
                }

                if (sectorNodesOpen) // if we have had a couple of open nodes couples
                {
                    if (openLength < Manager.MaxGateSize)
                    {
                        int steps = Mathf.FloorToInt(openLength * 0.5f) + startNodeOfGroup;
                        Tile neighbourTile =
                            area.TileGrid[(int) startInNeighbourSector.x + (int) direction.x * steps][
                                (int) startInNeighbourSector.y + (int) direction.y * steps];
                        Manager.CreateSectorNodes(sector, neighbourTile.SectorIndex, edgeIndex,
                            edgeIndexNeighbourSector,
                            area.TileGrid[(int) startInSector.x + (int) direction.x * steps][
                                (int) startInSector.y + (int) direction.y * steps], neighbourTile, area);
                    }
                    else
                    {
                        // to large, 2 transitions. on on each end
                        int multilayer = startNodeOfGroup;
                        Tile neighbourTile =
                            area.TileGrid[(int) startInNeighbourSector.x + (int) direction.x * multilayer][
                                (int) startInNeighbourSector.y + (int) direction.y * multilayer];
                        Manager.CreateSectorNodes(sector, neighbourTile.SectorIndex, edgeIndex,
                            edgeIndexNeighbourSector,
                            area.TileGrid[(int) startInSector.x + (int) direction.x * multilayer][
                                (int) startInSector.y + (int) direction.y * multilayer], neighbourTile, area);

                        multilayer = (startNodeOfGroup + openLength);
                        neighbourTile =
                            area.TileGrid[(int) startInNeighbourSector.x + (int) direction.x * multilayer][
                                (int) startInNeighbourSector.y + (int) direction.y * multilayer];
                        Manager.CreateSectorNodes(sector, neighbourTile.SectorIndex, edgeIndex,
                            edgeIndexNeighbourSector,
                            area.TileGrid[(int) startInSector.x + (int) direction.x * multilayer][
                                (int) startInSector.y + (int) direction.y * multilayer], neighbourTile, area);
                    }
                }
            }
        }

        #endregion

        #region PublicMethods

                public void SetupSectorConnections(WorldArea area)
        {
            // build connections on the lowest level
            foreach (MultiLevelSector sector in area.SectorGrid[0])
            {
                if (Manager.WorldData.Pathfinder.maxLevelAmount != 0)
                {
                    //bot
                    if (sector.Bottom < area.GridLength - 1)
                        RebuildNodesOnSectorEdge(sector, 1, 0, new Vector2(sector.Left, sector.Bottom),
                            new Vector2(sector.Left, sector.Bottom + 1), Vector2.right, area);

                    //right
                    if (sector.Right < area.GridWidth - 1)
                        RebuildNodesOnSectorEdge(sector, 3, 2, new Vector2(sector.Right, sector.Top),
                            new Vector2(sector.Right + 1, sector.Top), Vector2.up, area);
                }

                // recalculate sector sector nodes distances
                ReCalculateDistancesSectorNodes(sector, area);
            }
        }

        public void RebuildNodesOnSectorEdges(List<int> sides, int sectorIndex, WorldArea area)
        {
            Vector2 startInSector = Vector2.zero;
            Vector2 startInNeighbourSector = Vector2.zero;
            Vector2 direction = Vector2.zero;
            MultiLevelSector sector = area.SectorGrid[0][sectorIndex];

            foreach (int side in sides)
            {
                if (side == 0)
                {
                    startInSector = new Vector2(sector.Left, sector.Top);
                    startInNeighbourSector = new Vector2(sector.Left, sector.Top - 1);
                    direction = Vector2.right;
                }
                else if (side == 1)
                {
                    startInSector = new Vector2(sector.Left, sector.Bottom);
                    startInNeighbourSector = new Vector2(sector.Left, sector.Bottom + 1);
                    direction = Vector2.right;
                }
                else if (side == 2)
                {
                    startInSector = new Vector2(sector.Left, sector.Top);
                    startInNeighbourSector = new Vector2(sector.Left - 1, sector.Top);
                    direction = Vector2.up;
                }
                else if (side == 3)
                {
                    startInSector = new Vector2(sector.Right, sector.Top);
                    startInNeighbourSector = new Vector2(sector.Right + 1, sector.Top);
                    direction = Vector2.up;
                }

                RebuildNodesOnSectorEdge(sector, side, MultiLevelSectorManager.FlipDirection(side), startInSector,
                    startInNeighbourSector, direction, area);
            }
        }
        
        // recalculate distances to other high level nodes
        public void ReCalculateDistancesSectorNodes(MultiLevelSector sector, WorldArea area)
        {
            foreach (List<AbstractNode> list in sector.SectorNodesOnEdge)
            {
                foreach (AbstractNode node in list)
                    ConnectNodeInSector(sector, node, area);
            }
        }

        public void ConnectWorldAreaNodes(MultiLevelSector sector, WorldArea area)
        {
            foreach (AbstractNode node in sector.WorldAreaNodes.Keys)
                ConnectNodeInSector(sector, node, area);
        }

        // calculate distances to other high level nodes
        public void ConnectNodeInSector(MultiLevelSector sector, AbstractNode sectorNode, WorldArea area)
        {
            int maxNodes = sector.WorldAreaNodes.Count + sector.SectorNodesOnEdge[0].Count +
                           sector.SectorNodesOnEdge[1].Count + sector.SectorNodesOnEdge[2].Count +
                           sector.SectorNodesOnEdge[3].Count;
            //Debug.Log("low level sector ConnectNodeInSector " + maxNodes);

            // 2 sectorNodes on same location, connect them
            if (area.TileSectorNodeConnections[sector.Level][sectorNode.TileConnection].Count > 1 &&
                area.TileSectorNodeConnections[sector.Level].ContainsKey(sectorNode.TileConnection))
            {
                maxNodes--;
                Manager.ConnectSectorNodes(area.TileSectorNodeConnections[sector.Level][sectorNode.TileConnection][0],
                    area.TileSectorNodeConnections[sector.Level][sectorNode.TileConnection][1], 0);
            }

            _openSet.Clear();
            _closedSet.Clear();

            _openSet.Add(sectorNode.TileConnection);
            _openSet[0].IntegrationValue = 0;

            // Debug.Log("max nodes " + maxNodes);
            while (_openSet.Count > 0 && maxNodes != 0)
            {
                Tile currentNode = _openSet[0];

                foreach (Tile neighbour in Manager.WorldData.TileManager.GetAllNeighboursForSectorNodeSearch(
                    currentNode, area))
                {
                    //Debug.Log("neighbor");
                    if (!_openSet.Contains(neighbour))
                    {
                        _openSet.Add(neighbour);

                        // if true, there is a higher node here
                        if (neighbour.HasAbstractNodeConnection) //.higherLevelNodeIndex.Count > 0)
                        {
                            //Debug.Log("found connection");
                            //Get all HigherNodes on this Lower Node  & connect them
                            List<AbstractNode> neighbourSectorNodes =
                                area.TileSectorNodeConnections[sector.Level][
                                    neighbour]; // GetHigherLevelNodeList(neighbour, sector);
                            Manager.ConnectSectorNodes(sectorNode, neighbourSectorNodes,
                                neighbour.IntegrationValue /
                                10); // 10 times scaling - more accurate but slower/ Math.FloorToInt((neighbour.totalPathCost / 10f) + 0.5f)
                            maxNodes -= neighbourSectorNodes.Count;
                        }
                    }
                }

                _closedSet.Add(currentNode);
                _openSet.Remove(currentNode);
            }

            // reset
            foreach (Tile tile in _openSet)
                tile.IntegrationValue = TileManager.TileResetIntegrationValue;

            foreach (Tile tile in _closedSet)
                tile.IntegrationValue = TileManager.TileResetIntegrationValue;
        }

        #endregion        
    }
}