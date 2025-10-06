using System.Linq;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    bool playerSpawned = false;

    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        if (!playerSpawned && GameObject.Find("Subgraph"))
        {
            playerSpawned = true;
            var p = Instantiate(player, GameObject.Find("Subgraph").GetComponent<Graph>().nodes.Where(x => x.nature == Node.Nature.Start).FirstOrDefault().transform.position, Quaternion.identity);
            p.name = "Player";
        }
    }
}
