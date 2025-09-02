using UnityEngine;

public class NeighbourCheck : MonoBehaviour
{
    public Grid grid; // the grid-component connected to the grid that the cell is a part of
    public GameObject graph; // The graph which holds all the links

    public void CommenceCheck()
    {
        // Check each 4 directions for a potential neighbouring cell
        var up = Physics.OverlapCapsule(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z + grid.offsetAmount), new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z + grid.offsetAmount), 0.5f);
        var down = Physics.OverlapCapsule(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z - grid.offsetAmount), new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z - grid.offsetAmount), 0.5f);
        var right = Physics.OverlapCapsule(new Vector3(this.gameObject.transform.position.x + grid.offsetAmount, this.gameObject.transform.position.y, this.gameObject.transform.position.z), new Vector3(this.gameObject.transform.position.x + grid.offsetAmount, this.gameObject.transform.position.y, this.gameObject.transform.position.z), 0.5f);
        var left = Physics.OverlapCapsule(new Vector3(this.gameObject.transform.position.x - grid.offsetAmount, this.gameObject.transform.position.y, this.gameObject.transform.position.z), new Vector3(this.gameObject.transform.position.x - grid.offsetAmount, this.gameObject.transform.position.y, this.gameObject.transform.position.z), 0.5f);

        if (up != null)
        {
            foreach (var item in up)
            {
                AddLink(item);
            }
        }
        if (down != null)
        {
            foreach (var item in down)
            {
                AddLink(item);
            }
        }
        if (right != null)
        {
            foreach (var item in right)
            {
                AddLink(item);
            }
        }
        if (left != null)
        {
            foreach (var item in left)
            {
                AddLink(item);
            }
        }

        // Remove duplicates
        graph.GetComponent<Graph>().RemoveDuplicateLinks();
    }

    public void AddLink(Collider item)
    {
        if (item.gameObject.GetComponent<Node>() != null)
        {
            // Check if the link already exists, if it does, dont add a duplicate
            if (!graph.GetComponent<Graph>().CheckIfLinkAlreadyExists(this.gameObject.GetComponent<Node>(), item.gameObject.GetComponent<Node>()))
            {
                AddNode(item); // Add the nodes to the list
                Link newLink = graph.AddComponent<Link>();
                newLink.nodes.Add(this.gameObject.GetComponent<Node>());
                newLink.nodes.Add(item.gameObject.GetComponent<Node>());
                graph.GetComponent<Graph>().UpdateLinksList(newLink); // Update the actual list
            }
        }
    }

    public void AddNode(Collider item)
    {
        // Add the nodes to the Graph component, if not already added
        if (!graph.GetComponent<Graph>().nodes.Contains(this.gameObject.GetComponent<Node>()))
        {
            graph.GetComponent<Graph>().nodes.Add(this.gameObject.GetComponent<Node>());
        }
        if (!graph.GetComponent<Graph>().nodes.Contains(item.gameObject.GetComponent<Node>()))
        {
            graph.GetComponent<Graph>().nodes.Add(item.gameObject.GetComponent<Node>());
        }
    }
}
