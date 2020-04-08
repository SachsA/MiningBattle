using System.Collections.Generic;
using UnityEngine;

namespace FlowPathfinding
{
    public class WorldBuilder
    {
        #region PrivateVariables

        private bool _worldIsBuilt;

        private WorldData _worldData;

        private readonly List<TemporaryWorldArea> _temporaryWorldLayers = new List<TemporaryWorldArea>();

        #endregion

        #region PublicVariables

        public int GridWidth;
        public int GridLength;

        public readonly List<Vector3> VisibleTiles = new List<Vector3>();

        #endregion

        #region PrivateMethods

        private void GenerateWorldAreaIndexLayers()
        {
            _worldData.LayerWorldAreaIndexes = new List<int[][]>();

            for (float i = 0; i < _worldData.Pathfinder.worldHeight; i += _worldData.Pathfinder.characterHeight)
            {
                int[][] worldIndexArray = new int[GridWidth][];
                for (int j = 0; j < GridWidth; j++)
                    worldIndexArray[j] = new int[GridLength];


                for (int x = 0; x < GridWidth; x++)
                {
                    for (int y = 0; y < GridLength; y++)
                        worldIndexArray[x][y] = -1;
                }

                _worldData.LayerWorldAreaIndexes.Add(worldIndexArray);
            }
        }

