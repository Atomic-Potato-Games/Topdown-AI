using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class AgentsGenerator : MonoBehaviour
{
    [SerializeField] int _numberOfPoints = 8;
    [SerializeField] float _radius = 5f;
    [SerializeField] Agent _agentPrefab;

    void Awake()
    {
        List<Vector2> _circleEdges = GenerateCircleEdgePositions();
        
        int j = _circleEdges.Count / 2;
        for (int i=0; i < _circleEdges.Count; i++)
        {
            Agent agent = Instantiate(_agentPrefab, _circleEdges[i], Quaternion.identity, transform);
            agent.name = "Agent " + i;
            Transform target = new GameObject("Target " + i).transform;
            target.position = _circleEdges[i];
            
            target.transform.position = _circleEdges[j];
            agent.Target = target;
            agent.gameObject.SetActive(true);

            j = (j + 1) % _circleEdges.Count;
        }
    }

    List<Vector2> GenerateCircleEdgePositions()
    {
        List<Vector2> positions = new List<Vector2>();
        float angleIncrement = 360f / _numberOfPoints;
        for (int i = 0; i < _numberOfPoints; i++)
        {
            float angleInRadians = i * angleIncrement * Mathf.Deg2Rad;
            positions.Add(new Vector2( 
                _radius * Mathf.Cos(angleInRadians),
                _radius * Mathf.Sin(angleInRadians)));
        }
        return positions;
    }
}
