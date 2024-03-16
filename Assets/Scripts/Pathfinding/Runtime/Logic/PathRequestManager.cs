using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace Pathfinding
{
    /// <summary>
    /// Handles path requests on the pathfinder
    /// </summary>
    [RequireComponent(typeof(Pathfinder))]
    public class PathRequestManager : Singleton<PathRequestManager>
    {
        /// <summary>
        /// Responsible for calculating the shortest path between nodes. i.e. requests handler
        /// </summary>
        Pathfinder _pathfinder;
        /// <summary>
        /// Agents path request results queue. Used to call the callback functions on the main thread 
        /// </summary>
        Queue<PathRequestResult> _results = new Queue<PathRequestResult>();

        new void Awake()
        {
            base.Awake();
            _pathfinder = GetComponent<Pathfinder>();
        }

        void Update()
        {
            DequeueResults();
        }

        /// <summary>
        /// Dequeues and invokes the callback function for each path request result
        /// </summary>
        void DequeueResults()
        {
            // NOTE:
            //      This function is to be executed on the main thread
            //      Since if not, the callback function will be called on
            //      a different thread causing unpredictable behaviors
            
            if (_results.Count == 0)
                return;

            lock (_results)
            {
                while (_results.Count > 0)
                {
                    PathRequestResult result = _results.Dequeue();
                    result.Callback(result.Path, result.IsSuccess, result.EndNodeCache);
                }
            }
        }

        /// <summary>
        /// Sends the pathfinder a request for a path
        /// </summary>
        /// <param name="request"></param>
        public static void RequestPath(PathRequest request)
        {
            ThreadStart threadStart = delegate
            {
                Instance._pathfinder.FindPathOnGrid(request, Instance.FinishProcessingPathRequest);
            };
            threadStart.Invoke();
        }

        /// <summary>
        /// Adds the returned result from the pathfinder to the results queue
        /// </summary>
        /// <param name="result"></param>
        void FinishProcessingPathRequest(PathRequestResult result)
        {
            lock (_results)
            {
                _results.Enqueue(result);
            }
        }

    }

    /// <summary>
    /// Holds the data returned by the Pathfinder
    /// </summary>
    public struct PathRequestResult
    {
        /// <summary>
        /// Returned path
        /// </summary>
        public Vector2[] Path;
        /// <summary>
        /// If the path is found or not
        /// </summary>
        public bool IsSuccess;
        /// <summary>
        /// Cache to be used for checking if the target node has changed
        /// </summary>
        public Node EndNodeCache;
        /// <summary>
        /// The function to be called on the agent when the resulting path is returned
        /// </summary>
        public Action<Vector2[], bool, Node> Callback;

        public PathRequestResult(Vector2[] path, bool isSuccess, Node endNodeCache, Action<Vector2[], bool, Node> callback)
        {
            this.Path = path;
            this.IsSuccess = isSuccess;
            this.EndNodeCache = endNodeCache;
            this.Callback = callback;
        }
    }

    /// <summary>
    /// Holds the data required by the Pathfinder
    /// </summary>
    public struct PathRequest
    {
        /// <summary>
        /// The start of the path. Usually the agent's position
        /// </summary>
        public Vector2 StartPosition;
        /// <summary>
        /// The end target of the path
        /// </summary>
        public Vector3 EndPosition;
        /// <summary>
        /// Which grid to find the path on
        /// </summary>
        public Grid Grid;
        /// <summary>
        /// Cache to be used for checking if the target node has changed
        /// </summary>
        public Node EndNodeCache;
        /// <summary>
        /// The function to be called on the agent when the resulting path is returned
        /// </summary>
        public Action<Vector2[], bool, Node> Callback; 

        public PathRequest(Vector2 startPosition, Vector2 endPosition, Grid grid, Node endNodeCache, Action<Vector2[], bool, Node> callback)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Grid = grid;
            EndNodeCache = endNodeCache;
            Callback = callback;
        }
    }
}