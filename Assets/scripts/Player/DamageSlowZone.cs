using System.Collections;
using UnityEngine;

public class DamageSlowZone : MonoBehaviour
{
    public float slowedSpeed = 2f;   // Velocidad a la que se ralentizará el jugador
    public int damageAmount = 10;      // Daño que se aplicará al jugador al tocar la zona
    public float damageInterval = 1f;  // Intervalo de tiempo en segundos para el daño continuo

    private float originalSpeed;       // Para almacenar la velocidad original del jugador
    private bool speedChanged = false; // Para evitar reasignar la velocidad si ya se modificó

    private Coroutine damageCoroutine; // Referencia a la corrutina de daño continuo

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Obtener el componente de movimiento del jugador
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                if (!speedChanged)
                {
                    originalSpeed = playerMovement.speed;
                    playerMovement.speed = slowedSpeed;
                    speedChanged = true;
                    Debug.Log("El jugador ha entrado en la zona de daño y lentitud. Velocidad reducida a " + slowedSpeed);
                }
            }
            else
            {
                Debug.LogWarning("No se encontró el componente PlayerMovement en el jugador.");
            }

            // Obtener el componente de salud y aplicar daño
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Aplica daño inmediato al entrar en la zona
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("El jugador ha recibido " + damageAmount + " puntos de daño al entrar en la zona.");

                // Iniciar la corrutina para aplicar daño continuo si aún no está en ejecución
                if (damageCoroutine == null)
                {
                    damageCoroutine = StartCoroutine(ApplyContinuousDamage(playerHealth));
                }
            }
            else
            {
                Debug.LogWarning("No se encontró el componente PlayerHealth en el jugador.");
            }
        }
    }

    IEnumerator ApplyContinuousDamage(PlayerHealth playerHealth)
    {
        while (true)
        {
            yield return new WaitForSeconds(damageInterval);
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("El jugador ha recibido " + damageAmount + " puntos de daño continuo.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Restaurar la velocidad original del jugador al salir de la zona
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null && speedChanged)
            {
                playerMovement.speed = originalSpeed;
                speedChanged = false;
                Debug.Log("El jugador ha salido de la zona de daño y lentitud. Velocidad restaurada a " + originalSpeed);
            }
            // Detener la corrutina de daño continuo
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
                Debug.Log("Se ha detenido el daño continuo.");
            }
        }
    }
}