        private void GenerateTemporaryWorldLayered()
        {
            // we have temporary world areas that later on will be better refined
            _temporaryWorldLayers.Clear();
            for (float i = 0; i < _worldData.Pathfinder.worldHeight; i += _worldData.Pathfinder.characterHeight)
            {
                TemporaryWorldArea temporaryWorldLayer = new TemporaryWorldArea();
                temporaryWorldLayer.Setup(_worldData);
                _temporaryWorldLayers.Add(temporaryWorldLayer);
            }

            Vector3 startingPoint = _worldData.Pathfinder.worldStart;

            Vector3 rayStartingPoint =
                new Vector3(0, _worldData.Pathfinder.worldStart.y + _worldData.Pathfinder.worldHeight, 0);
            float tileOffset = (_worldData.Pathfinder.tileSize * 0.5f);

            int layerMask = (1 << _worldData.Pathfinder.groundLayer) | (1 << _worldData.Pathfinder.obstacleLayer);
            // find tiles and store them in temporary worldLayers; 
            for (int x = 0; x < _temporaryWorldLayers[0].GridWidth; x++)
            {
                for (int y = 0; y < _temporaryWorldLayers[0].GridHeight; y++)
                {
                    rayStartingPoint.x = startingPoint.x + (x * _worldData.Pathfinder.tileSize) + tileOffset;
                    rayStartingPoint.z = startingPoint.z - (y * _worldData.Pathfinder.tileSize) - tileOffset;

                    float distanceCovered = 0;

                    float blockedLowestY = _worldData.Pathfinder.worldStart.y + _worldData.Pathfinder.worldHeight;
                    while (distanceCovered < _worldData.Pathfinder.worldHeight)
                    {
                        if (Physics.Raycast(rayStartingPoint - new Vector3(0, distanceCovered, 0), Vector3.down,
                            out var hit, _worldData.Pathfinder.worldHeight - distanceCovered, layerMask)
                        ) // (worldHeight - distanceCovered) + 0.2f
                        {
                            if (hit.transform.gameObject.layer == _worldData.Pathfinder.groundLayer)
                            {
                                distanceCovered += hit.distance + _worldData.Pathfinder.characterHeight;


                                int yLayer = _worldData.TileManager.GetHeightLayer(hit.point.y);
                                if (yLayer < 0 || yLayer > _temporaryWorldLayers.Count - 1)
                                {
                                    Debug.Log(
                                        "<color=red> WARNING: piece of world geometry sticks out of the bounding box.</color>");
                                }
                                else
                                {
                                    _temporaryWorldLayers[yLayer].TileGrid[x][y] =
                                        new Tile
                                        {
                                            GridPos = new IntVector2(x, y),
                                            YWorldPos = hit.point.y,
                                            Angle = Vector3.Angle(hit.normal, Vector3.up)
                                        };


                                    if (hit.point.y > blockedLowestY)
                                    {
                                        _temporaryWorldLayers[yLayer].TileGrid[x][y].Blocked = true;
                                        _temporaryWorldLayers[yLayer].TileGrid[x][y].Cost +=
                                            TileManager.TileBlockedValue;
                                    }

                                    _temporaryWorldLayers[yLayer].TileSlotTakenGrid[x][y] = true;
                                }
                            }
                            else
                            {
                                blockedLowestY = hit.point.y - hit.collider.bounds.size.y;
                                distanceCovered += hit.distance + 0.05f;
                            }
                        }
                        else
                            break;
                    }
                }
            }

            // making sure tiles that stick out, beyond the edge of their area get removed, if there is nothing there
            int[] straightDirections = {0, 1, 0, -1, 1, 0, -1, 0};
            foreach (TemporaryWorldArea area in _temporaryWorldLayers)
            {
                for (int x = 0; x < area.GridWidth; x++)
                {
                    for (int y = 0; y < area.GridHeight; y++)
                    {
                        if (area.TileSlotTakenGrid[x][y])
                        {
                            for (int j = 0; j < straightDirections.Length; j += 2)
                            {
                                var checkX = x + straightDirections[j];
                                var checkY = y + straightDirections[j + 1];

                                //if position NOT within area or null or tiles canNOT connect (within range)
                                if (checkX < 0 || checkX > area.GridWidth - 1 || checkY < 0 ||
                                    checkY > area.GridHeight - 1 || !area.TileSlotTakenGrid[checkX][checkY] ||
                                    !_worldData.TileManager.TilesWithinRangeGeneration(area.TileGrid[x][y],
                                        area.TileGrid[checkX][checkY])) // null neighbour
                                {
                                    if (CheckIfTileOverEdge(new Vector3(
                                            startingPoint.x + (x * _worldData.Pathfinder.tileSize) + tileOffset,
                                            area.TileGrid[x][y].YWorldPos,
                                            startingPoint.z - (y * _worldData.Pathfinder.tileSize) - tileOffset),
                                        straightDirections[j], straightDirections[j + 1]))
                                    {
                                        area.TileGrid[x][y] = null;
                                        area.TileSlotTakenGrid[x][y] = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CheckIfTileOverEdge(Vector3 rayStartingPoint, int x, int y)
        {
            bool overEdge = !Physics.Raycast(
                rayStartingPoint + new Vector3(x * 0.5f * _worldData.Pathfinder.tileSize,
                    _worldData.Pathfinder.generationClimbHeight * 0.5f, -y * 0.5f * _worldData.Pathfinder.tileSize),
                Vector3.down, _worldData.Pathfinder.generationClimbHeight + 0.01f,
                1 << _worldData.Pathfinder.groundLayer);

            return overEdge;
        }

        private void GenerateWorldAreas()
        {
            _worldData.WorldAreas.Clear();

            // flat areas are defined
            for (int i = 0; i < _temporaryWorldLayers.Count; i++) // temporaryWorldLayers.Count 
                DefineFlatWorldAreas(_temporaryWorldLayers[i], i);


            // define sloped areas
            for (int i = 0; i < _temporaryWorldLayers.Count; i++) // temporaryWorldLayers.Count 
                DefineSlopedWorldAreas(_temporaryWorldLayers[i], i);
        }

        private void GenerateFlatWorldArea(Tile[][] tileGrid, IntVector2 tileGridOffset, int yLayer)
        {
            WorldArea area = new WorldArea {Index = _worldData.WorldAreas.Count};
            area.FlatAreaCopyTiles(tileGridOffset, tileGrid, yLayer, _worldData);
            _worldData.WorldAreas.Add(area);
        }

        private void DefineFlatWorldAreas(TemporaryWorldArea tempArea, int yLayer)
        {
            for (int x = 0; x < tempArea.GridWidth; x++)
            {
                for (int y = 0; y < tempArea.GridHeight; y++)
                {
                    // tile found, get the area its connected with
                    if (tempArea.TileSlotTakenGrid[x][y] && tempArea.TileGrid[x][y].Angle == 0)
                    {
                        int left = x;
                        int right = x;
                        int top = y;
                        int bot = y;

                        List<IntVector2> closedList = new List<IntVector2>();
                        List<IntVector2> openList = new List<IntVector2> {new IntVector2(x, y)};

                        while (openList.Count > 0)
                        {
                            var current = openList[0];

                            var directionValue = current.X + 1;
                            if (directionValue < tempArea.GridWidth &&
                                tempArea.TileSlotTakenGrid[directionValue][current.Y] &&
                                tempArea.TileGrid[directionValue][current.Y].Angle == 0) // right
                            {
                                tempArea.TileSlotTakenGrid[directionValue][current.Y] = false;
                                _worldData.LayerWorldAreaIndexes[yLayer][directionValue][current.Y] =
                                    _worldData.WorldAreas.Count; // set world Index

                                if (directionValue > right)
                                    right = current.X + 1;

                                openList.Add(new IntVector2(directionValue, current.Y));
                            }

                            directionValue = current.X - 1;
                            if (directionValue > -1 && tempArea.TileSlotTakenGrid[directionValue][current.Y] &&
                                tempArea.TileGrid[directionValue][current.Y].Angle == 0) // left
                            {
                                tempArea.TileSlotTakenGrid[directionValue][current.Y] = false;
                                _worldData.LayerWorldAreaIndexes[yLayer][directionValue][current.Y] =
                                    _worldData.WorldAreas.Count; // set world Index

                                if (directionValue < left)
                                    left = directionValue;
                                openList.Add(new IntVector2(directionValue, current.Y));
                            }

                            directionValue = current.Y - 1;
                            if (directionValue > -1 && tempArea.TileSlotTakenGrid[current.X][directionValue] &&
                                tempArea.TileGrid[current.X][directionValue].Angle == 0) // up
                            {
                                tempArea.TileSlotTakenGrid[current.X][directionValue] = false;
                                _worldData.LayerWorldAreaIndexes[yLayer][current.X][directionValue] =
                                    _worldData.WorldAreas.Count; // set world Index

                                if (directionValue < top)
                                    top = directionValue;
                                openList.Add(new IntVector2(current.X, directionValue));
                            }

                            directionValue = current.Y + 1;
                            if (directionValue < tempArea.GridHeight &&
                                tempArea.TileSlotTakenGrid[current.X][directionValue] &&
                                tempArea.TileGrid[current.X][directionValue].Angle == 0) // down
                            {
                                tempArea.TileSlotTakenGrid[current.X][directionValue] = false;
                                _worldData.LayerWorldAreaIndexes[yLayer][current.X][directionValue] =
                                    _worldData.WorldAreas.Count; // set world Index

                                if (directionValue > bot)
                                    bot = current.Y + 1;
                                openList.Add(new IntVector2(current.X, directionValue));
                            }

                            closedList.Add(current);
                            openList.Remove(current);
                        }

                        if (closedList.Count == 1 && tempArea.TileGrid[closedList[0].X][closedList[0].Y].Angle != 0)
                        {
                        }
                        else
                        {
                            // open list is empty, define world area
                            WorldArea area = new WorldArea {Index = _worldData.WorldAreas.Count};
                            area.FlatAreaCopyTiles(left, right, top, bot, closedList, tempArea.TileGrid, yLayer,
                                _worldData);
                            _worldData.WorldAreas.Add(area);
                        }
                    }
                }
            }
        }

        private void DefineSlopedWorldAreas(TemporaryWorldArea tempArea, int yLayer)
        {
            for (int x = 0; x < tempArea.GridWidth; x++)
            {
                for (int y = 0; y < tempArea.GridHeight; y++)
                {
                    // tile found, get the area its connected with
                    if (tempArea.TileSlotTakenGrid[x][y])
                    {
                        tempArea.TileSlotTakenGrid[x][y] = false;
                        _worldData.LayerWorldAreaIndexes[yLayer][x][y] =
                            _worldData.WorldAreas.Count; // set world Index
                        // open list is empty, define world area
                        WorldArea area = DefineSlopedWorldArea(x, y, tempArea.TileGrid[x][y].Angle, yLayer);
                        if (area != null)
                            _worldData.WorldAreas.Add(area);
                    }
                }
            }
        }

        private WorldArea DefineSlopedWorldArea(int xParam, int yParam, float angle, int yLayer)
        {
            //Debug.Log("angle  " + angle);
            float angleLeanWay = 0.1f;
            int left = xParam;
            int right = xParam;
            int top = yParam;
            int bot = yParam;

            List<List<IntVector2>> closedList = new List<List<IntVector2>>();
            List<IntVector2> openList = new List<IntVector2> {new IntVector2(xParam, yParam)};

            for (int i = yLayer; i < _temporaryWorldLayers.Count; i++)
            {
                List<IntVector2> closedListTemp = new List<IntVector2>();
                TemporaryWorldArea tempArea = _temporaryWorldLayers[i];

                int directionValue;
                if (i != yLayer
                ) // find all tiles around previous defined rectangle of tiles, so we can start of these tiles for our while loop search below
                {
                    int tempTop = top;
                    int tempBot = bot;
                    int tempLeft = left;
                    int tempRight = right;

                    // top row
                    directionValue = top - 1;
                    if (directionValue > -1)
                    {
                        for (int x = left; x < right + 1; x++)
                        {
                            if (tempArea.TileSlotTakenGrid[x][directionValue] &&
                                tempArea.TileGrid[x][directionValue].Angle > angle - angleLeanWay &&
                                tempArea.TileGrid[x][directionValue].Angle < angle + angleLeanWay)
                            {
                                tempTop = directionValue;

                                tempArea.TileSlotTakenGrid[x][directionValue] = false;
                                _worldData.LayerWorldAreaIndexes[i][x][directionValue] = _worldData.WorldAreas.Count;
                                // layerWorldAreaIndexes[yLayer][X][directionValue] = worldAreas.Count;
                                // set world Index
                                openList.Add(new IntVector2(x, directionValue));
                            }
                        }
                    }

                    // bot row
                    directionValue = bot + 1;
                    if (directionValue < tempArea.GridHeight)
                    {
                        for (int x = left; x < right + 1; x++)
                        {
                            if (tempArea.TileSlotTakenGrid[x][directionValue] &&
                                tempArea.TileGrid[x][directionValue].Angle > angle - angleLeanWay &&
                                tempArea.TileGrid[x][directionValue].Angle < angle + angleLeanWay)
                            {
                                tempBot = directionValue;

                                tempArea.TileSlotTakenGrid[x][directionValue] = false;
                                _worldData.LayerWorldAreaIndexes[i][x][directionValue] = _worldData.WorldAreas.Count;
                                //layerWorldAreaIndexes[yLayer][X][directionValue] = worldAreas.Count;
                                // set world Index
                                openList.Add(new IntVector2(x, directionValue));
                            }
                        }
                    }

                    // left row
                    directionValue = left - 1;
                    if (directionValue > -1)
                    {
                        for (int y = top; y < bot + 1; y++)
                        {
                            if (tempArea.TileSlotTakenGrid[directionValue][y] &&
                                tempArea.TileGrid[directionValue][y].Angle > angle - angleLeanWay &&
                                tempArea.TileGrid[directionValue][y].Angle < angle + angleLeanWay)
                            {
                                tempLeft = directionValue;

                                tempArea.TileSlotTakenGrid[directionValue][y] = false;
                                _worldData.LayerWorldAreaIndexes[i][directionValue][y] = _worldData.WorldAreas.Count;
                                //layerWorldAreaIndexes[yLayer][directionValue][Y] = worldAreas.Count;
                                //set world Index
                                openList.Add(new IntVector2(directionValue, y));
                            }
                        }
                    }

                    // right row
                    directionValue = right + 1;
                    if (directionValue < tempArea.GridWidth)
                    {
                        for (int y = top; y < bot + 1; y++)
                        {
                            if (tempArea.TileSlotTakenGrid[directionValue][y] &&
                                tempArea.TileGrid[directionValue][y].Angle > angle - angleLeanWay &&
                                tempArea.TileGrid[directionValue][y].Angle < angle + angleLeanWay)
                            {
                                tempRight = directionValue;

                                tempArea.TileSlotTakenGrid[directionValue][y] = false;
                                _worldData.LayerWorldAreaIndexes[i][directionValue][y] = _worldData.WorldAreas.Count;
                                // layerWorldAreaIndexes[yLayer][directionValue][Y] = worldAreas.Count;
                                // set world Index
                                openList.Add(new IntVector2(directionValue, y));
                            }
                        }
                    }

                    right = tempRight;
                    left = tempLeft;
                    bot = tempBot;
                    top = tempTop;
                }


                if (openList.Count == 0) // nothing left to find
                {
                    var area = new WorldArea {Index = _worldData.WorldAreas.Count};
                    area.SlopedAreaCopyTiles(angle, left, right, top, bot, closedList, _temporaryWorldLayers, yLayer,
                        _worldData);

                    //SubdivideSlopedWorldArea(area);
                    return area;
                }


                //search for tiles with matching angle in temporary world layer
                while (openList.Count > 0)
                {
                    var current = openList[0];

                    directionValue = current.X + 1;
                    if (directionValue < tempArea.GridWidth && tempArea.TileSlotTakenGrid[directionValue][current.Y] &&
                        tempArea.TileGrid[directionValue][current.Y].Angle > angle - angleLeanWay &&
                        tempArea.TileGrid[directionValue][current.Y].Angle < angle + angleLeanWay) // right
                    {
                        if (directionValue > right)
                            right = directionValue;

                        tempArea.TileSlotTakenGrid[directionValue][current.Y] = false;
                        _worldData.LayerWorldAreaIndexes[i][directionValue][current.Y] =
                            _worldData.WorldAreas.Count; // set world Index

                        openList.Add(new IntVector2(directionValue, current.Y));
                    }

                    directionValue = current.X - 1;
                    if (directionValue > -1 && tempArea.TileSlotTakenGrid[directionValue][current.Y] &&
                        tempArea.TileGrid[directionValue][current.Y].Angle > angle - angleLeanWay &&
                        tempArea.TileGrid[directionValue][current.Y].Angle < angle + angleLeanWay) // left
                    {
                        tempArea.TileSlotTakenGrid[directionValue][current.Y] = false;
                        _worldData.LayerWorldAreaIndexes[i][directionValue][current.Y] =
                            _worldData.WorldAreas.Count; // set world Index

                        if (directionValue < left)
                            left = directionValue;
                        openList.Add(new IntVector2(directionValue, current.Y));
                    }

                    directionValue = current.Y - 1;
                    if (directionValue > -1 && tempArea.TileSlotTakenGrid[current.X][directionValue] &&
                        tempArea.TileGrid[current.X][directionValue].Angle > angle - angleLeanWay &&
                        tempArea.TileGrid[current.X][directionValue].Angle < angle + angleLeanWay) // up
                    {
                        tempArea.TileSlotTakenGrid[current.X][directionValue] = false;
                        _worldData.LayerWorldAreaIndexes[i][current.X][directionValue] =
                            _worldData.WorldAreas.Count; // set world Index

                        if (directionValue < top)
                            top = directionValue;
                        openList.Add(new IntVector2(current.X, directionValue));
                    }

                    directionValue = current.Y + 1;
                    if (directionValue < tempArea.GridHeight && tempArea.TileSlotTakenGrid[current.X][directionValue] &&
                        tempArea.TileGrid[current.X][directionValue].Angle > angle - angleLeanWay &&
                        tempArea.TileGrid[current.X][directionValue].Angle < angle + angleLeanWay) // down
                    {
                        tempArea.TileSlotTakenGrid[current.X][directionValue] = false;
                        _worldData.LayerWorldAreaIndexes[i][current.X][directionValue] =
                            _worldData.WorldAreas.Count; // set world Index

                        if (directionValue > bot)
                            bot = directionValue;
                        openList.Add(new IntVector2(current.X, directionValue));
                    }

                    closedListTemp.Add(current);
                    openList.Remove(current);
                }

                closedList.Add(closedListTemp);
            }

            return null;
        }

        private void CreateSectorGraph()
        {
            foreach (var t in _worldData.WorldAreas)
            {
                _worldData.MultiLevelSectorManager.SetupSectorConnections(t);
            }
        }

        private void ConnectWorldAreas(bool manuallySet)
        {
            foreach (var area in _worldData.WorldAreas)
            {
                //inner tiles check
                for (int x = 0; x < area.GridWidth; x++)
                {
                    for (int y = 0; y < area.GridLength; y++)
                    {
                        // null tile space, there might be an other tile of other world area here
                        if (area.TileGrid[x][y] == null)
                        {
                            Debug.Log("Area.TileGrid is null");
                            // does this space connect to a tile?
                            foreach (Tile neighbour in _worldData.TileManager.GetStraightNeighbours(x, y, area))
                                FindWorldAreaConnection(x, y, neighbour, area, manuallySet);
                        }
                    }
                }

                // perimeter check
                //left
                if (area.LeftOffset - 1 > -1) // dont leave world
                {
                    for (int y = 0; y < area.GridLength; y++)
                    {
                        if (area.TileGrid[0][y] != null)
                        {
                            FindWorldAreaConnection(-1, y, area.TileGrid[0][y], area, manuallySet);
                        }
                    }
                }

                //right
                if (area.LeftOffset + area.GridWidth < GridWidth) // dont leave world
                {
                    for (int y = 0; y < area.GridLength; y++)
                    {
                        if (area.TileGrid[area.GridWidth - 1][y] != null)
                        {
                            FindWorldAreaConnection(area.GridWidth, y, area.TileGrid[area.GridWidth - 1][y], area,
                                manuallySet);
                        }
                    }
                }

                //top
                if (area.TopOffset - 1 > -1) // dont leave world
                {
                    for (int x = 0; x < area.GridWidth; x++)
                    {
                        if (area.TileGrid[x][0] != null)
                        {
                            FindWorldAreaConnection(x, -1, area.TileGrid[x][0], area, manuallySet);
                        }
                    }
                }

                //bot
                if (area.TopOffset + area.GridLength < GridLength) // dont leave world
                {
                    for (int x = 0; x < area.GridWidth; x++)
                    {
                        if (area.TileGrid[x][area.GridLength - 1] != null)
                        {
                            FindWorldAreaConnection(x, area.GridLength, area.TileGrid[x][area.GridLength - 1], area,
                                manuallySet);
                        }
                    }
                }
            }
        }

        private void FindWorldAreaConnection(int x, int y, Tile neighbour, WorldArea area, bool forceConnection)
        {
            //Debug.Log("area  " + area.index);
            WorldArea area2 = _worldData.TileManager.GetWorldAreaAtGuaranteedPosition(x + area.LeftOffset,
                neighbour.YWorldPos, y + area.TopOffset);
            if (area2 != null)
                ConnectWorldAreaTiles(neighbour, area, x + area.LeftOffset, y + area.TopOffset, area2, forceConnection);
        }

        private void ConnectWorldAreaTiles(Tile tile1, WorldArea area1, int worldX, int worldY, WorldArea area2,
            bool forceConnection)
        {
            //Debug.Log("connect world area tiles   worldX " + worldX + "  worldY  " + worldY + "    area1 " + area1.index);

            Tile tile2 = area2.TileGrid[worldX - area2.LeftOffset][worldY - area2.TopOffset];
            if (_worldData.TileManager.TilesWithinRangeGeneration(tile1, tile2)) // are the tiles within reach of each other?
            {
                // positions of each tile in their own area
                IntVector2 tile1Vector2 = tile1.GridPos; //.x, tile1.gridPos.y);
                IntVector2 tile2Vector2 = tile2.GridPos; //.x, tile2.gridPos.y);
                bool goOn = false;


                if (forceConnection) // we want to connect these 2   2D areas, but the system normally never has to connect flat areas next to each other, as they would be one and the same
                {
                    goOn = true;
                }
                else
                {
                    if (!area1.FlatArea)
                    {
                        if (area1.AngleDirectionX)
                        {
                            if (tile1.GridPos.Y + area1.TopOffset == tile2.GridPos.Y + area2.TopOffset)
                            {
                                goOn = true;
                            }
                        }
                        else
                        {
                            if (tile1.GridPos.X + area1.LeftOffset == tile2.GridPos.X + area2.LeftOffset)
                            {
                                goOn = true;
                            }
                        }
                    }
                    else if (!area2.FlatArea)
                    {
                        if (area2.AngleDirectionX)
                        {
                            if (tile1.GridPos.Y + area1.TopOffset == tile2.GridPos.Y + area2.TopOffset)
                            {
                                goOn = true;
                            }
                        }
                        else
                        {
                            if (tile1.GridPos.X + area1.LeftOffset == tile2.GridPos.X + area2.LeftOffset)
                            {
                                goOn = true;
                            }
                        }
                    }
                }

                if (goOn)
                {
                    // search from area 1, it has no data on this key/tile yet, so connect them
                    if (!area1.WorldAreaTileConnections.ContainsKey(tile1Vector2))
                    {
                        area1.WorldAreaTileConnections.Add(tile1Vector2,
                            new List<IntVector3> {new IntVector3(tile2.GridPos.X, tile2.GridPos.Y, area2.Index)});

                        if (!area2.WorldAreaTileConnections.ContainsKey(tile2Vector2))
                            area2.WorldAreaTileConnections.Add(tile2Vector2,
                                new List<IntVector3> {new IntVector3(tile1.GridPos.X, tile1.GridPos.Y, area1.Index)});
                        else
                            area2.WorldAreaTileConnections[tile2Vector2]
                                .Add(new IntVector3(tile1.GridPos.X, tile1.GridPos.Y, area1.Index));


                        if (!area1.WorldAreasConnectedIndexes.Contains(area2.Index))
                            area1.WorldAreasConnectedIndexes.Add(area2.Index);
                        if (!area2.WorldAreasConnectedIndexes.Contains(area1.Index))
                            area2.WorldAreasConnectedIndexes.Add(area1.Index);
                    }
                    else
                    {
                        // we already have data on this key/tile, but is it connected yet to the tile we are examining?
                        IntVector3 vectorToAdd = new IntVector3(tile2.GridPos.X, tile2.GridPos.Y, area2.Index);
                        bool alreadyConnected = false;
                        foreach (IntVector3 vector3 in area1.WorldAreaTileConnections[tile1Vector2])
                        {
                            if (vector3 == vectorToAdd)
                            {
                                alreadyConnected = true;
                                break;
                            }
                        }

                        // they are not connected yet, so connect them
                        if (!alreadyConnected)
                        {
                            area1.WorldAreaTileConnections[new IntVector2(tile1.GridPos.X, tile1.GridPos.Y)]
                                .Add(new IntVector3(tile2.GridPos.X, tile2.GridPos.Y, area2.Index));

                            if (!area2.WorldAreaTileConnections.ContainsKey(tile2Vector2))
                                area2.WorldAreaTileConnections.Add(tile2Vector2,
                                    new List<IntVector3>
                                        {new IntVector3(tile1.GridPos.X, tile1.GridPos.Y, area1.Index)});
                            else
                                area2.WorldAreaTileConnections[tile2Vector2]
                                    .Add(new IntVector3(tile1.GridPos.X, tile1.GridPos.Y, area1.Index));


                            if (!area1.WorldAreasConnectedIndexes.Contains(area2.Index))
                                area1.WorldAreasConnectedIndexes.Add(area2.Index);
                            if (!area2.WorldAreasConnectedIndexes.Contains(area1.Index))
                                area2.WorldAreasConnectedIndexes.Add(area1.Index);
                        }
                    }
                }
            }
        }

        private void CombineWorldAreaConnectionsWithSectorGraph()
        {
            CreateWorldAreaConnectionGroups();

            CreateWorldAreaConnectionNodes();
        }

        private void CreateWorldAreaConnectionGroups()
        {
            List<IntVector2> openList = new List<IntVector2>();
            Dictionary<IntVector2, IntVector3> leftOverKeysDictionary = new Dictionary<IntVector2, IntVector3>();

            foreach (WorldArea area in _worldData.WorldAreas)
            {
                foreach (int connectedWorldAreaIndex in area.WorldAreasConnectedIndexes)
                {
                    // find legit of connections
                    leftOverKeysDictionary.Clear();
                    foreach (IntVector2 key in area.WorldAreaTileConnections.Keys)
                    {
                        foreach (IntVector3 vector3 in area.WorldAreaTileConnections[key])
                        {
                            if (vector3.Z == connectedWorldAreaIndex)
                            {
                                leftOverKeysDictionary.Add(key, vector3);
                                break;
                            }
                        }
                    }

                    var leftOverKeysList = new List<IntVector2>(leftOverKeysDictionary.Keys);
                    Tile currentTile = null;
                    //find rows/groups of connections for each sector
                    foreach (IntVector2 key in leftOverKeysList)
                    {
                        if (leftOverKeysDictionary.ContainsKey(key)) // this key is still left over
                        {
                            var group = new List<IntVector2>();
                            openList.Add(key);
                            leftOverKeysDictionary.Remove(key);
                            bool newSearch = true;
                            bool swapInsertion = false;
                            bool swapTo0 = false;

                            while (openList.Count > 0) // find legit neighbours until no longer possible
                            {
                                var current = openList[0];
                                currentTile = area.TileGrid[current.X][current.Y];

                                int neighboursFound = 0;
                                foreach (Tile neighbour in _worldData.TileManager.GetStraightNeighbours(current.X,
                                    current.Y, area))
                                {
                                    if (neighbour.SectorIndex == currentTile.SectorIndex)
                                    {
                                        if (leftOverKeysDictionary.ContainsKey(neighbour.GridPos)
                                        ) // neighbour is part of the connection
                                        {
                                            neighboursFound++;
                                            openList.Add(neighbour.GridPos);

                                            leftOverKeysDictionary.Remove(neighbour.GridPos);
                                        }
                                    }
                                }

                                if (swapTo0)
                                    group.Insert(0, current);
                                else
                                    group.Add(current);

                                if (swapInsertion)
                                {
                                    if (neighboursFound == 0)
                                        swapInsertion = false;

                                    swapTo0 = !swapTo0;
                                }

                                //group.Add(current);
                                openList.Remove(current);


                                // order way tiles are added, we only do this at the first search of a new group
                                if (newSearch && openList.Count == 2) // somewhere not at the end of a group
                                    swapInsertion = true;

                                newSearch = false;
                            }

                            if (currentTile != null)
                            {
                                IntVector2 groupKey = new IntVector2(connectedWorldAreaIndex, currentTile.SectorIndex);

                                if (!area.GroupsInSectors.ContainsKey(groupKey))
                                    area.GroupsInSectors.Add(groupKey, new List<List<IntVector2>>());

                                area.GroupsInSectors[groupKey].Add(group);
                            }
                        }
                    }
                }
            }
        }

        private void CreateWorldAreaConnectionNodes()
        {
            foreach (WorldArea area in _worldData.WorldAreas)
            {
                foreach (IntVector2 key in area.GroupsInSectors.Keys)
                {
                    if (area.Index < key.X) // making sure world areas connect only 1 time with each other
                    {
                        foreach (List<IntVector2> group in area.GroupsInSectors[key])
                        {
                            GenerateWordConnectingNodesPerGroup(area, group, key);
                        }
                    }
                }
            }
        }

        private IntVector3 FindCorrectAreaNodeConnection(List<IntVector3> nodeSpots, int worldAreaIndex)
        {
            foreach (IntVector3 nodeSpot in nodeSpots)
            {
                if (nodeSpot.Z == worldAreaIndex &&
                    !_worldData.WorldAreas[worldAreaIndex].TileGrid[nodeSpot.X][nodeSpot.Y].Blocked)
                    return nodeSpot;
            }

            Debug.Log("shouldn't get here  " + "other pos  " + nodeSpots[0].X + "  " + nodeSpots[0].Y +
                      "   other world area  " + worldAreaIndex);
            return new IntVector3(0, 0, 0); // null
        }

        private void VisualizeTiles()
        {
            Vector3 end = new Vector3();
            VisibleTiles.Clear();
            Vector3 offset = new Vector3(-_worldData.Pathfinder.tileSize * 0.5f, 0.15f,
                _worldData.Pathfinder.tileSize * 0.5f);

            foreach (WorldArea worldArea in _worldData.WorldAreas)
            {
                var extraSlopedOffset = Vector3.zero;
                for (int x = 0; x < worldArea.GridWidth; x++)
                {
                    for (int y = 0; y < worldArea.GridLength; y++)
                    {
                        if (worldArea.TileGrid[x][y] != null)
                        {
                            var start = _worldData.TileManager.GetTileWorldPosition(worldArea.TileGrid[x][y],
                                worldArea);

                            if (x + 1 < worldArea.GridWidth)
                            {
                                if (worldArea.TileGrid[x + 1][y] == null)
                                    VisualizeTilesXStop(worldArea, worldArea.TileGrid[x][y], start, offset);
                                else
                                {
                                    if (!worldArea.FlatArea)
                                    {
                                        if (worldArea.AnglePositive < 0)
                                            extraSlopedOffset.y =
                                                Mathf.Tan(worldArea.TileGrid[x][y].Angle * (Mathf.PI / 180)) *
                                                _worldData.Pathfinder.tileSize * 0.5f;
                                        else
                                            extraSlopedOffset.y =
                                                -Mathf.Tan(worldArea.TileGrid[x][y].Angle * (Mathf.PI / 180)) *
                                                _worldData.Pathfinder.tileSize * 0.5f;
                                    }

                                    end = _worldData.TileManager.GetTileWorldPosition(worldArea.TileGrid[x + 1][y],
                                        worldArea);
                                }

                                VisibleTiles.Add(start + offset + extraSlopedOffset);
                                VisibleTiles.Add(end + offset + extraSlopedOffset);
                            }
                            else
                                VisualizeTilesXStop(worldArea, worldArea.TileGrid[x][y], start, offset);


                            if (y + 1 < worldArea.GridLength)
                            {
                                if (worldArea.TileGrid[x][y + 1] == null)
                                    VisualizeTilesZStop(worldArea, worldArea.TileGrid[x][y], start, offset);
                                else
                                {
                                    if (!worldArea.FlatArea)
                                    {
                                        if (worldArea.AnglePositive < 0)
                                            extraSlopedOffset.y =
                                                Mathf.Tan(worldArea.TileGrid[x][y].Angle * (Mathf.PI / 180)) *
                                                _worldData.Pathfinder.tileSize * 0.5f;
                                        else
                                            extraSlopedOffset.y =
                                                -Mathf.Tan(worldArea.TileGrid[x][y].Angle * (Mathf.PI / 180)) *
                                                _worldData.Pathfinder.tileSize * 0.5f;
                                    }

                                    end = _worldData.TileManager.GetTileWorldPosition(worldArea.TileGrid[x][y + 1],
                                        worldArea);
                                }

                                VisibleTiles.Add(start + offset + extraSlopedOffset);
                                VisibleTiles.Add(end + offset + extraSlopedOffset);
                            }
                            else
                                VisualizeTilesZStop(worldArea, worldArea.TileGrid[x][y], start, offset);
                        }
                    }
                }
            }
        }

        private void VisualizeTilesXStop(WorldArea area, Tile startTile, Vector3 start, Vector3 offset)
        {
            Vector3 end;
            Vector3 newEnd;
            if (area.FlatArea)
            {
                // x continue line
                end = start + new Vector3(_worldData.Pathfinder.tileSize, 0, 0);

                VisibleTiles.Add(start + offset);
                VisibleTiles.Add(end + offset);

                //// from end to z direction
                newEnd = end; //
                newEnd.z -= _worldData.Pathfinder.tileSize;

                VisibleTiles.Add(end + offset);
                VisibleTiles.Add(newEnd + offset);
            }
            else
            {
                Vector3 extraSlopedOffset = new Vector3(0, 0, 0);

                if (area.AnglePositive < 0)
                    extraSlopedOffset.y = Mathf.Tan(startTile.Angle * (Mathf.PI / 180)) *
                                          _worldData.Pathfinder.tileSize * 0.5f;
                else
                    extraSlopedOffset.y = -Mathf.Tan(startTile.Angle * (Mathf.PI / 180)) *
                                          _worldData.Pathfinder.tileSize * 0.5f;


                if (area.AngleDirectionX)
                {
                    end = start;
                    Vector2 values = TileCosSin(area, startTile.Angle);

                    if (area.AnglePositive == 1)
                    {
                        end.y += values.y;
                        end.x -= values.x;
                    }
                    else
                    {
                        end.y -= values.y;
                        end.x += values.x;
                    }

                    VisibleTiles.Add(start + offset + extraSlopedOffset);
                    VisibleTiles.Add(end + offset + extraSlopedOffset);


                    newEnd = end;
                    newEnd.z -= _worldData.Pathfinder.tileSize;
                    VisibleTiles.Add(end + offset + extraSlopedOffset);
                    VisibleTiles.Add(newEnd + offset + extraSlopedOffset);
                }
                else
                {
                    end = start + new Vector3(_worldData.Pathfinder.tileSize, 0, 0);

                    VisibleTiles.Add(start + offset + extraSlopedOffset);
                    VisibleTiles.Add(end + offset + extraSlopedOffset);

                    // from end to z direction
                    newEnd = end;
                    Vector2 values = TileCosSin(area, startTile.Angle); // *area.angledAreaResolutionDifference;

                    if (area.AnglePositive == -1)
                    {
                        newEnd.y -= values.y;
                        newEnd.z -= values.x;
                    }
                    else
                    {
                        newEnd.y += values.y;
                        newEnd.z += values.x;
                    }

                    VisibleTiles.Add(end + offset + extraSlopedOffset);
                    VisibleTiles.Add(newEnd + offset + extraSlopedOffset);
                }
            }
        }

        private void VisualizeTilesZStop(WorldArea area, Tile startTile, Vector3 start, Vector3 offset)
        {
            Vector3 end;
            Vector3 newEnd;
            if (area.FlatArea)
            {
                // x continue line
                end = start + new Vector3(0, 0, -_worldData.Pathfinder.tileSize);

                VisibleTiles.Add(start + offset);
                VisibleTiles.Add(end + offset);

                //// from end to z direction
                newEnd = end; //
                newEnd.x += _worldData.Pathfinder.tileSize;

                VisibleTiles.Add(end + offset);
                VisibleTiles.Add(newEnd + offset);
            }
            else
            {
                Vector3 extraSlopedOffset = new Vector3(0, 0, 0);

                if (area.AnglePositive < 0)
                    extraSlopedOffset.y = Mathf.Tan(startTile.Angle * (Mathf.PI / 180)) *
                                          _worldData.Pathfinder.tileSize * 0.5f;
                else
                    extraSlopedOffset.y = -Mathf.Tan(startTile.Angle * (Mathf.PI / 180)) *
                                          _worldData.Pathfinder.tileSize * 0.5f;


                if (area.AngleDirectionX)
                {
                    end = start + new Vector3(0, 0, -_worldData.Pathfinder.tileSize);

                    VisibleTiles.Add(start + offset + extraSlopedOffset);
                    VisibleTiles.Add(end + offset + extraSlopedOffset);

                    //// from end to z direction
                    newEnd = end;
                    Vector2 values = TileCosSin(area, startTile.Angle);


                    if (area.AnglePositive == 1)
                    {
                        newEnd.y += values.y;
                        newEnd.x -= values.x;
                    }
                    else
                    {
                        newEnd.y -= values.y;
                        newEnd.x += values.x;
                    }

                    VisibleTiles.Add(end + offset + extraSlopedOffset);
                    VisibleTiles.Add(newEnd + offset + extraSlopedOffset);
                }
                else
                {
                    end = start;
                    Vector2 values = TileCosSin(area, startTile.Angle);

                    if (area.AnglePositive == -1)
                    {
                        end.y -= values.y;
                        end.z -= values.x;
                    }
                    else
                    {
                        end.y += values.y;
                        end.z += values.x;
                    }

                    VisibleTiles.Add(start + offset + extraSlopedOffset);
                    VisibleTiles.Add(end + offset + extraSlopedOffset);


                    newEnd = end;
                    newEnd.x += _worldData.Pathfinder.tileSize;
                    VisibleTiles.Add(end + offset + extraSlopedOffset);
                    VisibleTiles.Add(newEnd + offset + extraSlopedOffset);
                }
            }
        }

        private Vector2 TileCosSin(WorldArea area, float angle)
        {
            Vector2 value = new Vector2
            {
                x = Mathf.Cos(angle * (Mathf.PI / 180)) * _worldData.Pathfinder.tileSize *
                    area.AngledAreaResolutionDifference * -area.AnglePositive,
                y = Mathf.Sin(angle * (Mathf.PI / 180)) * _worldData.Pathfinder.tileSize *
                    area.AngledAreaResolutionDifference
            };

            return value;
        }

        private void DefineFlatWorld()
        {
            _worldData.WorldAreas.Clear();

            WorldArea area = new WorldArea {Index = 0};
            area.SetValuesFlatWorld(0, GridWidth, 0, GridLength, _worldData);
            _worldData.MultiLevelSectorManager.SetupSectorsWorldArea(area);

            Vector3 startingPoint = _worldData.Pathfinder.worldStart;

            Vector3 rayStartingPoint =
                new Vector3(0, _worldData.Pathfinder.worldStart.y + _worldData.Pathfinder.worldHeight, 0);
            float tileOffset = (_worldData.Pathfinder.tileSize * 0.5f);

            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridLength; y++)
                {
                    rayStartingPoint.x = startingPoint.x + (x * _worldData.Pathfinder.tileSize) + tileOffset;
                    rayStartingPoint.z = startingPoint.z - (y * _worldData.Pathfinder.tileSize) - tileOffset;

                    if (Physics.Raycast(rayStartingPoint, Vector3.down, out var hit,
                        _worldData.Pathfinder.worldHeight + 0.15f, 1 << _worldData.Pathfinder.groundLayer)
                    ) // (worldHeight - distanceCovered) + 0.2f
                    {
                        area.TileGrid[x][y] = new Tile
                        {
                            GridPos = {X = x, Y = y},
                            IntegrationValue = TileManager.TileResetIntegrationValue,
                            WorldAreaIndex = 0,
                            YWorldPos = hit.point.y,
                            Angle = Vector3.Angle(hit.normal, Vector3.up)
                        };


                        int sectorX = Mathf.FloorToInt(x / (float) area.LevelDimensions[0][0]); // sectorWidth
                        int sectorY = Mathf.FloorToInt(y / (float) area.LevelDimensions[0][1]); // sectorHeight

                        area.TileGrid[x][y].SectorIndex =
                            (sectorY * area.LevelDimensions[0][2]) + sectorX; // *sectorGridWidth

                        // Debug.Log("world index  " + index + "   tileGrid[x][y].sector  " + tileGrid[x][y].sector + "  levelDimensions[0][0] " + levelDimensions[0][0]);
                        MultiLevelSector sector = area.SectorGrid[0][area.TileGrid[x][y].SectorIndex];
                        int deltaX = x - sector.Left;
                        int deltaY = y - sector.Top;
                        area.TileGrid[x][y].IndexWithinSector = (deltaY * sector.TilesInWidth) + deltaX;
                    }
                }
            }

            _worldData.WorldAreas.Add(area);
        }

        private Vector2 WorldSideToVector2(WorldArea.Side areaSide)
        {
            switch (areaSide)
            {
                case WorldArea.Side.Top:
                {
                    return Vector2.up;
                }
                case WorldArea.Side.Down:
                {
                    return -Vector2.up;
                }
                case WorldArea.Side.Left:
                {
                    return -Vector2.right;
                }
                case WorldArea.Side.Right:
                {
                    return Vector2.right;
                }
            }

            return Vector2.zero;
        }

        private int WorldSideToDir(WorldArea.Side areaSide)
        {
            switch (areaSide)
            {
                case WorldArea.Side.Top:
                {
                    return 1;
                }
                case WorldArea.Side.Down:
                {
                    return 2;
                }
                case WorldArea.Side.Left:
                {
                    return 6;
                }
                case WorldArea.Side.Right:
                {
                    return 3;
                }
            }

            return 0;
        }

        #endregion

        #region PublicMethods

        public void GenerateWorld(WorldData worldData, bool generateWhileInPlayMode, bool loadCostField)
        {
            _worldData = worldData;
            _temporaryWorldLayers.Clear();
            VisibleTiles.Clear();

            if (_worldData.Pathfinder.worldIsMultiLayered)
            {
                GenerateWorldAreaIndexLayers();
                GenerateTemporaryWorldLayered();
                GenerateWorldAreas();

                // load in saved cost field values 
                bool costFieldsDontMatchWorld = false;
                if (generateWhileInPlayMode && loadCostField && _worldData.CostFields.Count > 0)
                {
                    foreach (WorldArea area in _worldData.WorldAreas)
                    {
                        if (area.Index < _worldData.CostFields.Count)
                        {
                            for (int x = 0; x < area.GridWidth; x++)
                            {
                                for (int y = 0; y < area.GridLength; y++)
                                {
                                    if (x < _worldData.CostFields[area.Index].Length &&
                                        y < _worldData.CostFields[area.Index][x].Length)
                                    {
                                        if (area.TileGrid[x][y] != null)
                                            area.TileGrid[x][y].Cost += _worldData.CostFields[area.Index][x][y];
                                    }
                                    else
                                    {
                                        costFieldsDontMatchWorld = true;
                                        break;
                                    }
                                }

                                if (costFieldsDontMatchWorld)
                                    break;
                            }

                            if (costFieldsDontMatchWorld)
                                break;
                        }
                        else
                            costFieldsDontMatchWorld = true;
                    }

                    _worldData.CostFields.Clear();
                }

                if (costFieldsDontMatchWorld)
                    Debug.Log(
                        "<color=red> WARNING: Saved cost fields for this scene, do NOT match up with the world geometry.\ncreate new costFields or reset the geometry to match the existing one. </color>");

                CreateSectorGraph();
                ConnectWorldAreas(false);
                CombineWorldAreaConnectionsWithSectorGraph();
            }
            else
            {
                DefineFlatWorld();

                // load in saved cost field values 
                bool costFieldsDontMatchWorld = false;
                if (generateWhileInPlayMode && loadCostField && _worldData.CostFields.Count > 0)
                {
                    foreach (WorldArea area in _worldData.WorldAreas)
                    {
                        for (int x = 0; x < area.GridWidth; x++)
                        {
                            for (int y = 0; y < area.GridLength; y++)
                            {
                                if (x < _worldData.CostFields[area.Index].Length &&
                                    y < _worldData.CostFields[area.Index][x].Length)
                                {
                                    if (area.TileGrid[x][y] != null)
                                        area.TileGrid[x][y].Cost += _worldData.CostFields[area.Index][x][y];
                                }
                                else
                                {
                                    costFieldsDontMatchWorld = true;
                                    break;
                                }
                            }

                            if (costFieldsDontMatchWorld)
                                break;
                        }

                        if (costFieldsDontMatchWorld)
                            break;
                    }

                    _worldData.CostFields.Clear();
                }

                if (costFieldsDontMatchWorld)
                    Debug.Log(
                        "<color=red> WARNING: Saved cost fields for this scene, do NOT match up with the world geometry.\ncreate new costFields or reset the geometry to match the existing one. </color>");

                CreateSectorGraph();
            }

            _worldIsBuilt = true;
            VisualizeTiles();
        }

        public void GenerateWordConnectingNodesPerGroup(WorldArea area, List<IntVector2> group, IntVector2 key)
        {
            int startIndex = 0;
            int size = 0;

            for (int i = 0; i < group.Count; i++)
            {
                var newItemFound = false;
                foreach (IntVector3 otherTile in area.WorldAreaTileConnections[group[i]])
                {
                    if (otherTile.Z == key.X &&
                        _worldData.WorldAreas[otherTile.Z].TileGrid[otherTile.X][otherTile.Y] != null &&
                        !_worldData.WorldAreas[otherTile.Z].TileGrid[otherTile.X][otherTile.Y].Blocked
                    ) // if the tile connected is blocked we ignore this one as well
                    {
                        if (!area.TileGrid[group[i].X][group[i].Y].Blocked)
                        {
                            newItemFound = true;
                            size++;
                            break;
                        }
                    }
                }

                if (i == group.Count - 1)
                    newItemFound = false;

                if (!newItemFound
                ) // group closed   if size == 0 we immediately found a obstacle on one of the sides, dont try and make a connection here then
                {
                    if (size != 0)
                    {
                        int middle = startIndex + (size / 2);

                        Tile middleTile = area.TileGrid[group[middle].X][group[middle].Y];

                        IntVector3 connectedSpot =
                            FindCorrectAreaNodeConnection(area.WorldAreaTileConnections[group[middle]], key.X);
                        Tile tileConnectedWith =
                            _worldData.WorldAreas[connectedSpot.Z].TileGrid[connectedSpot.X][connectedSpot.Y];

                        for (int j = 0; j < _worldData.Pathfinder.maxLevelAmount; j++)
                        {
                            MultiLevelSector sector = area.SectorGrid[0][middleTile.SectorIndex];
                            MultiLevelSector sectorConnectedWith =
                                _worldData.WorldAreas[connectedSpot.Z].SectorGrid[0][tileConnectedWith.SectorIndex];

                            if (j != 0)
                            {
                                sector = _worldData.MultiLevelSectorManager.GetHigherSectorFromLower(j, sector, area);
                                sectorConnectedWith = _worldData.MultiLevelSectorManager.GetHigherSectorFromLower(j,
                                    sectorConnectedWith, _worldData.WorldAreas[connectedSpot.Z]);
                            }

                            AbstractNode node =
                                _worldData.MultiLevelSectorManager.CreateAbstractNodeInSector(sector, middleTile, area);
                            AbstractNode nodeConnectedWith =
                                _worldData.MultiLevelSectorManager.CreateAbstractNodeInSector(sectorConnectedWith,
                                    tileConnectedWith, _worldData.WorldAreas[connectedSpot.Z]);

                            sector.WorldAreaNodes.Add(node, connectedSpot.Z);
                            sectorConnectedWith.WorldAreaNodes.Add(nodeConnectedWith, area.Index);

                            node.NodeConnectionToOtherSector = nodeConnectedWith;
                            nodeConnectedWith.NodeConnectionToOtherSector = node;

                            _worldData.MultiLevelSectorManager.ConnectSectorNodes(node, nodeConnectedWith, 1);


                            _worldData.MultiLevelSectorManager.ConnectWorldAreaNodesToSectorNodes(sector, area);
                            _worldData.MultiLevelSectorManager.ConnectWorldAreaNodesToSectorNodes(sectorConnectedWith,
                                _worldData.WorldAreas[connectedSpot.Z]);


                            //visual
                            sectorConnectedWith.SearchConnections();
                            sector.SearchConnections();
                        }

                        startIndex = i + 1;
                        size = 0;
                    }
                    else
                    {
                        startIndex = i + 1;
                        size = 0;
                    }
                }
            }
        }

        public void GenerateWorldManually(WorldData worldData, List<Tile[][]> tileGrids,
            List<IntVector2> tileGridOffset, bool autoConnectWorldAreas)
        {
            if (tileGrids.Count != tileGridOffset.Count)
                Debug.Log("<color=red> WARNING: each manually created WorldArea needs an offset value. </color>");
            else
            {
                _worldData = worldData;
                _temporaryWorldLayers.Clear();
                VisibleTiles.Clear();

                GenerateWorldAreaIndexLayers();

                _worldData.WorldAreas.Clear();
                for (int i = 0; i < tileGrids.Count; i++)
                {
                    GenerateFlatWorldArea(tileGrids[i], tileGridOffset[i], 0);
                }

                _worldIsBuilt = true;
            }

            CreateSectorGraph();

            if (autoConnectWorldAreas)
            {
                ConnectWorldAreas(true);

                CombineWorldAreaConnectionsWithSectorGraph();
            }

            VisualizeTiles();
        }

        public void ConnectWorldAreas()
        {
            ConnectWorldAreas(true);

            CombineWorldAreaConnectionsWithSectorGraph();

            VisualizeTiles();
        }

        public void ForceWorldAreaConnection(WorldArea area1, WorldArea area2, WorldArea.Side area1Side,
            WorldArea.Side area2Side, bool autoConnect)
        {
            IntVector2 manualAreaConnectionKey = new IntVector2(area1.Index, area2.Index);
            if (!_worldData.ManualWorldAreaConnections.ContainsKey(manualAreaConnectionKey))
                _worldData.ManualWorldAreaConnections.Add(manualAreaConnectionKey, WorldSideToDir(area1Side));

            manualAreaConnectionKey.X = area2.Index;
            manualAreaConnectionKey.Y = area1.Index;
            if (!_worldData.ManualWorldAreaConnections.ContainsKey(manualAreaConnectionKey))
                _worldData.ManualWorldAreaConnections.Add(manualAreaConnectionKey, WorldSideToDir(area2Side));

            int area1XDirValue = 0;
            int area1YDirValue = 0;
            int area1XStart = 0;
            int area1YStart = 0;

            int area2XDirValue = 0;
            int area2YDirValue = 0;
            int area2XStart = 0;
            int area2YStart = 0;

            int length1 = 0;
            int length2 = 0;

            switch (area1Side)
            {
                case WorldArea.Side.Top:
                {
                    length1 = area1.GridWidth;
                    area1XDirValue = 1;
                    break;
                }
                case WorldArea.Side.Down:
                {
                    length1 = area1.GridWidth;
                    area1XDirValue = 1;
                    area1YStart = area1.TileGrid[0].Length - 1;
                    break;
                }
                case WorldArea.Side.Left:
                {
                    length1 = area1.GridLength;
                    area1YDirValue = 1;
                    break;
                }
                case WorldArea.Side.Right:
                {
                    length1 = area1.GridLength;
                    area1YDirValue = 1;
                    area1XStart = area1.TileGrid.Length - 1;
                    break;
                }
            }

            switch (area2Side)
            {
                case WorldArea.Side.Top:
                {
                    length2 = area2.GridWidth;
                    area2XDirValue = 1;
                    break;
                }
                case WorldArea.Side.Down:
                {
                    length2 = area2.GridWidth;
                    area2XDirValue = 1;
                    area2YStart = area2.TileGrid[0].Length - 1;
                    break;
                }
                case WorldArea.Side.Left:
                {
                    length2 = area2.GridLength;
                    area2YDirValue = 1;
                    break;
                }
                case WorldArea.Side.Right:
                {
                    length2 = area2.GridLength;
                    area2YDirValue = 1;
                    area2XStart = area2.TileGrid.Length - 1;
                    break;
                }
            }

            var length = Mathf.Min(length1, length2);

            for (int i = 0; i < length; i++)
            {
                var tileArea1 = area1.TileGrid[area1XStart + area1XDirValue * i][area1YStart + area1YDirValue * i];
                var tileArea2 = area2.TileGrid[area2XStart + area2XDirValue * i][area2YStart + area2YDirValue * i];
                ConnectWorldAreaTiles(tileArea1, area1, tileArea2.GridPos.X + area2.LeftOffset,
                    tileArea2.GridPos.Y + area2.TopOffset, area2, true);
            }

            if (autoConnect)
            {
                CombineWorldAreaConnectionsWithSectorGraph();
                VisualizeTiles();
            }
        }

        #endregion
    }
}