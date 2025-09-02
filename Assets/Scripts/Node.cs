using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public List<Link> links = new List<Link>(); // links of this node

    public enum Nature // is the node the start of the maze, the end of the maze, or just a regular node
    {
        Regular,
        Start,
        End
    }

    public Nature nature = Nature.Regular;
    public bool Edge = false;

    public enum EdgeLocation
    {
        None,
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    public EdgeLocation location = EdgeLocation.None;

    public bool explored = false;

    public bool simulated = false;
}