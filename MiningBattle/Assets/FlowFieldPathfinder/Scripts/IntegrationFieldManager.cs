using System.Collections.Generic;

namespace FlowPathfinding
{
    public class IntegrationFieldManager
    {
        #region PrivateVariables

        private readonly List<Tile> _closedSet = new List<Tile>();
        private readonly List<Tile> _closedSetFinish = new List<Tile>();
        private readonly List<Tile> _openSet = new List<Tile>();
        private readonly List<Tile> _tilesSearchList = new List<Tile>();

        #endregion

        #region PublicVariables

        public WorldData WorldData;

        #endregion

        #region PublicMethods

        public void StartIntegrationFieldCreation(Tile destinationTile, List<List<int>> areasAndSectorsParam,
            FlowFieldPath flowPathParam, SearchPathJob pathJob, int key, bool pathEdit)
        {
            FlowFieldPath flowPath = null;
            bool aPathIsCreated = false;
            List<List<int>> areasAndSectors = areasAndSectorsParam;

            List<int> areas = new List<int>();
            List<List<int>> sectors = new List<List<int>>();

            Dictionary<IntVector2, List<Tile>> areaConnectionTiles = new Dictionary<IntVector2, List<Tile>>();

            bool firstWorldArea = !pathEdit;

            for (int a = 0; a < areasAndSectors.Count; a++) // each separate search //areasAndSectors.Count
            {
                var firstWorldAreaOfNewSearch = true;

                var index = 0;
                areas.Clear();
                sectors.Clear();
                Dictionary<IntVector2, int[]> alreadyFilledInSector = null;
                IntVector2 areaSectorKey = new IntVector2(0, 0); //area.index, sectorIndex);


                if (pathEdit && flowPathParam != null)
                    flowPath = flowPathParam;

                //setup/starting point of the sectors- and areas-  lists
                int startIndex;
                if (a == 0 && !pathEdit) // first search
                {
                    startIndex = 0;
                    areas.Add(areasAndSectors[a][0]);
                    sectors.Add(new List<int>());
                }
                else // we start with a search that is not the first one. we might be able to skip a lot of already integrated sectors
                {
                    if (flowPath == null)
                        flowPath = WorldData.FlowFieldManager.FlowFieldPaths[
                            WorldData.FlowFieldManager.FlowFieldPaths.Count - 1];

                    alreadyFilledInSector = flowPath.IntegrationField.Field;
                    startIndex = -1;

                    for (int i = 0; i < areasAndSectors[a].Count; i += 2)
                    {
                        areaSectorKey.X = areasAndSectors[a][i];
                        areaSectorKey.Y = areasAndSectors[a][i + 1];

                        if (!alreadyFilledInSector.ContainsKey(areaSectorKey)) // sector not yet filled in
                        {
                            startIndex = i;
                            areas.Add(areasAndSectors[a][startIndex]);
                            sectors.Add(new List<int>());
                            break;
                        }
                    }
                }


                // if -1 we can skip it all
                if (startIndex != -1) // else entire path already calculated by a different search
                {
                    // set what tiles to cross over during a search

                    // separate areas and sectors in arrays
                    for (int i = startIndex; i < areasAndSectors[a].Count; i += 2)
                    {
                        areaSectorKey.X = areasAndSectors[a][i];
                        areaSectorKey.Y = areasAndSectors[a][i + 1];

                        if (areasAndSectors[a][i] == areas[index])
                            sectors[index].Add(areasAndSectors[a][i + 1]);
                        else
                        {
                            index++;
                            areas.Add(areasAndSectors[a][i]);
                            sectors.Add(new List<int>());
                            sectors[index].Add(areasAndSectors[a][i + 1]);
                        }

                        if (alreadyFilledInSector != null && alreadyFilledInSector.ContainsKey(areaSectorKey)
                        ) // added sector already filled in
                        {
                            // a couple of sectors where not already found, then they were, then they aren't again
                            // we split up this search, so that every search in the flowing steps is a list of sectors that all directly connect.
                            // no gaps of already filled in sectors
                            areasAndSectors.Add(new List<int>());
                            for (int j = i; j < areasAndSectors[a].Count; j++)
                                areasAndSectors[areasAndSectors.Count - 1].Add(areasAndSectors[a][j]);

                            break;
                        }
                    }


                    if (!firstWorldArea &&
                        areasAndSectors[a][startIndex] != areasAndSectors[a][startIndex - 2]) // different world areas
                        firstWorldAreaOfNewSearch = false;

                    // going through our areas- and sectors- lists
                    for (int i = 0; i < areas.Count; i++)
                    {
                        WorldData.MultiLevelSectorManager.SetSearchFields(areas[i], sectors[i], true);
                        WorldArea currentWorldArea = WorldData.WorldAreas[areas[i]];

                        if (firstWorldAreaOfNewSearch)
                        {
                            _openSet.Clear();

                            List<Tile> oldSectorTiles = new List<Tile>();

                            if (firstWorldArea)
                            {
                                _tilesSearchList.Add(destinationTile);
                                _tilesSearchList[0].IntegrationValue = 0;
                            }
                            else
                            {
                                WorldArea area = WorldData.WorldAreas[areasAndSectors[a][startIndex]];
                                MultiLevelSector start = area.SectorGrid[0][areasAndSectors[a][startIndex - 1]];
                                MultiLevelSector next = area.SectorGrid[0][areasAndSectors[a][startIndex + 1]];
                                oldSectorTiles =
                                    WorldData.MultiLevelSectorManager.RowBetweenSectorsWithinWorldArea(start, next,
                                        area);

                                if (pathEdit) // put old values back in the old tiles
                                {
                                    IntVector2 oldTileKey = new IntVector2();
                                    foreach (Tile tile in oldSectorTiles)
                                    {
                                        oldTileKey.X = tile.WorldAreaIndex;
                                        oldTileKey.Y = tile.SectorIndex;
                                        tile.IntegrationValue =
                                            flowPath.IntegrationField.Field[oldTileKey][tile.IndexWithinSector];
                                    }
                                }

                                _tilesSearchList.AddRange(oldSectorTiles);
                            }


                            foreach (Tile oldTile in oldSectorTiles)
                                _closedSet.Remove(oldTile);
                        }
                        else
                        {
                            WorldArea previousWorldArea;
                            int lastSectorOfPreviousWorldArea;

                            if (i == 0) // previous world area is not in array, removed because of already covered
                            {
                                previousWorldArea = WorldData.WorldAreas[areasAndSectors[a][startIndex - 2]];
                                lastSectorOfPreviousWorldArea = areasAndSectors[a][startIndex - 1];
                            }
                            else
                            {
                                previousWorldArea = WorldData.WorldAreas[areas[i - 1]];
                                lastSectorOfPreviousWorldArea = sectors[i - 1][sectors[i - 1].Count - 1];
                            }

                            int sectorOfCurrentArea = sectors[i][0];


                            IntVector2 areaConnectionKey =
                                new IntVector2(currentWorldArea.Index, previousWorldArea.Index);
                            if (!areaConnectionTiles.ContainsKey(areaConnectionKey))
                                areaConnectionTiles.Add(areaConnectionKey, new List<Tile>());

                            List<Tile> tiles = SwitchToNextWorldArea(previousWorldArea, currentWorldArea,
                                lastSectorOfPreviousWorldArea, sectorOfCurrentArea, flowPath);

                            areaConnectionTiles[areaConnectionKey].AddRange(tiles);
                        }

                        WaveExpansionSearchTiles(currentWorldArea);

                        _closedSetFinish.AddRange(_closedSet);

                        // all integration fields generated, create flow field
                        if (firstWorldArea)
                        {
                            aPathIsCreated = true;
                            WorldData.FlowFieldManager.CreateFlowFieldPath(_closedSet, sectors[i], areas,
                                destinationTile, currentWorldArea, key);
                        }
                        else
                        {
                            WorldData.FlowFieldManager.AddToFlowFieldPath(_closedSet, sectors[i], currentWorldArea);

                            if (pathEdit)
                                aPathIsCreated = true;
                        }


                        _closedSet.Clear();

                        firstWorldAreaOfNewSearch = false;
                        firstWorldArea = false;

                        WorldData.MultiLevelSectorManager.SetSearchFields(areas[i], sectors[i], false);
                    }
                }
            }


            if (flowPath == null && WorldData.FlowFieldManager.FlowFieldPaths.Count > 0)
                flowPath = WorldData.FlowFieldManager.FlowFieldPaths[
                    WorldData.FlowFieldManager.FlowFieldPaths.Count - 1];


            if (flowPath != null)
            {
                flowPath.FlowField.FillFlowField(_closedSetFinish, WorldData);
                WorldData.FlowFieldManager.AddAreaTilesToFlowFieldPath(areaConnectionTiles);
            }


            if (pathJob != null)
            {
                pathJob.PathCreated(aPathIsCreated ? flowPath : null, false);
            }
            else
            {
                if (aPathIsCreated)
                {
                    if (flowPath != null) WorldData.Pathfinder.PathCreated(flowPath, flowPath.Key, pathEdit);
                }
                else
                    WorldData.Pathfinder.PathCreated(null, 0, pathEdit);
            }

            ResetTilesAfterSearch();
        }

