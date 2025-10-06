using UnityEngine;

public class Point : MonoBehaviour
{
    int value = 5;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponentInParent<PlayerController>().totalPoints += value;
            Debug.Log("Player points: " + other.gameObject.GetComponentInParent<PlayerController>().totalPoints);
            Destroy(gameObject);
        }
    }

}
