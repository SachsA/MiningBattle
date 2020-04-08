namespace FlowPathfinding
{
    public class FlowFieldPath
    {
        #region PublicVariables

        public int Key;
        
        public Tile Destination;
        
        public FlowField FlowField;
        
        public IntegrationField IntegrationField;

        #endregion

        #region PublicMethods

        public void Create(Tile destination, IntegrationField integrationField, FlowField flowField, int key)
        {
            Destination = destination;
            IntegrationField = integrationField;
            FlowField = flowField;
            Key = key;
        }

        #endregion

    }
}
