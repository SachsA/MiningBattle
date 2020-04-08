using UnityEngine;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class TileManager
    {
        #region PrivateVariables

        private readonly int[] _straightDirections = {0, 1, 0, -1, 1, 0, -1, 0};
        private readonly int[] _diagonalDirections = {1, -1, 1, 1, -1, 1, -1, -1};

        #endregion

        #region PublicVariables

        public const int TileBlockedValue = 2000000000; // 2 billion
        public const int TileResetIntegrationValue = 2000000000; // 2 billion

        public WorldData WorldData;

        #endregion

        #region PrivateMethods

        private bool TilesWithinRange(float worldY1, float worldY2)
        {
            return Mathf.Abs(worldY1 - worldY2) <= WorldData.Pathfinder.generationClimbHeight;
        }

        #endregion

        #region PublicMethods

        public List<Tile> GetAllNeighboursForAStarSearch(Tile tile, WorldArea area)
        {
            List<Tile> neighbours = new List<Tile>();
            Tile neighbour;
            int checkX;
            int checkY;

            //straight
            for (int i = 0; i < _straightDirections.Length; i += 2)
            {
                checkX = tile.GridPos.X + _straightDirections[i];
                checkY = tile.GridPos.Y + _straightDirections[i + 1];

                if (checkX > -1 && checkX < area.GridWidth && checkY > -1 && checkY < area.GridLength)
                {
                    neighbour = area.TileGrid[checkX][checkY];

                    if (neighbour != null && !neighbour.Blocked && neighbour.SectorIndex == tile.SectorIndex &&
                        TilesWithinRangeGeneration(tile, neighbour))
                    {
                        neighbours.Add(neighbour);
                    }
                }
            }

            // diagonal
            for (int i = 0; i < _diagonalDirections.Length; i += 2)
            {
                checkX = tile.GridPos.X + _diagonalDirections[i];
                checkY = tile.GridPos.Y + _diagonalDirections[i + 1];

                if (checkX > -1 && checkX < area.GridWidth && checkY > -1 && checkY < area.GridLength)
                {
                    neighbour = area.TileGrid[checkX][checkY];
                    if (neighbour != null && !neighbour.Blocked && neighbour.SectorIndex == tile.SectorIndex)
                    {
                        if ((area.TileGrid[neighbour.GridPos.X][tile.GridPos.Y] == null ||
                             area.TileGrid[neighbour.GridPos.X][tile.GridPos.Y].Blocked) &&
                            (area.TileGrid[tile.GridPos.X][neighbour.GridPos.Y] == null ||
                             area.TileGrid[tile.GridPos.X][neighbour.GridPos.Y].Blocked))
                        {
                            // diagonal was blocked off
                        }
                        else if (TilesWithinRangeGeneration(tile, neighbour))
                        {
                            neighbours.Add(neighbour);
                        }
                    }
                }
            }

            return neighbours;
        }

        public List<Tile> GetAllNeighboursForSectorNodeSearch(Tile tile, WorldArea area)
        {
            List<Tile> neighbours = new List<Tile>();
            Tile neighbour;
            int checkX;
            int checkY;

            //straight
            for (int i = 0; i < _straightDirections.Length; i += 2)
            {
                checkX = tile.GridPos.X + _straightDirections[i];
                checkY = tile.GridPos.Y + _straightDirections[i + 1];

                if (checkX > -1 && checkX < area.GridWidth && checkY > -1 && checkY < area.GridLength)
                {
                    neighbour = area.TileGrid[checkX][checkY];

                    //if (worldData.worldBuilder.worldIsBuilt)
                    //    Debug.Log("tile.sector " + tile.sector + "   neighbour  " + neighbour.sector);

                    if (neighbour != null && !neighbour.Blocked && neighbour.SectorIndex == tile.SectorIndex &&
                        TilesWithinRangeGeneration(tile, neighbour))
                    {
                        int newCost = tile.IntegrationValue + neighbour.Cost * 10;

                        //if (worldData.worldBuilder.worldIsBuilt)
                        //    Debug.Log("add right?");
                        if (newCost < neighbour.IntegrationValue)
                        {
                            //if (worldData.worldBuilder.worldIsBuilt)
                            //    Debug.Log("ADDED");
                            neighbour.IntegrationValue = newCost;
                            neighbours.Add(neighbour);
                        }
                    }
                }
            }

            // diagonal
            for (int i = 0; i < _diagonalDirections.Length; i += 2)
            {
                checkX = tile.GridPos.X + _diagonalDirections[i];
                checkY = tile.GridPos.Y + _diagonalDirections[i + 1];

                if (checkX > -1 && checkX < area.GridWidth && checkY > -1 && checkY < area.GridLength)
                {
                    neighbour = area.TileGrid[checkX][checkY];
                    if (neighbour != null && !neighbour.Blocked && neighbour.SectorIndex == tile.SectorIndex)
                    {
                        int newCost = tile.IntegrationValue + neighbour.Cost * 14;
                        if (newCost < neighbour.IntegrationValue) // if not blocked
                        {
                            if ((area.TileGrid[neighbour.GridPos.X][tile.GridPos.Y] == null ||
                                 area.TileGrid[neighbour.GridPos.X][tile.GridPos.Y].Blocked) &&
                                (area.TileGrid[tile.GridPos.X][neighbour.GridPos.Y] == null ||
                                 area.TileGrid[tile.GridPos.X][neighbour.GridPos.Y].Blocked))
                            {
                                // diagonal was blocked off
                            }
                            else if (TilesWithinRangeGeneration(tile, neighbour))
                            {
                                neighbour.IntegrationValue = newCost;
                                neighbours.Add(neighbour);
                            }
                        }
                    }
                }
            }

            return neighbours;
        }

        public List<Tile> GetNeighboursExpansionSearch(Tile node, WorldArea area)
        {
            List<Tile> neighbours = new List<Tile>();

            int directionValue = node.GridPos.X + 1;
            if (directionValue < area.GridWidth && area.SearchField[directionValue][node.GridPos.Y] &&
                area.TileGrid[directionValue][node.GridPos.Y] != null) // right
            {
                if (!area.TileGrid[directionValue][node.GridPos.Y].Blocked &&
                    TilesWithinRangeGeneration(node, area.TileGrid[directionValue][node.GridPos.Y]))
                {
                    int newCost = node.IntegrationValue + area.TileGrid[directionValue][node.GridPos.Y].Cost;
                    if (newCost < area.TileGrid[directionValue][node.GridPos.Y].IntegrationValue)
                    {
                        area.TileGrid[directionValue][node.GridPos.Y].IntegrationValue = newCost;
                        neighbours.Add(area.TileGrid[directionValue][node.GridPos.Y]);
                    }
                }
            }

            directionValue = node.GridPos.X - 1;
            if (directionValue > -1 && area.SearchField[directionValue][node.GridPos.Y] &&
                area.TileGrid[directionValue][node.GridPos.Y] != null) // left
            {
                if (!area.TileGrid[directionValue][node.GridPos.Y].Blocked &&
                    TilesWithinRangeGeneration(node, area.TileGrid[directionValue][node.GridPos.Y]))
                {
                    int newCost = node.IntegrationValue + area.TileGrid[directionValue][node.GridPos.Y].Cost;
                    if (newCost < area.TileGrid[directionValue][node.GridPos.Y].IntegrationValue)
                    {
                        area.TileGrid[directionValue][node.GridPos.Y].IntegrationValue = newCost;
                        neighbours.Add(area.TileGrid[directionValue][node.GridPos.Y]);
                    }
                }
            }

            directionValue = node.GridPos.Y - 1;
            if (directionValue > -1 && area.SearchField[node.GridPos.X][directionValue] &&
                area.TileGrid[node.GridPos.X][directionValue] != null) // top
            {
                if (!area.TileGrid[node.GridPos.X][directionValue].Blocked &&
                    TilesWithinRangeGeneration(node, area.TileGrid[node.GridPos.X][directionValue]))
                {
                    int newCost = node.IntegrationValue + area.TileGrid[node.GridPos.X][directionValue].Cost;
                    if (newCost < area.TileGrid[node.GridPos.X][directionValue].IntegrationValue)
                    {
                        area.TileGrid[node.GridPos.X][directionValue].IntegrationValue = newCost;
                        neighbours.Add(area.TileGrid[node.GridPos.X][directionValue]);
                    }
                }
            }

            directionValue = node.GridPos.Y + 1;
            if (directionValue < area.GridLength && area.SearchField[node.GridPos.X][directionValue] &&
                area.TileGrid[node.GridPos.X][directionValue] != null) // bot
            {
                if (!area.TileGrid[node.GridPos.X][directionValue].Blocked &&
                    TilesWithinRangeGeneration(node, area.TileGrid[node.GridPos.X][directionValue]))
                {
                    int newCost = node.IntegrationValue + area.TileGrid[node.GridPos.X][directionValue].Cost;
                    if (newCost < area.TileGrid[node.GridPos.X][directionValue].IntegrationValue)
                    {
                        area.TileGrid[node.GridPos.X][directionValue].IntegrationValue = newCost;
                        neighbours.Add(area.TileGrid[node.GridPos.X][directionValue]);
                    }
                }
            }

            return neighbours;
        }

        public Tile GetLowestIntegrationCostTile(Tile tile, WorldArea area)
        {
            Tile lowestCostNode = tile;

            if (area != null)
            {
                //straight
                int checkX;
                int checkY;
                Tile neighbour;

                for (int i = 0; i < _straightDirections.Length; i += 2)
                {
                    checkX = tile.GridPos.X + _straightDirections[i];
                    checkY = tile.GridPos.Y + _straightDirections[i + 1];

                    if (checkX > -1 && checkX < area.GridWidth && checkY > -1 && checkY < area.GridLength &&
                        area.TileGrid[checkX][checkY] != null)
                    {
                        neighbour = area.TileGrid[checkX][checkY];
                        if (neighbour.IntegrationValue < lowestCostNode.IntegrationValue &&
                            TilesWithinRangeGeneration(tile, neighbour))
                            lowestCostNode = neighbour;
                    }
                }

                // diagonal
                for (int i = 0; i < _diagonalDirections.Length; i += 2)
                {
                    checkX = tile.GridPos.X + _diagonalDirections[i];
                    checkY = tile.GridPos.Y + _diagonalDirections[i + 1];

                    if (checkX > -1 && checkX < area.GridWidth && checkY > -1 && checkY < area.GridLength &&
                        area.TileGrid[checkX][checkY] != null)
                    {
                        neighbour = area.TileGrid[checkX][checkY];
                        if (neighbour.IntegrationValue < lowestCostNode.IntegrationValue &&
                            TilesWithinRangeGeneration(tile, neighbour))
                        {
                            if ((area.TileGrid[neighbour.GridPos.X][tile.GridPos.Y] == null ||
                                 area.TileGrid[neighbour.GridPos.X][tile.GridPos.Y].Blocked) ||
                                (area.TileGrid[tile.GridPos.X][neighbour.GridPos.Y] == null ||
                                 area.TileGrid[tile.GridPos.X][neighbour.GridPos.Y].Blocked))
                            {
                                // diagonal was blocked off
                            }
                            else
                                lowestCostNode = neighbour;
                        }
                    }
                }
            }

            return lowestCostNode;
        }

        public Vector3 GetTileWorldPosition(Tile tile, WorldArea worldArea)
        {
            return new Vector3(
                WorldData.Pathfinder.worldStart.x +
                ((worldArea.LeftOffset + tile.GridPos.X) * WorldData.Pathfinder.tileSize) +
                (WorldData.Pathfinder.tileSize * 0.5f), tile.YWorldPos,
                WorldData.Pathfinder.worldStart.z -
                ((worldArea.TopOffset + tile.GridPos.Y) * WorldData.Pathfinder.tileSize) -
                (WorldData.Pathfinder.tileSize * 0.5f));
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            //return new Vector3(-gridWidth * 0.5f + tile.gridPos.x + 0.5f, 0, gridHeight * 0.5f - tile.gridPos.y - 0.5f);
            return new Vector3(
                WorldData.Pathfinder.worldStart.x + (x * WorldData.Pathfinder.tileSize) +
                (WorldData.Pathfinder.tileSize * 0.5f), 0,
                WorldData.Pathfinder.worldStart.z - (y * WorldData.Pathfinder.tileSize) -
                (WorldData.Pathfinder.tileSize * 0.5f));
        }

        public WorldArea GetWorldAreaAtPosition(Vector3 location)
        {
            if (location.x > WorldData.Pathfinder.worldStart.x &&
                location.x < WorldData.Pathfinder.worldStart.x + WorldData.Pathfinder.worldWidth &&
                location.z < WorldData.Pathfinder.worldStart.z &&
                location.z > WorldData.Pathfinder.worldStart.z - WorldData.Pathfinder.worldLength)
            {
                if (WorldData.Pathfinder.worldIsMultiLayered)
                {
                    int worldX =
                        (int)((location.x - WorldData.Pathfinder.worldStart.x) /
                               WorldData.Pathfinder
                                   .tileSize); //Math.FloorToInt(location.x) + (int)(pathfinder.worldWidth * 0.5f);
                    int worldY = (int)(((WorldData.Pathfinder.worldStart.z - location.z) /
                                         WorldData.Pathfinder
                                             .tileSize)); //(int)(pathfinder.worldLength * 0.5f) - Math.CeilToInt(location.z);

                    int yLayer = GetHeightLayer(location.y);
                    int prevYLayer = -1;

                    int searchCount = 0;
                    while (searchCount < 3)
                    {
                        if (searchCount == 1)
                            yLayer = GetHeightLayer(location.y + WorldData.Pathfinder.generationClimbHeight);
                        else if (searchCount == 2)
                            yLayer = GetHeightLayer(location.y - WorldData.Pathfinder.generationClimbHeight);

                        //Debug.Log("YLayer " + yLayer + "   area  " + worldData.layerWorldAreaIndexes[yLayer][worldX][worldY]);
                        if (prevYLayer != yLayer && yLayer > -1 && yLayer < WorldData.LayerWorldAreaIndexes.Count &&
                            WorldData.LayerWorldAreaIndexes[yLayer][worldX][worldY] != -1) // not empty
                            return WorldData.WorldAreas[WorldData.LayerWorldAreaIndexes[yLayer][worldX][worldY]];

                        prevYLayer = yLayer;
                        searchCount++;
                    }
                }
                else
                {
                    foreach (WorldArea area in WorldData.WorldAreas)
                    {
                        if (location.x + 0.5f >= area.Origin.x && location.x + 0.5f < area.Origin.x + area.GridWidth && location.y - 0.5f <= area.Origin.z && location.y - 0.5f > area.Origin.z - area.GridLength)
                        {
                            return area;
                        }
                    }
                    return WorldData.WorldAreas[0];
                }
            }

            return null;
        }

        public WorldArea GetWorldAreaAtGuaranteedPosition(int worldX, float y, int worldY)
        {
            int yLayer = GetHeightLayer(y);
            int prevYLayer = -1;

            int searchCount = 0;
            while (searchCount < 3)
            {
                if (searchCount == 1)
                    yLayer = GetHeightLayer(y + WorldData.Pathfinder.generationClimbHeight);
                else if (searchCount == 2)
                    yLayer = GetHeightLayer(y - WorldData.Pathfinder.generationClimbHeight);


                if (prevYLayer != yLayer && yLayer > -1 && yLayer < WorldData.LayerWorldAreaIndexes.Count &&
                    WorldData.LayerWorldAreaIndexes[yLayer][worldX][worldY] != -1) // not empty
                    return WorldData.WorldAreas[WorldData.LayerWorldAreaIndexes[yLayer][worldX][worldY]];

                prevYLayer = yLayer;
                searchCount++;
            }

            return null;
        }

        public List<Tile> GetLeftAndRightNeighbour(Tile tile, WorldArea area)
        {
            List<Tile> tiles = new List<Tile>();

            if (tile.GridPos.X + 1 < area.GridWidth && area.TileGrid[tile.GridPos.X + 1][tile.GridPos.Y] != null)
                tiles.Add(area.TileGrid[tile.GridPos.X + 1][tile.GridPos.Y]);

            if (tile.GridPos.X - 1 > -1 && area.TileGrid[tile.GridPos.X - 1][tile.GridPos.Y] != null)
                tiles.Add(area.TileGrid[tile.GridPos.X - 1][tile.GridPos.Y]);

            return tiles;
        }

        public List<Tile> GetTopAndBottomNeighbour(Tile tile, WorldArea area)
        {
            List<Tile> tiles = new List<Tile>();

            if (tile.GridPos.Y + 1 < area.GridLength && area.TileGrid[tile.GridPos.X][tile.GridPos.Y + 1] != null)
                tiles.Add(area.TileGrid[tile.GridPos.X][tile.GridPos.Y + 1]);

            if (tile.GridPos.Y - 1 > -1 && area.TileGrid[tile.GridPos.X][tile.GridPos.Y - 1] != null)
                tiles.Add(area.TileGrid[tile.GridPos.X][tile.GridPos.Y - 1]);

            return tiles;
        }

        public int LocationToWorldGridX(Vector3 location)
        {
            return (int) ((location.x - WorldData.Pathfinder.worldStart.x) / WorldData.Pathfinder.tileSize);
        }

        public int LocationToWorldGridY(Vector3 location)
        {
            if (!WorldData.Pathfinder.worldIsMultiLayered && WorldData.Pathfinder.twoDimensionalMode)
                return Mathf.Abs((int) (((WorldData.Pathfinder.worldStart.z - location.y) /
                                         WorldData.Pathfinder.tileSize)));
            else
                return (int) (((WorldData.Pathfinder.worldStart.z - location.z) / WorldData.Pathfinder.tileSize));
        }

        public Tile GetTileInWorldArea(WorldArea area, Vector3 location)
        {
            if (area != null)
            {
                int worldX =
                    LocationToWorldGridX(
                        location); //Math.FloorToInt(location.x) + (int)(pathfinder.worldWidth * 0.5f);
                int worldY =
                    LocationToWorldGridY(
                        location); //(int)(pathfinder.worldLength * 0.5f) - Math.CeilToInt(location.z);

                int x = worldX - area.LeftOffset;
                int y = worldY - area.TopOffset;
                if (x > -1 && x < area.TileGrid.Length && y > -1 && y < area.TileGrid[0].Length)
                {
                    Tile tile = area.TileGrid[x][y];

                    if (tile != null && TilesWithinRange(location.z, tile.YWorldPos))
                    {
                        return tile;
                    }
                    return null;
                }

                return null;
            }

            return null;
        }

        public Tile GetTileInWorldArea(WorldArea area, int x, int y)
        {
            if (x > -1 && x < area.GridWidth && y > -1 && y < area.GridLength)
                return area.TileGrid[x][y];
            else
                return null;
        }

        public Tile GetTileFromPosition(Vector3 worldPosition)
        {
            WorldArea area = GetWorldAreaAtPosition(worldPosition);
            return area != null ? GetTileInWorldArea(area, worldPosition) : null;
        }

        public int GetHeightLayer(float y)
        {
            return Mathf.FloorToInt((y - WorldData.Pathfinder.worldStart.y) / WorldData.Pathfinder.characterHeight);
        }

        public bool TilesWithinRangeGeneration(Tile a, Tile b)
        {
            return Mathf.Abs(a.YWorldPos - b.YWorldPos) <= WorldData.Pathfinder.generationClimbHeight;
        }

        public bool TilesWithinRangeGenerationTemp(Tile a, Tile b)
        {
            return Mathf.Abs(a.YWorldPos - b.YWorldPos) <= 0.3f;
        }

        public List<Tile> GetStraightNeighbours(int x, int y, WorldArea area)
        {
            List<Tile> neighbours = new List<Tile>();

            int directionValue = x + 1;
            if (directionValue < area.GridWidth && area.TileGrid[directionValue][y] != null) // right
                neighbours.Add(area.TileGrid[directionValue][y]);

            directionValue = x - 1;
            if (directionValue > -1 && area.TileGrid[directionValue][y] != null) // left
                neighbours.Add(area.TileGrid[directionValue][y]);

            directionValue = y - 1;
            if (directionValue > -1 && area.TileGrid[x][directionValue] != null) // top
                neighbours.Add(area.TileGrid[x][directionValue]);

            directionValue = y + 1;
            if (directionValue < area.GridLength && area.TileGrid[x][directionValue] != null) // bot
                neighbours.Add(area.TileGrid[x][directionValue]);

            return neighbours;
        }

        #endregion
    }
}