using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// The agent class to be attached to a game object for it to be able to follow a target on grid path 
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Agent : MonoBehaviour
    {
        #region Global Variables
        [Tooltip ("The target to follow")]
        public Transform Target;

        [Space]
        [Tooltip ("The speed of the agent")]
        [Min (0)] public float SpeedMultiplier = 1f;
        [Tooltip ("The distance to the destination before the agent starts slowing down to a complete halt")]
        public float StoppingDistance = 1f;
        [Tooltip ("The detection radius of other agents. By default it is only used to avoid other agents in this radius. "
            + "(Is Draw Neighbors Detection Radius to visualize)")]
        [SerializeField, Min(0)] protected float _neighborsDetectionRadius = 1f;
        public float NeighborsDetectionRadius => _neighborsDetectionRadius;
        
        [Space, Header("Smooth Path")]
        [Tooltip ("Smoothes the agent movement and rotation (if Is Rotate With Movement is enabled)")]
        [SerializeField] bool _isUseSmoothPath;
        public bool IsUseSmoothPath => _isUseSmoothPath;
        [Tooltip ("The distance offset from the actual path waypoint. At this offset the agent will start turning. "
            + "(Is Draw Path to visualize)")]
        [SerializeField, Min(0)] float _smoothPathTurningDistance = 0f;
        public float SmoothPathTurningDistance => _smoothPathTurningDistance;
        [Min (0)] public float SmoothPathTurningSpeed = 4.5f; 

        [Space, Header("Other")]
        [Tooltip ("Rotates the agent forward (to be more specific transfrom.right) along with its movement direction")]
        public bool IsRotateWithMovement;
        [Tooltip ("Goes to the exact position of the target instead of sticking to the grid")]
        public bool IsReachExactTargetPosition;
        [Tooltip ("If disabled, the agent will not try to move towards the final waypoint if its position is changed by other factors")]
        public bool IsKeepFollowingLastWaypoint = true;
        [Tooltip ("Currently only affects which grid the agent will be assigned to. Type A gets Grid A and so on. "
            + "Useful in case if you have agents with different sizes")]
        public Type SelectedType = Type.A;
        [Tooltip ("The movement behavior of the agent. By default it is a composite behavior" +
            " containing the follow path and avoidance behaviors")]
        public AgentBehavior Behavior;
        [Tooltip ("Agents layer to help detect other agents "
            + "(Note: Make sure that agents on the same layer do not collide in the projects collision matrix)")]
        [SerializeField] protected LayerMask _agentsLayer;

        [Space, Header("Gizmos")]
        [SerializeField] bool _isDrawGizmos;
        [SerializeField] bool _isDrawPath;
        [SerializeField] Color _pathColor = new Color(0f, 0f, 1f, .5f);
        [SerializeField] bool _isRandomPathColor = false;
        [SerializeField] bool _isDrawNeighborsDetectionRadius;

        /// <summary>
        /// Used for other agents to detect this agent as a neighbor. 
        /// (Note: Make sure that agents on the same layer do not collide in the projects collision matrix)
        /// </summary>
        public Collider2D Collider {get; protected set;}

        /// <summary>
        /// Contains common data shared between agents that is assigned at the start of the game
        /// </summary>
        protected AgentsManager _agentsManager;
        /// <summary>
        /// The grid that the agent is using to create a path with
        /// </summary>
        public Grid Grid {get; protected set;}
        /// <summary>
        /// The current path to the target
        /// </summary>
        public Path Path {get; protected set;}

        /// <summary>
        /// Agents priority are currently only used in the avoidance agent behavior 
        /// to make agents with higher priority not get affect by lower priority neighbors.
        /// Priority is stored in PriorityCache when agent is not moving and this is set to 0.
        /// </summary>
        [HideInInspector] public int Priority;
        /// <summary>
        /// Stores the priority when the agent is not moving
        /// </summary>
        [HideInInspector] public int PriorityCache;
        /// <summary>
        /// A cache to be used for smooth vector rotation in smooth paths follow behavior
        /// </summary>
        [HideInInspector] public Vector2 MoveDirectionCache;
        /// <summary>
        /// True if the agent movement is caused from the behavior assigned to it
        /// </summary>
        public bool IsMoving {get; protected set;}
        /// <summary>
        /// True if the agent is already looking for a path to the target. 
        /// Except in the case of ForceSendPathRequest()
        /// </summary>
        public bool IsPathRequestSent {get; protected set;}

        /// <summary>
        /// Stores the previous path request target position node. 
        /// Used to elminate non necessary path requests by comparing the target node with the previous path request
        /// </summary>
        protected Node _endNodeCache = null;

        /// <summary>
        /// Currently only corresponds to the different grid types contained in the grid manager
        /// </summary>
        public enum Type {A, B, C, D, E,}
        #endregion

        #region Execution
        void OnDrawGizmos()
        {
            if (!_isDrawGizmos)
                return;

            if (_isDrawPath && Path != null)
                Path.DrawPathWithGizmos(Path.CurrentPathIndex, _pathColor, transform.position, Target.position);

            if (_isDrawNeighborsDetectionRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, _neighborsDetectionRadius);
            }
        }

        void Awake()
        {
            Collider = GetComponent<Collider2D>();
            if (_isRandomPathColor)
                _pathColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1);
        }

        void Start()
        {
            GetDataFromAgentsManager();
            Grid = GridsManager.Instance.GetGrid(SelectedType);

            void GetDataFromAgentsManager()
            {
                _agentsManager = AgentsManager.Instance;
                _agentsManager.Agents.Add(this);
                if (_agentsManager.GeneralTarget != null)
                    Target = _agentsManager.GeneralTarget;
                Behavior = _agentsManager.AgentBehavior;
                Priority = AgentsManager.Instance.GetUniqueAgentID();
            }
        }

        void Update()
        {
            SendPathRequest();
            Move();
            UpdatePriority();
        }
        #endregion

        #region Getting a Path
        Coroutine _pathRequestCoroutine;
        /// <summary>
        /// Sends a request to update the path 
        /// </summary>
        public void SendPathRequest()
        {
            if (IsPathRequestSent)
                return;
                
            if (_pathRequestCoroutine == null)
                _pathRequestCoroutine = StartCoroutine(SendRequest());

            IEnumerator SendRequest()
            {
                if (Target == null)
                    yield break;

                // Delaying the path request at the start of the game
                // since delta time is quite high at the start
                if (Time.timeSinceLevelLoad < .3f)
                    yield return new WaitForSecondsRealtime(.3f);

                PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, Grid, _endNodeCache, UpdatePath));
                IsPathRequestSent = true;
                _pathRequestCoroutine = null;
            }
        }

        Coroutine _forcePathRequestCoroutine;
        /// <summary>
        /// Sends a request to update the path regardless of any restrictions
        /// (Currently the only restriction is the target end node must be different than the last path request)
        /// </summary>
        public void ForceSendPathRequest()
        {
            if (_pathRequestCoroutine != null)
            {
                StopCoroutine(_pathRequestCoroutine);
                _pathRequestCoroutine = null;
            }
            if (IsPathRequestSent == true)
                IsPathRequestSent = false;

            if (_forcePathRequestCoroutine == null)
                _forcePathRequestCoroutine = StartCoroutine(SendRequest());

            IEnumerator SendRequest()
            {
                if (Target == null)
                    yield break;

                // Delaying the path request at the start of the game
                // since delta time is quite high at the start
                if (Time.timeSinceLevelLoad < .3f)
                    yield return new WaitForSecondsRealtime(.3f);
                
                PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, Grid, null, UpdatePath));
                _forcePathRequestCoroutine = null;
            }
        }

        /// <summary>
        /// Updates the current path, when a result is returned from the PathRequestManager
        /// </summary>
        /// <param name="newPath">The new requested path</param>
        /// <param name="isFoundPath">If a path exists to the target. If false, the path will not update</param>
        /// <param name="endNode">Used to elminate non necessary path requests by comparing the target node with the previous path request</param>
        void UpdatePath(Vector2[] newPath, bool isFoundPath, Node endNode)
        {
            IsPathRequestSent = false;   
            _endNodeCache = endNode;

            if (!isFoundPath)
                return;

            Path = CreatePath(newPath);
            UpdatePathIndex();

        }

        /// <summary>
        /// Creates and returns a path based on the agent properties and the provided waypoints
        /// </summary>
        protected Path CreatePath(Vector2[] waypoints)
        {
            Vector2? exactTargetPosition = IsReachExactTargetPosition ? (Vector2?)Target.position : null;
            return IsUseSmoothPath ? 
                (Path) new SmoothPath(waypoints, transform.position, _smoothPathTurningDistance, StoppingDistance, exactTargetPosition) :
                (Path) new StraightPath(waypoints, StoppingDistance, exactTargetPosition);
        }
        #endregion

        #region Moving
        /// <summary>
        /// Calculates the average velocity from each behavior (such as Follow Path or Avoidance behaviors)
        /// and moves the agent transform with that velocity.
        /// </summary>
        void Move()
        {
            Vector2 currentWaypoint = Path == null ? Vector2.zero : Path.CurrentWaypointPosition;
            Vector3 velocity = (Vector3)Behavior.CalculateBehaviorVelocity(this, GetNeighbors(), currentWaypoint);
            if (velocity != Vector3.zero)
            {
                IsMoving = true;
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                IsMoving = false;
            }

            if (IsRotateWithMovement)
            {
                if (velocity != Vector3.zero)
                {
                    float angle = Mathf.Atan2(velocity.normalized.y, velocity.normalized.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }
        }
        
        /// <summary>
        /// Populates and returns a list of surrounding agents that are in the agent neighbors detection radius
        /// </summary>
        public List<Agent> GetNeighbors()
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _neighborsDetectionRadius, Vector2.zero, Mathf.Infinity, _agentsLayer);
            List<Agent> neighbors = new List<Agent>();
            foreach(RaycastHit2D hit in hits)
            {
                if (hit.collider != Collider)
                    neighbors.Add(hit.collider.gameObject.GetComponent<Agent>());
            }
            return neighbors;
        }

        Coroutine _updatePathIndexCoroutine;
        /// <summary>
        /// Updates the current path index based on the agent position.
        /// </summary>
        void UpdatePathIndex()
        {
            // NOTE:    I have tried to place the methods inside of the paths classes
            //          but you cant create a new instance of class that uses MonoBehavior
            //          (because you need IEnumerator). 
            //          The other solution was to add a new component instead of using the
            //          new keyword, but that felt messy in my opinion.

            if (_updatePathIndexCoroutine != null)
                StopCoroutine(_updatePathIndexCoroutine);
            _updatePathIndexCoroutine = IsUseSmoothPath ?  
                StartCoroutine(UpdateSmoothPathWaypoint()) : 
                StartCoroutine(UpdateStraightPathWaypoint());
            
            IEnumerator UpdateStraightPathWaypoint()
            {
                StraightPath straightPath = (StraightPath)Path;
                if (straightPath.WayPoints.Length == 0)
                    yield break;

                while(true)
                {
                    bool isReachedCurrentWayPoint = Vector2.Distance(transform.position, straightPath.CurrentWaypointPosition) < 0.01f;
                    if (isReachedCurrentWayPoint)
                    {
                        straightPath.IncrementPathIndex();
                        if (straightPath.IsReachedEndOfPath)
                        {
                            FinishPath();
                            yield break;
                        }
                    }
                    yield return new WaitForEndOfFrame();
                }
            }

            IEnumerator UpdateSmoothPathWaypoint()
            {
                SmoothPath smoothPath = (SmoothPath)Path;

                while (true)
                {
                    while (smoothPath.TurningBoundaries[smoothPath.CurrentPathIndex].IsCorssedLine(transform.position))
                    {
                        if (smoothPath.IsReachedEndOfPath)
                        {
                            FinishPath();
                            yield break;
                        }
                        else
                        {
                            smoothPath.IncrementPathIndex();
                        }
                    }
                    yield return null;
                }
            }

            void FinishPath()
            {
                _updatePathIndexCoroutine = null;
            }
        }
        #endregion

        /// <summary>
        /// Sets the priority to 0 when not moving to allow other agents to pass 
        /// if the agent is blocking the path of other agents
        /// </summary>
        void UpdatePriority()
        {
            if (!IsMoving)
            {
                if (Priority != 0)
                {
                    PriorityCache = Priority;
                    Priority = 0;
                }
            }
            else
            {
                if (PriorityCache != 0)
                {
                    Priority = PriorityCache;
                    PriorityCache = 0;
                }
            }
        }
    }
}