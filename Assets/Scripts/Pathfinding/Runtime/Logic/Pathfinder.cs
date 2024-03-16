using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

namespace Pathfinding
{
    /// <summary>
    /// Handles the calculation of the A* algorithm which finds the shortest path between 2 nodes on a grid 
    /// (Further info: https://youtu.be/-L-WgKMFuhE?si=3rsrBzKm9MGdusFL)
    /// </summary>
    public class Pathfinder : MonoBehaviour
    {
        [Space, Header("Performance")]
        [SerializeField] bool _isLogTimeToGetPath;

        /// <summary>
        /// The set of nodes discovered and to be evaluated
        /// </summary>
        Heap<Node> _openSet;
        /// <summary>
        /// The set of nodes explored and are already evaluated
        /// </summary>
        HashSet<Node> _closedSet;

        void Start()
        {
            _openSet = new Heap<Node>(GridsManager.GlobalMaxSize);
            _closedSet = new HashSet<Node>();
        }

        /// <summary>
        /// Gets the shortest path on the grid between 2 nodes
        /// </summary>
        /// <param name="request">A request containing all the necessary data, such as the start and end node</param>
        /// <param name="callback">The function to be called at the end to return the path result</param>
        public void FindPathOnGrid(PathRequest request, Action<PathRequestResult> callback)
        {
            // Debugging performance
            Stopwatch sw = null;
            if (_isLogTimeToGetPath)
            {
                sw = new Stopwatch();
                sw.Start();
            } 

            Node startNode = request.Grid.GetNodeFromWorldPosition(request.StartPosition);
            Node endNode = request.Grid.GetNodeFromWorldPosition(request.EndPosition);

            Vector2[] pathWaypoints = null;
            bool isFoundPath = false;

            if (!IsEndNodeSameAsCached())
            {
                // This is the equivalent of creating a new list, but without generating garbage
                _openSet.Clear();
                _closedSet.Clear();
                _openSet.Add(startNode);
                    
                while (_openSet.Count > 0)
                {
                    // The first node in the heap is the one with the lowest F cost
                    Node currentNode = _openSet.RemoveFirst();
                    _closedSet.Add(currentNode);
                    bool isReachedEndNode = currentNode == endNode;

                    if (isReachedEndNode)
                    {
                        if (_isLogTimeToGetPath)
                        {
                            sw?.Stop();
                            UnityEngine.Debug.Log("Time: " + sw.ElapsedMilliseconds + "ms");
                        }
                        isFoundPath = true;
                        break;
                    }

                    UpddateNeighboors(currentNode);
                }
            }
            
            if (isFoundPath)
            {
                pathWaypoints = RetracePath(startNode, endNode);
                // In cases where the agent is above its target, 
                // then the target has already reached its destination
                // so there is no actual path.
                // If we dont do this, it may result in an index out of bounds when following the path.
                isFoundPath = pathWaypoints.Length > 0;
            }

            callback(new PathRequestResult(pathWaypoints, isFoundPath, endNode, request.Callback));
            
            // Updates the G,F,H costs and parents of the neighboring nods
            // More info: https://youtu.be/-L-WgKMFuhE?si=xKQMF33U1CehauBs
            void UpddateNeighboors(Node currentNode)
            {
                foreach (Node neighbor in currentNode.Neighboors)
                {
                    if (!neighbor.IsWalkable || _closedSet.Contains(neighbor))
                        continue;
                    
                    int distanceToNeighboorUsingCurrentPath = currentNode.G_Cost + GetDistanceToNode(currentNode, neighbor);
                    if (distanceToNeighboorUsingCurrentPath < neighbor.G_Cost ||!_openSet.Contains(neighbor))
                    {
                        neighbor.G_Cost = distanceToNeighboorUsingCurrentPath;
                        neighbor.H_Cost = GetDistanceToNode(neighbor, endNode);

                        neighbor.Parent = currentNode;

                        if (!_openSet.Contains(neighbor))
                            _openSet.Add(neighbor);
                        else
                            _openSet.UpdateItem(neighbor);
                    }
                }
            }

            bool IsEndNodeSameAsCached()
            {
                return endNode == request.EndNodeCache;
            }
        }

        /// <summary>
        /// Returns the distance between 2 nodes in terms of cost
        /// </summary>
        /// <returns>The cost distance between the 2 nodes</returns>
        int GetDistanceToNode(Node a, Node b)
        {
            // It is aggreed upon in A* pathfinding that
            // diagnoally adjacent nodes have a distance of 14 (touching corners)
            // and parallel adjacent nodes have a distance of 10 (touching borders)
            // So the distnace between nodes is the sum of straight and diagonal moves 
            // taken to reach that node
            int distanceX = Mathf.Abs(a.GridPositionX - b.GridPositionX);
            int distanceY = Mathf.Abs(a.GridPositionY - b.GridPositionY);

            int greaterDistance;
            int smallerDistance;
            if (distanceX > distanceY)
            {
                greaterDistance = distanceX;
                smallerDistance = distanceY;
            }
            else
            {
                greaterDistance = distanceY;
                smallerDistance = distanceX;
            }

            return 14 * smallerDistance + 10 * (greaterDistance - smallerDistance);
        }

        /// <summary>
        /// Creats a nodes array by traversing the parents of the endNode until reaching the startNode
        /// </summary>
        Vector2[] RetracePath(Node startNode, Node endNode)
        {
            if (startNode == null || endNode == null)
                return null;

            Node currentNode = endNode;
            List<Node> path = new List<Node>();
            
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;    
            }

            path.Add(currentNode);  
            path.Reverse();
            return SimplifyPath();

            // Removes uneeded nodes from the path by only keeping nodes where the direction changes 
            Vector2[] SimplifyPath()
            {
                Vector2 previousDirection = Vector2.zero;
                List<Vector2> simplifiedPath = new List<Vector2>();

                for (int i=1; i < path.Count; i++)
                {
                    Vector2 newDirection = (path[i].GridPosition - path[i-1].GridPosition).normalized;
                    if (previousDirection != newDirection)
                    {
                        if (i != 1) // Remove if you want to include first point in the path
                                    // Generally the agent's position is the first point
                                    // and skipping this gives nicer results
                            simplifiedPath.Add(path[i-1].WorldPosition);
                    }
                    previousDirection = newDirection;

                    // The path will sometimes skip a waypoint needed to get around corners
                    // so we check on the last node if the direction with the startNode doesnt match the previous
                    // if so, we add the last node
                    // https://i.imgur.com/KkA3Y3Q.png
                    if (i == path.Count-1)
                    {
                        Vector2 directionToStartNode = new Vector2(path[i].GridPositionX - startNode.GridPositionX, path[i].GridPositionY - startNode.GridPositionY);
                        if (directionToStartNode != previousDirection)
                            simplifiedPath.Add(path[i].WorldPosition);
                    }
                }
                            
                return simplifiedPath.ToArray();
            }
        }
    }
}
