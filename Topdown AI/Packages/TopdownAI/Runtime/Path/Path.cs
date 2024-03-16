using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// The path containing the waypoints and boundaries of a smooth path.
    /// (Further Info: https://youtu.be/NjQjl-ZBXoY?si=AdBJxxU53rdT40rL)
    /// </summary>
    public abstract class Path
    {
        /// <summary>
        /// The waypoints returned from A*
        /// </summary>
        public Vector2[] WayPoints {get; protected set;}
        /// <summary>
        /// The path index where the agent should start slowing down
        /// </summary>
        public int StoppingIndex {get; protected set;}
        /// <summary>
        /// The waypoints array index for the current waypoint (Increment using IncrementPathIndex())
        /// </summary>
        public int CurrentPathIndex {get; protected set;}
        /// <summary>
        /// The position of the current agent waypoint on the path
        /// </summary>
        public Vector2 CurrentWaypointPosition => WayPoints[CurrentPathIndex];
        /// <summary>
        /// Is true in case of the path index has reached the end of the waypoints array 
        /// (Updates in CurrentWaypointPosition)
        /// </summary>
        public bool IsReachedEndOfPath {get; protected set;}

        /// <summary>
        /// Draws the path in Gizmos (Note: Must be called in OnDrawGizmos)
        /// </summary>
        public abstract void DrawPathWithGizmos(int startingIndex, Color pathColor, Vector3? agentPosition = null, Vector3? targetPosition = null);

        /// <summary>
        /// Increments the waypoints index CurrentPathIndex and updates IsReachedDestination
        /// </summary>
        public abstract bool IncrementPathIndex();

        /// <summary>
        /// Gets the index of which node in the path where the agent should start slowing down
        /// </summary>
        protected int GetStoppingIndex(Vector2[] wayPoints, float stoppingDistance)
        {
            float distanceFromEndPoint = 0;
            for (int i=wayPoints.Length - 1; i > 0; i--)
            {
                distanceFromEndPoint += Vector2.Distance(wayPoints[i], wayPoints[i-1]);
                if (distanceFromEndPoint > stoppingDistance)
                    return i;
            }
            return wayPoints.Length - 1;
        }
    }
}
