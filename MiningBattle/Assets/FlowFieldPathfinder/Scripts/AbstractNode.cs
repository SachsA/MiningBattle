using System.Collections.Generic;

namespace FlowPathfinding
{
    public class AbstractNode : IHeapItem<AbstractNode>
    {
        #region PrivateVariables

        private int F => G + H;

        #endregion
        
        #region PublicVariables

        public int G = 0;
        public int H = 0;
        public int Sector = 0;
        public int WorldAreaIndex;
        
        public Tile TileConnection = null;

        public AbstractNode Parent = null;
        public AbstractNode NodeConnectionToOtherSector = null;

        public readonly Dictionary<AbstractNode, int> Connections = new Dictionary<AbstractNode, int>();

        #endregion

        #region PublicMethods

        public int HeapIndex { get; set; }
        
        public int CompareTo(AbstractNode nodeToCompare)
        {
            int compare = F.CompareTo(nodeToCompare.F);
            if (compare == 0)
                compare = H.CompareTo(nodeToCompare.H);

            return -compare;
        }

        #endregion
    }
}