        public IntegrationField CreateIntegrationField(List<Tile> tiles, List<int> sectors, WorldArea area)
        {
            IntegrationField integrationField = new IntegrationField();
            integrationField.AddFields(sectors,
                WorldData.MultiLevelSectorManager.GetSectorWidthAtLevel(area, 0) *
                WorldData.MultiLevelSectorManager.GetSectorHeightAtLevel(area, 0), tiles, area);
            return integrationField;
        }

        // create extra field, requested by unit if he accidentally leaves a valid sector
        public void CreateExtraField(WorldArea area, Tile tile, FlowFieldPath flowFieldPath)
        {
            if (tile != null && tile != flowFieldPath.Destination)
            {
                List<int> neighbourSectors = new List<int>();

                foreach (int neighbour in WorldData.MultiLevelSectorManager.GetNeighboursIndexes(
                    area.SectorGrid[0][tile.SectorIndex], area))
                {
                    if (flowFieldPath.IntegrationField.Field.ContainsKey(new IntVector2(area.Index, neighbour)))
                        neighbourSectors.Add(neighbour);
                }

                if (neighbourSectors.Count > 0)
                    CreateFieldToAdd(tile.SectorIndex, neighbourSectors, flowFieldPath, area);
                else
                {
                    // there are no valid neighbour sectors to guide the flow field
                    // we must find a new connection to the path from here, and fill the flow fields accordingly
                    //Debug.Log("there are no valid neighbour sector");
                    WorldData.Pathfinder.AddToPath(area, tile, flowFieldPath);
                }
            }
        }

