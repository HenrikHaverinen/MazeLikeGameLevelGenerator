using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PointGenerator : MonoBehaviour
{
    bool endPointGeneration = false;

    int linkVectorMagnitude; // the length of the link between nodes (same as grid offsetAmount)
    public int vectorDivisor = 1; // divides the link vector magnitude by this amount of times

    public GameObject point;

    enum Direction
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    private void Start()
    {
        // get the distance between nodes, set as value of linkVectorMagnitude
        if (linkVectorMagnitude == 0) { linkVectorMagnitude = GameObject.Find("Grid").GetComponent<Grid>().offsetAmount; }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("Subgraph") != null && !endPointGeneration) { GeneratePoints(); }
    }

    // Generate points inside the maze
    void GeneratePoints()
    {
        endPointGeneration = true;

        var nodes = GameObject.Find("Subgraph").GetComponent<Graph>().nodes; // all nodes
        var links = GameObject.Find("Subgraph").GetComponent<Graph>().links; // all links

        // Generate the points at the locations of the nodes
        foreach (var node in nodes)
        {
            // generate a point at the location of each node
            var p = Instantiate(point, node.transform.position, Quaternion.identity);
            p.name = node.name + "point";
        }

        // Delete duplicate links before generating points between the nodes of the links
        for (int i = links.Count - 1; i >= 0; i--)
        {
            if (links.Where(x => x.nodes.Contains(links[i].nodes[0]) && x.nodes.Contains(links[i].nodes[1])).ToList().Count >= 2) // duplicate -> delete duplicate
            {
                links.RemoveAt(i);
            }
        }

        // the distance between the points
        float pointDistance = float.Parse(linkVectorMagnitude.ToString()) / vectorDivisor; 
        Debug.Log("Distance between points: " + pointDistance);

        foreach (var link in links) // generate points in-between nodes of each link
        {
            Node node = link.nodes[0];
            Node other = link.nodes[1];

            Vector3 vec = node.transform.position; // vector that determines the position of the point

            Direction dir = NodeDirection(node, other);

            // generate points in-between nodes starting from one node, towards the other node
            for (int i = 1; i < vectorDivisor; i++)
            {
                if (dir == Direction.Left){vec = new Vector3(node.transform.position.x + (i * pointDistance), node.transform.position.y, node.transform.position.z);}
                if (dir == Direction.Right){vec = new Vector3(node.transform.position.x - (i * pointDistance), node.transform.position.y, node.transform.position.z);}
                if (dir == Direction.Up){vec = new Vector3(node.transform.position.x, node.transform.position.y, node.transform.position.z - (i * pointDistance));}
                if (dir == Direction.Down){vec = new Vector3(node.transform.position.x, node.transform.position.y, node.transform.position.z + (i * pointDistance));}

                var p = Instantiate(point, vec, Quaternion.identity); // instantiate point at position of vec
                p.name = node + "-" + other + "point";
            }
        }
    }

    Direction NodeDirection(Node node0, Node node1)
    {
        if (node0.transform.position.x < node1.transform.position.x) { return Direction.Left; }
        if (node0.transform.position.x > node1.transform.position.x) { return Direction.Right; }
        if (node0.transform.position.z > node1.transform.position.z) { return Direction.Up; }
        if (node0.transform.position.z < node1.transform.position.z) { return Direction.Down; }

        return Direction.None;
    }
}