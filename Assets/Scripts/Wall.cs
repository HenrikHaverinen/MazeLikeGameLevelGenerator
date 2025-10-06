using UnityEngine;

public class Wall : MonoBehaviour
{
    private void Awake()
    {
        Physics.IgnoreLayerCollision(10, 10, true); // walls dont physically interact with other walls
    }
}