        #endregion

        #region PrivateMethods

        // get the tiles between 2 world area in that align with the specific sectors
        private List<Tile> SwitchToNextWorldArea(WorldArea previousWorldArea, WorldArea currentWorldArea,
            int sectorOfPreviousWorldArea, int sectorOfCurrentArea, FlowFieldPath flowPath)
        {
            List<Tile> searchedTilesOnAreaEdges = new List<Tile>();
            // go through each list of positions from the previous world area that align with its sector  And  border against the current world area
            foreach (List<IntVector2> groupLists in previousWorldArea.GroupsInSectors[
                new IntVector2(currentWorldArea.Index, sectorOfPreviousWorldArea)])
            {
                foreach (IntVector2 pos in groupLists)
                {
                    // if tile / position  not blocked off
                    if (!previousWorldArea.TileGrid[pos.X][pos.Y].Blocked)
                    {
                        // go through each tile its linked with in other areas. usually only 1 but can be a maximum of 3
                        foreach (IntVector3 posOtherArea in previousWorldArea.WorldAreaTileConnections[pos])
                        {
                            // if the "other pos" matches with the (current)Area we are looking for, is not blocked, and is in the correct sector
                            if (posOtherArea.Z == currentWorldArea.Index &&
                                !currentWorldArea.TileGrid[posOtherArea.X][posOtherArea.Y].Blocked &&
                                currentWorldArea.TileGrid[posOtherArea.X][posOtherArea.Y].SectorIndex ==
                                sectorOfCurrentArea)
                            {
                                int cost;
                                if (flowPath != null)
                                    cost = flowPath.IntegrationField.Field[new IntVector2(previousWorldArea.Index, previousWorldArea.TileGrid[pos.X][pos.Y].SectorIndex)][previousWorldArea.TileGrid[pos.X][pos.Y].IndexWithinSector];
                                else
                                    cost = previousWorldArea.TileGrid[pos.X][pos.Y].IntegrationValue;

                                // we "jump" from tile to tile, give it the proper integration value
                                currentWorldArea.TileGrid[posOtherArea.X][posOtherArea.Y].IntegrationValue =
                                    cost + currentWorldArea.TileGrid[posOtherArea.X][posOtherArea.Y]
                                        .Cost; //previousWorldArea.tileGrid[pos.x][pos.y].cost;

                                _tilesSearchList.Add(currentWorldArea.TileGrid[posOtherArea.X][posOtherArea.Y]);

                                // store tiles on area edges, for special flow field pointing
                                searchedTilesOnAreaEdges.Add(currentWorldArea.TileGrid[posOtherArea.X][posOtherArea.Y]);
                                searchedTilesOnAreaEdges.Add(previousWorldArea.TileGrid[pos.X][pos.Y]);
                            }
                        }
                    }
                }
            }

            return searchedTilesOnAreaEdges;
        }

