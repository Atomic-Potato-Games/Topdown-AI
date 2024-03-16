using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// Nodes to fill the Grid class with which can be marked as either walkable or unwalkable
    /// </summary>
    public class Node : IHeapItem<Node>
    {
        /// <summary>
        /// Connects the node with another node so the path can be retraced back 
        /// from this node back to the path starting node
        /// </summary>
        public Node Parent;

        /// <summary>
        /// The node horizontal index in the grid 
        /// </summary>
        public int GridPositionX;
        /// <summary>
        /// The node vertical index in the grid
        /// </summary>
        public int GridPositionY;
        /// <summary>
        /// The horizontal and vertical indices of the node inside the grid
        /// </summary>
        public Vector2 GridPosition;
        
        /// <summary>
        /// distance from starting node
        /// </summary>
        public int G_Cost;
        /// <summary>
        /// distance from end node
        /// </summary>
        public int H_Cost;
        /// <summary>
        /// H_Cost + G_Cost
        /// </summary>
        public int F_Cost => H_Cost + G_Cost; 

        int _heapIndex;
        /// <summary>
        /// The index of the node in the heap tree. Used to optimize the node search performance in A*
        /// </summary>
        public int HeapIndex
        {
            get
            {
                return _heapIndex;
            }
            set
            {
                _heapIndex = value;
            }
        }
        
        /// <summary>
        /// Can the agent walk on this node / will it be avoided in the A* algorithm
        /// </summary>
        public bool IsWalkable;
        /// <summary>
        /// The world cooridnates of the node (Use GridPosition to get the node indices in the grid)
        /// </summary>
        public Vector2 WorldPosition;
        /// <summary>
        /// The surrounding nodes (Usually 8 nodes unless the node is on the edge of the grid)
        /// </summary>
        public List<Node> Neighboors;

        public Node(bool isWalkable, Vector2 worldPosition, int gridPositionX, int gridPositionY)
        {
            IsWalkable = isWalkable;
            WorldPosition = worldPosition;
            GridPositionX = gridPositionX;
            GridPositionY = gridPositionY;
            GridPosition = new Vector2(gridPositionX, gridPositionY);
            Neighboors = new List<Node>();
        }

        /// <summary>
        /// Finds which node has a higher priority
        /// in other words, which is closer to the target
        /// </summary>
        public int CompareTo(Node n)
        {
            // Note: The CompareTo method returns a priority. In case of interger, larger means higher
            //       higher priority = 1
            //       equal priority = 0   
            //       lower priority = -1

            int result = F_Cost.CompareTo(n.F_Cost);
            if (result == 0)
                result = H_Cost.CompareTo(n.H_Cost);

            // Since we want the prirority to be higher for lower costs, we simply reverse our result
            return -result;
        }
    }
}
