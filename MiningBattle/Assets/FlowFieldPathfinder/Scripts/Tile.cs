namespace FlowPathfinding
{
    public class Tile
    {
        #region PublicVariables

        // index as if it was in a separate grid in a sector 
        public int IndexWithinSector = -1;
        public int SectorIndex = -1;
        public int WorldAreaIndex = -1;
        public int Cost = 1;
        public int IntegrationValue = 0;

        public float YWorldPos = 0;
        public float Angle = 0;
        
        // an abstract node matches this tile
        public bool HasAbstractNodeConnection = false;
        public bool Blocked = false;

        public IntVector2 GridPos = new IntVector2(-1, -1);

        #endregion
    }
}
