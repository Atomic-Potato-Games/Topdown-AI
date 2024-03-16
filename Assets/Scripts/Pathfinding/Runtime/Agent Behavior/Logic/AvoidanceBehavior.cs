using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// A basic behavior to avoid other agents in the path preventing any overlap with other agents.
    /// It is based on BOIDs (flock simulation)
    /// </summary>
    [CreateAssetMenu (fileName = "Avoidance Behavior", menuName = "Pathfinding/Behavior/Avoidance")]
    public class AvoidanceBehavior : AgentBehavior
    {
        [Tooltip(
            "Toggles agents priority. Agents with greater priority than others are not affected by other agents." +
            "This opption helps remove point contesting (When 2 or more agents are heading to the same destination)")]
        [SerializeField] bool _isUseCongestionControl;
        
        public override Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            // Debug.Log(agent.gameObject.name + " neighbors : " + neighbors.Count);
            bool isNeighborsExist = neighbors.Count != 0;
            if (!isNeighborsExist)
                return Vector2.zero;
                
            return GetNeighborsAvgAvoidanceDirection() * agent.SpeedMultiplier;

            // In BOIDs, this function is the avoidance rule. Taking the sum of the opposite direction from
            // each neighbor, and returning the average.
            Vector3 GetNeighborsAvgAvoidanceDirection()
            {
                Vector3 sum = Vector3.zero;
                foreach (Agent neighbor in neighbors)
                {
                    if (neighbor.Priority == 0 && agent.Priority != 0)
                    {
                        if (neighbor.PriorityCache > agent.Priority)
                        {
                            SwitchPriorityWithAgent(neighbor);
                            continue;
                        }
                    }

                    bool isGreaterPriorityThanNeighbor = _isUseCongestionControl && neighbor.Priority < agent.Priority; 
                    if (isGreaterPriorityThanNeighbor)
                        continue;

                    Vector3 direction = agent.transform.position - neighbor.transform.position; 
                    sum += direction != Vector3.zero ? 
                        direction :
                        // in case the agent is in the exact position of its neighbor, we take a random direction
                        new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
                }

                // Debug.Log(agent.gameObject.name + " heading : " + (sum / neighborsToAvoidCount).normalized);
                return (neighbors.Count > 0 ? sum / neighbors.Count : sum).normalized;

                void SwitchPriorityWithAgent(Agent other)
                {
                    int aux = other.PriorityCache;
                    other.PriorityCache = agent.Priority;
                    agent.Priority = aux;
                }
            }
        }
    }
}
