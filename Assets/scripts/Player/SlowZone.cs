using UnityEngine;

public class SlowZone : MonoBehaviour
{
    public float slowedSpeed = 2f;  // Velocidad a la que se ralentizar� el jugador
    private float originalSpeed;    // Para guardar la velocidad original

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                originalSpeed = playerMovement.speed;
                playerMovement.speed = slowedSpeed;
                Debug.Log("El jugador ha entrado en la zona de ralentizaci�n. Velocidad reducida a " + slowedSpeed);
            }
            else
            {
                Debug.LogWarning("No se encontr� el componente PlayerMovement en el jugador.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.speed = originalSpeed;
                Debug.Log("El jugador ha salido de la zona de ralentizaci�n. Velocidad restaurada a " + originalSpeed);
            }
        }
    }
}
