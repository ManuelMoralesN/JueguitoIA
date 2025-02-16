using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Door doorScript;

    void OnTriggerEnter(Collider other)
    {
        if (doorScript != null)
        {
            doorScript.TryOpenDoor(other);
        }
    }
}
