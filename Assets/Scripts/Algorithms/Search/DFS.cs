using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DFS : MonoBehaviour
{
    public List<Link> links = new List<Link>(); // list for manipulating the links
    public List<Node> nodes = new List<Node>(); // list for manipulating the nodes
    List<Node> BranchingNodes = new List<Node>(); // list of nodes to return to in case of a dead end
    Path_ path = new Path_(); // The path that is formed by the depth-first-search

    List<Node> disjointGraph = new List<Node>(); // holds the data for the entire disjoint graph
    public List<List<Node>> disjointGraphs = new List<List<Node>>();

    bool cycleExists = false; // to determine whether a cycle was found or not
    bool endSearch = false; // determine whether to keep searching or not

    Node possibleCulprit = null; // the node that may result in the cycle forming when linked to the simulated node
    Node simulatedNode = null; // the simulated node

    List<Node> CycleBetween = new List<Node>(); // The nodes that form the link that forms the cycle

    // Returns a path that contains every node connected to the start node
    public List<List<Node>> GetDisjointGraphs()
    {
        List<List<Node>> allDisjointGraphs = new List<List<Node>>();

        path.nodes.Clear();
        disjointGraph.Clear();
        disjointGraphs.Clear();
        endSearch = false;
        BranchingNodes.Clear();

        List<Node> toDeleteNodes = new List<Node>();

        foreach (var item in nodes)
        {
            item.explored = false;
            // Ignore simulated nodes
            if (item.simulated)
            {
                toDeleteNodes.Add(item);
            }

            foreach (var link in item.links)
            {
                link.explored = false;
            }
        }

        nodes = nodes.Except(toDeleteNodes).ToList(); // Never include simulated nodes

        // In the case that the node has no links
        foreach (var n in nodes)
        {
            if (n.links == null || n.links.Count == 0)
            {
                List<Node> singular = new List<Node>();
                singular.Add(n);
                allDisjointGraphs.Add(singular);

                n.explored = true;
            }
        }

        while (!endSearch)
        {
            if (UnexploredLinksRemaining()) // while every link of every node hasnt been checked
            {
                List<Node> unexplored = nodes.Where(x => x.explored == false).ToList();
                if (unexplored.Count != 0) // nodes left to explore
                {
                    ExploreNodeIgnoreCycle(unexplored[Random.Range(0, unexplored.Count)]); // Explore an unexplored node

                    // add the disjoint graph
                    List<Node> tempList = disjointGraph.Distinct().ToList();
                    allDisjointGraphs.Add(tempList);

                    disjointGraph.Clear(); // clear the disjoint graph
                    disjointGraph = new List<Node>();
                }
                else { endSearch = true; }
            }
            else { endSearch = true; }
        }

        disjointGraphs = allDisjointGraphs;

        return allDisjointGraphs;
    }

    void ExploreNodeIgnoreCycle(Node node) // Explore the node, dont mind cycles
    {
        node.explored = true;

        disjointGraph.Add(node); // Add also to disjointGraph

        // Set the node as branching if node has multiple links
        if (node.links.Where(x => x.explored == false).ToList().Count > 1)
        {
            if (!BranchingNodes.Contains(node)) // dont add same node multiple times
            {
                BranchingNodes.Add(node); // Node has multiple links
            }
        }

        path.nodes.Add(node); // Add the node to the path

        // Select one of the links of the node and explore the node on the other end
        // Explore the links
        foreach (var item in node.links.Where(x => x.explored == false))
        {
            if (item.nodes[0] != node)
            {
                ExploreLink(item.nodes[0], item.nodes[1]);
                ExploreNodeIgnoreCycle(item.nodes[0]); // Enter the new node in search
            }
            if (item.nodes[1] != node)
            {
                ExploreLink(item.nodes[0], item.nodes[1]);
                ExploreNodeIgnoreCycle(item.nodes[1]); // Enter the new node in search
            }
        }

        // if the node doesnt have any links, it is going to automatically return to the last branching node, but the path from that branching node to the dead end must be cleared before returning
        DeadEnd();

        if (BranchingNodes.Contains(node) && node.links.Where(x => x.explored == true).ToList().Count == node.links.Count) // Remove the node as a branching node when all of its links have been explored
        {
            BranchingNodes.Remove(node);
            DeadEnd();
        }
    }

    void ExploreNode(Node node) // Explore the node
    {
        node.explored = true;

        disjointGraph.Add(node); // Add also to disjointGraph

        // Set the node as branching if node has multiple links
        if (node.links.Where(x => x.explored == false).ToList().Count > 1)
        {
            if (!BranchingNodes.Contains(node)) // dont add same node multiple times
            {
                BranchingNodes.Add(node); // Node has multiple links
            }
        }

        path.nodes.Add(node); // Add the node to the path

        if (Cycle()) // The cycle check
        {
            cycleExists = true;
            endSearch = true;
            return;
        }
        else
        {
            // Select one of the links of the node and explore the node on the other end
            // Explore the links
            foreach (var item in node.links.Where(x => x.explored == false))
            {
                if (cycleExists)
                {
                    return;
                }
                else
                {
                    if (!cycleExists)
                    {
                        if (item.nodes[0] != node)
                        {
                            ExploreLink(item.nodes[0], item.nodes[1]);
                            ExploreNode(item.nodes[0]); // Enter the new node in search
                        }
                    }
                    else { return; }
                    if (!cycleExists) // Cease exploring nodes once a cycle has been identified
                    {
                        if (item.nodes[1] != node)
                        {
                            ExploreLink(item.nodes[0], item.nodes[1]);
                            ExploreNode(item.nodes[1]); // Enter the new node in search
                        }
                    }
                    else { return; }
                }
            }

            if (!cycleExists)
            {
                // if the node doesnt have any links, it is going to automatically return to the last branching node, but the path from that branching node to the dead end must be cleared before returning
                DeadEnd();

                if (BranchingNodes.Contains(node) && node.links.Where(x => x.explored == true).ToList().Count == node.links.Count) // Remove the node as a branching node when all of its links have been explored
                {
                    BranchingNodes.Remove(node);
                    DeadEnd();
                }
            }
        }
    }

    void ExploreLink(Node from, Node to) // set links as explored we used to come to the current node
    {
        if (from.links.Where(x => x.nodes.Contains(from) && x.nodes.Contains(to)).ToList().Count > 0)
        {
            from.links.Where(x => x.nodes.Contains(from) && x.nodes.Contains(to)).FirstOrDefault().explored = true;
        }
        if (to.links.Where(x => x.nodes.Contains(from) && x.nodes.Contains(to)).ToList().Count > 0)
        {
            to.links.Where(x => x.nodes.Contains(from) && x.nodes.Contains(to)).FirstOrDefault().explored = true;
        }
    }

    void DeleteLink(Link link) // Deletion of the link we used to come to the current node
    {
        Node from = link.nodes[0];
        Node to = link.nodes[1];

        from.links.Remove(from.links.Where(x => x.nodes.Contains(from) && x.nodes.Contains(to)).FirstOrDefault());
        to.links.Remove(to.links.Where(x => x.nodes.Contains(from) && x.nodes.Contains(to)).FirstOrDefault());
    }

    void DeadEnd() // Delete the nodes from branch node to dead end for path
    {
        Node latestBranchingNode = new Node();
        if (BranchingNodes.Count > 0)
        {
            latestBranchingNode = BranchingNodes[BranchingNodes.Count - 1]; // latest branch
        }

        List<Node> toDelete = new List<Node>(); // the nodes that will be deleted from the path
        bool deleteFromNowOn = false;
        foreach (var item in path.nodes)
        {
            if (deleteFromNowOn) // Delete every node after the branching node has been identified
            {
                toDelete.Add(item);
            }
            if (item == latestBranchingNode)
            {
                deleteFromNowOn = true;
            }
        }

        path.nodes = path.nodes.Except(toDelete).ToList(); // delete the nodes that lead to dead end
    }

    // Rewritten, so that in case a cycle exists, we can get the nodes that ultimately form the cycle
    bool Cycle() // Checks if the path forms a cycle
    {
        CycleBetween.Clear();
        possibleCulprit = null;

        Dictionary<Node, int> NodeOccurances = new Dictionary<Node, int>(); // Node, times it occurs in path
        Node previousNode = null; // The node that comes before the node that forms cycle
        Debug.Log("Cycle check: ");
        string pathStr = "";

        bool capturePossibleCulpritNode = false;

        foreach (var item in path.nodes)
        {
            if (capturePossibleCulpritNode)
            {
                possibleCulprit = item;
                capturePossibleCulpritNode = false;
            }

            if (item.simulated)
            {
                // capture the node that comes after the simulated node, for possible future use in identifying the problematic link that is the cause of the cycle, also capture the simulated node
                capturePossibleCulpritNode = true;
                simulatedNode = item;
            }

            pathStr += item.name + ", ";
            if (NodeOccurances.ContainsKey(item))
            {
                NodeOccurances[item]++; // Cycle exists
                Debug.Log(pathStr);
                Debug.Log("Cycle exists");

                if (possibleCulprit != null)
                {
                    // the cycle formed as a result of a link between the simulated node and the culprit node (a node that occured after the simulated node in terms of the DFS node visit order)
                    CycleBetween.Add(possibleCulprit);
                    CycleBetween.Add(simulatedNode);
                }
                else
                {
                    // the cycle formed as a result of a link between the simulated node and the previous node 
                    CycleBetween.Add(item);
                    CycleBetween.Add(previousNode);
                }

                return true;
            }
            else
            {
                NodeOccurances[item] = 1;
            }
            previousNode = item; // Keep track of the previous node
        }
        Debug.Log(pathStr);

        return false;
    }

    bool UnexploredLinksRemaining()
    {
        foreach (var item in nodes)
        {
            if (item.links.Where(x => x.explored == true).ToList().Count != item.links.Count) { return true; }
        }
        return false;
    }

    // Kruskal DFS methods (links are deleted rather than explored)
    public bool DetectCycle() // Search the graph for cycles
    {
        endSearch = false;
        cycleExists = false;
        BranchingNodes.Clear();

        // unexplore nodes
        foreach (var item in nodes)
        {
            item.explored = false;
        }

        // searches through every node for cycles, even if graph is disjointed
        while (!endSearch)
        {
            if (UnexploredLinksRemainingKruskal() && !cycleExists) // while every link of every node hasnt been checked
            {
                List<Node> unexplored = nodes.Where(x => x.explored == false).ToList();
                if (unexplored.Count != 0) // nodes left to explore
                {
                    ExploreNodeKruskal(unexplored[Random.Range(0, unexplored.Count)]); // Explore an unexplored node
                    path.nodes.Clear(); // clear the path, in case of moving to a new isolated node
                }
                else { endSearch = true; }
            }
            else { endSearch = true; }
        }

        // Debug
        if (!cycleExists)
        {
            Debug.Log("No cycle!");
        }
        else
        {
            Debug.Log("Yes cycle!");
        }

        return cycleExists;
    }

    void ExploreNodeKruskal(Node node) // Explore the node
    {
        node.explored = true;

        // Set the node as branching if node has multiple links
        if (node.links.Count > 1)
        {
            if (!BranchingNodes.Contains(node)) // dont add same node multiple times
            {
                BranchingNodes.Add(node); // Node has multiple links
            }
        }

        path.nodes.Add(node); // Add the node to the path

        if (Cycle()) // The cycle check
        {
            cycleExists = true;
            endSearch = true;
            return;
        }
        else
        {
            // Select one of the links of the node and explore the node on the other end
            // Remove links from the end of the collection
            for (int i = node.links.Count - 1; i >= 0; i--)
            {
                if (cycleExists)
                {
                    return;
                }
                else
                {
                    Link refLink = null; // reference to the link that will be deleted
                    if (node.links.Count > 0) { refLink = node.links[i]; } // set value
                    if (!cycleExists)
                    {
                        if (refLink != null)
                        {
                            if (refLink.nodes[0] != node)
                            {
                                DeleteLink(node.links[i]); // Traveling to new node, delete the link
                                ExploreNodeKruskal(refLink.nodes[0]); // Enter the new node in search
                            }
                        }
                    }
                    else { return; }
                    if (!cycleExists) // Cease exploring nodes once a cycle has been identified
                    {
                        if (refLink != null)
                        {
                            if (refLink.nodes[1] != node)
                            {
                                DeleteLink(node.links[i]);
                                ExploreNodeKruskal(refLink.nodes[1]);
                            }
                        }
                    }
                    else { return; }
                }
            }

            if (!cycleExists)
            {
                // if the node doesnt have any links, it is going to automatically return to the last branching node, but the path from that branching node to the dead end must be cleared before returning
                DeadEnd();

                if (BranchingNodes.Contains(node) && node.links.Count == 0) // Remove the node as a branching node when all of its links have been explored
                {
                    BranchingNodes.Remove(node);
                    DeadEnd();
                }
            }
        }
    }

    bool UnexploredLinksRemainingKruskal()
    {
        foreach (var item in nodes)
        {
            if (item.links.Count > 0) { return true; }
        }
        return false;
    }
}