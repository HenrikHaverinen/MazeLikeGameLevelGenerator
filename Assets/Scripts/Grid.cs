using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Grid : MonoBehaviour
{
    [SerializeField] int width, height;
    public int offsetAmount;
    [SerializeField] GameObject cell;
    [SerializeField] GameObject graph;
    [SerializeField] GameObject search;
    Vector3 offsetVector;

    public List<GameObject> wfc_states = new List<GameObject>();

    public enum WallGenerationMethod
    {
        Prim,
        Kruskal,
        WFC
    }

    public WallGenerationMethod method;

    void Start()
    {
        if (method == WallGenerationMethod.Prim) { Prim(); }
        else if (method == WallGenerationMethod.Kruskal) { Kruskal(); }
        else if (method == WallGenerationMethod.WFC) { WFC(); }
    }

    void Prim()
    {
        int j = 1; // Cell number

        for (int x = 0; x < height; x++)
        {
            offsetVector.x = 0; // reset the horizontal offset when entering a new row

            for (int i = 0; i < width; i++)
            {
                GameObject newCell = Instantiate(cell, offsetVector, Quaternion.identity, this.gameObject.transform); // Instantiate new cell in the grid
                Node newNode = newCell.AddComponent<Node>();

                // Determine whether node is on the edge of the grid or not
                if (i == 0) // first cell of row, always an edge node
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Left;
                }
                if (i == width - 1) // last cell of row, always an edge node
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Right;
                }
                if (x == 0) // first row, all nodes are edge nodes
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Up;

                    if (i == 0) // first row, first column UpLeft
                    {
                        newNode.location = Node.EdgeLocation.UpLeft;
                    }
                    if (i == width - 1) // first row, last column UpLeft
                    {
                        newNode.location = Node.EdgeLocation.UpRight;
                    }
                }
                if (x == height - 1) // last row, all nodes are edge nodes
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Down;

                    if (i == 0) // last row, first column DownLeft
                    {
                        newNode.location = Node.EdgeLocation.DownLeft;
                    }
                    if (i == width - 1) // last row, last column UpLeft
                    {
                        newNode.location = Node.EdgeLocation.DownRight;
                    }
                }

                newCell.GetComponent<NeighbourCheck>().grid = this; // set the grid value
                newCell.name = "Cell" + j;
                newNode.name = "Node" + j;

                offsetVector.x += offsetAmount; // Increase the horizontal offset by 1
                j++;
            }
            offsetVector.z -= offsetAmount; // Enter the next row
        }
        // Create graph
        graph = Instantiate(graph, gameObject.transform.position, Quaternion.identity, this.gameObject.transform);
        // Invoke the NeighbourCheck method for each cell in the grid
        foreach (Transform c in this.gameObject.transform)
        {
            if (c.GetComponent<NeighbourCheck>() != null)
            {
                c.gameObject.GetComponent<NeighbourCheck>().graph = graph;
                c.gameObject.GetComponent<NeighbourCheck>().CommenceCheck();
            }
        }

        // Randomize the weights for a more random maze
        graph.GetComponent<Graph>().RandomWeights();

        // Set the links for each node of the graph
        graph.GetComponent<Graph>().SetNodeLinks();

        // Prim algorithm maze wall generation
        //Set the graph for the search
        search.GetComponent<Prim>().Graph = graph;
        // Invoke subgraph formation algorithm
        search.GetComponent<Prim>().ExecuteAlgorithm();

        //Pick a random start and end position, set the graph as the subgraphed that formed
        search.GetComponent<PickStartEnd>().graph = search.GetComponent<Prim>().SubGraph;
        search.GetComponent<PickStartEnd>().PickStartEndNodes();
        search.GetComponent<Prim>().SubGraph.GetComponent<Graph>().RemoveStartAndEndWalls(offsetAmount);
    }

    void Kruskal()
    {
        int j = 1; // Cell number

        for (int x = 0; x < height; x++)
        {
            offsetVector.x = 0; // reset the horizontal offset when entering a new row

            for (int i = 0; i < width; i++)
            {
                GameObject newCell = Instantiate(cell, offsetVector, Quaternion.identity, this.gameObject.transform); // Instantiate new cell in the grid
                Node newNode = newCell.AddComponent<Node>();

                // Determine whether node is on the edge of the grid or not
                if (i == 0) // first cell of row, always an edge node
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Left;
                }
                if (i == width - 1) // last cell of row, always an edge node
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Right;
                }
                if (x == 0) // first row, all nodes are edge nodes
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Up;

                    if (i == 0) // first row, first column UpLeft
                    {
                        newNode.location = Node.EdgeLocation.UpLeft;
                    }
                    if (i == width - 1) // first row, last column UpLeft
                    {
                        newNode.location = Node.EdgeLocation.UpRight;
                    }
                }
                if (x == height - 1) // last row, all nodes are edge nodes
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Down;

                    if (i == 0) // last row, first column DownLeft
                    {
                        newNode.location = Node.EdgeLocation.DownLeft;
                    }
                    if (i == width - 1) // last row, last column UpLeft
                    {
                        newNode.location = Node.EdgeLocation.DownRight;
                    }
                }

                newCell.GetComponent<NeighbourCheck>().grid = this; // set the grid value
                newCell.name = "Cell" + j;
                newNode.name = "Node" + j;

                offsetVector.x += offsetAmount; // Increase the horizontal offset by 1
                j++;
            }
            offsetVector.z -= offsetAmount; // Enter the next row
        }
        // Create graph
        graph = Instantiate(graph, gameObject.transform.position, Quaternion.identity, this.gameObject.transform);
        // Invoke the NeighbourCheck method for each cell in the grid
        foreach (Transform c in this.gameObject.transform)
        {
            if (c.GetComponent<NeighbourCheck>() != null)
            {
                c.gameObject.GetComponent<NeighbourCheck>().graph = graph;
                c.gameObject.GetComponent<NeighbourCheck>().CommenceCheck();
            }
        }

        // Randomize the weights for a more random maze
        graph.GetComponent<Graph>().RandomWeights();

        // Set the links for each node of the graph
        graph.GetComponent<Graph>().SetNodeLinks();

        // Kruskal algorithm maze wall generation
        search.GetComponent<Kruskal>().Graph = graph;
        search.GetComponent<Kruskal>().ExecuteAlgorithm();

        // Pick a random start and end position, set the graph as the subgraphed that formed
        search.GetComponent<PickStartEnd>().graph = search.GetComponent<Kruskal>().SubGraph;
        search.GetComponent<PickStartEnd>().PickStartEndNodes();
        search.GetComponent<Kruskal>().SubGraph.GetComponent<Graph>().RemoveStartAndEndWalls(offsetAmount);
    }

    void WFC()
    {
        int j = 1; // Cell number

        for (int x = 0; x < height; x++)
        {
            offsetVector.x = 0; // reset the horizontal offset when entering a new row

            for (int i = 0; i < width; i++)
            {
                GameObject newCell = Instantiate(cell, offsetVector, Quaternion.identity, this.gameObject.transform); // Instantiate new cell in the grid
                Node newNode = newCell.AddComponent<Node>();

                // Determine whether node is on the edge of the grid or not
                if (i == 0) // first cell of row, always an edge node
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Left;
                }
                if (i == width - 1) // last cell of row, always an edge node
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Right;
                }
                if (x == 0) // first row, all nodes are edge nodes
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Up;

                    if (i == 0) // first row, first column UpLeft
                    {
                        newNode.location = Node.EdgeLocation.UpLeft;
                    }
                    if (i == width - 1) // first row, last column UpRight
                    {
                        newNode.location = Node.EdgeLocation.UpRight;
                    }
                }
                if (x == height - 1) // last row, all nodes are edge nodes
                {
                    newNode.Edge = true;
                    newNode.location = Node.EdgeLocation.Down;

                    if (i == 0) // last row, first column DownLeft
                    {
                        newNode.location = Node.EdgeLocation.DownLeft;
                    }
                    if (i == width - 1) // last row, last column DownRight
                    {
                        newNode.location = Node.EdgeLocation.DownRight;
                    }
                }

                newCell.GetComponent<NeighbourCheck>().grid = this; // set the grid value
                newCell.name = "Cell" + j;
                newNode.name = "Node" + j;

                newCell.GetComponent<WFC_Cell>().states.AddRange(wfc_states); // Set all the possible states
                newCell.GetComponent<WFC_Cell>().grid = this;

                offsetVector.x += offsetAmount; // Increase the horizontal offset by 1
                j++;
            }
            offsetVector.z -= offsetAmount; // Enter the next row
        }

        // Create graph
        graph = Instantiate(graph, gameObject.transform.position, Quaternion.identity, this.gameObject.transform);
        // Invoke the NeighbourCheck method for each cell in the grid
        foreach (Transform c in this.gameObject.transform)
        {
            if (c.GetComponent<NeighbourCheck>() != null)
            {
                c.gameObject.GetComponent<NeighbourCheck>().graph = graph;
                c.gameObject.GetComponent<NeighbourCheck>().CommenceCheck();
            }
            // Set the initial states
            if (c.GetComponent<WFC_Cell>() != null)
            {
                c.gameObject.GetComponent<WFC_Cell>().SetInitialStates();
                c.gameObject.GetComponent<Node>().links = graph.GetComponent<Graph>().links.Where(x => x.nodes.Contains(c.GetComponent<Node>())).ToList();
                foreach (var link in c.gameObject.GetComponent<Node>().links)
                {
                    if (link.nodes[0] != c.gameObject.GetComponent<Node>())
                    {
                        c.gameObject.GetComponent<WFC_Cell>().neighbours.Add(link.nodes[0].gameObject);
                    }
                    if (link.nodes[1] != c.gameObject.GetComponent<Node>())
                    {
                        c.gameObject.GetComponent<WFC_Cell>().neighbours.Add(link.nodes[1].gameObject);
                    }
                }
            }
        }

        search.GetComponent<WFC>().ExecuteAlgorithm();

        // Pick the start and the end nodes
        search.GetComponent<PickStartEnd>().graph = search.GetComponent<WFC>().Subgraph;
        search.GetComponent<PickStartEnd>().PickStartEndNodes();
        search.GetComponent<WFC>().Subgraph.GetComponent<Graph>().RemoveStartAndEndWalls(offsetAmount);
    }
}
