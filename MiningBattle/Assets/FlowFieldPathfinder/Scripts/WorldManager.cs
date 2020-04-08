using System.Collections.Generic;
using UnityEngine;

namespace FlowPathfinding
{
    public class WorldManager
    {
        #region PrivateVariables

        private readonly List<IntVector2> _sectorChanges = new List<IntVector2>();
        private readonly Dictionary<IntVector2, List<int>> _sectorEdgeChangesLowLevel = new Dictionary<IntVector2, List<int>>();
        private readonly Dictionary<IntVector3, int> _sectorEdgeChangesHighLevel = new Dictionary<IntVector3, int>();

        #endregion

        #region PublicVariables

        public readonly List<Tile> TilesBlockedAdjusted = new List<Tile>();
        public readonly List<Tile> TilesCostAdjusted = new List<Tile>();
        
        public WorldData WorldData;

        #endregion

        #region PrivateMethods

                private void CostChanged(Tile tile, int value)
        {
            tile.Cost = value;
            if (!TilesCostAdjusted.Contains(tile))
                TilesCostAdjusted.Add(tile);
        }

        private void SetBlock(Tile tile)
        {
            tile.Blocked = true;
            tile.Cost += TileManager.TileBlockedValue;

            if (TilesBlockedAdjusted.Contains(tile))
                TilesBlockedAdjusted.Remove(tile);
            else
                TilesBlockedAdjusted.Add(tile);
        }

        private void RemoveBlock(Tile tile)
        {
            tile.Blocked = false;
            tile.Cost -= TileManager.TileBlockedValue;

            if (TilesBlockedAdjusted.Contains(tile))
                TilesBlockedAdjusted.Remove(tile);
            else
                TilesBlockedAdjusted.Add(tile);
        }
        
        private void AddChange(IntVector2 key, Tile tile, WorldArea area, int side)
        {
            IntVector2 otherSectorKey = new IntVector2();
            int otherSector = -1;
            // if other side of the sector has already been added, we dont need to add this
            if (side == 0)
                otherSector = tile.SectorIndex - WorldData.MultiLevelSectorManager.GetSectorGridWidthAtLevel(area, 0);
            else if (side == 1)
                otherSector = tile.SectorIndex + WorldData.MultiLevelSectorManager.GetSectorGridWidthAtLevel(area, 0);
            else if (side == 2 && area.SectorGrid[0][tile.SectorIndex].GridX > 0)
                otherSector = tile.SectorIndex - 1;
            else if (side == 3 && area.SectorGrid[0][tile.SectorIndex].GridX <
                     WorldData.MultiLevelSectorManager.GetSectorGridWidthAtLevel(area, 0) - 1)
                otherSector = tile.SectorIndex + 1;

            if (WorldData.Pathfinder.maxLevelAmount > 1)
            {
                // add high level sector changes
                if (WorldData.MultiLevelSectorManager.GetHigherSectorFromLower(1, area.SectorGrid[0][tile.SectorIndex],
                        area) != WorldData.MultiLevelSectorManager.GetHigherSectorFromLower(1,
                        area.SectorGrid[0][otherSector], area))
                {
                    IntVector3 highLevelEdgeKey = new IntVector3(area.Index, 0, 0);
                    int sideValue;
                    if (tile.SectorIndex < otherSector)
                    {
                        sideValue = side;
                        highLevelEdgeKey.Y = tile.SectorIndex;
                        highLevelEdgeKey.Z = otherSector;
                    }
                    else
                    {
                        sideValue = MultiLevelSectorManager.FlipDirection(side);
                        highLevelEdgeKey.Y = otherSector;
                        highLevelEdgeKey.Z = tile.SectorIndex;
                    }

                    if (!_sectorEdgeChangesHighLevel.ContainsKey(highLevelEdgeKey))
                        _sectorEdgeChangesHighLevel.Add(highLevelEdgeKey, sideValue);
                }
            }

            if (otherSector > 0 && otherSector < area.SectorGrid[0].Length)
            {
                otherSectorKey.X = tile.WorldAreaIndex;
                otherSectorKey.Y = otherSector;

                if (_sectorEdgeChangesLowLevel.ContainsKey(otherSectorKey))
                {
                    if (_sectorEdgeChangesLowLevel[otherSectorKey].Contains(MultiLevelSectorManager.FlipDirection(side))
                    ) // other side already filled in
                    {
                        if (!_sectorChanges.Contains(key)
                        ) // other sector exist and the side. add our sector for general change
                            _sectorChanges.Add(key);
                    }
                    else if (!_sectorEdgeChangesLowLevel[key].Contains(side)
                    ) //  other sector exist but not the side. add our sector for Edge change
                        _sectorEdgeChangesLowLevel[key].Add(side);
                }
                else // other sector not (yet? )added.   add ourselves and other sector for general change
                {
                    if (!_sectorChanges.Contains(otherSectorKey))
                        _sectorChanges.Add(otherSectorKey);

                    if (!_sectorEdgeChangesLowLevel[key].Contains(side))
                        _sectorEdgeChangesLowLevel[key].Add(side);
                }
            }
            else if (!_sectorEdgeChangesLowLevel[key].Contains(side)) // other sector does not exist, add ourselves
                _sectorEdgeChangesLowLevel[key].Add(side);
        }

