using System.Collections.Generic;
using UnityEngine;

namespace FlowPathfinding
{
    public class FlowFieldManager
    {
        #region PrivateVariables

        private readonly Vector2[] _flowVectors =
        {
            Vector2.zero, //0 = 0 
            Vector2.up, //1 = up 
            -Vector2.up, //2 = down 
            Vector2.right, //3 = right 
            new Vector2(1, 1).normalized, //4 = right, top 
            new Vector2(1, -1).normalized, //5 = right, down 
            -Vector2.right, //6 = left 
            new Vector2(-1, 1).normalized, //7 = left, top 
            new Vector2(-1, -1).normalized //8 = left, down
        };

        #endregion

        #region PublicVariables

        public WorldData WorldData = null;
        public readonly List<FlowFieldPath> FlowFieldPaths = new List<FlowFieldPath>();

        #endregion

        #region PublicMethods

        // create flow field
        public void CreateFlowFieldPath(List<Tile> tiles, List<int> sectors, List<int> worldAreas, Tile destination, WorldArea area, int key)
        {
            FlowFieldPath path = new FlowFieldPath();
            path.Create(destination,
                WorldData.IntegrationFieldManager.CreateIntegrationField(tiles, sectors, area),
                CreateFlowField(sectors, area), key);
            FlowFieldPaths.Add(path);
        }

        // add sectors/fields
        public void AddToFlowFieldPath(List<Tile> tiles, List<int> sectors, WorldArea area)
        {
            int amount = WorldData.MultiLevelSectorManager.GetSectorWidthAtLevel(area, 0) *
                         WorldData.MultiLevelSectorManager.GetSectorHeightAtLevel(area, 0);

            FlowFieldPaths[FlowFieldPaths.Count - 1].IntegrationField.AddFields(sectors, amount, tiles, area);
            FlowFieldPaths[FlowFieldPaths.Count - 1].FlowField.AddFields(sectors, amount, area);
        }

        // add sectors/fields
        public void AddToFlowFieldPath(List<Tile> tiles, List<int> sectors, WorldArea area, FlowFieldPath path)
        {
            int amount = WorldData.MultiLevelSectorManager.GetSectorWidthAtLevel(area, 0) *
                         WorldData.MultiLevelSectorManager.GetSectorHeightAtLevel(area, 0);

            path.IntegrationField.AddFields(sectors, amount, tiles, area);
            path.FlowField.AddFields(sectors, amount, area);
        }
        
        // set flow on tiles that point towards a different world area (World area connecting tiles)
        public void AddAreaTilesToFlowFieldPath(Dictionary<IntVector2, List<Tile>> areaConnectionTiles)
        {
            foreach (IntVector2 key in areaConnectionTiles.Keys)
            {
                var current = WorldData.WorldAreas[key.X];
                var previous = WorldData.WorldAreas[key.Y];
                var value = areaConnectionTiles[key];

                for (int i = 0; i < value.Count; i += 2)
                {
                    var flowKey = new IntVector2(key.X, value[i].SectorIndex);
                    var manuallySet = false;

                    //previous curr
                    if (WorldData.WorldCreatedManually)
                    {
                        IntVector2 manualKey = new IntVector2(key.X, key.Y);
                        if (WorldData.ManualWorldAreaConnections.ContainsKey(manualKey))
                        {
                            manuallySet = true;
                            FlowFieldPaths[FlowFieldPaths.Count - 1].FlowField.Field[flowKey][
                                value[i].IndexWithinSector] = WorldData.ManualWorldAreaConnections[manualKey];
                        }
                    }

                    if (!manuallySet)
                    {
                        var lowestCostNode = value[i + 1];
                        FlowFieldPaths[FlowFieldPaths.Count - 1].FlowField.Field[flowKey][value[i].IndexWithinSector] =
                            VectorToDir(
                                new Vector2(
                                        (lowestCostNode.GridPos.X + previous.LeftOffset) -
                                        (value[i].GridPos.X + current.LeftOffset),
                                        (value[i].GridPos.Y + current.TopOffset) -
                                        (lowestCostNode.GridPos.Y + previous.TopOffset))
                                    .normalized); // GetDirBetweenVectors();
                        //new Vector2((lowestCostNode.gridPos.x + previous.leftOffset) - (value[i].gridPos.x + current.leftOffset), (value[i].gridPos.y + current.topOffset) - (lowestCostNode.gridPos.y + previous.topOffset)).normalized;
                    }
                }
            }
        }

        public Vector2 DirToVector(int dir)
        {
            return _flowVectors[dir];
        }

        public int GetDirBetweenVectors(IntVector2 start, IntVector2 end)
        {
            if (start == end)
            {
                return 0;
            }

            if (start.X == end.X)
            {
                if (end.Y > start.Y)
                {
                    return 2;
                }

                return 1;
            }

            if (end.X > start.X)
            {
                if (end.Y > start.Y)
                {
                    return 5;
                }

                if (end.Y < start.Y)
                {
                    return 4;
                }

                return 3;
            }

            if (end.X < start.X)
            {
                if (end.Y > start.Y)
                {
                    return 8;
                }

                if (end.Y < start.Y)
                {
                    return 7;
                }

                return 6;
            }

            return 0;
        }

        #endregion

        #region PrivateMethods

        private FlowField CreateFlowField(List<int> sectors, WorldArea area)
        {
            FlowField flowField = new FlowField();
            flowField.AddFields(sectors,
                WorldData.MultiLevelSectorManager.GetSectorWidthAtLevel(area, 0) *
                WorldData.MultiLevelSectorManager.GetSectorHeightAtLevel(area, 0), area); //.amountOfTilesPerSector);

            return flowField;
        }
        
        private int VectorToDir(Vector2 vec)
        {
            if (vec.x == 0 && vec.y == 0)
            {
                return 0;
            }

            if (vec.x == 0)
            {
                if (vec.y < 0)
                {
                    return 2;
                }

                return 1;
            }

            if (vec.x > 0)
            {
                if (vec.y < 0)
                {
                    return 5;
                }

                if (vec.y > 0)
                {
                    return 4;
                }

                return 3;
            }

            if (vec.x < 0)
            {
                if (vec.y < 0)
                {
                    return 8;
                }

                if (vec.y > 0)
                {
                    return 7;
                }

                return 6;
            }

            return 0;
        }

        #endregion
    }
}