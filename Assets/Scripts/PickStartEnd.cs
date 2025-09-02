using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PickStartEnd : MonoBehaviour
{
    public GameObject graph;

    public void PickStartEndNodes() // Pick the start and end nodes of the maze
    {
        // List of all the nodes on the edge
        List<Node> edgeNodes = graph.GetComponent<Graph>().nodes.Where(x => x.Edge == true).ToList();

        int startIndex = Random.Range(0, edgeNodes.Count);
        int endIndex = startIndex;
        while (endIndex == startIndex) // change the endIndex to something other than startIndex
        {
            endIndex = Random.Range(0, edgeNodes.Count);
        }

        Node startNode = edgeNodes[startIndex];
        Node endNode = edgeNodes[endIndex];

        graph.GetComponent<Graph>().SetEndAndStartNodes(startNode, endNode);
    }
}
