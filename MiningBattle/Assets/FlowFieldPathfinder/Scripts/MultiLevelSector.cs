using System.Collections.Generic;

namespace FlowPathfinding
{
    public class MultiLevelSector
    {
        #region PrivateVariables

        // connection Lengths for visual debugging
        private readonly List<int> _connectionLengths = new List<int>();        

        #endregion

        #region PublicVariables

        // level depth.  0 = first layer of abstraction,  1 = second, etc.
        public int Level = 0;

        public int Id = 0;

        // index world area this sector is in
        public int WorldAreaIndex;

        public int GridX = 0;
        public int GridY = 0;

        public int Top = 0;
        public int Bottom = 0;
        public int Left = 0;
        public int Right = 0;

        public int TilesInWidth = 0;
        public int TilesInHeight = 0;

        // connections for visual debugging
        public readonly List<Tile> Connections = new List<Tile>();

        // list of higher nodes, ordered by edge  up 0, down 1, left 2, right 3
        public readonly List<AbstractNode>[] SectorNodesOnEdge = new List<AbstractNode>[4];

        // nodes that have a direct connection to a node on a different World Area
        public readonly Dictionary<AbstractNode, int> WorldAreaNodes = new Dictionary<AbstractNode, int>();

        #endregion

        #region PrivateMethods

        // set lists for visual debugging
        private void TryToAddConnection(Tile sectorTile, Tile connectedTile, int distance)
        {
            bool alreadyOnList = false;

            for (int i = 0; i < Connections.Count; i += 2)
            {
                if ((Connections[i] == sectorTile && Connections[i + 1] == connectedTile) || (Connections[i] == connectedTile && Connections[i + 1] == sectorTile))
                {
                    alreadyOnList = true;
                    break;
                }
            }

            if (!alreadyOnList)
            {
                Connections.Add(sectorTile);
                Connections.Add(connectedTile);
                _connectionLengths.Add(distance);
            }
        }

        #endregion

        #region PublicMethods

        public void Setup()
        {
            for (int i = 0; i < SectorNodesOnEdge.Length; i++)
                SectorNodesOnEdge[i] = new List<AbstractNode>();
        }

        // set lists for visual debugging
        public void SearchConnections()
        {
            Connections.Clear();
            _connectionLengths.Clear();

            foreach (List<AbstractNode> list in SectorNodesOnEdge)
            {
                foreach (AbstractNode sectorNode in list)
                {
                    foreach (AbstractNode connection in sectorNode.Connections.Keys)
                        TryToAddConnection(sectorNode.TileConnection, connection.TileConnection, sectorNode.Connections[connection]);
                }
            }
        }

        #endregion        
    }
}
