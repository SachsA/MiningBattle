using UnityEngine;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class HighLevelSectorManager
    {
        #region PublicVariables

        public MultiLevelSectorManager Manager;

        #endregion

        #region PrivateMethods

                private void RebuildNodesOnHighSectorEdge(int level, MultiLevelSector sector, int edgeNumber, Vector2 direction,
            WorldArea area)
        {
            // get all lower level sectors within
            foreach (int[] list in Manager.GetLowerSectorsFromHigher(level, sector.Id, area)
            ) //   .lookUpLowerSectors[level - 1][sector.ID])
            {
                // go through lowerLevel Sector (indexes), make copies of the abstract nodes on the correct edges
                foreach (int lowerLevelSectorIndex in list)
                {
                    MultiLevelSector lowerSector = area.SectorGrid[level - 1][lowerLevelSectorIndex];

                    if (MultiLevelSectorManager.LowerSectorEdgeMatchesHigher(sector, lowerSector, edgeNumber)) // match edge
                    {
                        foreach (AbstractNode node in lowerSector.SectorNodesOnEdge[edgeNumber]
                        ) // get nodes to copy from the edge
                        {
                            int neighbourId = 0;
                            int neighbourEdgeNumber = 0;

                            if (edgeNumber == 0)
                            {
                                neighbourId =
                                    sector.Id - Manager.GetSectorGridWidthAtLevel(area,
                                        level); // levelDimensions[level][2];
                                neighbourEdgeNumber = 1;
                            }
                            else if (edgeNumber == 1)
                            {
                                neighbourId =
                                    sector.Id + Manager.GetSectorGridWidthAtLevel(area,
                                        level); // levelDimensions[level][2];
                                neighbourEdgeNumber = 0;
                            }
                            else if (edgeNumber == 2)
                            {
                                neighbourId = sector.Id - 1;
                                neighbourEdgeNumber = 3;
                            }
                            else if (edgeNumber == 3)
                            {
                                neighbourId = sector.Id + 1;
                                neighbourEdgeNumber = 2;
                            }

                            Manager.CreateSectorNodes(sector, neighbourId, edgeNumber, neighbourEdgeNumber,
                                node.TileConnection,
                                Manager.WorldData.TileManager.GetTileInWorldArea(area,
                                    node.TileConnection.GridPos.X + (int) direction.x,
                                    node.TileConnection.GridPos.Y + (int) direction.y), area);
                        }
                    }
                }
            }
        }

        private void ReCalculateDistancesHigherSectorNodes(MultiLevelSector sector, WorldArea area)
        {
            List<AbstractNode> allNodesInSector = new List<AbstractNode>();
            foreach (List<AbstractNode> list in sector.SectorNodesOnEdge)
                allNodesInSector.AddRange(list);
            allNodesInSector.AddRange(sector.WorldAreaNodes.Keys);

            List<AbstractNode> allNodesInSector2 = new List<AbstractNode>(allNodesInSector);


            foreach (AbstractNode node in allNodesInSector2)
            {
                foreach (AbstractNode nodeToFind in allNodesInSector)
                    Manager.WorldData.HierarchicalPathfinder.FindConnectionInsideSectorOnly(node, nodeToFind, sector,
                        area);
            }
            
            //foreach (List<AbstractNode> list in sector.sectorNodesOnEdge)
            //{
            //    foreach (AbstractNode node in list)
            //    {
            //        foreach (AbstractNode nodeToFind in allNodesInSector)
            //            manager.worldData.hierarchicalPathfinder.FindConnectionInsideSectorOnly(node, nodeToFind, sector, area);
            //    }
            //}
        }

        #endregion

        #region PublicMethods

        public void SetupSectorConnections(WorldArea area)
        {
            // skip lowest level
            for (int i = 1; i < Manager.WorldData.Pathfinder.maxLevelAmount; i++)
            {
                foreach (MultiLevelSector sector in area.SectorGrid[i])
                {
                    //bot
                    if (sector.Bottom < area.GridLength - 1)
                        RebuildNodesOnHighSectorEdge(i, sector, 1, Vector2.up, area);

                    //right
                    if (sector.Right < area.GridWidth - 1)
                        RebuildNodesOnHighSectorEdge(i, sector, 3, Vector2.right, area);

                    // recalculate sector sector nodes distances
                    ReCalculateDistancesHigherSectorNodes(sector, area);
                }
            }
        }

        public void RebuildNodesOnHighSectorEdges(IntVector3 areaWithSectors, int side)
        {
            WorldArea area = Manager.WorldData.WorldAreas[areaWithSectors.X];
            MultiLevelSector sector = area.SectorGrid[0][areaWithSectors.Y];
            MultiLevelSector higherSector = Manager.GetHigherSectorFromLower(1, sector, area);

            Debug.Log("Lower level  start " + areaWithSectors.Y + "  end " + areaWithSectors.Z);

            // clear out both sides  //area.sectorGrid[1][areaWithSectors.z]
            Manager.RemoveAllAbstractNodesOnSectorEdge(higherSector, side);
            Manager.RemoveAllAbstractNodesOnSectorEdge(
                Manager.GetHigherSectorFromLower(1, area.SectorGrid[0][areaWithSectors.Z], area),
                MultiLevelSectorManager.FlipDirection(side));

            // rebuild side
            RebuildNodesOnHighSectorEdge(1, higherSector, side, MultiLevelSectorManager.EdgeIndexToVector(side), area);
        }
        
        public void HighLevelSectorAdjustedRecalculate(MultiLevelSector sector)
        {
            WorldArea area = Manager.WorldData.WorldAreas[sector.WorldAreaIndex];
            MultiLevelSectorManager.RemoveAllConnectionsWithinSector(sector);
            ReCalculateDistancesHigherSectorNodes(sector, area);

            // visual
            sector.SearchConnections();
        }

        public void ConnectWorldAreaNodes(MultiLevelSector sector, WorldArea area)
        {
            foreach (AbstractNode node in sector.WorldAreaNodes.Keys)
                ConnectNodeInSector(sector, node, area);
        }

        public void ConnectNodeInSector(MultiLevelSector sector, AbstractNode node, WorldArea area)
        {
            List<AbstractNode> allNodesInSector = new List<AbstractNode>();
            foreach (List<AbstractNode> list in sector.SectorNodesOnEdge)
                allNodesInSector.AddRange(list);

            allNodesInSector.AddRange(sector.WorldAreaNodes.Keys);

            foreach (AbstractNode nodeToFind in allNodesInSector)
                Manager.WorldData.HierarchicalPathfinder.FindConnectionInsideSectorOnly(node, nodeToFind, sector, area);
        }

        #endregion
    }
}