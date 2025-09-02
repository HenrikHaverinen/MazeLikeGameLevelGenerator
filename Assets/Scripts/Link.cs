using UnityEngine;
using System.Collections.Generic;

public class Link : MonoBehaviour
{
    public List<Node> nodes = new List<Node>(); // the nodes of the link
    public int weight = 0; // the weight of the link
    public bool explored = false;
    public bool simulated = false;
}