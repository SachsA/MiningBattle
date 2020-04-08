namespace FlowPathfinding
{
    public class TemporaryWorldArea
    {
        #region PublicVariables

        public int GridWidth;
        public int GridHeight;

        public bool[][] TileSlotTakenGrid;

        public Tile[][] TileGrid;

        #endregion

        #region PublicMethods

        public void Setup(WorldData worldData)
        {
            GridWidth = (int)(worldData.Pathfinder.worldWidth / worldData.Pathfinder.tileSize);
            GridHeight = (int)(worldData.Pathfinder.worldLength / worldData.Pathfinder.tileSize);

            TileGrid = new Tile[GridWidth][];
            TileSlotTakenGrid = new bool[GridWidth][];
            for (int j = 0; j < GridWidth; j++)
            {
                TileGrid[j] = new Tile[GridHeight];
                TileSlotTakenGrid[j] = new bool[GridHeight];
            }
        }

        #endregion
    }
}
