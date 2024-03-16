using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// As the name suggests, this class contains multiple behaviors, and it returns their average velocity
    /// </summary>
    [CreateAssetMenu (fileName = "Composite Behavior", menuName = "Pathfinding/Behavior/Composite")]
    public class CompositeBehavior : AgentBehavior
    {
        [Tooltip("The behaviors to be used on the agent")]
        [SerializeField] AgentBehavior[] _behaviors;
        [Tooltip(
            "The weight / strength of each behavior. Number of weights must match the number of behaviors. " +
            "The index of the weight corresponds to the behavior with the same index " )]
        [SerializeField, Min(0)] float[] _weights;
        public override Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            if (_behaviors.Length != _weights.Length)
                throw new System.Exception("Inequal weights count to behaviors!");
            
            float averageSpeed = 0f;
            Vector2 averageDirection = Vector2.zero;
            int totalNonZeroDirections = 0;
            int totalNonZeroSpeeds = 0;

            for (int i=0; i < _behaviors.Length; i++)
            {
                Vector2 behaviorVelocity = _behaviors[i].CalculateBehaviorVelocity(agent, neighbors, destination);
                bool behaviorDirectionNotZero = behaviorVelocity.normalized != Vector2.zero;
                bool behaviorSpeedNotZero = behaviorVelocity.magnitude != 0f;

                if (behaviorDirectionNotZero)
                {
                    // Debug.Log(agent.gameObject.name + "[" + i + "]" +  "behavior direciton: " + behaviorVelocity.normalized);
                    averageDirection += behaviorVelocity.normalized * _weights[i];
                    totalNonZeroDirections++;
                }
                if (behaviorSpeedNotZero)
                {
                    averageSpeed += behaviorVelocity.magnitude;
                    totalNonZeroSpeeds++;
                }
            }
            
            if (totalNonZeroDirections == 0 || totalNonZeroSpeeds == 0)
                return Vector2.zero;
            
            averageDirection.Normalize();
            averageDirection /= totalNonZeroDirections;
            averageSpeed /= totalNonZeroSpeeds;
            return averageDirection * averageSpeed;
        }
    }
}
