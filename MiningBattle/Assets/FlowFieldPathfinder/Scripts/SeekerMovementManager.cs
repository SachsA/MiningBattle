using System.Collections.Generic;
using UnityEngine;

namespace FlowPathfinding
{
    public class SeekerMovementManager : MonoBehaviour
    {
        #region PrivateVariables

        private int _maxAmountOfTrees = 1;

        private List<QuadTree> _openQuadTreeList;
        private List<OcTree> _openOctTreeList;
        private readonly List<Seeker> _allSeekers = new List<Seeker>();

        private IntVector2 _key;

        private Pathfinder _pathfinder;

        private WorldData _worldData;

        private Rect _searchQuad = new Rect(0, 0, 0, 0);

        #endregion

        #region PublicVariables

        public QuadTree quadTree = null;

        public OcTree octTree;

        public const int MaxNeighbourCount = 10;

        #endregion

        #region PrivateMethods

        // make seekers move and update their "neighbour" array  for proper steering forces
        private void FixedUpdate()
        {
            if (quadTree != null)
                quadTree.Clear();
            
            foreach (var t in _allSeekers)
            {
                t.Tick();
            }
        }

        private void SetSearchBoxQuad(Seeker seeker)
        {
            var position = seeker.transform.position;

            _searchQuad.xMin = position.x - _pathfinder.worldStart.x - seeker.neighbourRadius;
            _searchQuad.yMin = _pathfinder.worldStart.z - position.z - seeker.neighbourRadius;
            _searchQuad.width = _searchQuad.height = seeker.neighbourRadius * 2f;
        }

        private void RetrieveSeekerNeighboursQuad(Seeker seeker, float radiusSquared)
        {
            for (int i = 0; i < MaxNeighbourCount; i++)
                seeker.neighbours[i] = null;

            _openQuadTreeList.Clear();
            _openQuadTreeList.Add(quadTree);

            int foundCount = 0;
            while (_openQuadTreeList.Count > 0 && foundCount < MaxNeighbourCount)
            {
                var current = _openQuadTreeList[0];

                if (current.Bounds.Overlaps(_searchQuad))
                {
                    foreach (var t in current.Objects)
                    {
                        if (t != seeker)
                        {
                            //vector3, sphere radius.   !use vector2 if you want circle radius!
                            var directionToTarget = t.transform.position - seeker.transform.position;
                            var dSqrToTarget = directionToTarget.sqrMagnitude;

                            if (foundCount == MaxNeighbourCount)
                                break;

                            if (dSqrToTarget < radiusSquared)
                            {
                                seeker.neighbours[foundCount] = t;
                                foundCount++;
                            }
                        }
                    }

                    if (current.NodesInUse)
                    {
                        _openQuadTreeList.Add(current.Nodes[0]);
                        _openQuadTreeList.Add(current.Nodes[1]);
                        _openQuadTreeList.Add(current.Nodes[2]);
                        _openQuadTreeList.Add(current.Nodes[3]);
                    }
                }

                _openQuadTreeList.Remove(current);
            }
        }

        private int CheckIfMovingToOtherArea(Seeker seeker, int xDir, int yDir)
        {
            //if (_worldData.Pathfinder.worldIsMultiLayered)
            //{
                IntVector2 key = seeker.currentTile.GridPos;
                if (seeker.currentWorldArea.WorldAreaTileConnections.ContainsKey(key)
                ) // if our current tile connects to tile in another world area, outside of our own tile grid
                {
                    List<IntVector3> values = seeker.currentWorldArea.WorldAreaTileConnections[key];

                    for (int i = 0; i < values.Count; i++)
                    {
                        if (!_worldData.WorldAreas[values[i].Z].TileGrid[values[i].X][values[i].Y].Blocked)
                        {
                            if (xDir != 0)
                            {
                                if (_worldData.WorldAreas[values[i].Z].TopOffset + values[i].Y ==
                                    seeker.currentWorldArea.TopOffset + key.Y
                                ) // align on Z. its straight ahead or behind
                                {
                                    if (xDir == 1 && (_worldData.WorldAreas[values[i].Z].LeftOffset + values[i].X >
                                                      seeker.currentWorldArea.LeftOffset + key.X)) // right
                                        return values[i].Z;
                                    if (xDir == -1 &&
                                        (_worldData.WorldAreas[values[i].Z].LeftOffset + values[i].X <
                                         seeker.currentWorldArea.LeftOffset + key.X)) // left
                                        return values[i].Z;
                                }
                            }
                            else
                            {
                                if (_worldData.WorldAreas[values[i].Z].LeftOffset + values[i].X ==
                                    seeker.currentWorldArea.LeftOffset + key.X &&
                                    !_worldData.WorldAreas[values[i].Z].TileGrid[values[i].X][values[i].Y].Blocked
                                ) // align on X. its straight ahead or behind
                                {
                                    if (yDir == -1 && (_worldData.WorldAreas[values[i].Z].TopOffset + values[i].Y <
                                                       seeker.currentWorldArea.TopOffset + key.Y)) // forward
                                        return values[i].Z;
                                    if (yDir == 1 &&
                                        (_worldData.WorldAreas[values[i].Z].TopOffset + values[i].Y >
                                         seeker.currentWorldArea.TopOffset + key.Y)) // back
                                        return values[i].Z;
                                }
                            }
                        }
                    }
                }
            //}

            return -1;
        }

