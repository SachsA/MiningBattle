using System.Collections.Generic;
using System.Linq;

namespace FlowPathfinding
{
    public class FlowField
    {
        #region PublicVariables

        public readonly Dictionary<IntVector2, int[]> Field;

        #endregion

        #region PublicMethods

        public FlowField()
        {
            IntVectorComparer comparer = new IntVectorComparer();
            Field = new Dictionary<IntVector2, int[]>(comparer);
        }

        public void AddFields(List<int> sectors, int tilesInSectorAmount, WorldArea area)
        {
            IntVector2 key = new IntVector2 {X = area.Index};
            foreach (int sectorIndex in sectors)
            {
                key.Y = sectorIndex;
                if (!Field.ContainsKey(key))
                    Field.Add(key, new int[tilesInSectorAmount]);
            }
        }

        public void FillFlowField(List<Tile> tiles, WorldData worldData)
        {
//            WorldArea area = null;

//            if (tiles.Count > 0)
//                area = worldData.WorldAreas[tiles[0].WorldAreaIndex];

            IntVector2 key = new IntVector2();
            //Vector2 vec = new Vector2();

            foreach (Tile tile in tiles)
            {
     
                key.X = tile.WorldAreaIndex;
                key.Y = tile.SectorIndex;

                WorldArea area = worldData.WorldAreas[tile.WorldAreaIndex];

                var lowestCostTile = worldData.TileManager.GetLowestIntegrationCostTile(tile, area);

                if (lowestCostTile != tile && Field.ContainsKey(key))
                { 
                    Field[key][tile.IndexWithinSector] = worldData.FlowFieldManager.GetDirBetweenVectors(tile.GridPos, lowestCostTile.GridPos); 

                    //vec.x = lowestCostTile.gridPos.x - tile.gridPos.x;
                    //vec.y = tile.gridPos.y - lowestCostTile.gridPos.y;
                    //field[key][tile.indexWithinSector] = vec.normalized;
                    //new Vector2(lowestCostTile.gridPos.x - tile.gridPos.x, tile.gridPos.y - lowestCostTile.gridPos.y).normalized;
                }
            }
        }

        public void AddEmptyField(int sectorIndex, int tilesInSectorAmount, WorldArea area)
        {
            Field.Add(new IntVector2(area.Index, sectorIndex), new int[tilesInSectorAmount]);
        }

        #endregion
    }
}
