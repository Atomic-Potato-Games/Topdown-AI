using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// A class used to implement the agent movement behavior
    /// </summary>
    public abstract class AgentBehavior : ScriptableObject
    {
        /// <summary>
        /// A function that calculates and returns a direction multiplied by a speed, i.e. velocity
        /// of which the agent should move according to.
        /// </summary>
        /// <param name="agent">The agent to which calculate the behavior for</param>
        /// <param name="neighbors">The provided agent nehiboring agents</param>
        /// <param name="destination">The current heading destination of the agent (usually the next point in its path)</param>
        /// <returns></returns>
        public abstract Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination);
    }
}
