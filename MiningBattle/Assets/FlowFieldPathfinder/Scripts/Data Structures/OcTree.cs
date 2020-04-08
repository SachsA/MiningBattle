using System.Collections.Generic;

namespace FlowPathfinding
{
    public class OcTree
    {
        #region PrivateVariables

        private const int MaxDepthLevel = 4;
        private const int MaxObjectsPerQuad = 15;

        private readonly List<Seeker> _objects;
        
        private readonly Pathfinder _pathfinder;

        #endregion

        #region PublicVariables

        public readonly int Level;

        public readonly float[] Bounds;
        //!quad is different!
        //oct = !center/pivot position: x,y,z  -  half:width,length,height

        public bool NodesInUse; //if false, the child nodes are not used
        
        public readonly OcTree[] Nodes;

        #endregion

        #region PrivateMethods

        private OcTree(int level, float[] bounds, Pathfinder pathfinder)
        {
            Level = level;
            _objects = new List<Seeker>(MaxObjectsPerQuad + 1);
            Bounds = bounds;
            Nodes = new OcTree[8];
            _pathfinder = pathfinder;
        }

        private void Split()
        {
            float subWidth = Bounds[3] * 0.5f;
            float subLength = Bounds[4] * 0.5f;
            float subHeight = Bounds[5] * 0.5f;

            float x = Bounds[0];
            float y = Bounds[1];
            float z = Bounds[2];

            // bottom half
            Nodes[0] = new OcTree(Level + 1,
                new[] {x + subWidth, y + subLength, z - subHeight, subWidth, subLength, subHeight}, _pathfinder);
            Nodes[1] = new OcTree(Level + 1,
                new[] {x - subWidth, y + subLength, z - subHeight, subWidth, subLength, subHeight}, _pathfinder);
            Nodes[2] = new OcTree(Level + 1,
                new[] {x - subWidth, y - subLength, z - subHeight, subWidth, subLength, subHeight}, _pathfinder);
            Nodes[3] = new OcTree(Level + 1,
                new[] {x + subWidth, y - subLength, z - subHeight, subWidth, subLength, subHeight}, _pathfinder);

            // top half
            Nodes[4] = new OcTree(Level + 1,
                new[] {x + subWidth, y + subLength, z + subHeight, subWidth, subLength, subHeight}, _pathfinder);
            Nodes[5] = new OcTree(Level + 1,
                new[] {x - subWidth, y + subLength, z + subHeight, subWidth, subLength, subHeight}, _pathfinder);
            Nodes[6] = new OcTree(Level + 1,
                new[] {x - subWidth, y - subLength, z + subHeight, subWidth, subLength, subHeight}, _pathfinder);
            Nodes[7] = new OcTree(Level + 1,
                new[] {x + subWidth, y - subLength, z + subHeight, subWidth, subLength, subHeight}, _pathfinder);
        }
                
        private int GetIndex(Seeker seeker)
        {
            int index = -1;

            // Seeker is inside the top half // 2D view
            var position = seeker.transform.position;
            
            bool topQuadrant = position.z > Bounds[1];
            // Seeker is inside the bottom half // 2D view
            bool bottomQuadrant = position.z <= Bounds[1];
            // Seeker is inside the top height half
            bool topWorldYQuadrant = position.y >= Bounds[2];

            // Seeker is inside the left half
            if (seeker.transform.position.x < Bounds[0])
            {
                if (topWorldYQuadrant)
                {
                    if (topQuadrant)
                        index = 5;
                    else if (bottomQuadrant)
                        index = 6;
                }
                else
                {
                    if (topQuadrant)
                        index = 1;
                    else if (bottomQuadrant)
                        index = 2;
                }
            }

            // Seeker is inside the right half
            else if (seeker.transform.position.x >= Bounds[0])
            {
                if (topWorldYQuadrant)
                {
                    if (topQuadrant)
                        index = 4;
                    else if (bottomQuadrant)
                        index = 7;
                }
                else
                {
                    if (topQuadrant)
                        index = 0;
                    else if (bottomQuadrant)
                        index = 3;
                }
            }

            return index;
        }

        #endregion

        #region PublicMethods

        public void Setup()
        {
            if (Level < MaxDepthLevel)
            {
                Split();
                foreach (var t in Nodes)
                    t.Setup();
            }
        }

        public void Clear()
        {
            _objects.Clear();
            if (NodesInUse)
            {
                foreach (var t in Nodes)
                    t.Clear();
            }

            NodesInUse = false;
        }

        public void Insert(Seeker seeker)
        {
            if (NodesInUse)
            {
                int index = GetIndex(seeker);

                if (index != -1)
                {
                    Nodes[index].Insert(seeker);

                    return;
                }
            }

            _objects.Add(seeker);

            if (_objects.Count > MaxObjectsPerQuad && Level < MaxDepthLevel)
            {
                NodesInUse = true;

                int i = 0;
                while (i < _objects.Count)
                {
                    Seeker obj = _objects[i];

                    int index = GetIndex(obj);
                    if (index != -1)
                    {
                        Nodes[index].Insert(obj);
                        _objects.Remove(obj);
                    }
                    else
                        i++;
                }
            }
        }

        #endregion
    }
}