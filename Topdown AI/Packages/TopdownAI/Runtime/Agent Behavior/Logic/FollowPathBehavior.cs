using System.Collections.Generic;
using UnityEngine;
namespace Pathfinding
{
    /// <summary>
    /// Calculates the velocity to follow a path, either smooth or straight 
    /// </summary>
    [CreateAssetMenu (fileName = "Follow Path Behavior", menuName = "Pathfinding/Behavior/Follow Path")]
    public class FollowPathBehavior : AgentBehavior
    {
        public override Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            if (agent.Path == null || (agent.Path.IsReachedEndOfPath && !agent.IsKeepFollowingLastWaypoint))
                return Vector2.zero;

            float speedPercent = GetSlowDownSpeedPercent();     // Used to slow down the agent as it gets closer to the target
            Vector2 direction = GetDirection();
            speedPercent = speedPercent > .01f ? speedPercent : 0;
            return direction * agent.SpeedMultiplier * speedPercent;

            float GetSlowDownSpeedPercent()
            {
                    if (agent.Path.CurrentPathIndex >= agent.Path.StoppingIndex && agent.StoppingDistance > 0)
                    {
                        Path path = agent.Path;
                        float remainingDistance = agent.IsUseSmoothPath ? 
                            ((SmoothPath)path).TurningBoundaries[((SmoothPath)path).LastTruningBoundaryIndex].GetDistanceFromPoint(agent.transform.position):
                            Vector2.Distance(agent.transform.position, path.WayPoints[path.WayPoints.Length - 1]);
                        return Mathf.Clamp01(remainingDistance / agent.StoppingDistance);
                    }
                return 1;
            }

            Vector2 GetDirection()
            {
                if (!agent.IsUseSmoothPath)
                {
                    return (destination - (Vector2)agent.transform.position).normalized;
                }
                else
                {
                    Vector3 targetDirection = (destination - (Vector2)agent.transform.position).normalized;
                    agent.MoveDirectionCache = Vector2.Lerp(agent.MoveDirectionCache, targetDirection, Time.deltaTime * agent.SmoothPathTurningSpeed).normalized;
                    return agent.MoveDirectionCache;
                }
            }
        }
    }
}
