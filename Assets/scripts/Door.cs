using UnityEngine;

public class Door : MonoBehaviour
{
    public string requiredKey = "Llave";

    public Collider barrierCollider;

    private bool isOpened = false;

    void Start()
    {
        // Si no se asignó el collider, se toma el del mismo objeto
        if (barrierCollider == null)
        {
            barrierCollider = GetComponent<Collider>();
        }
    }

    // Método que intenta abrir la puerta cuando el jugador se acerca
    public void TryOpenDoor(Collider other)
    {
        if (!isOpened && other.CompareTag("Player"))
        {
            if (InventoryManager.Instance.HasKey(requiredKey))
            {
                OpenDoor();
                InventoryManager.Instance.RemoveKey(requiredKey);
            }
            else
            {
                Debug.Log("Necesitas la llave para abrir la puerta.");
            }
        }
    }

    void OpenDoor()
    {
        isOpened = true;
        Debug.Log("Puerta abierta automáticamente.");
        if (barrierCollider != null)
        {
            barrierCollider.enabled = false;
        }
        Destroy(gameObject);
    }
}