        // keep expanding finding neighbouring tiles & setting their integration values
        private void WaveExpansionSearchTiles(WorldArea area)
        {
            while (_tilesSearchList.Count > 0)
            {
                Tile currentTile = _tilesSearchList[0];

                // keep expanding finding neighbouring tiles & setting their integration values
                foreach (Tile neighbour in WorldData.TileManager.GetNeighboursExpansionSearch(currentTile, area))
                {
                    if (!_tilesSearchList.Contains(neighbour))
                        _tilesSearchList.Add(neighbour);
                }

                _closedSet.Add(currentTile);
                _tilesSearchList.Remove(currentTile);
            }
        }

        // reset values after search
        private void ResetTilesAfterSearch()
        {
            //reset Tile values for future searches
            foreach (Tile tile in _closedSetFinish)
                tile.IntegrationValue = TileManager.TileResetIntegrationValue;

            _closedSetFinish.Clear();
            _closedSet.Clear();
        }

        // extra fields are being created, create & add a field by expanding from neighbouring sectors 
        private void CreateFieldToAdd(int emptySector, List<int> neighbourSectors, FlowFieldPath flowFieldPath,
            WorldArea area)
        {
            // get all tiles between the sectors
            List<Tile> tilesChangedInNeighbourSectors = new List<Tile>();

            List<int> allSectors = new List<int> {emptySector};
            allSectors.AddRange(neighbourSectors);

            // set which fields/sectors we can do a tile expansion over
            WorldData.MultiLevelSectorManager.SetSearchFields(allSectors, area, true);

            // for each neighbour sector in the Path
            foreach (int neighbourSector in neighbourSectors)
            {
                // get the tiles on edge
                var tilesOnEdge = WorldData.MultiLevelSectorManager.RowBetweenSectorsWithinWorldArea(
                    area.SectorGrid[0][neighbourSector], area.SectorGrid[0][emptySector], area);

                //request integration values
                int[] fieldValues = flowFieldPath.IntegrationField.Field[new IntVector2(area.Index, neighbourSector)];

                // put the integration back in the tiles
                foreach (Tile tile in WorldData.MultiLevelSectorManager.GetTilesInSector(
                    area.SectorGrid[0][neighbourSector], area))
                {
                    tile.IntegrationValue = fieldValues[tile.IndexWithinSector];
                    tilesChangedInNeighbourSectors.Add(tile);
                }

                _tilesSearchList.AddRange(tilesOnEdge);
            }

            // expand over this and neighbouring sectors, starting at the edges
            WaveExpansionSearchTiles(area);

            // remove tiles that have direct connection to a different World Area, their flow direction must not change
            foreach (int neighbourSector in neighbourSectors)
            {
                foreach (AbstractNode node in area.SectorGrid[0][neighbourSector].WorldAreaNodes.Keys)
                    _closedSet.Remove(node.TileConnection);
            }

            // add new values to flow field pat
            WorldData.FlowFieldManager.AddToFlowFieldPath(_closedSet, allSectors, area, flowFieldPath);
            flowFieldPath.FlowField.FillFlowField(_closedSet, WorldData);

            // reset earlier removed tiles
            foreach (int neighbourSector in neighbourSectors)
            {
                foreach (AbstractNode node in area.SectorGrid[0][neighbourSector].WorldAreaNodes.Keys)
                    node.TileConnection.IntegrationValue = TileManager.TileResetIntegrationValue;
            }

            _closedSet.AddRange(tilesChangedInNeighbourSectors);
            _closedSetFinish.AddRange(_closedSet);

            ResetTilesAfterSearch();

            // set search fields back to  false
            WorldData.MultiLevelSectorManager.SetSearchFields(allSectors, area, false);
        }

        #endregion
    }
}