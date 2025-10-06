using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WFC : MonoBehaviour
{
    public GameObject Grid; // The grid the algorithm works on
    List<WFC_Cell> cells = new List<WFC_Cell>(); // The cells that need to be observed
    List<GameObject> ObservedCells = new List<GameObject>(); // The final states
    bool ObservingToDo = true;

    DFS dfs = null;

    public GameObject Subgraph; // the subgraph which this algorithm forms
    public GameObject Empty; // Empty gameobject for holding data during simulation

    List<List<Node>> trees = new List<List<Node>>();
    Path_ mergeGraph = new Path_();

    List<Node> AllObservedNodes = new List<Node>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dfs = gameObject.GetComponent<DFS>();
    }

    public void ExecuteAlgorithm()
    {
        // performance
        GameObject.Find("PerformanceTrackerMazeGen").GetComponent<PerformanceTracker>().StartTracking();

        foreach (Transform c in this.Grid.transform)
        {
            if (c.GetComponent<WFC_Cell>() != null)
            {
                cells.Add(c.GetComponent<WFC_Cell>());
            }
        }

        while (ObservingToDo)
        {
            // Update the connectivities of each cell
            foreach (var item in cells)
            {
                item.CheckConnectivity();
            }

            foreach (var item in cells)
            {
                item.UpdateStates();
            }

            foreach (var item in cells.Where(x => x.Observed))
            {
                item.UpdateLinks();
            }

            // Pick the unobserved cell with the lowest entropy
            WFC_Cell c = cells.Where(c => c.Observed != true).OrderBy(x => x.entropy).FirstOrDefault();

            string dirsStr = "";
            foreach (var item in c.expansionDirections)
            {
                dirsStr += item;
            }
            //Debug
            Debug.Log("Observed: " + c.gameObject.name + ", entropy: " + c.entropy + ", expansion directions: " + dirsStr);

            c.Observe();
            ObservedCells.Add(c.FinalStateRef);

            // Add the node attached to the observed cells FinalState reference gameobject
            AllObservedNodes.Add(c.FinalStateRef.GetComponent<Node>());

            foreach (var item in cells)
            {
                item.CheckConnectivity();
            }

            foreach (var item in cells)
            {
                item.UpdateStates();
            }

            foreach (var item in cells.Where(x => x.Observed))
            {
                item.UpdateLinks();
            }

            UpdateTrees(); // Update the trees that exist within the subgraph

            CycleCheck();

            IsolatedCheck();

            if (cells.Where(x => x.Observed == false).Count() == 0)
            {
                ObservingToDo = false;
            }
        }

        // performance
        GameObject.Find("PerformanceTrackerMazeGen").GetComponent<PerformanceTracker>().EndTracking();

        // Create the subgraph gameobject, add the graph component and add the nodes and the links
        var subg = Instantiate(Empty);
        subg.name = "Subgraph";
        subg.AddComponent<Graph>();
        subg.GetComponent<Graph>().nodes = AllObservedNodes;
        foreach (var item in subg.GetComponent<Graph>().nodes)
        {
            foreach (var l in item.links)
            {
                subg.GetComponent<Graph>().links.Add(l);
            }
        }
        Subgraph = subg;
    }

    // Get all of the trees
    void UpdateTrees()
    {
        foreach (var node in AllObservedNodes)
        {
            node.explored = false;

            foreach (var link in node.links)
            {
                link.explored = false;
            }
        }

        dfs.nodes = AllObservedNodes;
        trees = dfs.GetTrees();
    }

    void CycleCheck()
    {
        // Get a new random cell to start the cycle check from
        WFC_Cell randomCell = null;

        mergeGraph.nodes.Clear();

        // Continue until every observed cell has been processed
        while (mergeGraph.nodes.Count < ObservedCells.Count)
        {
            if (ObservedCells.Where(x => !mergeGraph.nodes.Contains(x.GetComponent<Node>())).FirstOrDefault() != null)
            {
                randomCell = ObservedCells.Where(x => !mergeGraph.nodes.Contains(x.GetComponent<Node>())).FirstOrDefault().GetComponent<WFC_Cell>();
            }

            if (randomCell != null)
            {
                Node rootNode = randomCell.GetComponent<Node>();

                // Get the graph that the rootNode belongs to
                List<Node> tree = dfs.trees.Where(x => x.Where(y => y == rootNode).FirstOrDefault() != null).FirstOrDefault();

                if (tree != null)
                {
                    mergeGraph.nodes.AddRange(tree);
                    mergeGraph.nodes = mergeGraph.nodes.Distinct().ToList();

                    // unexplore the links and nodes
                    foreach (var n in mergeGraph.nodes)
                    {
                        n.explored = false;

                        foreach (var l in n.links)
                        {
                            l.explored = false;

                            foreach (var item2 in l.nodes)
                            {
                                item2.explored = false;
                            }
                        }
                    }

                    foreach (var item in tree)
                    {
                        item.explored = false;

                        foreach (var item2 in item.links)
                        {
                            item2.explored = false;
                        }
                    }

                    // The neighbouring cells that meet the conditions for expanding towards each other (key= neighbour, value=cell(neighbour of neighbour))
                    Dictionary<WFC_Cell, WFC_Cell> qualifyingNeighbours = new Dictionary<WFC_Cell, WFC_Cell>();

                    // loop through the observed cells which are included in the tree
                    foreach (var cell in ObservedCells)
                    {
                        // loop through the links of the observed cell which include a node that is part of the tree, this is to guarantee that the cell is part of the tree
                        foreach (var link in cell.GetComponent<WFC_Cell>().GetComponent<Node>().links.Where(x => tree.Contains(x.nodes[0]) || tree.Contains(x.nodes[1])))
                        {
                            // each unobserved neighbour of the cell
                            foreach (var neighbour in cell.GetComponent<WFC_Cell>().neighbours.Where(x => x.GetComponent<WFC_Cell>().Observed == false))
                            {
                                // Add the unobserved neighbours that are capable of expanding towards the cell
                                if (neighbour.gameObject.transform.position.x < cell.gameObject.transform.position.x)
                                {
                                    if (neighbour.gameObject.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Right) && cell.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Left))
                                    {
                                        if (!qualifyingNeighbours.ContainsKey(neighbour.GetComponent<WFC_Cell>()))
                                        {
                                            qualifyingNeighbours.Add(neighbour.GetComponent<WFC_Cell>(), cell.GetComponent<WFC_Cell>());
                                        }
                                    }
                                }
                                if (neighbour.gameObject.transform.position.x > cell.gameObject.transform.position.x)
                                {
                                    if (neighbour.gameObject.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Left) && cell.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Right))
                                    {
                                        if (!qualifyingNeighbours.ContainsKey(neighbour.GetComponent<WFC_Cell>()))
                                        {
                                            qualifyingNeighbours.Add(neighbour.GetComponent<WFC_Cell>(), cell.GetComponent<WFC_Cell>());
                                        }
                                    }
                                }
                                if (neighbour.gameObject.transform.position.z < cell.gameObject.transform.position.z)
                                {
                                    if (neighbour.gameObject.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Up) && cell.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Down))
                                    {
                                        if (!qualifyingNeighbours.ContainsKey(neighbour.GetComponent<WFC_Cell>()))
                                        {
                                            qualifyingNeighbours.Add(neighbour.GetComponent<WFC_Cell>(), cell.GetComponent<WFC_Cell>());
                                        }
                                    }
                                }
                                if (neighbour.gameObject.transform.position.z > cell.gameObject.transform.position.z)
                                {
                                    if (neighbour.gameObject.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Down) && cell.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Up))
                                    {
                                        if (!qualifyingNeighbours.ContainsKey(neighbour.GetComponent<WFC_Cell>()))
                                        {
                                            qualifyingNeighbours.Add(neighbour.GetComponent<WFC_Cell>(), cell.GetComponent<WFC_Cell>());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Simulate adding final state for each cell that is neighbouring the tree
                    foreach (var item in qualifyingNeighbours)
                    {
                        CheckCycle(item.Key);
                    }
                }
            }
        }
    }

    // simple way to check for cycles
    void CheckCycle(WFC_Cell cell)
    {
        GameObject simulatedState = Instantiate(Empty, cell.transform.position, Quaternion.identity);

        Dictionary<Node, WFC_Cell.ExpansionDirection> linkedNodes = new Dictionary<Node, WFC_Cell.ExpansionDirection>(); // The nodes to which the simulated node is linked to, as well as the direction of the linked node relative to the simulated node

        Node simNode = simulatedState.AddComponent<Node>(); // reference
        simNode.simulated = true;
        simNode.links = new List<Link>();
        simNode.name = cell.GetComponent<Node>().name + "fs(sim)";

        // simulate adding links to the neighbours
        foreach (var n in cell.neighbours.Where(x => x.GetComponent<WFC_Cell>().Observed == true))
        {
            if (n.transform.position.x < cell.transform.position.x && cell.expansionDirections.Contains(WFC_Cell.ExpansionDirection.Left) && n.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Right))
            {
                // Create links to the neighbouring cells
                Link l = new Link();
                l.simulated = true; // Mark the link as a simulated link
                l.nodes.Add(simulatedState.GetComponent<Node>());
                l.nodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());

                simNode.links.Add(l);

                linkedNodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>(), WFC_Cell.ExpansionDirection.Left);
            }
            if (n.transform.position.x > cell.transform.position.x && cell.expansionDirections.Contains(WFC_Cell.ExpansionDirection.Right) && n.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Left))
            {
                Link l = new Link();
                l.simulated = true; // Mark the link as a simulated link
                l.nodes.Add(simulatedState.GetComponent<Node>());
                l.nodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());

                simNode.links.Add(l);

                linkedNodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>(), WFC_Cell.ExpansionDirection.Right);
            }
            if (n.transform.position.z > cell.transform.position.z && cell.expansionDirections.Contains(WFC_Cell.ExpansionDirection.Up) && n.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Down))
            {
                Link l = new Link();
                l.simulated = true; // Mark the link as a simulated link
                l.nodes.Add(simulatedState.GetComponent<Node>());
                l.nodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());

                simNode.links.Add(l);

                linkedNodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>(), WFC_Cell.ExpansionDirection.Up);
            }
            if (n.transform.position.z < cell.transform.position.z && cell.expansionDirections.Contains(WFC_Cell.ExpansionDirection.Down) && n.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Up))
            {
                Link l = new Link();
                l.simulated = true; // Mark the link as a simulated link
                l.nodes.Add(simulatedState.GetComponent<Node>());
                l.nodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());

                simNode.links.Add(l);

                linkedNodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>(), WFC_Cell.ExpansionDirection.Down);
            }
        }

        // Key = Tree, Value = amount of links to tree from simulated node
        Dictionary<List<Node>, int> linksToTree = new Dictionary<List<Node>, int>();

        foreach (var link in simNode.links)
        {
            Node linkedTo = null;

            if (link.nodes[0] != simNode) { linkedTo = link.nodes[0]; }
            if (link.nodes[1] != simNode) { linkedTo = link.nodes[1]; }

            List<Node> t = trees.Where(x => x.Where(x => x == linkedTo).FirstOrDefault() != null).FirstOrDefault();

            if (t.Contains(linkedTo))
            {
                if (!linksToTree.Keys.Contains(t)) { linksToTree.Add(t, 1); }
                else { linksToTree[t]++; } // increment int signifying the amount of links to the graph
            }
        }

        Debug.Log("Cycle check for " + simNode);

        while (linksToTree.ContainsValue(2) || linksToTree.ContainsValue(3) || linksToTree.ContainsValue(4))
        {
            // Loop through each tree that has multiple links to the simulated node
            foreach (var g in linksToTree.Where(x => x.Value > 1).ToList())
            {
                // loop through the nodes linked to the simulated node
                foreach (var n in linkedNodes)
                {
                    // if the node is part of a graph with more than 1 links to the simulated node, remove the possiblity of a link between the node and the simulated node
                    if (g.Key.Contains(n.Key))
                    {
                        // leave at most 1 link from simulated node to the same graph
                        if (linksToTree[g.Key] > 1)
                        {
                            cell.expansionDirections.Remove(n.Value);
                            cell.blockExpansionDirections.Add(n.Value);
                            linksToTree[g.Key]--;
                        }
                    }
                }
            }
        }

        Destroy(simulatedState);
    }

    void IsolatedCheck()
    {
        mergeGraph.nodes.Clear();

        // Go through each node of the graph
        while (mergeGraph.nodes.Count < ObservedCells.Count)
        {
            // Get a new random cell from the observed cells which is not part of the merge graph
            WFC_Cell randomCell = null;

            if (ObservedCells.Where(x => !mergeGraph.nodes.Contains(x.GetComponent<Node>())).FirstOrDefault() != null)
            {
                randomCell = ObservedCells.Where(x => !mergeGraph.nodes.Contains(x.GetComponent<Node>())).FirstOrDefault().GetComponent<WFC_Cell>();
            }

            if (randomCell != null)
            {
                List<Node> t = new List<Node>();
                Node rootNode = randomCell.GetComponent<Node>();

                // Get the graph based on the rootNode
                t = dfs.trees.Where(x => x.Contains(rootNode)).FirstOrDefault();

                mergeGraph.nodes.AddRange(t);
                mergeGraph.nodes = mergeGraph.nodes.Distinct().ToList();

                // unexplore the links and nodes
                foreach (var n in mergeGraph.nodes)
                {
                    foreach (var l in n.links)
                    {
                        l.explored = false;
                    }
                }

                // The neighbouring cells that meet the conditions for expanding towards each other (key= neighbour, value=cell(neighbour of neighbour))
                Dictionary<WFC_Cell, WFC_Cell> qualifyingNeighbours = new Dictionary<WFC_Cell, WFC_Cell>();

                // loop through every neighbour, of every node in the graph
                foreach (var item in t)
                {
                    var cell = item.gameObject;

                    // loop through each unobserved neighbour
                    foreach (var neighbour in item.GetComponent<WFC_Cell>().neighbours.Where(x => x.GetComponent<WFC_Cell>().Observed == false))
                    {
                        // Add the unobserved neighbours that are capable of expanding towards the cell
                        if (neighbour.gameObject.transform.position.x < cell.gameObject.transform.position.x)
                        {
                            if (neighbour.gameObject.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Right) && cell.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Left))
                            {
                                if (!qualifyingNeighbours.ContainsKey(neighbour.GetComponent<WFC_Cell>()))
                                {
                                    qualifyingNeighbours.Add(neighbour.GetComponent<WFC_Cell>(), cell.GetComponent<WFC_Cell>());
                                }
                            }
                        }
                        if (neighbour.gameObject.transform.position.x > cell.gameObject.transform.position.x)
                        {
                            if (neighbour.gameObject.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Left) && cell.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Right))
                            {
                                if (!qualifyingNeighbours.ContainsKey(neighbour.GetComponent<WFC_Cell>()))
                                {
                                    qualifyingNeighbours.Add(neighbour.GetComponent<WFC_Cell>(), cell.GetComponent<WFC_Cell>());
                                }
                            }
                        }
                        if (neighbour.gameObject.transform.position.z < cell.gameObject.transform.position.z)
                        {
                            if (neighbour.gameObject.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Up) && cell.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Down))
                            {
                                if (!qualifyingNeighbours.ContainsKey(neighbour.GetComponent<WFC_Cell>()))
                                {
                                    qualifyingNeighbours.Add(neighbour.GetComponent<WFC_Cell>(), cell.GetComponent<WFC_Cell>());
                                }
                            }
                        }
                        if (neighbour.gameObject.transform.position.z > cell.gameObject.transform.position.z)
                        {
                            if (neighbour.gameObject.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Down) && cell.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Up))
                            {
                                if (!qualifyingNeighbours.ContainsKey(neighbour.GetComponent<WFC_Cell>()))
                                {
                                    qualifyingNeighbours.Add(neighbour.GetComponent<WFC_Cell>(), cell.GetComponent<WFC_Cell>());
                                }
                            }
                        }
                    }
                }

                // graph has only one option for expansion
                if (qualifyingNeighbours.Count == 1)
                {
                    WFC_Cell thisCell = qualifyingNeighbours.FirstOrDefault().Key;

                    // Always update mustChoiceDirections (neighbouring unobserved expandable cells of thisCell)
                    thisCell.mustChoiceExpansionDirections.Clear();

                    // Like in cycle check, create a simulatedState that holds the node
                    GameObject simulatedState = Instantiate(Empty, thisCell.transform.position, Quaternion.identity);
                    Node simNode = simulatedState.AddComponent<Node>(); // reference
                    simNode.simulated = true;
                    simNode.links = new List<Link>();
                    simNode.name = thisCell.GetComponent<Node>().name + "fs(sim_iso)";

                    // also keep track of the unobserved neighbours of the simNode
                    List<WFC_Cell> unobservedNeighbours = new List<WFC_Cell>();

                    // Create links for the neighbour, to keep track of all the possible nodes to expand towards
                    // Links from the neighbour to each of its possible expansion direction
                    // For neighbouring trees with links from cell, force the mustExpansionDirection
                    // Also add one random mustChoiceExpansionDirection, that is towards an unobserved cell
                    // Create links to each neighbour of this neighbour when expansion directions allow it
                    foreach (var n in thisCell.neighbours)
                    {
                        // check if the neighbour is unobserved and add it to the list if it is
                        if (n.GetComponent<WFC_Cell>().FinalStateRef == null) //unobserved
                        {
                            unobservedNeighbours.Add(n.GetComponent<WFC_Cell>());
                        }
                        else // the observed neighbours
                        {
                            if (n.transform.position.x < thisCell.transform.position.x && n.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Right) && thisCell.expansionDirections.Contains(WFC_Cell.ExpansionDirection.Left))
                            {
                                Link link = new Link();
                                link.nodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());
                                link.nodes.Add(simNode);
                                simNode.links.Add(link);

                                // add the expansion direction immediately
                                thisCell.mustExpansionDirections.Add(WFC_Cell.ExpansionDirection.Left);
                            }
                            if (n.transform.position.x > thisCell.transform.position.x && n.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Left) && thisCell.expansionDirections.Contains(WFC_Cell.ExpansionDirection.Right))
                            {
                                Link link = new Link();
                                link.nodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());
                                link.nodes.Add(simNode);
                                simNode.links.Add(link);

                                thisCell.mustExpansionDirections.Add(WFC_Cell.ExpansionDirection.Right);
                            }
                            if (n.transform.position.z < thisCell.transform.position.z && n.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Up) && thisCell.expansionDirections.Contains(WFC_Cell.ExpansionDirection.Down))
                            {
                                Link link = new Link();
                                link.nodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());
                                link.nodes.Add(simNode);
                                simNode.links.Add(link);

                                thisCell.mustExpansionDirections.Add(WFC_Cell.ExpansionDirection.Down);
                            }
                            if (n.transform.position.z > thisCell.transform.position.z && n.GetComponent<WFC_Cell>().expansionDirections.Contains(WFC_Cell.ExpansionDirection.Down) && thisCell.expansionDirections.Contains(WFC_Cell.ExpansionDirection.Up))
                            {
                                Link link = new Link();
                                link.nodes.Add(n.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());
                                link.nodes.Add(simNode);
                                simNode.links.Add(link);

                                thisCell.mustExpansionDirections.Add(WFC_Cell.ExpansionDirection.Up);
                            }
                        }
                    }

                    // the connections to unobserved cells
                    if (unobservedNeighbours.Count > 0)
                    {
                        if (thisCell.mustChoiceExpansionDirections.Count == 0)
                        {
                            List<WFC_Cell.ExpansionDirection> dirs = new List<WFC_Cell.ExpansionDirection>();

                            // add all the possible directions
                            foreach (var u in unobservedNeighbours)
                            {
                                if (thisCell.transform.position.x < u.transform.position.x) { dirs.Add(WFC_Cell.ExpansionDirection.Right); }
                                if (thisCell.transform.position.x > u.transform.position.x) { dirs.Add(WFC_Cell.ExpansionDirection.Left); }
                                if (thisCell.transform.position.z < u.transform.position.z) { dirs.Add(WFC_Cell.ExpansionDirection.Up); }
                                if (thisCell.transform.position.z > u.transform.position.z) { dirs.Add(WFC_Cell.ExpansionDirection.Down); }
                            }
                            thisCell.mustChoiceExpansionDirections.AddRange(dirs); // add the directions as possibilities
                        }
                    }

                    Destroy(simulatedState);
                }
            }
        }
    }
}