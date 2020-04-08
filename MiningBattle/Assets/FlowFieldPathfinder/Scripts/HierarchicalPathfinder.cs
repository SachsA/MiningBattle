using System.Collections.Generic;
using UnityEngine;

namespace FlowPathfinding
{
    public class HierarchicalPathfinder
    {
        #region PrivateVariables

        private int _previousNodesStartingPointCount;

        private List<AbstractNode> _multiSectorNodes;

        private readonly List<List<int>> _sectorIndexesAllLowerPaths = new List<List<int>>();
        private readonly List<AbstractNode> _previousAbstractNodes = new List<AbstractNode>();

        private readonly Dictionary<IntVector2, bool> _validSectorsFound = new Dictionary<IntVector2, bool>();

        private AbstractNode _startMultiSectorNode;
        private AbstractNode _destinationMultiSectorNode;

        private MultiLevelSector _startNodeSector;
        private MultiLevelSector _destinationNodeSector;

        #endregion

        #region PublicVariables

        public WorldData WorldData;

        #endregion

        #region PrivateMethods

        private void AddToPath(List<int> areasAndSectors)
        {
            _sectorIndexesAllLowerPaths.Add(areasAndSectors);
        }

        private List<int> SearchHighLevelPath(AbstractNode start, AbstractNode destination, bool returnPath)
        {
            Heap<AbstractNode> openSet = new Heap<AbstractNode>(3000);
            HashSet<AbstractNode> closedSet = new HashSet<AbstractNode>();
            openSet.Add(start);
            
            while (openSet.Count > 0)
            {
                AbstractNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == destination)
                {
                    //Debug.Log("Retrace Path");
                    return RetracePath(start, destination, returnPath);
                }

                foreach (AbstractNode neighbour in currentNode.Connections.Keys)
                {
                    if (closedSet.Contains(neighbour))
                        continue;
                    
                    int newMovementCostToNeighbour = currentNode.G + currentNode.Connections[neighbour];
                    if (newMovementCostToNeighbour < neighbour.G || !openSet.Contains(neighbour))
                    {
                        neighbour.G = newMovementCostToNeighbour;

                        // divide by 10, G works on a factor 10 smaller
                        neighbour.H = GetDistance(neighbour, destination); // / 10;
                        neighbour.Parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
            
            Debug.Log("no Low Level path found");

            return null;
        }

        private List<int> SearchHighLevelPath(AbstractNode start, AbstractNode destination,
            Dictionary<IntVector2, bool> validSectors, bool returnPath)
        {
            Heap<AbstractNode> openSet = new Heap<AbstractNode>(2000);
            HashSet<AbstractNode> closedSet = new HashSet<AbstractNode>();
            openSet.Add(start);
            while (openSet.Count > 0)
            {
                AbstractNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == destination)
                    return RetracePath(start, destination, returnPath);

                foreach (AbstractNode neighbour in currentNode.Connections.Keys)
                {
                    var neighbourVector = new IntVector2(neighbour.WorldAreaIndex, neighbour.Sector);
                    if (closedSet.Contains(neighbour) || !validSectors.ContainsKey(neighbourVector))
                        continue;

                    int newMovementCostToNeighbour = currentNode.G + currentNode.Connections[neighbour];
                    if (newMovementCostToNeighbour < neighbour.G || !openSet.Contains(neighbour))
                    {
                        neighbour.G = newMovementCostToNeighbour;
                        neighbour.H = GetDistance(neighbour, destination);
                        neighbour.Parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
            
            Debug.Log("no High Level path found");

            return null;
        }

        private int GetDistance(AbstractNode sectorA, AbstractNode sectorB)
        {
            Vector3 a = WorldData.TileManager.GetTileWorldPosition(sectorA.TileConnection,
                WorldData.WorldAreas[sectorA.WorldAreaIndex]);
            Vector3 b = WorldData.TileManager.GetTileWorldPosition(sectorB.TileConnection,
                WorldData.WorldAreas[sectorB.WorldAreaIndex]);

            return (int) ((Vector3.Distance(a, b) + 0.5f) / WorldData.Pathfinder.tileSize);
        }

        private List<int> RetracePath(AbstractNode startNode, AbstractNode endNode, bool retracePath)
        {
            _previousAbstractNodes.Clear();

            List<int> path = new List<int>();
            if (retracePath)
            {
                AbstractNode currentNode = endNode;
                _previousAbstractNodes.Add(currentNode); // visual
                path.Add(currentNode.WorldAreaIndex);
                path.Add(currentNode.Sector);

                while (currentNode != startNode)
                {
                    // if sector OR worldArea dont match, its a new one
                    if (path[path.Count - 1] != currentNode.Sector ||
                        path[path.Count - 2] != currentNode.WorldAreaIndex)
                    {
                        path.Add(currentNode.WorldAreaIndex);
                        path.Add(currentNode.Sector);
                    }

                    _previousAbstractNodes.Add(currentNode); // visual
                    currentNode = currentNode.Parent;
                }

                if (path[path.Count - 1] != currentNode.Sector)
                {
                    _previousAbstractNodes.Add(currentNode); // visual
                    path.Add(currentNode.WorldAreaIndex);
                    path.Add(currentNode.Sector);
                }
            }
            else
            {
                path.Add(0);
                AbstractNode currentNode = endNode;

                while (currentNode != startNode)
                {
                    if (currentNode.Connections.ContainsKey(currentNode.Parent))
                        path[0] += currentNode.Connections[currentNode.Parent];
                    else
                        path[0] += 1;

                    currentNode = currentNode.Parent;
                }
            }

            return path;
        }

        private int FindConnectionThroughPathInsideSectorOnly(List<AbstractNode> start, List<AbstractNode> destination,
            int[][] sectorIndexes)
        {
            Dictionary<IntVector2, bool> lowerSectors = new Dictionary<IntVector2, bool>();
            foreach (int[] list in sectorIndexes)
            {
                foreach (int index in list)
                    lowerSectors.Add(new IntVector2(start[0].WorldAreaIndex, index), false);
            }

            List<int> connectionDistance =
                SearchHighLevelPath(start[0], destination[0], lowerSectors, false); // new List<int>();

            if (connectionDistance != null && connectionDistance.Count > 0)
                return connectionDistance[0];
            //no connection path
            return -1;
        }

        #endregion

        #region PublicMethods

        // find path on higher level between start and goal/destination
        public List<List<int>> FindPaths(Dictionary<IntVector2, Tile> startingPoints, Tile destinationTile,
            WorldArea destinationArea)
        {
            _multiSectorNodes = new List<AbstractNode>();

            int maxLevelAmount = WorldData.Pathfinder.maxLevelAmount;
            if (maxLevelAmount == 0)
            {
                if (WorldData.Pathfinder.worldIsMultiLayered)
                {
                    Debug.Log(
                        "Invalid search: A  Multi Layered world cannot find paths with no Levels Of Abstraction ");
                    return null;
                }

                maxLevelAmount = 1;
            }

            // adding all the start/destination nodes per level.
            for (int i = 0; i < maxLevelAmount; i++)
            {
                foreach (IntVector2 key in startingPoints.Keys)
                {
                    Tile startTile = startingPoints[key];
                    
//                    Debug.Log("starting point : " + WorldData.TileManager.GetTileWorldPosition(startTile,
//                        WorldData.WorldAreas[0]));
//                    Debug.Log("ending point : " + WorldData.TileManager.GetTileWorldPosition(destinationTile,
//                                  WorldData.WorldAreas[0]));
                    _startNodeSector =
                        WorldData.MultiLevelSectorManager.GetSectorOfTile(i, startTile, WorldData.WorldAreas[key.X]);
                    _startMultiSectorNode =
                        WorldData.MultiLevelSectorManager.CreateAbstractNodeInSector(_startNodeSector, startTile,
                            WorldData.WorldAreas[key.X]);

                    WorldData.MultiLevelSectorManager.ConnectNodeInSector(_startNodeSector, _startMultiSectorNode,
                        WorldData.WorldAreas[key.X]); //, new List<HigherNode>()); 
                    _multiSectorNodes.Add(_startMultiSectorNode);
                }

                // create temporary high level node on the start node
                // calculate its distance to the other high level nodes in the same sector, build connections between them
                _destinationNodeSector =
                    WorldData.MultiLevelSectorManager.GetSectorOfTile(i, destinationTile, destinationArea);
                _destinationMultiSectorNode =
                    WorldData.MultiLevelSectorManager.CreateAbstractNodeInSector(_destinationNodeSector,
                        destinationTile, destinationArea);
                WorldData.MultiLevelSectorManager.ConnectNodeInSector(_destinationNodeSector,
                    _destinationMultiSectorNode, destinationArea);

                _multiSectorNodes.Add(_destinationMultiSectorNode);
            }

            _previousNodesStartingPointCount = startingPoints.Count;
            _sectorIndexesAllLowerPaths.Clear();

            for (int j = 0; j < startingPoints.Count; j++) // for each starting point
            {
                for (int i = _multiSectorNodes.Count - 1; i > -1; i -= startingPoints.Count + 1)
                {
                    var nextStep = false;
                    int level = i / (startingPoints.Count + 1);

                    var start = _multiSectorNodes[i - (j + 1)];
                    var destination = _multiSectorNodes[i];

                    if (start.Sector == destination.Sector) // starting point == destination
                    {
                        if (WorldData.Pathfinder.maxLevelAmount == 0)
                        {
                            AddToPath(new List<int> {start.WorldAreaIndex, start.Sector});
                            nextStep = true;
                        }
                        else
                        {
                            foreach (AbstractNode sectorNode in destination.Connections.Keys)
                            {
                                if (sectorNode == start) // direct connection in sector
                                {
                                    if (level != 0) // go to the next layer
                                        nextStep = true;
                                    else // level = 0, our goal is right next to us

                                        AddToPath(new List<int> {start.WorldAreaIndex, start.Sector});

                                    break;
                                }
                            }
                        }
                    }

                    //validSectorsFound
                    if (!nextStep)
                    {
                        if (level != 0
                        ) // we have to search the path between them, using A-star on the high level network
                        {
                            var sectorIndexes = new List<int>();
                            if (_validSectorsFound.Count == 0)
                            {
                                sectorIndexes = SearchHighLevelPath(start, destination, true);
                            }
                            else
                            {
                                sectorIndexes = SearchHighLevelPath(start, destination, _validSectorsFound, true);
                            }
//                            var sectorIndexes = _validSectorsFound.Count == 0
//                                ? SearchHighLevelPath(start, destination, true)
//                                : SearchHighLevelPath(start, destination, _validSectorsFound, true);

                            if (sectorIndexes == null)
                            {
                                Debug.Log("indexes ARE null,  no Path  on this hierarchical Level: " + level);
                            }
                            else
                            {
                                _validSectorsFound.Clear();
                                for (int k = 0; k < sectorIndexes.Count; k += 2)
                                {
                                    foreach (int[] list in WorldData.MultiLevelSectorManager.GetLowerSectorsFromHigher(
                                        level, sectorIndexes[k + 1], WorldData.WorldAreas[sectorIndexes[k]]))
                                    {
                                        foreach (int lowerIndex in list)
                                            _validSectorsFound.Add(new IntVector2(sectorIndexes[k], lowerIndex), false);
                                    }
                                }
                            }
                        }
                        else // lowest level reached
                        {
                            var sectorIndexes = new List<int>();
                            if (_validSectorsFound.Count == 0)
                            {
                                sectorIndexes = SearchHighLevelPath(start, destination, true);
                            }
                            else
                            {
                                sectorIndexes = SearchHighLevelPath(start, destination, _validSectorsFound, true);
                            }
                            _validSectorsFound.Clear();

                            if (sectorIndexes != null)
                                AddToPath(sectorIndexes);
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            return _sectorIndexesAllLowerPaths;
        }

        public void RemovePreviousSearch()
        {
            for (int i = _multiSectorNodes.Count - 1; i > -1; i -= _previousNodesStartingPointCount + 1)
            {
                int level = (i / (_previousNodesStartingPointCount + 1));

                for (int j = 0; j < _previousNodesStartingPointCount + 1; j++)
                    WorldData.MultiLevelSectorManager.RemoveAbstractNode(level, _multiSectorNodes[i - j]);
            }
        }

        public void FindConnectionInsideSectorOnly(AbstractNode start, AbstractNode destination,
            MultiLevelSector sector, WorldArea area)
        {
            // if we look for our self or already have a connection, we are done here
            if (start == destination || start.Connections.ContainsKey(destination))
                return;

            int lowerLevel = sector.Level - 1;

            if (!area.TileSectorNodeConnections[lowerLevel].ContainsKey(start.TileConnection))
                Debug.Log("node index  " + start.WorldAreaIndex + "   actual world index  " + area.Index +
                          "   start high " + start.TileConnection.GridPos.X + "   y " + start.TileConnection.GridPos.Y);

            int connectionDistance = FindConnectionThroughPathInsideSectorOnly(
                area.TileSectorNodeConnections[lowerLevel][start.TileConnection],
                area.TileSectorNodeConnections[lowerLevel][destination.TileConnection],
                WorldData.MultiLevelSectorManager.GetLowerSectorsFromHigher(sector.Level, sector.Id, area));

            if (connectionDistance > -1)
                WorldData.MultiLevelSectorManager.ConnectSectorNodes(start, destination, connectionDistance);
        }

        #endregion
    }
}