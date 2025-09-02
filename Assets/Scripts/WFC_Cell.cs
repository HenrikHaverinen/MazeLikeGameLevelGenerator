using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WFC_Cell : MonoBehaviour
{
    public int entropy
    {
        get { return states.Count; }
        private set { entropy = value; }
    }
    public List<GameObject> states = new List<GameObject>();
    public GameObject FinalState = null;
    public GameObject FinalStateRef;
    public bool Observed = false;

    public List<GameObject> neighbours = new List<GameObject>();

    public Grid grid;

    public List<Link> links = new List<Link>(); // links that are created between nodes of the subgraph that forms

    public enum ExpansionDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public List<ExpansionDirection> expansionDirections = new List<ExpansionDirection>(); // The directions of which at least one of the cells neighbours must connect to this cell from.

    public List<ExpansionDirection> mustExpansionDirections = new List<ExpansionDirection>(); // The direction towards which this cell must expand towards

    public List<ExpansionDirection> mustChoiceExpansionDirections = new List<ExpansionDirection>(); // Directions from which a direction must be decided to expand into, for preventing isolated graphs from forming

    public List<ExpansionDirection> blockExpansionDirections = new List<ExpansionDirection>(); // The directions towards which this cell must have a wall (for preventing cycles)

    public void SetInitialStates()
    {
        // Remove possible states based on the location on the grid
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.UpLeft)
        {
            states = states.Where(x => x.name.Contains("Up") && x.name.Contains("Left")).ToList();

            expansionDirections.Add(ExpansionDirection.Down);
            expansionDirections.Add(ExpansionDirection.Right);
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.UpRight)
        {
            states = states.Where(x => x.name.Contains("Up") && x.name.Contains("Right")).ToList();

            expansionDirections.Add(ExpansionDirection.Down);
            expansionDirections.Add(ExpansionDirection.Left);
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.DownLeft)
        {
            states = states.Where(x => x.name.Contains("Down") && x.name.Contains("Left")).ToList();

            expansionDirections.Add(ExpansionDirection.Up);
            expansionDirections.Add(ExpansionDirection.Right);
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.DownRight)
        {
            states = states.Where(x => x.name.Contains("Down") && x.name.Contains("Right")).ToList();

            expansionDirections.Add(ExpansionDirection.Up);
            expansionDirections.Add(ExpansionDirection.Left);
        }

        // Regular edges // Must have walls on the edge always
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Left)
        {
            states = states.Where(x => x.name.Contains("Left")).ToList();

            expansionDirections.Add(ExpansionDirection.Up);
            expansionDirections.Add(ExpansionDirection.Down);
            expansionDirections.Add(ExpansionDirection.Right);
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Right)
        {
            states = states.Where(x => x.name.Contains("Right")).ToList();

            expansionDirections.Add(ExpansionDirection.Up);
            expansionDirections.Add(ExpansionDirection.Down);
            expansionDirections.Add(ExpansionDirection.Left);
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Up)
        {
            states = states.Where(x => x.name.Contains("Up")).ToList();

            expansionDirections.Add(ExpansionDirection.Down);
            expansionDirections.Add(ExpansionDirection.Left);
            expansionDirections.Add(ExpansionDirection.Right);
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Down)
        {
            states = states.Where(x => x.name.Contains("Down")).ToList();

            expansionDirections.Add(ExpansionDirection.Up);
            expansionDirections.Add(ExpansionDirection.Left);
            expansionDirections.Add(ExpansionDirection.Right);
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.None)
        {
            expansionDirections.Add(ExpansionDirection.Up);
            expansionDirections.Add(ExpansionDirection.Down);
            expansionDirections.Add(ExpansionDirection.Left);
            expansionDirections.Add(ExpansionDirection.Right);
        }

        SetNeighbours();
    }

    void SetNeighbours()
    {
        var node = this.gameObject.GetComponent<Node>();
        foreach (var link in node.links)
        {
            // Set the neighbours
            if (link.nodes[0] != node)
            {
                neighbours.Add(link.nodes[0].gameObject);
            }
            if (link.nodes[1] != node)
            {
                neighbours.Add(link.nodes[1].gameObject);
            }
        }
    }

    public void UpdateStates()
    {
        // If there are directions that the cell must expand towards, only allow states that have an absense of a wall in the directions towards which the cell must expand
        if (mustExpansionDirections.Count > 0)
        {
            // weird glitch with some edge node setting a mustExpandDirection in the direction of the edge of the maze
            if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Up)
            {
                mustExpansionDirections.Remove(ExpansionDirection.Up);
            }
            if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Down)
            {
                mustExpansionDirections.Remove(ExpansionDirection.Down);
            }
            if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Left)
            {
                mustExpansionDirections.Remove(ExpansionDirection.Left);
            }
            if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Right)
            {
                mustExpansionDirections.Remove(ExpansionDirection.Right);
            }

            bool mustUp = mustExpansionDirections.Contains(ExpansionDirection.Up);
            bool mustDown = mustExpansionDirections.Contains(ExpansionDirection.Down);
            bool mustLeft = mustExpansionDirections.Contains(ExpansionDirection.Left);
            bool mustRight = mustExpansionDirections.Contains(ExpansionDirection.Right);

            // Select the states, that do not block the directions of necessary expansion with walls
            // Up allowed
            if (mustUp && !mustDown && !mustLeft && !mustRight)
            {
                states = states.Where(x => !x.name.Contains("Up")).ToList();
            }
            // Down allowed
            if (!mustUp && mustDown && !mustLeft && !mustRight)
            {
                states = states.Where(x => !x.name.Contains("Down")).ToList();
            }
            // Left allowed
            if (!mustUp && !mustDown && mustLeft && !mustRight)
            {
                states = states.Where(x => !x.name.Contains("Left")).ToList();
            }
            // Right allowed
            if (!mustUp && !mustDown && !mustLeft && mustRight)
            {
                states = states.Where(x => !x.name.Contains("Right")).ToList();
            }
            // Combinations 2 allowed
            // UpDown
            if (mustUp && mustDown && !mustLeft && !mustRight)
            {
                states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Down")).ToList();
            }
            // UpLeft
            if (mustUp && !mustDown && mustLeft && !mustRight)
            {
                states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Left")).ToList();
            }
            // UpRight
            if (mustUp && !mustDown && !mustLeft && mustRight)
            {
                states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Right")).ToList();
            }
            // DownLeft
            if (!mustUp && mustDown && mustLeft && !mustRight)
            {
                states = states.Where(x => !x.name.Contains("Down") && !x.name.Contains("Left")).ToList();
            }
            // DownRight
            if (!mustUp && mustDown && !mustLeft && mustRight)
            {
                states = states.Where(x => !x.name.Contains("Down") && !x.name.Contains("Right")).ToList();
            }
            // LeftRight
            if (!mustUp && !mustDown && mustLeft && mustRight)
            {
                states = states.Where(x => !x.name.Contains("Left") && !x.name.Contains("Right")).ToList();
            }
            // Combinations 3 allowed
            if (!mustUp && mustDown && mustLeft && mustRight)
            {
                states = states.Where(x => !x.name.Contains("Down") && !x.name.Contains("Left") && !x.name.Contains("Right")).ToList();
            }
            if (mustUp && !mustDown && mustLeft && mustRight)
            {
                states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Left") && !x.name.Contains("Right")).ToList();
            }
            if (mustUp && mustDown && !mustLeft && mustRight)
            {
                states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Down") && !x.name.Contains("Right")).ToList();
            }
            if (mustUp && mustDown && mustLeft && !mustRight)
            {
                states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Down") && !x.name.Contains("Left")).ToList();
            }
            // All 4 directions allowed
            if (mustUp && mustDown && mustLeft && mustRight)
            {
                states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Down") && !x.name.Contains("Left") && !x.name.Contains("Right")).ToList();
            }
        }
        else
        {
            // Only include the states which do not include walls in the direction of the expansion direction
            // Check if the expansionDirections list contains Up, down, left, right
            // If the expansionDirections contains just one direction, make sure to include only the states, which do not include that direction in the name of the wall gameobject
            // If there are 2 expansion states, there has to be 2 checks, so only include the states, which do not include either of the directions in the name of the wall gameobject
            // If there are 3 expansion states, there has to be 3 checks, only include the states, which do not include any of the 3 directions in the name of the wall gameobject

            if (this.gameObject.GetComponent<Node>().Edge)
            {
                // dont apply the updated states to edges
            }
            else
            {
                bool allowUp = expansionDirections.Contains(ExpansionDirection.Up);
                bool allowDown = expansionDirections.Contains(ExpansionDirection.Down);
                bool allowLeft = expansionDirections.Contains(ExpansionDirection.Left);
                bool allowRight = expansionDirections.Contains(ExpansionDirection.Right);

                // Select the states, that do not block the directions of potential expansion with walls
                // Up allowed
                if (allowUp && !allowDown && !allowLeft && !allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Up")).ToList();
                }
                // Down allowed
                if (!allowUp && allowDown && !allowLeft && !allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Down")).ToList();
                }
                // Left allowed
                if (!allowUp && !allowDown && allowLeft && !allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Left")).ToList();
                }
                // Right allowed
                if (!allowUp && !allowDown && !allowLeft && allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Right")).ToList();
                }
                // Combinations 2 allowed
                // UpDown
                if (allowUp && allowDown && !allowLeft && !allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Down")).ToList();
                }
                // UpLeft
                if (allowUp && !allowDown && allowLeft && !allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Left")).ToList();
                }
                // UpRight
                if (allowUp && !allowDown && !allowLeft && allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Right")).ToList();
                }
                // DownLeft
                if (!allowUp && allowDown && allowLeft && !allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Down") && !x.name.Contains("Left")).ToList();
                }
                // DownRight
                if (!allowUp && allowDown && !allowLeft && allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Down") && !x.name.Contains("Right")).ToList();
                }
                // LeftRight
                if (!allowUp && !allowDown && allowLeft && allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Left") && !x.name.Contains("Right")).ToList();
                }
                // Combinations 3 allowed
                if (!allowUp && allowDown && allowLeft && allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Down") && !x.name.Contains("Left") && !x.name.Contains("Right")).ToList();
                }
                if (allowUp && !allowDown && allowLeft && allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Left") && !x.name.Contains("Right")).ToList();
                }
                if (allowUp && allowDown && !allowLeft && allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Down") && !x.name.Contains("Right")).ToList();
                }
                if (allowUp && allowDown && allowLeft && !allowRight)
                {
                    states = states.Where(x => !x.name.Contains("Up") && !x.name.Contains("Down") && !x.name.Contains("Left")).ToList();
                }
                // All 4 directions allowed
                if (allowUp && allowDown && allowLeft && allowRight)
                {
                    // dont change a thing
                }
            }
        }

        // if a direction must include a wall
        if (blockExpansionDirections.Count > 0)
        {
            bool blockUp = blockExpansionDirections.Contains(ExpansionDirection.Up);
            bool blockDown = blockExpansionDirections.Contains(ExpansionDirection.Down);
            bool blockLeft = blockExpansionDirections.Contains(ExpansionDirection.Left);
            bool blockRight = blockExpansionDirections.Contains(ExpansionDirection.Right);

            // Select the states, that do block the directions of potential expansion with walls
            // Up allowed
            if (blockUp && !blockDown && !blockLeft && !blockRight)
            {
                states = states.Where(x => x.name.Contains("Up")).ToList();
            }
            // Down allowed
            if (!blockUp && blockDown && !blockLeft && !blockRight)
            {
                states = states.Where(x => x.name.Contains("Down")).ToList();
            }
            // Left allowed
            if (!blockUp && !blockDown && blockLeft && !blockRight)
            {
                states = states.Where(x => x.name.Contains("Left")).ToList();
            }
            // Right allowed
            if (!blockUp && !blockDown && !blockLeft && blockRight)
            {
                states = states.Where(x => x.name.Contains("Right")).ToList();
            }
            // Combinations 2 blocked
            // UpDown
            if (blockUp && blockDown && !blockLeft && !blockRight)
            {
                states = states.Where(x => x.name.Contains("Up") && x.name.Contains("Down")).ToList();
            }
            // UpLeft
            if (blockUp && !blockDown && blockLeft && !blockRight)
            {
                states = states.Where(x => x.name.Contains("Up") && x.name.Contains("Left")).ToList();
            }
            // UpRight
            if (blockUp && !blockDown && !blockLeft && blockRight)
            {
                states = states.Where(x => x.name.Contains("Up") && x.name.Contains("Right")).ToList();
            }
            // DownLeft
            if (!blockUp && blockDown && blockLeft && !blockRight)
            {
                states = states.Where(x => x.name.Contains("Down") && x.name.Contains("Left")).ToList();
            }
            // DownRight
            if (!blockUp && blockDown && !blockLeft && blockRight)
            {
                states = states.Where(x => x.name.Contains("Down") && x.name.Contains("Right")).ToList();
            }
            // LeftRight
            if (!blockUp && !blockDown && blockLeft && blockRight)
            {
                states = states.Where(x => x.name.Contains("Left") && x.name.Contains("Right")).ToList();
            }
            // Combinations 3 allowed
            if (!blockUp && blockDown && blockLeft && blockRight)
            {
                states = states.Where(x => x.name.Contains("Down") && x.name.Contains("Left") && x.name.Contains("Right")).ToList();
            }
            if (blockUp && !blockDown && blockLeft && blockRight)
            {
                states = states.Where(x => x.name.Contains("Up") && x.name.Contains("Left") && x.name.Contains("Right")).ToList();
            }
            if (blockUp && blockDown && !blockLeft && blockRight)
            {
                states = states.Where(x => x.name.Contains("Up") && x.name.Contains("Down") && x.name.Contains("Right")).ToList();
            }
            if (blockUp && blockDown && blockLeft && !blockRight)
            {
                states = states.Where(x => x.name.Contains("Up") && x.name.Contains("Down") && x.name.Contains("Left")).ToList();
            }
            // All 4 directions blocked // Should never happen, would cause an isolated cell if such state existed
            if (blockUp && blockDown && blockLeft && blockRight)
            {
                // dont change a thing
            }
        }
    }

    GameObject ForceState()
    {
        List<GameObject> from = grid.GetComponent<Grid>().wfc_states;

        // take into account if the node is a edge node
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Left)
        {
            from = from.Where(x => x.name.Contains("Left")).ToList();
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Right)
        {
            from = from.Where(x => x.name.Contains("Right")).ToList();
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Up)
        {
            from = from.Where(x => x.name.Contains("Up")).ToList();
        }
        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.Down)
        {
            from = from.Where(x => x.name.Contains("Down")).ToList();
        }

        // if a direction must include a wall
        if (blockExpansionDirections.Count > 0)
        {
            bool blockUp = blockExpansionDirections.Contains(ExpansionDirection.Up);
            bool blockDown = blockExpansionDirections.Contains(ExpansionDirection.Down);
            bool blockLeft = blockExpansionDirections.Contains(ExpansionDirection.Left);
            bool blockRight = blockExpansionDirections.Contains(ExpansionDirection.Right);

            // Select the states, that do block the directions of potential expansion with walls
            // Up allowed
            if (blockUp && !blockDown && !blockLeft && !blockRight)
            {
                //return grid.GetComponent<Grid>().wfc_states.Where(x => x.name.Contains("Up")).FirstOrDefault();
                return from.Where(x => x.name.Contains("Up")).FirstOrDefault();
            }
            // Down allowed
            if (!blockUp && blockDown && !blockLeft && !blockRight)
            {
                return from.Where(x => x.name.Contains("Down")).FirstOrDefault();
            }
            // Left allowed
            if (!blockUp && !blockDown && blockLeft && !blockRight)
            {
                return from.Where(x => x.name.Contains("Left")).FirstOrDefault();
            }
            // Right allowed
            if (!blockUp && !blockDown && !blockLeft && blockRight)
            {
                return from.Where(x => x.name.Contains("Right")).FirstOrDefault();
            }
            // Combinations 2 blocked
            // UpDown
            if (blockUp && blockDown && !blockLeft && !blockRight)
            {
                return from.Where(x => x.name.Contains("Up") && x.name.Contains("Down")).FirstOrDefault();
            }
            // UpLeft
            if (blockUp && !blockDown && blockLeft && !blockRight)
            {
                return from.Where(x => x.name.Contains("Up") && x.name.Contains("Left")).FirstOrDefault();
            }
            // UpRight
            if (blockUp && !blockDown && !blockLeft && blockRight)
            {
                return from.Where(x => x.name.Contains("Up") && x.name.Contains("Right")).FirstOrDefault();
            }
            // DownLeft
            if (!blockUp && blockDown && blockLeft && !blockRight)
            {
                return from.Where(x => x.name.Contains("Down") && x.name.Contains("Left")).FirstOrDefault();
            }
            // DownRight
            if (!blockUp && blockDown && !blockLeft && blockRight)
            {
                return from.Where(x => x.name.Contains("Down") && x.name.Contains("Right")).FirstOrDefault();
            }
            // LeftRight
            if (!blockUp && !blockDown && blockLeft && blockRight)
            {
                return from.Where(x => x.name.Contains("Left") && x.name.Contains("Right")).FirstOrDefault();
            }
            // Combinations 3 allowed
            if (!blockUp && blockDown && blockLeft && blockRight)
            {
                return from.Where(x => x.name.Contains("Down") && x.name.Contains("Left") && x.name.Contains("Right")).FirstOrDefault();
            }
            if (blockUp && !blockDown && blockLeft && blockRight)
            {
                return from.Where(x => x.name.Contains("Up") && x.name.Contains("Left") && x.name.Contains("Right")).FirstOrDefault();
            }
            if (blockUp && blockDown && !blockLeft && blockRight)
            {
                return from.Where(x => x.name.Contains("Up") && x.name.Contains("Down") && x.name.Contains("Right")).FirstOrDefault();
            }
            if (blockUp && blockDown && blockLeft && !blockRight)
            {
                return from.Where(x => x.name.Contains("Up") && x.name.Contains("Down") && x.name.Contains("Left")).FirstOrDefault();
            }
            // All 4 directions blocked // Should never happen, would result in an isolated area
            if (blockUp && blockDown && blockLeft && blockRight)
            {
            }
        }

        if (this.gameObject.GetComponent<Node>().location == Node.EdgeLocation.None)
        {
            return grid.GetComponent<Grid>().wfc_states.Where(x => x.name.Contains("Empty")).FirstOrDefault();
        }
        else
        {
            return from.FirstOrDefault();
        }
    }

    public void Observe()
    {
        // If the cell has no directions it must expand into, add 1 value from expansionDirections as a must, to guarantee the cell will expand towards a direction that wont be blocked by a wall
        if (mustExpansionDirections.Count == 0 && mustChoiceExpansionDirections.Count == 0)
        {
            mustChoiceExpansionDirections.Add(expansionDirections[Random.Range(0, expansionDirections.Count)]);
        }

        // in case the mustChoiceExpansionDirections has values, update states one last time
        if (mustChoiceExpansionDirections.Count > 0)
        {
            ExpansionDirection dir = mustChoiceExpansionDirections[Random.Range(0, mustChoiceExpansionDirections.Count)]; // pick one random direction from mustChoiceExpansionDirections
            mustExpansionDirections.Add(dir);
            UpdateStates();
        }

        if (states.Count > 0)
        {
            FinalState = states[Random.Range(0, entropy)]; // Pick one of the states as the final state
        }
        if (states.Count == 0)
        {
            // force a state according to the block directions
            FinalState = ForceState();
        }

        if (FinalState != null)
        {
            foreach (var item in FinalState.GetComponents<Node>())
            {
                Destroy(item);
            }

            FinalStateRef = Instantiate(FinalState, this.gameObject.transform.position, Quaternion.identity); // Instantiate the final state
            FinalStateRef.GetComponent<WFC_Cell>().Observed = true;
            FinalStateRef.GetComponent<WFC_Cell>().neighbours = neighbours;
            FinalStateRef.GetComponent<WFC_Cell>().states = states;
            FinalStateRef.GetComponent<WFC_Cell>().expansionDirections = expansionDirections;

            // Add the Node component, and add the relevant information to it
            FinalStateRef.AddComponent<Node>();
            FinalStateRef.GetComponent<Node>().Edge = this.gameObject.GetComponent<Node>().Edge;
            FinalStateRef.GetComponent<Node>().location = this.gameObject.GetComponent<Node>().location;

            FinalStateRef.name = this.gameObject.name + "fs";
        }

        Observed = true;

        // Check connectivity, in case that this cell must be connected to a particular neighbour 
        CheckConnectivity();

        // Check the connectivity of all neighbours also
        foreach (var item in neighbours)
        {
            item.GetComponent<WFC_Cell>().CheckConnectivity();
        }

        UpdateLinks(); // set the links of the cell for the node component
    }

    public void UpdateLinks()
    {
        var node = this.gameObject.GetComponent<Node>(); // Node that contains all the original links

        // Set the links based on neighbours and expansionDirections
        foreach (var item in neighbours.Where(x => x.GetComponent<WFC_Cell>().Observed))
        {
            if (item.transform.position.x < gameObject.transform.position.x)
            {
                if (expansionDirections.Contains(ExpansionDirection.Left) && item.GetComponent<WFC_Cell>().expansionDirections.Contains(ExpansionDirection.Right))  // no wall, create a link
                {
                    // subgraph
                    if (FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).ToList().Count == 0)
                    {
                        Link l = new Link();
                        l.nodes.Add(FinalStateRef.GetComponent<Node>());
                        l.nodes.Add(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());
                        FinalStateRef.GetComponent<Node>().links.Add(l);
                        item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>().links.Add(l);
                    }
                    // update the links regular
                    if (links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).ToList().Count > 0) { }
                    else
                    {
                        links.Add(node.links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).FirstOrDefault());
                    }
                }
                else // Remove
                {
                    if (FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).ToList().Count > 0)
                    {
                        FinalStateRef.GetComponent<Node>().links.Remove(FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).FirstOrDefault());
                        item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>().links.Remove(FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).FirstOrDefault());
                    }
                    // regular
                    if (links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).ToList().Count > 0)
                    {
                        links.Remove(node.links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).FirstOrDefault());
                    }
                }
            }
            if (item.transform.position.x > gameObject.transform.position.x)
            {
                if (expansionDirections.Contains(ExpansionDirection.Right) && item.GetComponent<WFC_Cell>().expansionDirections.Contains(ExpansionDirection.Left))  // no wall, create a link
                {
                    // subgraph
                    if (FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).ToList().Count == 0)
                    {
                        Link l = new Link();
                        l.nodes.Add(FinalStateRef.GetComponent<Node>());
                        l.nodes.Add(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());
                        FinalStateRef.GetComponent<Node>().links.Add(l);
                        item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>().links.Add(l);
                    }
                    if (links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).ToList().Count > 0) { }
                    else
                    {
                        links.Add(node.links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).FirstOrDefault());
                    }
                }
                else // Remove
                {
                    if (FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).ToList().Count > 0)
                    {
                        FinalStateRef.GetComponent<Node>().links.Remove(FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).FirstOrDefault());
                        item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>().links.Remove(FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).FirstOrDefault());
                    }
                    if (links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).ToList().Count > 0)
                    {
                        links.Remove(node.links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).FirstOrDefault());
                    }
                }
            }
            if (item.transform.position.z > gameObject.transform.position.z)
            {
                if (expansionDirections.Contains(ExpansionDirection.Up) && item.GetComponent<WFC_Cell>().expansionDirections.Contains(ExpansionDirection.Down))  // no wall, create a link
                {
                    if (FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).ToList().Count == 0)
                    {
                        Link l = new Link();
                        l.nodes.Add(FinalStateRef.GetComponent<Node>());
                        l.nodes.Add(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());
                        FinalStateRef.GetComponent<Node>().links.Add(l);
                        item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>().links.Add(l);
                    }
                    if (links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).ToList().Count > 0) { }
                    else
                    {
                        links.Add(node.links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).FirstOrDefault());
                    }
                }
                else // Remove
                {
                    if (FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).ToList().Count > 0)
                    {
                        FinalStateRef.GetComponent<Node>().links.Remove(FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).FirstOrDefault());
                        item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>().links.Remove(FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).FirstOrDefault());
                    }
                    if (links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).ToList().Count > 0)
                    {
                        links.Remove(node.links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).FirstOrDefault());
                    }
                }
            }
            if (item.transform.position.z < gameObject.transform.position.z)
            {
                if (expansionDirections.Contains(ExpansionDirection.Down) && item.GetComponent<WFC_Cell>().expansionDirections.Contains(ExpansionDirection.Up))  // no wall, create a link
                {
                    if (FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).ToList().Count == 0)
                    {
                        Link l = new Link();
                        l.nodes.Add(FinalStateRef.GetComponent<Node>());
                        l.nodes.Add(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>());
                        FinalStateRef.GetComponent<Node>().links.Add(l);
                        item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>().links.Add(l);
                    }
                    if (links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).ToList().Count > 0) { }
                    else
                    {
                        links.Add(node.links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).FirstOrDefault());
                    }
                }
                else // Remove
                {
                    if (FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).ToList().Count > 0)
                    {
                        FinalStateRef.GetComponent<Node>().links.Remove(FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).FirstOrDefault());
                        item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>().links.Remove(FinalStateRef.GetComponent<Node>().links.Where(x => x.nodes.Contains(FinalStateRef.GetComponent<Node>()) && x.nodes.Contains(item.GetComponent<WFC_Cell>().FinalStateRef.GetComponent<Node>())).FirstOrDefault());
                    }
                    if (links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).ToList().Count > 0)
                    {
                        links.Remove(node.links.Where(x => x.nodes.Contains(item.GetComponent<Node>())).FirstOrDefault());
                    }
                }
            }
        }
        if (FinalStateRef != null)
        {
            FinalStateRef.GetComponent<WFC_Cell>().links = links;
        }
    }
   
    // Check that the cell has at least 1 direction that it can move into
    public void CheckConnectivity()
    {
        // If this cell is not yet observed, it has no final state -> create a temporary final state
        if (FinalState == null)
        {
            FinalState = new GameObject();
            FinalState.name = "delete";
        }

        foreach (var item in neighbours) // Check connectivity to neighbouring nodes
        {
            if (item.GetComponent<WFC_Cell>().Observed)
            {
                GameObject nFinalState = item.GetComponent<WFC_Cell>().FinalState; // Get the neighbour FinalState

                if (item.gameObject.transform.position.x < this.gameObject.transform.position.x) // Left neighbour
                {
                    if (nFinalState.name.Contains("Right")) // if the left neighbour has a right wall
                    {
                        expansionDirections.Remove(ExpansionDirection.Left); // Then we cant expand to the left
                        item.GetComponent<WFC_Cell>().expansionDirections.Remove(ExpansionDirection.Right);
                    }
                    else // if the left neighbour doesnt have a right wall
                    {
                        if (!FinalState.name.Contains("Left")) // And this cell doesnt have a left wall
                        {
                            expansionDirections.Add(ExpansionDirection.Left); // Then we can expand to the left
                            item.GetComponent<WFC_Cell>().expansionDirections.Add(ExpansionDirection.Right);
                        }
                    }
                }
                if (item.gameObject.transform.position.x > this.gameObject.transform.position.x) // Right neighbour
                {
                    if (nFinalState.name.Contains("Left")) // if the right neighbour has a left wall
                    {
                        expansionDirections.Remove(ExpansionDirection.Right); // Then we cant expand to the right
                        item.GetComponent<WFC_Cell>().expansionDirections.Remove(ExpansionDirection.Left);
                    }
                    else // if the right neighbour doesnt have a left wall
                    {
                        if (!FinalState.name.Contains("Right")) // And this cell doesnt have a right wall
                        {
                            expansionDirections.Add(ExpansionDirection.Right);
                            item.GetComponent<WFC_Cell>().expansionDirections.Add(ExpansionDirection.Left);
                        }
                    }
                }
                if (item.gameObject.transform.position.z > this.gameObject.transform.position.z) // Above neighbour
                {
                    if (nFinalState.name.Contains("Down")) // if the above neighbour has a down wall
                    {
                        expansionDirections.Remove(ExpansionDirection.Up); // Then we cant expand up
                        item.GetComponent<WFC_Cell>().expansionDirections.Remove(ExpansionDirection.Down);
                    }
                    else
                    {
                        if (!FinalState.name.Contains("Up"))
                        {
                            expansionDirections.Add(ExpansionDirection.Up);
                            item.GetComponent<WFC_Cell>().expansionDirections.Add(ExpansionDirection.Down);
                        }
                    }
                }
                if (item.gameObject.transform.position.z < this.gameObject.transform.position.z) // Below neighbour
                {
                    if (nFinalState.name.Contains("Up")) // if the below neighbour has a up wall
                    {
                        expansionDirections.Remove(ExpansionDirection.Down); // Then we cant expand down
                        item.GetComponent<WFC_Cell>().expansionDirections.Remove(ExpansionDirection.Up);
                    }
                    else
                    {
                        if (!FinalState.name.Contains("Down"))
                        {
                            expansionDirections.Add(ExpansionDirection.Down);
                            item.GetComponent<WFC_Cell>().expansionDirections.Add(ExpansionDirection.Up);
                        }
                    }
                }
            }
            else
            {
                // if the neighbour is not observed, and this cell doesnt block the way, this is a possible way to expand
                if (item.gameObject.transform.position.x < this.gameObject.transform.position.x) // Left neighbour
                {
                    if (FinalState.name.Contains("Left")) // if this cell has a left wall
                    {
                        expansionDirections.Remove(ExpansionDirection.Left); // Then we cant expand to the left
                        item.GetComponent<WFC_Cell>().expansionDirections.Remove(ExpansionDirection.Right); // Likewise
                    }
                    else
                    {
                        expansionDirections.Add(ExpansionDirection.Left); // We can
                        item.GetComponent<WFC_Cell>().expansionDirections.Add(ExpansionDirection.Right); // Likewise
                    }
                }
                if (item.gameObject.transform.position.x > this.gameObject.transform.position.x) // Right neighbour
                {
                    if (FinalState.name.Contains("Right")) // if this cell has a right wall
                    {
                        expansionDirections.Remove(ExpansionDirection.Right); // Then we cant expand to the right
                        item.GetComponent<WFC_Cell>().expansionDirections.Remove(ExpansionDirection.Left);
                    }
                    else
                    {
                        expansionDirections.Add(ExpansionDirection.Right); // We can
                        item.GetComponent<WFC_Cell>().expansionDirections.Add(ExpansionDirection.Left); // Likewise
                    }
                }
                if (item.gameObject.transform.position.z > this.gameObject.transform.position.z) // Above neighbour
                {
                    if (FinalState.name.Contains("Up")) // if this cell has a up wall
                    {
                        expansionDirections.Remove(ExpansionDirection.Up); // Then we cant expand up
                        item.GetComponent<WFC_Cell>().expansionDirections.Remove(ExpansionDirection.Down);
                    }
                    else
                    {
                        expansionDirections.Add(ExpansionDirection.Up); // We can
                        item.GetComponent<WFC_Cell>().expansionDirections.Add(ExpansionDirection.Down); // Likewise
                    }
                }
                if (item.gameObject.transform.position.z < this.gameObject.transform.position.z) // Below neighbour
                {
                    if (FinalState.name.Contains("Down")) // if this cell has a down wall
                    {
                        expansionDirections.Remove(ExpansionDirection.Down); // Then we cant expand down
                        item.GetComponent<WFC_Cell>().expansionDirections.Remove(ExpansionDirection.Up);
                    }
                    else
                    {
                        expansionDirections.Add(ExpansionDirection.Down); // We can
                        item.GetComponent<WFC_Cell>().expansionDirections.Add(ExpansionDirection.Up); // Likewise
                    }
                }
            }

            // no duplicates
            item.GetComponent<WFC_Cell>().expansionDirections = item.GetComponent<WFC_Cell>().expansionDirections.Distinct().ToList();
            expansionDirections = expansionDirections.Distinct().ToList();

            // check if this cell is the only option for neighbouring cell to expand to
            if (item.GetComponent<WFC_Cell>().expansionDirections.Count == 1)
            {
                item.GetComponent<WFC_Cell>().mustExpansionDirections.Add(item.GetComponent<WFC_Cell>().expansionDirections[0]); // set it as the only direction
                item.GetComponent<WFC_Cell>().mustExpansionDirections = item.GetComponent<WFC_Cell>().mustExpansionDirections.Distinct().ToList();
                mustExpansionDirections.Add(OppositeDirection(item.GetComponent<WFC_Cell>().expansionDirections[0]));
                mustExpansionDirections = mustExpansionDirections.Distinct().ToList();
            }

            // Set the ref values also for neighbours
            if (item.GetComponent<WFC_Cell>().FinalStateRef != null)
            {
                item.GetComponent<WFC_Cell>().FinalStateRef.gameObject.GetComponent<WFC_Cell>().expansionDirections = item.GetComponent<WFC_Cell>().expansionDirections;
                item.GetComponent<WFC_Cell>().FinalStateRef.gameObject.GetComponent<WFC_Cell>().mustExpansionDirections = item.GetComponent<WFC_Cell>().mustExpansionDirections;
            }
        }

        if (expansionDirections.Count == 1) // If the cell can only expand to one direction
        {
            mustExpansionDirections.Add(expansionDirections[0]); // Get the one direction which this cell must expand towards
            mustExpansionDirections = mustExpansionDirections.Distinct().ToList();
            // Set the must expand direction to the unobserved neighbour also
            foreach (var item in neighbours.Where(x => !x.GetComponent<WFC_Cell>().Observed))
            {
                if (item.transform.position.x < gameObject.transform.position.x && mustExpansionDirections[0] == ExpansionDirection.Left)
                {
                    item.GetComponent<WFC_Cell>().mustExpansionDirections.Add(OppositeDirection(mustExpansionDirections[0]));
                }
                if (item.transform.position.x > gameObject.transform.position.x && mustExpansionDirections[0] == ExpansionDirection.Right)
                {
                    item.GetComponent<WFC_Cell>().mustExpansionDirections.Add(OppositeDirection(mustExpansionDirections[0]));
                }
                if (item.transform.position.z > gameObject.transform.position.z && mustExpansionDirections[0] == ExpansionDirection.Up)
                {
                    item.GetComponent<WFC_Cell>().mustExpansionDirections.Add(OppositeDirection(mustExpansionDirections[0]));
                }
                if (item.transform.position.z < gameObject.transform.position.z && mustExpansionDirections[0] == ExpansionDirection.Down)
                {
                    item.GetComponent<WFC_Cell>().mustExpansionDirections.Add(OppositeDirection(mustExpansionDirections[0]));
                }
                item.GetComponent<WFC_Cell>().mustExpansionDirections = item.GetComponent<WFC_Cell>().mustExpansionDirections.Distinct().ToList();

                // Set the ref values also for neighbours
                if (item.GetComponent<WFC_Cell>().FinalStateRef != null)
                {
                    item.GetComponent<WFC_Cell>().FinalStateRef.gameObject.GetComponent<WFC_Cell>().expansionDirections = item.GetComponent<WFC_Cell>().expansionDirections;
                    item.GetComponent<WFC_Cell>().FinalStateRef.gameObject.GetComponent<WFC_Cell>().mustExpansionDirections = item.GetComponent<WFC_Cell>().mustExpansionDirections;
                }
            }
        }

        // set for ref also
        if (FinalStateRef != null)
        {
            FinalStateRef.gameObject.GetComponent<WFC_Cell>().expansionDirections = expansionDirections;
            FinalStateRef.gameObject.GetComponent<WFC_Cell>().mustExpansionDirections = mustExpansionDirections;
        }

        // Update the states for this cell, and all neighbours
        UpdateStates();
        foreach (var item in neighbours)
        {
            item.GetComponent<WFC_Cell>().UpdateStates();
        }

        // if FinalState was set to be temporary, delete it from the scene
        if (FinalState.name == "delete")
        {
            Destroy(FinalState);
        }
    }

    public ExpansionDirection OppositeDirection(ExpansionDirection dir)
    {
        if (dir == ExpansionDirection.Left) { return ExpansionDirection.Right; }
        else if (dir == ExpansionDirection.Right) { return ExpansionDirection.Left; }
        else if (dir == ExpansionDirection.Up) { return ExpansionDirection.Down; }
        else if (dir == ExpansionDirection.Down) { return ExpansionDirection.Up; }
        else { return dir; }
    }
}
