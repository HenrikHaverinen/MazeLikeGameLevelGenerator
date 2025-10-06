using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Kruskal : MonoBehaviour
{
    public GameObject Graph; // The target graph that this algorithm is executed on
    public GameObject SubGraph; // The subgraph that forms as a result of running the algorithm
    public DFS dfs; // depth-first-search
    List<Link> LinksOriginalGraph = new List<Link>(); // The links of the original graph
    public List<Link> Links = new List<Link>(); // The links of the subgraph
    public List<Node> Nodes = new List<Node>(); // The nodes of the subgraph

    public void ExecuteAlgorithm()
    {
        // performance
        GameObject.Find("PerformanceTrackerMazeGen").GetComponent<PerformanceTracker>().StartTracking();

        // Get a copy of the links that exist in the graph we want to run the algorithm on
        LinksOriginalGraph = Graph.GetComponent<Graph>().links;

        // add links to the tree, until all nodes are connected
        while (Nodes.Count < Graph.GetComponent<Graph>().nodes.Count)
        {
            foreach (var item in LinksOriginalGraph.OrderBy(x => x.weight)) // Links of original graph by weight
            {
                if (!UnqualifiedLink(item)) // Make sure that link to be added doesnt form a cycle if added
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
                else
                {
                    Links.Remove(item);
                }
            }
        }

        //Debug.Log("MST: ");
        //foreach (var item in Links.Distinct().ToList())
        //{
        //    Debug.Log(item.nodes[0] + "-" + item.nodes[1]);
        //}

        // performance
        GameObject.Find("PerformanceTrackerMazeGen").GetComponent<PerformanceTracker>().EndTracking();

        // add the subgraph links back to the nodes before creating MST_subgraph
        foreach (var item in Nodes)
        {
            foreach (var l in Links.Where(x => x.nodes.Contains(item)))
            {
                if (!item.links.Contains(l))
                {
                    item.links.Add(l);
                }
            }
        }

        // Create the subgraph gameobject
        SubGraph = new GameObject();
        SubGraph.name = "Subgraph";
        SubGraph.AddComponent<Graph>();
        SubGraph.GetComponent<Graph>().nodes = Nodes;
        SubGraph.GetComponent<Graph>().links = Links.Distinct().ToList();
        SubGraph.GetComponent<Graph>().GenerateMaze();
    }

    bool UnqualifiedLink(Link link)
    {
        // Check if adding the link to the tree forms a cycle, if yes -> unqualified
        List<Node> UnexploredNodes = new List<Node>(); // The nodes which we havent visited yet
        List<Link> linksWithAddition = Links;
        linksWithAddition.Add(link); // Add the link that may cause a cycle to form

        foreach (var item in linksWithAddition) // the links already added to the subgraph + the new addition
        {
            if (!UnexploredNodes.Contains(item.nodes[0])) // Dont add duplicate nodes
            {
                UnexploredNodes.Add(item.nodes[0]);
            }
            if (!UnexploredNodes.Contains(item.nodes[1])) // Dont add duplicate nodes
            {
                UnexploredNodes.Add(item.nodes[1]);
            }
        }

        // set links of nodes to only the links that have been added to the MST, + the potential addition link
        foreach (var item in UnexploredNodes)
        {
            item.links.Clear(); // Clear all of the original links for the node
            foreach (var l in linksWithAddition.Where(x => x.nodes.Contains(item)))
            {
                if (!item.links.Contains(l))
                {
                    item.links.Add(l);
                    item.links = item.links.Distinct().ToList(); // Dont add the same link multiple times
                }
            }
        }

        Debug.Log("ATTEMPT: add link " + link.nodes[0].name + "-" + link.nodes[1].name);

        dfs.nodes = UnexploredNodes;
        return dfs.DetectCycle(); // If cycle found -> unqualified
    }
}