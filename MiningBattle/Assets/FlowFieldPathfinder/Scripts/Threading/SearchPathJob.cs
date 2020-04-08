using UnityEngine;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class SearchPathJob : ThreadedJob
    {
        #region PrivateVariables

        private int _key;

        private bool _pathEdit;

        private FlowFieldPath _path;

        private readonly Tile _destinationNode;

        private readonly List<Seeker> _units;

        private readonly Pathfinder _pathfinder;

        #endregion

        #region ProtectedMethods

        protected override void ThreadFunction()
        {
            if (_destinationNode != null)
            {
                WorldArea destinationArea = _pathfinder.worldData.WorldAreas[_destinationNode.WorldAreaIndex];
                List<Seeker> selectedUnits = new List<Seeker>(_units);
                Dictionary<IntVector2, Tile> startingPoints = new Dictionary<IntVector2, Tile>();
                IntVector2 pointKey = new IntVector2(0, 0);

                foreach (Seeker unit in _units)
                {
                    if (unit.currentWorldArea != null && unit.currentTile != null)
                    {
                        pointKey.X = unit.currentWorldArea.Index;
                        pointKey.Y = unit.currentTile.SectorIndex;
                        if (!startingPoints.ContainsKey(pointKey))
                            startingPoints.Add(pointKey, unit.currentTile);
                    }
                    else
                    {
                        selectedUnits.Remove(unit);
                        Debug.Log("A Selected Unit cannot be located,  is it to far above or below the ground? ");
                    }
                }

                if (startingPoints.Count > 0 && _destinationNode != null && !_destinationNode.Blocked)
                {
                    List<List<int>> areasAndSectors =
                        _pathfinder.worldData.HierarchicalPathfinder.FindPaths(startingPoints, _destinationNode,
                            destinationArea);
                    
                    _pathfinder.KeepTrackOfUnitsInPaths(selectedUnits);
                    int key = _pathfinder.GenerateKey(selectedUnits);

                    if (areasAndSectors != null)
                    {
                        _pathfinder.worldData.IntegrationFieldManager.StartIntegrationFieldCreation(_destinationNode,
                            areasAndSectors, null, null, key, false);
                        _pathfinder.worldData.HierarchicalPathfinder.RemovePreviousSearch();
                    }
                    else
                    {
                        foreach (Seeker unit in _units)
                        {
                            unit.stopUnitNow = true;
                        }
                    }
                }
            }
        }

        protected override void OnFinished()
        {
            _pathfinder.PathCreated(_path, _key, _pathEdit);
        }

        #endregion

        #region PublicMethods

        public SearchPathJob(Tile destinationNode, List<Seeker> units, Pathfinder pathfinder)
        {
            _destinationNode = destinationNode;
            _units = units;
            _pathfinder = pathfinder;
        }

        public void PathCreated(FlowFieldPath path, bool pathEdit)
        {
            _path = path;
            _key = _path.Key;
            _pathEdit = pathEdit;
        }

        #endregion
    }
}