using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Prim : MonoBehaviour
{
    public GameObject Graph; // The target graph that this algorithm is executed on
    public GameObject SubGraph; // The subgraph that forms as a result of running the algorithm
    List<Link> LinksOriginalGraph = new List<Link>(); // The links of the original graph
    public List<Link> Links = new List<Link>(); // The links of the subgraph
    public List<Node> Nodes = new List<Node>(); // The nodes of the subgraph

    public void ExecuteAlgorithm()
    {
        // performance
        GameObject.Find("PerformanceTrackerMazeGen").GetComponent<PerformanceTracker>().StartTracking();

        // Get a copy of the links that exist in the graph we want to run the algorithm on
        LinksOriginalGraph = Graph.GetComponent<Graph>().links;

        var index = Random.Range(0, Graph.GetComponent<Graph>().nodes.Count); // Get random index
        Node startNode = Graph.GetComponent<Graph>().nodes[index]; // Get the corresponding node

        // start with a link that is connected to the start node, get the one with the lowest weight
        Links.Add(Graph.GetComponent<Graph>().links.Where(x => x.nodes.Contains(startNode)).OrderBy(x => x.weight).FirstOrDefault());

        foreach (var item in Links) // Add the nodes from the first link to the Nodes list
        {
            Nodes.AddRange(item.nodes);
        }

        // look for links that have not yet been added to the subgraph, until all nodes are part of subgraph
        while (Nodes.Count < Graph.GetComponent<Graph>().nodes.Count)
        {
            foreach (var item in LinksOriginalGraph.OrderBy(x => x.weight)) // Links of original graph by weight
            {
                if (Nodes.Contains(item.nodes[0]) || Nodes.Contains(item.nodes[1])) // Select a link that includes a node already in subgraph
                {
                    if (!UnqualifiedLink(item)) // Make sure that links both nodes arent already part of the subgraph
                    {
                        // Add the link, and the node which isnt already in the Nodes list
                        Links.Add(item);
                        if (!Nodes.Contains(item.nodes[0]))
                        {
                            Nodes.Add(item.nodes[0]);
                        }
                        if (!Nodes.Contains(item.nodes[1]))
                        {
                            Nodes.Add(item.nodes[1]);
                        }
                    }                    
                }
            }
        }

        // performance
        GameObject.Find("PerformanceTrackerMazeGen").GetComponent<PerformanceTracker>().EndTracking();

        // only include the subgraph links for the nodes
        foreach (var item in Nodes)
        {
            List<Link> toRemove = new List<Link>();
            foreach (var link in item.links)
            {
                if (!Links.Contains(link))
                {
                    toRemove.Add(link);
                }
            }
            item.links = item.links.Except(toRemove).ToList();
        }

        // Create the subgraph gameobject
        SubGraph = new GameObject();
        SubGraph.name = "Subgraph";
        SubGraph.AddComponent<Graph>();
        SubGraph.GetComponent<Graph>().nodes = Nodes;
        SubGraph.GetComponent<Graph>().links = Links;
        SubGraph.GetComponent<Graph>().GenerateMaze();
    }

    bool UnqualifiedLink(Link link)
    {
        if (Nodes.Contains(link.nodes[0]) && Nodes.Contains(link.nodes[1])) // Dont add a link which both nodes are already part of the subgraph
        {
            return true;
        }
        return false;
    }
}