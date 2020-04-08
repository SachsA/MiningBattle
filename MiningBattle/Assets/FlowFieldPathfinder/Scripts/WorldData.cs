using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace FlowPathfinding
{
    [Serializable]
    public class WorldData : ISerializable
    {
        #region PublicVariables

        public bool drawCost;

        [NonSerialized] public bool WorldCreatedManually;
        [NonSerialized] public bool WorldGenerated;

        [NonSerialized] public List<int[][]> CostFields = new List<int[][]>();
        [NonSerialized] public List<int[][]> LayerWorldAreaIndexes = new List<int[][]>();
        [NonSerialized] public readonly List<WorldArea> WorldAreas = new List<WorldArea>();

        [NonSerialized] public Pathfinder Pathfinder;

        [NonSerialized]
        public readonly Dictionary<IntVector2, int> ManualWorldAreaConnections = new Dictionary<IntVector2, int>();

        [NonSerialized] public readonly TileManager TileManager = new TileManager();
        [NonSerialized] public readonly HierarchicalPathfinder HierarchicalPathfinder = new HierarchicalPathfinder();
        [NonSerialized] public readonly MultiLevelSectorManager MultiLevelSectorManager = new MultiLevelSectorManager();
        [NonSerialized] public readonly ColorLists ColorLists = new ColorLists();
        [NonSerialized] public readonly IntegrationFieldManager IntegrationFieldManager = new IntegrationFieldManager();
        [NonSerialized] public readonly FlowFieldManager FlowFieldManager = new FlowFieldManager();
        [NonSerialized] public readonly WorldBuilder WorldBuilder = new WorldBuilder();
        [NonSerialized] public readonly WorldManager WorldManager = new WorldManager();

        #endregion

        #region PrivateMethods

        private void DrawTree()
        {
            Gizmos.color = Color.red;

            if (Pathfinder != null && Pathfinder.seekerManager != null)
            {
                if (Pathfinder.seekerManager.quadTree != null)
                {
                    //quad
                    List<QuadTree> openList = new List<QuadTree> {Pathfinder.seekerManager.quadTree};

                    while (openList.Count > 0)
                    {
                        var current = openList[0];

                        Vector3 origin = new Vector3(Pathfinder.worldStart.x + current.Bounds.xMin,
                            Pathfinder.worldStart.y + Pathfinder.worldHeight + 1,
                            Pathfinder.worldStart.z - current.Bounds.yMin);

                        Gizmos.DrawLine(origin, origin + new Vector3(current.Bounds.width, 0, 0));
                        Gizmos.DrawLine(origin + new Vector3(current.Bounds.width, 0, 0),
                            origin + new Vector3(current.Bounds.width, 0, -current.Bounds.height));
                        Gizmos.DrawLine(origin + new Vector3(current.Bounds.width, 0, -current.Bounds.height),
                            origin + new Vector3(0, 0, -current.Bounds.height));
                        Gizmos.DrawLine(origin + new Vector3(0, 0, -current.Bounds.height), origin);

                        if (current.NodesInUse)
                        {
                            openList.Add(current.Nodes[0]);
                            openList.Add(current.Nodes[1]);
                            openList.Add(current.Nodes[2]);
                            openList.Add(current.Nodes[3]);
                        }

                        openList.Remove(current);
                    }
                }
                else
                {
                    // oct
                    Gizmos.color = new Color(1, 0, 0, 0.04F);
                    List<OcTree> openList = new List<OcTree> {Pathfinder.seekerManager.octTree};

                    while (openList.Count > 0)
                    {
                        var current = openList[0];
                        Gizmos.color = new Color(1, 0, 0, 0.1F + current.Level * 0.1f);
                        Gizmos.DrawCube(new Vector3(current.Bounds[0], current.Bounds[2], current.Bounds[1]),
                            new Vector3(current.Bounds[3] * 2f, current.Bounds[5] * 2f, current.Bounds[4] * 2f));

                        if (current.NodesInUse)
                        {
                            openList.Add(current.Nodes[0]);
                            openList.Add(current.Nodes[1]);
                            openList.Add(current.Nodes[2]);
                            openList.Add(current.Nodes[3]);
                            openList.Add(current.Nodes[4]);
                            openList.Add(current.Nodes[5]);
                            openList.Add(current.Nodes[6]);
                            openList.Add(current.Nodes[7]);
                        }

                        openList.Remove(current);
                    }
                }
            }
        }

        #endregion

        #region PublicMethods

        public void Setup()
        {
            TileManager.WorldData = MultiLevelSectorManager.WorldData = HierarchicalPathfinder.WorldData =
                IntegrationFieldManager.WorldData = FlowFieldManager.WorldData = WorldManager.WorldData = this;
            ColorLists.Setup();
        }

        public void GenerateWorld(Pathfinder pathfinder, bool generateWhileInPlayMode, bool loadCostField)
        {
            Pathfinder = pathfinder;
            float startTime = Time.realtimeSinceStartup;

            TileManager.WorldData = MultiLevelSectorManager.WorldData = HierarchicalPathfinder.WorldData =
                IntegrationFieldManager.WorldData = FlowFieldManager.WorldData = this;
            MultiLevelSectorManager.LowLevel.Manager =
                MultiLevelSectorManager.HighLevel.Manager = MultiLevelSectorManager;

            WorldBuilder.GridWidth = (int) (Pathfinder.worldWidth / Pathfinder.tileSize);
            WorldBuilder.GridLength = (int) (Pathfinder.worldLength / Pathfinder.tileSize);

            WorldBuilder.GenerateWorld(this, generateWhileInPlayMode, loadCostField);

            WorldGenerated = true;
            float endTime = Time.realtimeSinceStartup;
            float timeElapsed = (endTime - startTime);
            Debug.Log("timeElapsed Generate World ms: " + (timeElapsed * 1000f));
        }

        public void GenerateWorldManually(Pathfinder pathfinder, List<Tile[][]> tileGrids,
            List<IntVector2> tileGridOffset, bool autoConnectWorldAreas)
        {
            WorldCreatedManually = true;

            Pathfinder = pathfinder;

            TileManager.WorldData = MultiLevelSectorManager.WorldData = HierarchicalPathfinder.WorldData =
                IntegrationFieldManager.WorldData = FlowFieldManager.WorldData = this;
            MultiLevelSectorManager.LowLevel.Manager =
                MultiLevelSectorManager.HighLevel.Manager = MultiLevelSectorManager;

            WorldBuilder.GridWidth = (int) (Pathfinder.worldWidth / Pathfinder.tileSize);
            WorldBuilder.GridLength = (int) (Pathfinder.worldLength / Pathfinder.tileSize);

            WorldBuilder.GenerateWorldManually(this, tileGrids, tileGridOffset, autoConnectWorldAreas);
            
            WorldGenerated = true;
        }

        public void DrawGizmos()
        {
            // draw World area boundaries

            //Gizmos.color = Color.red;
            //if (worldAreas.Count > 0)
            //{
            //    foreach (WorldArea worldArea in worldAreas)
            //    {
            //        Gizmos.DrawLine(worldArea.origin, worldArea.origin + new Vector3(worldArea.gridWidth * pathfinder.tileSize, 0, 0));
            //        Gizmos.DrawLine(worldArea.origin + new Vector3(worldArea.gridWidth * pathfinder.tileSize, 0, 0), worldArea.origin + new Vector3(worldArea.gridWidth * pathfinder.tileSize, 0, -worldArea.gridLength * pathfinder.tileSize));
            //        Gizmos.DrawLine(worldArea.origin + new Vector3(worldArea.gridWidth * pathfinder.tileSize, 0, -worldArea.gridLength * pathfinder.tileSize), worldArea.origin + new Vector3(0, 0, -worldArea.gridLength * pathfinder.tileSize));
            //        Gizmos.DrawLine(worldArea.origin + new Vector3(0, 0, -worldArea.gridLength * pathfinder.tileSize), worldArea.origin);
            //    }
            //}

            if (WorldGenerated)
            {
                if (Pathfinder.drawTree)
                    DrawTree();

                if (Pathfinder.drawTiles)
                {
                    Gizmos.color = Color.yellow;
                    for (int i = 0; i < WorldBuilder.VisibleTiles.Count; i += 2)
                        Gizmos.DrawLine(WorldBuilder.VisibleTiles[i], WorldBuilder.VisibleTiles[i + 1]);
                }

                // draw sectors
                if (Pathfinder.drawSectors)
                {
                    Gizmos.color = Color.green;
                    foreach (WorldArea worldArea in WorldAreas)
                    {
                        int level = 0;
                        foreach (MultiLevelSector sector in worldArea.SectorGrid[level])
                        {
                            var start = worldArea.Origin +
                                        new Vector3((sector.Left * Pathfinder.tileSize) - (Pathfinder.tileSize * 0.5f),
                                            0,
                                            -((sector.Top * Pathfinder.tileSize) - (Pathfinder.tileSize * 0.5f)));
                            float width = MultiLevelSectorManager.GetSectorWidthAtLevel(worldArea, level) *
                                          Pathfinder.tileSize;
                            float length = MultiLevelSectorManager.GetSectorHeightAtLevel(worldArea, level) *
                                           Pathfinder.tileSize;

                            Gizmos.DrawLine(start, start + new Vector3(width, 0, 0));
                            Gizmos.DrawLine(start + new Vector3(width, 0, 0), start + new Vector3(width, 0, -length));
                            Gizmos.DrawLine(start + new Vector3(width, 0, -length), start + new Vector3(0, 0, -length));
                            Gizmos.DrawLine(start + new Vector3(0, 0, -length), start);
                        }
                    }
                }

                if (Pathfinder.drawSectorNetwork)
                {
                    foreach (WorldArea worldArea in WorldAreas)
                    {
                        if (worldArea.SectorGrid != null && Pathfinder.drawSectorLevel != -1 &&
                            Pathfinder.drawSectorLevel < Pathfinder.maxLevelAmount)
                        {
                            foreach (MultiLevelSector sector in worldArea.SectorGrid[Pathfinder.drawSectorLevel])
                            {
                                Gizmos.color = Color.black;

                                for (int i = 0; i < sector.Connections.Count; i += 2)
                                {
                                    Gizmos.DrawLine(TileManager.GetTileWorldPosition(sector.Connections[i], worldArea),
                                        TileManager.GetTileWorldPosition(sector.Connections[i + 1], worldArea));
                                }

                                if (Pathfinder.worldIsMultiLayered)
                                {
                                    Gizmos.color = Color.blue;
                                    foreach (AbstractNode node in sector.WorldAreaNodes.Keys)
                                    {
                                        Vector3 posStart =
                                            TileManager.GetTileWorldPosition(node.TileConnection, worldArea);

                                        Vector3 posEnd = TileManager.GetTileWorldPosition(
                                            node.NodeConnectionToOtherSector.TileConnection,
                                            WorldAreas[sector.WorldAreaNodes[node]]);

                                        Gizmos.DrawLine(posStart, posEnd);
                                    }
                                }

                                Gizmos.color = Color.black;
                                foreach (AbstractNode node in sector.WorldAreaNodes.Keys)
                                {
                                    Vector3 posStart = TileManager.GetTileWorldPosition(node.TileConnection, worldArea);

                                    foreach (AbstractNode nodeConnected in node.Connections.Keys)
                                    {
                                        if (nodeConnected != node.NodeConnectionToOtherSector)
                                        {
                                            Vector3 posEnd =
                                                TileManager.GetTileWorldPosition(nodeConnected.TileConnection,
                                                    worldArea);

                                            Gizmos.DrawLine(posStart, posEnd);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public WorldData(Pathfinder pathfinder)
        {
            Pathfinder = pathfinder;
        }

        public WorldData(SerializationInfo info, StreamingContext context)
        {
            CostFields = new List<int[][]>();
            CostFields = (info.GetValue("costFields", CostFields.GetType())) as List<int[][]>;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("costFields", CostFields);
        }

        public static WorldData Load(byte[] levelBytes)
        {
            return SerializeUtility.DeserializeObject(levelBytes) as WorldData;
        }

        public byte[] Save()
        {
            return SerializeUtility.SerializeObject(this);
        }

        #endregion
    }
}