        private void RemoveWorldAreaNodes(IntVector3 key)
        {
            WorldArea area = WorldData.WorldAreas[key.X];
            List<AbstractNode> abstractNodes = new List<AbstractNode>(area.SectorGrid[0][key.Z].WorldAreaNodes.Keys);

            foreach (var abstractNode in abstractNodes)
            {
                if (area.SectorGrid[0][key.Z].WorldAreaNodes[abstractNode] == key.Y)
                {
                    List<AbstractNode> abstractNodes2 = null;
                    WorldArea otherArea = WorldData.WorldAreas[key.Y];
                    MultiLevelSector otherSector =
                        otherArea.SectorGrid[0][abstractNode.NodeConnectionToOtherSector.TileConnection.SectorIndex];

                    // remove in other connected sector first
                    WorldData.MultiLevelSectorManager.RemoveAbstractNode(0, abstractNode.NodeConnectionToOtherSector);
                    otherSector.WorldAreaNodes.Remove(abstractNode.NodeConnectionToOtherSector);

                    // visual
                    otherSector.SearchConnections();

                    if (WorldData.Pathfinder.maxLevelAmount > 1)
                    {
                        abstractNodes2 = new List<AbstractNode>(WorldData.MultiLevelSectorManager
                            .GetHigherSectorFromLower(1, otherSector, otherArea).WorldAreaNodes.Keys);
                        foreach (var t in abstractNodes2)
                        {
                            var worldAreaNode = t;
                            if (worldAreaNode.NodeConnectionToOtherSector.WorldAreaIndex == area.Index &&
                                worldAreaNode.TileConnection.SectorIndex == abstractNode.NodeConnectionToOtherSector
                                    .TileConnection
                                    .SectorIndex) // if this node connects with other, and in the right lower sector
                            {
                                WorldData.MultiLevelSectorManager.RemoveAbstractNode(1, worldAreaNode);
                                otherArea.SectorGrid[1][worldAreaNode.Sector].WorldAreaNodes.Remove(worldAreaNode);

                                // visual
                                otherArea.SectorGrid[1][worldAreaNode.Sector].SearchConnections();
                            }
                        }
                    }

                    WorldData.MultiLevelSectorManager.RemoveAbstractNode(0, abstractNode);
                    area.SectorGrid[0][key.Z].WorldAreaNodes.Remove(abstractNode);

                    // visual
                    area.SectorGrid[0][key.Z].SearchConnections();

                    if (WorldData.Pathfinder.maxLevelAmount > 1)
                    {
                        if (abstractNodes2 != null)
                        {
                            abstractNodes2.Clear();
                            abstractNodes2.AddRange(WorldData.MultiLevelSectorManager
                                .GetHigherSectorFromLower(1, area.SectorGrid[0][key.Z], area).WorldAreaNodes.Keys);

                            foreach (var worldAreaNode in abstractNodes2)
                            {
                                if (worldAreaNode.NodeConnectionToOtherSector.WorldAreaIndex == key.Y &&
                                    worldAreaNode.TileConnection.SectorIndex == key.Z
                                ) // if this node connects with other, and in the right lower sector
                                {
                                    WorldData.MultiLevelSectorManager.RemoveAbstractNode(1, worldAreaNode);

                                    Debug.Log("area.sectorGrid  " + area.SectorGrid.Length + "   " +
                                              worldAreaNode.Sector);
                                    area.SectorGrid[1][worldAreaNode.Sector].WorldAreaNodes.Remove(worldAreaNode);

                                    // visual
                                    area.SectorGrid[1][worldAreaNode.Sector].SearchConnections();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region PublicMethods

        public void BlockTile(Tile tile)
        {
            if (tile != null && !tile.Blocked)
                SetBlock(tile);
        }

        public void UnBlockTile(Tile tile)
        {
            if (tile != null && tile.Blocked)
                RemoveBlock(tile);
        }

        public void SetTileCost(Tile tile, int cost)
        {
            if (tile != null)
                CostChanged(tile, cost);
        }

        public void InputChanges()
        {
            if (WorldData.Pathfinder.maxLevelAmount != 0)
            {
                _sectorChanges.Clear();
                _sectorEdgeChangesLowLevel.Clear();
                _sectorEdgeChangesHighLevel.Clear();
                WorldArea area;
                IntVector2 key = new IntVector2();
                IntVector3 sectorsAreaKey = new IntVector3();
                List<IntVector3> sectorsAreaRedo = new List<IntVector3>();

                foreach (var tile in TilesBlockedAdjusted)
                {
                    var tileOnEdge = false;

                    if (TilesCostAdjusted.Contains(tile))
                        TilesCostAdjusted.Remove(tile);

                    var tilePosKey = tile.GridPos;
                    key.X = tile.WorldAreaIndex;
                    key.Y = tile.SectorIndex;

                    if (!_sectorEdgeChangesLowLevel.ContainsKey(key))
                        _sectorEdgeChangesLowLevel.Add(key, new List<int>());

                    area = WorldData.WorldAreas[tile.WorldAreaIndex];
                    var sector = area.SectorGrid[0][tile.SectorIndex];

                    if (tile.GridPos.Y == sector.Top && sector.Top != 0) //top
                    {
                        tileOnEdge = true;
                        if (!_sectorEdgeChangesLowLevel[key].Contains(0))
                            AddChange(key, tile, area, 0);
                    }

                    if (tile.GridPos.Y == sector.Bottom && sector.GridY <
                        WorldData.MultiLevelSectorManager.GetSectorGridHeightAtLevel(area, 0) - 1
                    ) //sector.bottom != worldData.tileManager.gridHeight - 1) //bot
                    {
                        tileOnEdge = true;
                        if (!_sectorEdgeChangesLowLevel[key].Contains(1))
                            AddChange(key, tile, area, 1);
                    }

                    if (tile.GridPos.X == sector.Left && sector.Left != 0) //left
                    {
                        tileOnEdge = true;
                        if (!_sectorEdgeChangesLowLevel[key].Contains(2))
                            AddChange(key, tile, area, 2);
                    }

                    if (tile.GridPos.X == sector.Right && sector.GridX <
                        WorldData.MultiLevelSectorManager.GetSectorGridWidthAtLevel(area, 0) - 1) //right
                    {
                        tileOnEdge = true;
                        if (!_sectorEdgeChangesLowLevel[key].Contains(3))
                            AddChange(key, tile, area, 3);
                    }

                    if (!tileOnEdge)
                    {
                        if (!_sectorChanges.Contains(key))
                            _sectorChanges.Add(key);
                    }

                    // store tiles that will change how world areas connect
                    if (area.WorldAreaTileConnections.ContainsKey(tilePosKey))
                    {
                        sectorsAreaKey.Z = area.TileGrid[tilePosKey.X][tilePosKey.Y].SectorIndex;
                        foreach (IntVector3 value in area.WorldAreaTileConnections[tilePosKey])
                        {
                            sectorsAreaKey.Y = value.Z;

                            WorldArea tempArea;
                            if (area.Index < sectorsAreaKey.X) // change nothing
                            {
                                tempArea = area;
                            }
                            else // we only generate these connections from the world area with the lowest index
                            {
                                tempArea = WorldData.WorldAreas[value.Z];

                                sectorsAreaKey.Y = area.Index;
                                sectorsAreaKey.Z = tempArea.TileGrid[value.X][value.Y].SectorIndex;
                            }

                            sectorsAreaKey.X = tempArea.Index;

                            if (!sectorsAreaRedo.Contains(sectorsAreaKey))
                            {
                                RemoveWorldAreaNodes(sectorsAreaKey);
                                sectorsAreaRedo.Add(sectorsAreaKey);
                            }
                        }
                    }
                }

                List<MultiLevelSector> higherSectors = new List<MultiLevelSector>();

                // rebuild sector edges
                List<IntVector2> keys = new List<IntVector2>(_sectorEdgeChangesLowLevel.Keys);
                IntVector2 indexKey;
                foreach (var t in keys)
                {
                    indexKey = t;

                    area = WorldData.WorldAreas[indexKey.X];
                    WorldData.MultiLevelSectorManager.LowLevel.RebuildNodesOnSectorEdges(
                        _sectorEdgeChangesLowLevel[indexKey], indexKey.Y, area);
                    MultiLevelSectorManager.RemoveAllConnectionsWithinSector(area.SectorGrid[0][indexKey.Y]);
                    // get all sectors that have to recalculate
                    if (!_sectorChanges.Contains(indexKey))
                        _sectorChanges.Add(indexKey);
                }

                foreach (var t in TilesCostAdjusted)
                {
                    key.X = t.WorldAreaIndex;
                    key.Y = t.SectorIndex;

                    if (!_sectorChanges.Contains(key))
                        _sectorChanges.Add(key);
                }

                // now we must recalculate connections on sector edges
                foreach (var t in _sectorChanges)
                {
                    indexKey = t;

                    area = WorldData.WorldAreas[indexKey.X];
                    WorldData.MultiLevelSectorManager.LowLevel.ReCalculateDistancesSectorNodes(
                        area.SectorGrid[0][indexKey.Y], area);

                    if (WorldData.Pathfinder.maxLevelAmount > 1)
                    {
                        MultiLevelSector highSector =
                            WorldData.MultiLevelSectorManager.GetHigherSectorFromLower(1,
                                area.SectorGrid[0][indexKey.Y], area);

                        if (!higherSectors.Contains(highSector))
                            higherSectors.Add(highSector);
                    }
                }

                List<IntVector3> sectorEdgeChangesHighLevel = new List<IntVector3>(_sectorEdgeChangesHighLevel.Keys);
                foreach (var t in sectorEdgeChangesHighLevel)
                {
                    WorldData.MultiLevelSectorManager.HighLevel.RebuildNodesOnHighSectorEdges(
                        t, _sectorEdgeChangesHighLevel[t]);
                }

                foreach (var t in higherSectors)
                {
                    WorldData.MultiLevelSectorManager.HighLevel.HighLevelSectorAdjustedRecalculate(t);
                }

                //visual
                foreach (var t in _sectorChanges)
                {
                    indexKey = t;
                    WorldData.WorldAreas[indexKey.X].SectorGrid[0][indexKey.Y].SearchConnections();
                }

                // all sector connections fixed & correct world area nodes removed
                // rebuild world area nodes were we removed them
                for (int i = 0; i < sectorsAreaRedo.Count; i++)
                {
                    area = WorldData.WorldAreas[sectorsAreaRedo[i].X];
                    IntVector2 newKey = new IntVector2(sectorsAreaRedo[i].Y, sectorsAreaRedo[i].Z);

                    foreach (List<IntVector2> group in area.GroupsInSectors[newKey])
                        WorldData.WorldBuilder.GenerateWordConnectingNodesPerGroup(area, group, newKey);
                }
            }

            TilesCostAdjusted.Clear();
            TilesBlockedAdjusted.Clear();
            //WorldData.Pathfinder.WorldHasBeenChanged(_sectorChanges);
        }

        #endregion
    }
}