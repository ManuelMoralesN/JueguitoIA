using System.Collections;
using UnityEngine;

public class DamageSlowZone : MonoBehaviour
{
    public float slowedSpeed = 2f;   // Velocidad a la que se ralentizar� el jugador
    public int damageAmount = 10;      // Da�o que se aplicar� al jugador al tocar la zona
    public float damageInterval = 1f;  // Intervalo de tiempo en segundos para el da�o continuo

    private float originalSpeed;       // Para almacenar la velocidad original del jugador
    private bool speedChanged = false; // Para evitar reasignar la velocidad si ya se modific�

    private Coroutine damageCoroutine; // Referencia a la corrutina de da�o continuo

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
                    Debug.Log("El jugador ha entrado en la zona de da�o y lentitud. Velocidad reducida a " + slowedSpeed);
                }
            }
            else
            {
                Debug.LogWarning("No se encontr� el componente PlayerMovement en el jugador.");
            }

            // Obtener el componente de salud y aplicar da�o
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Aplica da�o inmediato al entrar en la zona
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("El jugador ha recibido " + damageAmount + " puntos de da�o al entrar en la zona.");

                // Iniciar la corrutina para aplicar da�o continuo si a�n no est� en ejecuci�n
                if (damageCoroutine == null)
                {
                    damageCoroutine = StartCoroutine(ApplyContinuousDamage(playerHealth));
                }
            }
            else
            {
                Debug.LogWarning("No se encontr� el componente PlayerHealth en el jugador.");
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
                Debug.Log("El jugador ha recibido " + damageAmount + " puntos de da�o continuo.");
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
                Debug.Log("El jugador ha salido de la zona de da�o y lentitud. Velocidad restaurada a " + originalSpeed);
            }
            // Detener la corrutina de da�o continuo
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
                Debug.Log("Se ha detenido el da�o continuo.");
            }
        }
    }
}
