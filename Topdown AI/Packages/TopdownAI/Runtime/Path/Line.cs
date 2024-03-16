using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// To construct a smooth path we will need the path way points and turning boundaries.
    /// This class is used for the boundaries. The agent should path at the boundary point.
    /// Each boundary line will have a specific side which the path will go through based on
    /// the previous point. (Further info in here: https://youtu.be/NjQjl-ZBXoY?si=s40s4XqjRSOvSIX6)
    /// </summary>
    public struct Line 
    {
        /// <summary>
        /// for a line to be vertical, the slope of the line has to be infinite
        /// </summary>
        const float _VERTICAL_LINE_GRADIENT = 1e5f;

        /// <summary>
        /// the m in y = mx + c. Used to get a second point on a line
        /// </summary>
        float _gradient;
        float _gradientPerpendicular;

        /// <summary>
        /// the c in y = mx + c. 
        /// </summary>
        float _yIntercept;

        Vector2 _pointOnLine1;
        Vector2 _pointOnLine2;

        /// <summary>
        /// Used for gizmos drawing
        /// </summary>
        public Vector2 LineCenterPoint => _pointOnLine1;

        /// <summary>
        /// From which side does the path go through the boundary
        /// </summary>
        Side _approachingSide;

        public enum Side
        {
            Above,
            Below
        }

        public Line (Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
        {
            float deltaX = pointOnLine.x - pointPerpendicularToLine.x;
            float deltaY = pointOnLine.y - pointPerpendicularToLine.y;

            _gradientPerpendicular = deltaX != 0 ? deltaY / deltaX : _VERTICAL_LINE_GRADIENT;
            _gradient = _gradientPerpendicular != 0 ? -1 / _gradientPerpendicular : _VERTICAL_LINE_GRADIENT;
            _yIntercept = pointOnLine.y - _gradient * pointOnLine.x;

            // We will need any 2 points on the line to be used in GetSide()
            // The second point explained visually:
            // https://www.desmos.com/calculator/eabenf7ioo (Further explanation in your notes)
            _pointOnLine1 = pointOnLine;
            _pointOnLine2 = pointOnLine + new Vector2(1, _gradient);
            
            _approachingSide = Side.Above;
            _approachingSide = GetSide(pointPerpendicularToLine); 
        }

        /// <summary>
        /// Returns if this point is above or below the line.
        /// </summary>
        /// <returns>Above or Below</returns>
        public Side GetSide(Vector2 point)
        {
            // Note:
            //      I have not checked but in this case i assume:
            //      True: above the line
            //      False: below the line

            // Explained here (more details in your notes):
            // https://www.youtube.com/watch?v=KHuI9bXZS74
            if ((point.x - _pointOnLine1.x) * (_pointOnLine2.y - _pointOnLine1.y)
                > (point.y - _pointOnLine1.y) * (_pointOnLine2.x - _pointOnLine1.y))
                return Side.Above;
            return Side.Below;
        }

        /// <summary>
        /// Checks if the provided point is on the opposite side of the path boundary. 
        /// </summary>
        public bool IsCorssedLine(Vector2 point)
        {
            return GetSide(point) != _approachingSide;
        }
        
        public float GetDistanceFromPoint(Vector2 point)
        {
            // Honestly i cannot bother explaining this, im too tired
            // but here it is (timestamped) https://youtu.be/bfevcsANSr4?si=uPc_XbeaMkIVt2p2&t=588
            float yInterceptPerpendicular = point.y - _gradientPerpendicular * point.x;
            float intersectX = (yInterceptPerpendicular - _yIntercept) / (_gradient - _gradientPerpendicular);
            float intersectY = _gradient * intersectX + _yIntercept;
            return (point - new Vector2(intersectX, intersectY)).sqrMagnitude;
        }

        /// <summary>
        /// Displays the line in gizmos
        /// </summary>
        public void DrawLineWithGizmos(float length)
        {
            Vector2 direction = new Vector2(1, _gradient).normalized;
            Gizmos.color = _approachingSide == Side.Above ? Color.blue : Color.white;
            Gizmos.DrawLine(_pointOnLine1, _pointOnLine1 + direction * length / 2f);
            Gizmos.color = _approachingSide == Side.Above ? Color.white : Color.blue;
            Gizmos.DrawLine(_pointOnLine1 - direction * length / 2f, _pointOnLine1);
        }
    }
}
