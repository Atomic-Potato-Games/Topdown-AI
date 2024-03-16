using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public class SmoothPath : Path
    {
        /// <summary>
        /// The boundary points that the agent should cross for a smooth path
        /// </summary>
        public readonly Line[] TurningBoundaries;
        public readonly int LastTruningBoundaryIndex;

        public SmoothPath(Vector2[] wayPoints, Vector2 startingPosition, float turningDistance, float stoppingDistance, Vector2? exactTargetPosition = null)
        {
            if (exactTargetPosition != null)
                AddExactTargetPositionToWaypoints();
            
            WayPoints = wayPoints;
            TurningBoundaries = new Line[wayPoints.Length];
            LastTruningBoundaryIndex = wayPoints.Length-1;
            ConstructBoundaryPoints();
            StoppingIndex = GetStoppingIndex(wayPoints, stoppingDistance);
            
            void ConstructBoundaryPoints()
            {
                Vector2 previousPoint = startingPosition;
                for (int i = 0; i < wayPoints.Length; i++)
                {
                    Vector2 directionToPoint = (wayPoints[i] - previousPoint).normalized;
                    // The boundary/turning point is turningDistance away from the actual waypoint  
                    Vector2 turningPoint = (i == TurningBoundaries.Length - 1) ? wayPoints[i] : wayPoints[i] - directionToPoint * turningDistance;
                    // We use the previous point as the perpendicular point so we can know what is the approaching side
                    // of the boundary. And in case of turningDistance > distance from current to previous pointing,
                    // we just provide the previous point moved away from the boundary with the same turning distance.
                    TurningBoundaries[i] = new Line(turningPoint, previousPoint - directionToPoint * turningDistance); 

                    previousPoint = wayPoints[i];
                }
            }

            void AddExactTargetPositionToWaypoints()
            {
                List<Vector2> wayPointsWithExactTarget = wayPoints.ToList();
                wayPointsWithExactTarget.RemoveAt(wayPoints.Length - 1);
                wayPointsWithExactTarget.Add((Vector2)exactTargetPosition);
                wayPoints = wayPointsWithExactTarget.ToArray();
            }
        }

        public override void DrawPathWithGizmos(int startingIndex, Color pathColor, Vector3? agentPosition = null, Vector3? targetPosition = null)
        {
            if (WayPoints == null)
                return;

            Gizmos.color = pathColor;
            if (agentPosition != null)
                Gizmos.DrawLine(CurrentWaypointPosition, (Vector3)agentPosition);
            for (int i = startingIndex; i <= LastTruningBoundaryIndex; i++)
            {
                Gizmos.color = pathColor;
                Gizmos.DrawCube(WayPoints[i], Vector3.one * .25f);
                TurningBoundaries[i].DrawLineWithGizmos(.5f);
            }
        }

        public override bool IncrementPathIndex()
        {
            if (IsReachedEndOfPath)
                return false;
            CurrentPathIndex++;
            if (CurrentPathIndex > LastTruningBoundaryIndex)
            {
                IsReachedEndOfPath = true;
                CurrentPathIndex--;
                return false;
            }
            return true;
        }
    }
}
