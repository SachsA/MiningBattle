using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FlowPathfinding
{
    public class Seeker : MonoBehaviour
    {
        #region PrivateVariables

        private int _indexSeeker;
        
        private float _neighbourRadiusSquaredIdle;
        private float _neighbourRadiusSquaredMoving;
        
        private const float FlowWeight = 0.9f;
        private const float SepWeight = 2.5f;
        private const float AlignWeight = 0.3f;
        private const float CohWeight = 0f;
        private const float MaxMoveSpeed = 4f; // maximum magnitude of the (combined) velocity vector
        private const float MaxIdleSpeed = 4f; // maximum magnitude of the (combined) velocity vector

        private readonly Vector2[] _combinedForces = new Vector2[4];

        [FormerlySerializedAs("StopUnitNow")] public bool stopUnitNow = false;
        
        private Vector3 _idleDestination;
        private Vector3 _idleRelativePosition;
        
        private readonly List<float> _ringDistance = new List<float>();
        private readonly List<int> _ringCount = new List<int>();

        private readonly List<Vector3> _positionListAround = new List<Vector3>();
        private readonly List<Vector3> _positionListAroundRing = new List<Vector3>();

        private CharacterController2D _controller2D;

        private Pathfinder _pathfinder;

        private Spaceship _spaceship;

        public enum Mode
        {
            Automatic,
            Manual
        }

        public Mode _mode = Mode.Automatic;
        protected Miner _miner;
        public bool _lastMinerAutomation = true;

        public State _seekerState = State.Idle;

        public enum State
        {
            Idle,
            Moving
        }

        #endregion

        #region PublicVariables

        public float moveSpeed = 1;
        public float idleSpeed = 1;
        
        [HideInInspector]
        public float maxForce = 4; // maximum magnitude of the (combined) force vector that is applied each tick

        [HideInInspector] public float neighbourRadiusMoving = 2.2f;
        [HideInInspector] public float neighbourRadiusIdle = 0.95f;
        [HideInInspector] public float neighbourRadius;
        [HideInInspector] public float neighbourRadiusSquared;

        [HideInInspector] public Seeker[] neighbours;

        [HideInInspector] public Vector2 desiredFlowValue = Vector2.zero;
        [HideInInspector] public Vector2 velocity = Vector2.zero;
        [HideInInspector] public Vector3 movement;
        [HideInInspector] public Tile currentTile;
        [HideInInspector] public FlowFieldPath flowFieldPath;
        [HideInInspector] public WorldArea currentWorldArea;

        #endregion

        #region PrivateMethods

        // Use this for initialization
        private void Start()
        {
            var position = transform.position;

            //_miner = GetComponent<Miner>();

            _idleDestination = position;

            _spaceship = GetComponent<Spaceship>();
            _controller2D = GetComponent<CharacterController2D>();

            _neighbourRadiusSquaredMoving = neighbourRadiusMoving * neighbourRadiusMoving;
            _neighbourRadiusSquaredIdle = neighbourRadiusIdle * neighbourRadiusIdle;
            SetNeighbourRadius();

            neighbours = new Seeker[SeekerMovementManager.MaxNeighbourCount];
            _pathfinder = GameObject.Find("Pathfinder2D").GetComponent<Pathfinder>();

            _pathfinder.seekerManager.AddSeeker(this);
            _pathfinder.seekerManager.SetUnitAreaAndTile(this, position);
        }

        private void OnDestroy()
        {
            //if (GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
            _pathfinder.seekerManager.RemoveSeeker(this);
        }

        private void SetNeighbourRadius()
        {
            switch (_seekerState)
            {
                case State.Idle:
                {
                    neighbourRadius = neighbourRadiusIdle;
                    neighbourRadiusSquared = _neighbourRadiusSquaredIdle;
                }
                    break;
                case State.Moving:
                {
                    neighbourRadius = neighbourRadiusMoving;
                    neighbourRadiusSquared = _neighbourRadiusSquaredMoving;
                }
                    break;
            }
        }

        private void Idle()
        {
            _spaceship.SetIsMoving(false);
            
            velocity = SeparationIdle().normalized * MaxIdleSpeed;

            // move
            movement = new Vector3(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime, 0);
            movement *= idleSpeed;
        }

        private void Move()
        {
            _spaceship.SetIsMoving(true);

            if (flowFieldPath != null)
            {
//                if (_miner || currentTile == _miner.TileDestination)
                if (currentTile == flowFieldPath.Destination)
                {
                    ReachedDestination();
                }
                else
                {
                    // 4 steering Vectors in order: Flow, separation, alignment, cohesion
                    // adjusted with user defined weights
                    _combinedForces[0] = FlowWeight * FlowFieldFollow();
                    _combinedForces[1] = SepWeight * SeparationMove(_neighbourRadiusSquaredMoving);
                    _combinedForces[2] = AlignWeight * Alignment();
                    _combinedForces[3] = CohWeight * Cohesion();

                    // calculate the combined force, but dont go over the maximum force
                    var netForce = CombineForces(maxForce, _combinedForces);
                    //var netForce = FlowFieldFollow();
                    // velocity gets adjusted by the calculated force
                    //velocity += netForce * 0.08f;
                    velocity = netForce;

                    // dont go over the maximum movement speed possible
                    if (velocity.magnitude > MaxMoveSpeed)
                        velocity = (velocity / velocity.magnitude) * MaxMoveSpeed;

                    // move
                    movement = new Vector3(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime, 0);
                    movement *= moveSpeed;
                }
            }
        }

        // become idle, and the characters around you
        private void ReachedDestination()
        {
            velocity = Vector2.zero;
            movement = Vector3.zero;

            _seekerState = State.Idle;
            
            _idleDestination.x = _idleRelativePosition.x + transform.position.x;
            _idleDestination.y = _idleRelativePosition.y + transform.position.y;

            if (_miner != null)
            {
//                if (_mode == Mode.Manual)
//                    _miner.SetAutomation(_lastMinerAutomation);
//                else
                _miner.DestinationIsReached();
            }
            stopUnitNow = false;
        }

        private static Vector2 CombineForces(float maxForceParam, Vector2[] forces)
        {
            Vector2 force = Vector2.zero;

            foreach (var t in forces)
            {
                Vector2 newForce = force + t;

                if (newForce.magnitude > maxForceParam)
                {
                    float amountNeeded = maxForceParam - force.magnitude;
                    float amountAdded = t.magnitude;
                    float division = amountNeeded / amountAdded;

                    force += division * t;

                    return force;
                }

                force = newForce;
            }

            return force;
        }

        private Vector2 FlowFieldFollow()
        {
            desiredFlowValue = _pathfinder.seekerManager.FindFlowValueFromPosition(
                transform.position,
                flowFieldPath.FlowField,
                this
            );

            if (desiredFlowValue == Vector2.zero && currentWorldArea != null && currentTile != null &&
                currentTile != flowFieldPath.Destination && !_pathfinder.WorkingOnPathAdjustment(flowFieldPath.Key,
                    currentWorldArea.Index, currentTile.SectorIndex))
            {
                _pathfinder.AddSectorInPath(currentWorldArea, currentTile, flowFieldPath);
            }

            // return the velocity we desire to go to
            desiredFlowValue *= MaxMoveSpeed; // we desire this velocity

            return desiredFlowValue;
        }

        private Vector2 SeparationMove(float squaredRadius)
        {
            var position = transform.position;

            if (neighbours[0] == null)
                return Vector2.zero;

            Vector2 totalForce = Vector2.zero;

            int neighbourAmount = 0;
            // get average push force away from neighbours
            foreach (var t in neighbours)
            {
                if (t == null)
                    break;

                var positionT = t.transform.position;

                Vector2 pushForce = new Vector2(position.x - positionT.x,
                    position.z - positionT.z);
                totalForce += pushForce.normalized * Mathf.Max(0.05f, (squaredRadius - pushForce.magnitude));
                neighbourAmount++;
            }

            totalForce /= neighbourAmount; //neighbours.Count;
            totalForce *= maxForce;

            return totalForce;
        }

        private Vector2 SeparationIdle()
        {
            var position = transform.position;

            var idleDestination = new Vector2(
                _idleDestination.x - position.x,
                _idleDestination.y - position.y
            );

            if (idleDestination.magnitude < 0.1)
                return new Vector2();
            //currentTile = _pathfinder.worldData.TileManager.GetTileFromPosition(transform.position);
            return idleDestination;
        }

        private Vector2 Cohesion()
        {
            if (neighbours[0] == null)
                return Vector2.zero;

            var position = transform.position;

            Vector2 pos = new Vector2(position.x, position.z);
            Vector2 centerOfMass = pos;

            int neighbourAmount = 0;
            foreach (var t in neighbours)
            {
                if (t == null)
                    break;

                var positionT = t.transform.position;

                centerOfMass += new Vector2(positionT.x, positionT.z);
                neighbourAmount++;
            }

            centerOfMass /= neighbourAmount;

            Vector2 desired = centerOfMass - pos;
            desired *= (MaxMoveSpeed / desired.magnitude);

            Vector2 force = desired - velocity;
            return force * (maxForce / MaxMoveSpeed);
        }

        private Vector2 Alignment()
        {
            if (neighbours[0] == null)
                return Vector2.zero;

            // get average velocity from neighbours
            Vector2 averageHeading = velocity.normalized;

            int neighbourAmount = 0;
            foreach (var t in neighbours)
            {
                if (t == null)
                    break;

                averageHeading += t.velocity.normalized;
                neighbourAmount++;
            }

            averageHeading /= neighbourAmount;

            Vector2 desired = averageHeading * MaxMoveSpeed;

            Vector2 force = desired - velocity;
            return force * (maxForce / MaxMoveSpeed);
        }

        private void GetPositionListAroundRing(List<float> ringDistance, List<int> ringCount)
        {
            Vector3 startPosition = new Vector3(0, 0, 0);
            
            _positionListAroundRing.Clear();
            _positionListAroundRing.Add(startPosition);

            for (var ring = 0; ring < ringCount.Count; ring++)
            {
                GetPositionListAround(startPosition, ringDistance[ring], ringCount[ring]);
                _positionListAroundRing.AddRange(_positionListAround);
            }
        }

        private void GetPositionListAround(Vector3 startPosition, float distance, int positionCount)
        {
            _positionListAround.Clear();

            for (var i = 0; i < positionCount; i++)
            {
                var angle = i * (360 / positionCount);
                var dir = ApplyRotationToVector(new Vector3(0, 1, 0), angle);
                var position = startPosition + dir * distance;
                _positionListAround.Add(position);
            }
        }

        private static Vector3 ApplyRotationToVector(Vector3 vec, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vec;
        }

        #endregion

        #region PublicMethods

        // Update is called once per frame
        public void Tick()
        {
            if (!this)
                return;

            if (stopUnitNow)
            {
                ReachedDestination();
                return;
            }
            switch (_seekerState)
            {
                case State.Idle:
                    Idle();
                    break;
                case State.Moving:
                    Move();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // make sure Seeker will not fall in null area, off map or go into a blocked of tile
            _pathfinder.seekerManager.CheckIfMovementLegit(this);

            _spaceship.SetMoveTo(velocity);
            _controller2D.Move(movement);
        }

        public void SetIndexSeeker(int indexSeeker)
        {
            _indexSeeker = indexSeeker;
            
            _ringCount.Clear();
            _ringDistance.Clear();
            
            float dist = 0;
            var count = 0;

            for (var i = _indexSeeker; i > 0; i -= count)
            {
                dist += 1.5f;
                _ringDistance.Add(dist);
                count += 5;
                _ringCount.Add(count);
            }

            GetPositionListAroundRing(_ringDistance, _ringCount);

            _idleRelativePosition = _positionListAroundRing[_indexSeeker];
        }

        public void SetFlowField(FlowFieldPath flowFieldPathParam, bool pathEdit)
        {
            if (!pathEdit)
            {
                SetNeighbourRadius();
            }
            
            _seekerState = State.Moving;
            flowFieldPath = flowFieldPathParam;
        }
        
        #endregion
    }
}