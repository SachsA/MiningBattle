using UnityEngine;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class QuadTree
    {
        #region PrivateVariables

        private readonly int _level;
        private const int MaxObjectsPerQuad = 15;
        
        private readonly Pathfinder _pathfinder;

        #endregion

        #region PublicVariables

        public const int MaxDepthLevel = 5;
        
        public bool NodesInUse; //if false, the child nodes are not used
        
        public readonly List<Seeker> Objects;
        
        public Rect Bounds;
        
        public readonly QuadTree[] Nodes;

        #endregion

        #region PrivateMethods

        private void Split()
        {
            float subWidth = Bounds.width / 2f;
            float subHeight = Bounds.height / 2f;
            float x = Bounds.xMin;
            float y = Bounds.yMin;

            Nodes[0] = new QuadTree(_level + 1, new Rect(x + subWidth, y, subWidth, subHeight), _pathfinder);
            Nodes[1] = new QuadTree(_level + 1, new Rect(x, y, subWidth, subHeight), _pathfinder);
            Nodes[2] = new QuadTree(_level + 1, new Rect(x, y + subHeight, subWidth, subHeight), _pathfinder);
            Nodes[3] = new QuadTree(_level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight), _pathfinder);
        }

        private int GetIndex(Seeker seeker)
        {
            int index = -1;
            float verticalMidpoint = Bounds.xMin + (Bounds.width / 2f);
            float horizontalMidpoint = Bounds.yMin + (Bounds.height / 2f);

            var position = seeker.transform.position;
            // Seeker is inside the top half

            bool topQuadrant = _pathfinder.worldStart.z - position.z < horizontalMidpoint;
            // Seeker is inside the bottom half
            bool bottomQuadrant = _pathfinder.worldStart.z - position.z >= horizontalMidpoint;

            // Seeker is inside the left half
            if (seeker.transform.position.x - _pathfinder.worldStart.x < verticalMidpoint)
            {
                if (topQuadrant)
                    index = 1;
                else if (bottomQuadrant)
                    index = 2;
            }

            // Seeker is inside the right half
            else if (seeker.transform.position.x - _pathfinder.worldStart.x >= verticalMidpoint)
            {
                if (topQuadrant)
                    index = 0;
                else if (bottomQuadrant)
                    index = 3;
            }

            return index;
        }

        #endregion

        #region PublicMethods

        public QuadTree(int level, Rect bounds, Pathfinder pathfinder)
        {
            _level = level;
            Objects = new List<Seeker>(MaxObjectsPerQuad + 1);
            Bounds = bounds;
            Nodes = new QuadTree[4];
            _pathfinder = pathfinder;
        }

        public void Setup()
        {       
            if (_level < MaxDepthLevel)
            {
                Split();
                foreach (var t in Nodes)
                    t.Setup();
            }
        }

        public void Clear()
        {    
            Objects.Clear();
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

            Objects.Add(seeker);

            if (Objects.Count > MaxObjectsPerQuad && _level < MaxDepthLevel)
            {
                NodesInUse = true;

                int i = 0;
                while (i < Objects.Count)
                {
                    Seeker obj = Objects[i];

                    int index = GetIndex(obj);
                    if (index != -1)
                    {
                        Nodes[index].Insert(obj);
                        Objects.Remove(obj);
                    }
                    else
                        i++;
                }
            }
        }

        #endregion
    }
}