        #endregion

        #region PublicMethods

        // quadTree
        //  /*
        public void Setup(Pathfinder pathfinder, WorldData worldData)
        {
            quadTree = new QuadTree(0, new Rect(0, 0, pathfinder.worldWidth, pathfinder.worldLength), pathfinder);
            quadTree.Setup();
            _worldData = worldData;
            _pathfinder = pathfinder;

            int currentStep = 1;
            for (int i = 1; i < QuadTree.MaxDepthLevel + 1; i++)
            {
                currentStep = currentStep * 4;
                _maxAmountOfTrees += currentStep;
            }

            _openQuadTreeList = new List<QuadTree>(_maxAmountOfTrees);
        }

        public void SetNeighboursQuad(Seeker seeker, float neighbourRadiusSquared)
        {
            SetSearchBoxQuad(seeker);
            RetrieveSeekerNeighboursQuad(seeker, neighbourRadiusSquared);
        }

        // add seeker to manager, now the seeker's tick() will be called,  thus making him move
        public void AddSeeker(Seeker seeker)
        {
            _allSeekers.Add(seeker);
        }

        // remove a seeker when you do not want him to move / or is dead/removed
        public void RemoveSeeker(Seeker seeker)
        {
            _allSeekers.Remove(seeker);
        }

        public Vector2 FindFlowValueFromPosition(Vector3 worldPosition, FlowField flowField, Seeker seeker)
        {
            seeker.currentWorldArea = null;
            seeker.currentTile = null;

            Vector2 vec = Vector2.zero;
            WorldArea area = _worldData.TileManager.GetWorldAreaAtPosition(worldPosition);

            if (area != null)
            {
                int worldX = _worldData.TileManager.LocationToWorldGridX(worldPosition);
                int worldY = _worldData.TileManager.LocationToWorldGridY(worldPosition);

                //Debug.Log("Area Index: " + area.Index);
                //Debug.Log("WorldX: " + (worldX - area.LeftOffset));

                Tile tile = area.TileGrid[worldX - area.LeftOffset][worldY - area.TopOffset];
                if (tile != null)
                {
                    seeker.currentWorldArea = area;
                    seeker.currentTile = tile;

                    _key.X = area.Index;
                    _key.Y = tile.SectorIndex;

                    if (flowField.Field.ContainsKey(_key))
                        vec = _worldData.FlowFieldManager.DirToVector(flowField.Field[_key][tile.IndexWithinSector]);
                }
            }

            return vec;
        }

        public void SetUnitAreaAndTile(Seeker seeker, Vector3 worldPosition)
        {
            WorldArea area = _worldData.TileManager.GetWorldAreaAtPosition(worldPosition);

            if (area != null)
            {
                int worldX = _worldData.TileManager.LocationToWorldGridX(worldPosition);
                int worldY = _worldData.TileManager.LocationToWorldGridY(worldPosition);

                Tile tile = area.GetTileFromGrid(worldX - area.LeftOffset, worldY - area.TopOffset);
                if (tile != null)
                {
                    seeker.currentWorldArea = area;
                    seeker.currentTile = tile;
                }
            }
        }

