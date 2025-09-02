using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Graph : MonoBehaviour
{
    public List<Node> nodes = new List<Node>();
    public List<Link> links = new List<Link>();

    public bool CheckIfLinkAlreadyExists(Node from, Node to)
    {
        var x = this.gameObject.GetComponents<Link>();

        foreach (var item in x)
        {
            if (item.nodes.Contains(from) && item.nodes.Contains(to))
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateLinksList(Link newLink)
    {
        links.Add(newLink);
    }

    public void RemoveDuplicateLinks()
    {
        links = links.Distinct().ToList();
    }

    public void RandomWeights()
    {
        foreach (var item in links)
        {
            item.weight = Random.Range(0, 100);
        }
    }

    public void SetEndAndStartNodes(Node start, Node end)
    {
        nodes.Where(x => x == start).FirstOrDefault().nature = Node.Nature.Start;
        nodes.Where(x => x == end).FirstOrDefault().nature = Node.Nature.End;
    }

    public void RemoveStartAndEndWalls(int offset)
    {
        Node start = nodes.Where(x => x.nature == Node.Nature.Start).FirstOrDefault();
        Node end = nodes.Where(x => x.nature == Node.Nature.End).FirstOrDefault();

        RemoveNodeWall(start, offset);
        RemoveNodeWall(end, offset);
    }

    void RemoveNodeWall(Node node, int offset)
    {
        Collider[] x = null;

        if (node.location == Node.EdgeLocation.Up)
        {
            x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x, node.transform.position.y, node.transform.position.z + offset), 0.25f);
        }
        if (node.location == Node.EdgeLocation.Down)
        {
            x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x, node.transform.position.y, node.transform.position.z - offset), 0.25f);
        }
        if (node.location == Node.EdgeLocation.Left)
        {
            x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x - offset, node.transform.position.y, node.transform.position.z), 0.25f);
        }
        if (node.location == Node.EdgeLocation.Right)
        {
            x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x + offset, node.transform.position.y, node.transform.position.z), 0.25f);
        }
        // Corners
        if (node.location == Node.EdgeLocation.UpRight)
        {
            int n = Random.Range(0, 1);
            if (n == 0)
            {
                x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x, node.transform.position.y, node.transform.position.z + offset), 0.25f);
            }
            if (n == 1)
            {
                x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x + offset, node.transform.position.y, node.transform.position.z), 0.25f);
            }
        }
        if (node.location == Node.EdgeLocation.UpLeft)
        {
            int n = Random.Range(0, 1);
            if (n == 0)
            {
                x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x, node.transform.position.y, node.transform.position.z + offset), 0.25f);
            }
            if (n == 1)
            {
                x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x - offset, node.transform.position.y, node.transform.position.z), 0.25f);
            }
        }
        if (node.location == Node.EdgeLocation.DownRight)
        {
            int n = Random.Range(0, 1);
            if (n == 0)
            {
                x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x, node.transform.position.y, node.transform.position.z - offset), 0.25f);
            }
            if (n == 1)
            {
                x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x + offset, node.transform.position.y, node.transform.position.z), 0.25f);
            }
        }
        if (node.location == Node.EdgeLocation.DownLeft)
        {
            int n = Random.Range(0, 1);
            if (n == 0)
            {
                x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x, node.transform.position.y, node.transform.position.z - offset), 0.25f);
            }
            if (n == 1)
            {
                x = Physics.OverlapCapsule(node.transform.position, new Vector3(node.transform.position.x - offset, node.transform.position.y, node.transform.position.z), 0.25f);
            }
        }

        if (x != null)
        {
            foreach (var col in x) // check colliders for walls
            {
                if (col.gameObject.GetComponent<Wall>() != null)
                {
                    // the gameobject is a wall, therefore destroy it
                    Destroy(col.gameObject);
                }
            }
        }
    }

    public void SetNodeLinks()
    {
        foreach (var item in links)
        {
            if (!item.nodes[0].links.Contains(item)) // Dont add the same link multiple times
            {
                SetNodeLink(item.nodes[0], item);
            }
            if (!item.nodes[1].links.Contains(item))
            {
                SetNodeLink(item.nodes[1], item);
            }
        }
    }

    void SetNodeLink(Node node, Link link)
    {
        node.links.Add(link);
        node.links = node.links.Distinct().ToList(); // Dont add the same link multiple times
    }

    public void GenerateMaze() // Destroy the walls that are located in positions through which the links of MST travel
    {
        foreach (var item in links)
        {
            // Create collider from links first node to last node, and store the overlapping colliders in a variable
            var x = Physics.OverlapCapsule(item.nodes[0].transform.position, item.nodes[1].transform.position, 0.25f, ~0, QueryTriggerInteraction.UseGlobal);

            if (x != null)
            {
                foreach (var col in x)
                {
                    if (col.gameObject.GetComponent<Wall>() != null)
                    {
                        // the gameobject is a wall, therefore destroy it
                        Destroy(col.gameObject);
                    }
                }
            }
        }
    }
}
