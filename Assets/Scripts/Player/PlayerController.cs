using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameObject Player;

    public float movementSpeed = 1.0f;

    public int totalPoints = 0;

    private void Start()
    {
        Player = GameObject.Find("Player");
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 newPos = new Vector3(Player.transform.position.x, Player.transform.position.y, Player.transform.position.z + movementSpeed);
            Player.GetComponent<Rigidbody>().MovePosition(newPos);
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 newPos = new Vector3(Player.transform.position.x - movementSpeed, Player.transform.position.y, Player.transform.position.z);
            Player.GetComponent<Rigidbody>().MovePosition(newPos);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 newPos = new Vector3(Player.transform.position.x, Player.transform.position.y, Player.transform.position.z - movementSpeed);
            Player.GetComponent<Rigidbody>().MovePosition(newPos);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 newPos = new Vector3(Player.transform.position.x + movementSpeed, Player.transform.position.y, Player.transform.position.z);
            Player.GetComponent<Rigidbody>().MovePosition(newPos);
        }
    }
}