        public void CheckIfMovementLegit(Seeker seeker)
        {
            if (seeker.currentTile != null && seeker.currentWorldArea != null)
            {
                int xDir = 0;
                int yDir = 0;

                bool capXMovement = false;
                bool capZMovement = false;
                int areaXMovement = -1;
                int areaZMovement = -1;

                if (seeker.movement.x != 0)
                {
                    if (seeker.movement.x > 0)
                        xDir = 1;
                    else
                        xDir = -1;
                }

                if (seeker.movement.z != 0)
                {
                    if (seeker.movement.z > 0)
                        yDir = -1;
                    else
                        yDir = 1;
                }

                if (yDir == 0 && xDir == 0)
                {
                    // nothing changed
                }
                else
                {
                    Tile destination;
                    if (seeker.currentTile.GridPos.X + xDir > -1 &&
                        seeker.currentTile.GridPos.X + xDir < seeker.currentWorldArea.GridWidth)
                    {
                        // inside world area
                        destination =
                            seeker.currentWorldArea.TileGrid[seeker.currentTile.GridPos.X + xDir][
                                seeker.currentTile.GridPos.Y];
                        if (destination == null) // null, can we travel to another world area, or is there nothing?
                        {
                            areaXMovement = CheckIfMovingToOtherArea(seeker, xDir, 0);
                            capXMovement = true;
                        }
                        else if (destination.Blocked)
                        {
                            capXMovement = true;
                        }
                    }
                    else
                    {
                        areaXMovement = CheckIfMovingToOtherArea(seeker, xDir, 0);
                        capXMovement = true;
                    }

                    if (seeker.currentTile.GridPos.Y + yDir > -1 &&
                        seeker.currentTile.GridPos.Y + yDir < seeker.currentWorldArea.GridLength)
                    {
                        // inside world area
                        destination =
                            seeker.currentWorldArea.TileGrid[seeker.currentTile.GridPos.X][
                                seeker.currentTile.GridPos.Y + yDir];
                        if (destination == null) // null, can we travel to another world area, or is there nothing?
                        {
                            areaZMovement = CheckIfMovingToOtherArea(seeker, 0, yDir);
                            capZMovement = true;
                        }
                        else if (destination.Blocked)
                        {
                            capZMovement = true;
                        }
                    }
                    else
                    {
                        // outside
                        areaZMovement = CheckIfMovingToOtherArea(seeker, 0, yDir);
                        capZMovement = true;
                    }

                    if (!capXMovement && !capZMovement) // diagonal check
                    {
                        destination =
                            seeker.currentWorldArea.TileGrid[seeker.currentTile.GridPos.X + xDir][
                                seeker.currentTile.GridPos.Y + yDir];
                        if (destination == null || destination.Blocked)
                        {
                            if (seeker.desiredFlowValue == Vector2.zero)
                                seeker.movement = Vector3.zero;
                            else
                            {
                                if (seeker.desiredFlowValue.x == 0 ||
                                    (seeker.desiredFlowValue.x > 0 && seeker.movement.x < 0) ||
                                    (seeker.desiredFlowValue.x < 0 && seeker.movement.x > 0))
                                {
                                    float futureXPos = seeker.transform.position.x + seeker.movement.x;
                                    float border =
                                        _worldData.TileManager
                                            .GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).x +
                                        (_worldData.Pathfinder.tileSize * 0.49f * xDir);
                                    if (xDir == 1 && futureXPos > border)
                                        seeker.movement.x -= (futureXPos - border);
                                    else if (xDir == -1 && futureXPos < border)
                                        seeker.movement.x -= (futureXPos - border);
                                }

                                if (seeker.desiredFlowValue.y == 0 ||
                                    (seeker.movement.z > 0 && seeker.desiredFlowValue.y < 0) ||
                                    (seeker.movement.z < 0 && seeker.desiredFlowValue.y > 0))
                                {
                                    float futureZPos = seeker.transform.position.z + seeker.movement.z;
                                    float border =
                                        _worldData.TileManager
                                            .GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).z +
                                        (_worldData.Pathfinder.tileSize * 0.49f * -yDir);
                                    if (yDir == -1 && futureZPos > border)
                                        seeker.movement.z -= (futureZPos - border);
                                    else if (yDir == 1 && futureZPos < border)
                                        seeker.movement.z -= (futureZPos - border);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (areaXMovement != -1 && areaZMovement != -1)
                        {
                            if (seeker.desiredFlowValue.x == 0 ||
                                (seeker.desiredFlowValue.x > 0 && seeker.movement.x < 0) ||
                                (seeker.desiredFlowValue.x < 0 && seeker.movement.x > 0))
                                areaXMovement = -1;
                            else
                                areaZMovement = -1;
                        }
                    }

                    if (capXMovement)
                    {
                        if (areaXMovement == -1)
                        {
                            float futureXPos = seeker.transform.position.x + seeker.movement.x;
                            float border =
                                _worldData.TileManager.GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea)
                                    .x + (_worldData.Pathfinder.tileSize * 0.49f * xDir);
                            if (xDir == 1 && futureXPos > border)
                                seeker.movement.x -= (futureXPos - border);
                            else if (xDir == -1 && futureXPos < border)
                                seeker.movement.x -= (futureXPos - border);
                        }
                        else
                        {
                            if (yDir != 0)
                            {
                                bool capZ = false;
                                int destinationAreaGridX =
                                    (seeker.currentTile.GridPos.X + xDir + seeker.currentWorldArea.LeftOffset) -
                                    _worldData.WorldAreas[areaXMovement].LeftOffset;
                                int destinationAreaGridY =
                                    (seeker.currentTile.GridPos.Y + yDir + seeker.currentWorldArea.TopOffset) -
                                    _worldData.WorldAreas[areaXMovement].TopOffset;

                                if (destinationAreaGridY < _worldData.WorldAreas[areaXMovement].GridLength &&
                                    destinationAreaGridY > -1)
                                {
                                    destination =
                                        _worldData.WorldAreas[areaXMovement].TileGrid[destinationAreaGridX][
                                            destinationAreaGridY];
                                    if (destination == null || destination.Blocked)
                                        capZ = true;
                                }
                                else
                                    capZ = true;

                                if (capZ)
                                {
                                    float futureZPos = seeker.transform.position.z + seeker.movement.z;
                                    float border =
                                        _worldData.TileManager
                                            .GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).z +
                                        (_worldData.Pathfinder.tileSize * 0.49f * -yDir);
                                    if (yDir == -1 && futureZPos > border)
                                        seeker.movement.z -= (futureZPos - border);
                                    else if (yDir == 1 && futureZPos < border)
                                        seeker.movement.z -= (futureZPos - border);
                                }
                            }
                        }
                    }

                    if (capZMovement)
                    {
                        if (areaZMovement == -1)
                        {
                            float futureZPos = seeker.transform.position.z + seeker.movement.z;
                            float border =
                                _worldData.TileManager.GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea)
                                    .z + (_worldData.Pathfinder.tileSize * 0.49f * -yDir);
                            if (yDir == -1 && futureZPos > border)
                                seeker.movement.z -= (futureZPos - border);
                            else if (yDir == 1 && futureZPos < border)
                                seeker.movement.z -= (futureZPos - border);
                        }
                        else
                        {
                            if (yDir != 0)
                            {
                                bool capX = false;
                                int destinationAreaGridX =
                                    (seeker.currentTile.GridPos.X + xDir + seeker.currentWorldArea.LeftOffset) -
                                    _worldData.WorldAreas[areaZMovement].LeftOffset;
                                int destinationAreaGridY =
                                    (seeker.currentTile.GridPos.Y + yDir + seeker.currentWorldArea.TopOffset) -
                                    _worldData.WorldAreas[areaZMovement].TopOffset;

                                if (destinationAreaGridX < _worldData.WorldAreas[areaZMovement].GridWidth &&
                                    destinationAreaGridX > -1)
                                {
                                    destination =
                                        _worldData.WorldAreas[areaZMovement].TileGrid[destinationAreaGridX][
                                            destinationAreaGridY];
                                    if (destination == null || destination.Blocked)
                                        capX = true;
                                }
                                else
                                    capX = true;

                                if (capX)
                                {
                                    float futureXPos = seeker.transform.position.x + seeker.movement.x;
                                    float border =
                                        _worldData.TileManager
                                            .GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).x +
                                        (_worldData.Pathfinder.tileSize * 0.49f * xDir);
                                    if (xDir == 1 && futureXPos > border)
                                        seeker.movement.x -= (futureXPos - border);
                                    else if (xDir == -1 && futureXPos < border)
                                        seeker.movement.x -= (futureXPos - border);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}