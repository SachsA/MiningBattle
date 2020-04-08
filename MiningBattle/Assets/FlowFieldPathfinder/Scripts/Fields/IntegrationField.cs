using UnityEngine;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class IntegrationField
    {
        #region PublicVariables

        public readonly Dictionary<IntVector2, int[]> Field = new Dictionary<IntVector2, int[]>();

        #endregion

        #region PublicMethods

        public void AddFields(List<int> sectors, int tilesInSectorAmount, List<Tile> tiles, WorldArea area)
        {
            IntVector2 key = new IntVector2();

            foreach (int sectorIndex in sectors)
            {
                key = new IntVector2(area.Index, sectorIndex);
                //Debug.Log("key  " + key.x   + " " + key.y);

                if (!Field.ContainsKey(key))
                    Field.Add(key, new int[tilesInSectorAmount]);
            }
            
            foreach (Tile tile in tiles)
            {
                key.X = area.Index;
                key.Y = tile.SectorIndex;
                if (!Field.ContainsKey(key))
                {
                    Field.Add(key, new int[tile.IndexWithinSector]);
                    Debug.Log("FALSE KEY  " + key.X + "  " + key.Y);
                }

                Field[key][tile.IndexWithinSector] = tile.IntegrationValue;
            }
        }

        public void AddEmptyField(int sectorIndex, int tilesInSectorAmount, WorldArea area)
        {
            Field.Add(new IntVector2(area.Index, sectorIndex), new int[tilesInSectorAmount]);
        }

        #endregion
    }
}
