using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// Contains a list of all the agents, and acts as a container for common values between all agents
    /// </summary>
    public class AgentsManager : Singleton<AgentsManager>
    {
        [Tooltip("A target that will be set for all the agents at the start of the game")]
        [SerializeField] Transform _generalTarget;
        /// <summary>
        /// Agents themselves must set this general target at the start of the game.
        /// Thats because of execution order complexities.
        /// </summary>
        public Transform GeneralTarget => _generalTarget;
        [Tooltip("The movement behavior for all agents. (Compoisite behavior by default. " +
            "Generally this shouldnt be changed, and you simply add custom behaviors to the Composite Behavior)")]
        [SerializeField] AgentBehavior _generalBehavior;
        /// <summary>
        /// Agents themselves must set this general behavior at the start of the game.
        /// Thats because of execution order complexities.
        /// </summary>
        public AgentBehavior AgentBehavior => _generalBehavior;

        /// <summary>
        /// A list containing all agents. Generally agents, choose at the start of the game to add themselves to this list.
        /// </summary>
        public List<Agent> Agents  {get; protected set;}

        /// <summary>
        /// A cache holding a unique priority number. Agents priority are currently only used in the avoidance agent behavior
        /// </summary>
        int _currentAgentPriority = 1; // Note: we start at 1 because agents will switch to 0 when not moving

        new void Awake()
        {
            base.Awake();
            Agents = new List<Agent>();
        }

        public int GetUniqueAgentID()
        {
            return _currentAgentPriority++;
        }
    }
}
