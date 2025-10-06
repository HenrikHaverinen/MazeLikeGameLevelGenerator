using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class BFS : MonoBehaviour
{
    public List<Link> links = new List<Link>(); // list for manipulating the links
    public List<Node> nodes = new List<Node>(); // list for manipulating the nodes
    List<Node> rootNodes = new List<Node>(); // the nodes from which the exploration of nodes continues

    public List<Path_> paths = new List<Path_>(); // the paths created by BFS

    public bool autoCheckCompletion = false;
    bool endCheckCanComplete = false; // determines whether to run CheckCanComplete method or not
    bool canComplete = false; // can the level be completed

    Path_ path = new Path_(); // The path that is formed by the breadth-first-search

    Path_ completionPath = new Path_();

    void Update()
    {
        // Check automatically whether the level can be completed or not
        if (GameObject.Find("Subgraph") != null && endCheckCanComplete == false && autoCheckCompletion)
        {
            GameObject.Find("PerformanceTrackerBFS").GetComponent<PerformanceTracker>().StartTracking();
            CheckCanComplete();
        }
    }

    // Check that the level can be completed
    void CheckCanComplete()
    {
        endCheckCanComplete = true;
        path.nodes.Clear();
        rootNodes.Clear();

        nodes = GameObject.Find("Subgraph").GetComponent<Graph>().nodes;

        // unexplore the nodes
        foreach (var item in nodes)
        {
            item.explored = false;
            foreach (var link in item.links)
            {
                link.explored = false;
            }
        }

        Node start = nodes.Where(x => x.nature == Node.Nature.Start).FirstOrDefault(); // start node
        ExploreNodeForCompletion(start); // begin BFS

        // debug all paths found with BFS execution (steps)
        //int i = 1;
        //foreach (var p in paths)
        //{
        //    string pathStr = "Path " + i + ": ";
        //    foreach (var n in p.nodes)
        //    {
        //        pathStr += n + ", ";
        //    }
        //    Debug.Log(pathStr);

        //    i++;
        //}

        GameObject.Find("PerformanceTrackerBFS").GetComponent<PerformanceTracker>().EndTracking();

        Debug.Log("Level can be completed: " + canComplete);

        if (canComplete)
        {
            string completionPathStr = "";
            foreach (var item in completionPath.nodes)
            {
                completionPathStr += item + ", ";
            }
            Debug.Log("Completion path: " + completionPathStr);
        }
    }

    void ExploreNodeForCompletion(Node node) // Explore the node
    {
        // start node -> create the first path
        if (node.nature  == Node.Nature.Start && paths.Count == 0)
        {
            // create the first ever path
            Path_ path1 = new Path_();
            path1.nodes.Add(node);
            paths.Add(path1);
        }

        //Debug.Log("Current: " + node);

        if (!canComplete)
        {
            // Explore all of the nodes connected to this node
            foreach (var item in node.links.Where(x => x.explored == false))
            {
                if (item.nodes[0] != node)
                {
                    item.nodes[0].explored = true;
                    rootNodes.Add(item.nodes[0]);
                    ExploreLink(item.nodes[0], item.nodes[1]);

                    // Get the path from the paths list where the current node is found -> append the path with node on the other end of the link
                    Path_ BFSPath = new Path_();
                    bool latestFound = false; // found the path

                    // select the path where the current node is found
                    foreach (var p in paths.Where(x => x.nodes.Contains(node)))
                    {
                        // start from the latest node of the path, move backwards
                        for (int i = p.nodes.Count - 1; i >= 0; i--)
                        {
                            if (p.nodes[i] == node && !latestFound) // found the path
                            {
                                // only add nodes from start node up until the current node to new path
                                for (int j = 0; j <= i; j++)
                                {
                                    BFSPath.nodes.Add(p.nodes[j]);
                                }
                                latestFound = true; // dont go deeper backwards
                            }
                        }
                    }
                    BFSPath.nodes.Add(item.nodes[0]); // add the node linked to the current node
                    paths.Add(BFSPath);

                    if (item.nodes[0].nature == Node.Nature.End && completionPath.nodes.Count == 0)
                    {
                        completionPath.nodes = BFSPath.nodes;
                        canComplete = true;
                    }
                }
                if (item.nodes[1] != node)
                {
                    item.nodes[1].explored = true;
                    rootNodes.Add(item.nodes[1]);
                    ExploreLink(item.nodes[0], item.nodes[1]);

                    Path_ BFSPath = new Path_();

                    bool latestFound = false;

                    // select the path where the current node is found
                    foreach (var p in paths.Where(x => x.nodes.Contains(node)))
                    {
                        // start from the latest node of the path, move backwards
                        for (int i = p.nodes.Count - 1; i >= 0; i--)
                        {
                            if (p.nodes[i] == node && !latestFound) // found the path
                            {
                                // only add nodes from start node up until the current node to new path
                                for (int j = 0; j <= i; j++)
                                {
                                    BFSPath.nodes.Add(p.nodes[j]);
                                }
                                latestFound = true; // dont go deeper backwards
                            }
                        }
                    }
                    BFSPath.nodes.Add(item.nodes[1]); // add the node linked to the current node
                    paths.Add(BFSPath);

                    if (item.nodes[1].nature == Node.Nature.End && completionPath.nodes.Count == 0)
                    {
                        completionPath.nodes = BFSPath.nodes;
                        canComplete = true;
                    }
                }
            }

            // delete the node after nodes connected to it have been explored
            if (rootNodes.Contains(node))
            {
                rootNodes.Remove(node);
            }

            if (rootNodes.Count > 0)
            {
                ExploreNodeForCompletion(rootNodes[0]); // Enter the rootnode that was explored first
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
}
