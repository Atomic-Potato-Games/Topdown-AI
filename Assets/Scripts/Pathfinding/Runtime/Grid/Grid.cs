using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// Pathfinding grid on which the A* Pathfinder class operates to finds the shortest path between 2 points
    /// </summary>
    public class Grid : MonoBehaviour
    {
        #region Global Variables
        [Tooltip("ON: Grid will be generated at the start of the game. " +
            "OFF: Grid will not be generated at the start of the game.")]
        [SerializeField] bool _isActive;
        /// <summary>
        /// FALSE: Grid has no nodes /
        /// TRUE: Gid has nodes | Use ActivateGrid() and DeactivateGrid() 
        /// </summary>
        public bool IsActive => _isActive;
        [Tooltip("The size of the gird. Its recommended if its a square area.")]
        [SerializeField, Min(0f)] Vector2 _worldSize = new Vector2(1f,1f);
        [Tooltip("The area size that each node of the grid")]
        [SerializeField, Min(0f)] float _nodeRadius = 1f;  
        [Tooltip("The layer where nodes covering over a collider with this layer will be marked as unwalkable")]
        [SerializeField] LayerMask _unwalkableMask;

        [Space, Header("Gizmos")]
        [SerializeField] bool _isDisplayGrid;
        [SerializeField] Color _gridColor = new Color(1f,1f,1f,.5f);
        [SerializeField] Color _walkableNodesColor = new Color(0f, 1f, 0f, .5f);
        [SerializeField] Color _unwalkableNodesColor = new Color(1f, 0f, 0f, .5f);

        /// <summary>
        /// The width of the grid in nodes count
        /// </summary>
        public int NodesCountX {get; protected set;}
        /// <summary>
        /// The heigt of the grid in nodes count
        /// </summary>
        public int NodesCountY {get; protected set;}
        /// <summary>
        /// The number of nodes that exist in the grid
        /// </summary>
        public int MaxSize => NodesCountX * NodesCountY;
        
        /// <summary>
        /// The nodes contained in this grid
        /// </summary>
        public Node[,] Nodes {get; protected set;}
        /// <summary>
        /// The node radius * 2
        /// </summary>
        float _nodeDiameter;
        #endregion

        void OnDrawGizmos()
        {
            if (!_isDisplayGrid)
                return;

            Gizmos.color = _gridColor;
            Gizmos.DrawCube(transform.position, _worldSize);

            if (Nodes != null)
            {
                foreach (Node n in Nodes)
                {
                    Gizmos.color = n.IsWalkable ? _walkableNodesColor : _unwalkableNodesColor;
                    Gizmos.DrawCube(n.WorldPosition, new Vector3(_nodeDiameter - .1f, _nodeDiameter - .1f));
                }
            }
        }

        void Awake()
        {
            if (!_isActive)
                return;
            ActivateGrid();
        }

        /// <summary>
        /// Fills the grid with nodes. (NOTE: Use UpdateGrid() or UpdateGridSection() to update the grid)
        /// </summary>
        public void ActivateGrid()
        {
            _isActive = true;
            _nodeDiameter = _nodeRadius * 2f;
            NodesCountX = Mathf.RoundToInt(_worldSize.x/_nodeDiameter);
            NodesCountY = Mathf.RoundToInt(_worldSize.y/_nodeDiameter);
            // Ajusting world size to fit the grid
            _worldSize.x = _nodeDiameter * NodesCountX;
            _worldSize.y = _nodeDiameter * NodesCountY;
            CreateGrid();
        }

        /// <summary>
        /// Removes all nodes from the grid.
        /// </summary>
        public void DeactivateGrid()
        {
            _isActive = false;
            Nodes = null;
        }

        void CreateGrid()
        {
            Nodes = new Node[NodesCountX, NodesCountY];
            GenerateNodes();
            FindNodesNeighbors();

            // Creates nodes and marks them as either walkable or unwalkable
            void GenerateNodes()
            {
                Vector2 worldBottomLeftPosition = (Vector2)transform.position - new Vector2(_worldSize.x * .5f, _worldSize.y * .5f);
                for (int x = 0; x < NodesCountX; x++)
                {
                    for (int y = 0; y < NodesCountY; y++)
                    {
                        Vector2 worldPosition = worldBottomLeftPosition + new Vector2(x * _nodeDiameter + _nodeRadius, y * _nodeDiameter + _nodeRadius); 
                        bool isWalkable = !Physics2D.OverlapBox(worldPosition, new Vector2(_nodeDiameter, _nodeDiameter), 0f, _unwalkableMask);
                        Nodes[x,y] = new Node(isWalkable, worldPosition, x, y);
                    }
                }
            }

            // Assigns the neighbors for each node
            void FindNodesNeighbors()
            {
                foreach (Node n in Nodes)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            if(x == 0 && y == 0)
                                continue;

                            int neighboorX = n.GridPositionX + x;
                            int neighboorY = n.GridPositionY + y;
                            if(neighboorX < 0)
                                break;

                            if (neighboorX >= 0 && neighboorX < NodesCountX && neighboorY >= 0 && neighboorY < NodesCountY)
                            {
                                n.Neighboors.Add(Nodes[neighboorX,neighboorY]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the node in the grid where the provided world coordinate lies
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public Node GetNodeFromWorldPosition(Vector2 worldPosition)
        {
            // if we consider the size of the grid in percentage,
            // i.e bottom left would be (0%, 0%) and top right (100%, 100%)
            // then we can mapp it to the array row and col coordinates 
            
            // this is the expanded formula, just for explaining purposes
            // float positionPerecentX = (worldPosition.x + (transform.position.x + worldSize.x) * .5f) / (transform.position.x + worldSize.x);
            // the following way is simplified for performance
            float positionPerecentX = worldPosition.x / (transform.position.x + _worldSize.x) + .5f;
            float positionPerecentY = worldPosition.y / (transform.position.y + _worldSize.y) + .5f;

            // Note this does not make sense on its own, to simplify,
            // it is just this
            // Mathf.RoundToInt((_nodesCountX - 1) * positionPerecentX)
            // Mathf.RoundToInt((_nodesCountY - 1) * positionPerecentY)
            // but if you round down a percentage multiplied by a grid-1 
            // you can end with a target one node away from your actual current node.
            int x = Mathf.Abs(Mathf.FloorToInt(Mathf.Clamp(NodesCountX * positionPerecentX, 0, NodesCountX-1)));
            int y = Mathf.Abs(Mathf.FloorToInt(Mathf.Clamp(NodesCountY * positionPerecentY, 0, NodesCountY-1)));

            return Nodes[x,y];
        }

        /// <summary>
        /// Updates the nodes IsWalkable property in a provided rectangular area
        /// </summary>
        /// <param name="bottomLeftCornerPosition">The bottom left edge world position of the rectangular area to be updated</param>
        /// <param name="topRightCornerPosition">The top left edge world position of the rectangular area to be updated</param>
        public void UpdateGridSection(Vector2 bottomLeftCornerPosition, Vector2 topRightCornerPosition)
        {
            Node nodeBottomLeft = GetNodeFromWorldPosition(bottomLeftCornerPosition);
            Node nodeTopRight = GetNodeFromWorldPosition(topRightCornerPosition);
            
            for (int i = nodeBottomLeft.GridPositionX; i <= nodeTopRight.GridPositionX; i++)
            {
                for (int j = nodeBottomLeft.GridPositionY; j <= nodeTopRight.GridPositionY; j++)
                {
                    Nodes[i,j].IsWalkable = !Physics2D.OverlapBox(Nodes[i,j].WorldPosition, new Vector2(_nodeDiameter, _nodeDiameter), 0f, _unwalkableMask);
                }
            }
        }

        /// <summary>
        /// Updates the nodes IsWalkable property for the entire grid
        /// </summary>
        public void UpdateGrid()
        {
            
            for (int i = 0; i <= NodesCountX - 1; i++)
            {
                for (int j = 0; j <= NodesCountY - 1; j++)
                {
                    Nodes[i,j].IsWalkable = !Physics2D.OverlapBox(Nodes[i,j].WorldPosition, new Vector2(_nodeDiameter, _nodeDiameter), 0f, _unwalkableMask);
                }
            }
        }
    }
}