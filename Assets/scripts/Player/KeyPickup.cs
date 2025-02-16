using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyName = "Llave";
    public float rotationSpeed = 50f; // Velocidad de rotación, ajustable desde el inspector

    void Update()
    {
        // Rota la llave sobre su eje Y de forma continua
        transform.Rotate(Vector3.left * rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Agrega la llave al inventario automáticamente
            InventoryManager.Instance.AddKey(keyName);
            Debug.Log("Llave recogida automáticamente.");

            // Elimina la llave de la escena
            Destroy(gameObject);
        }
    }
}
