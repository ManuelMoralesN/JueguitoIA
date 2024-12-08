using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;
    private Animator animator;
    private bool isDead = false; // Para evitar múltiples llamadas a Die()

    private UIManager uiManager; // Referencia al UIManager

    [Header("Efecto de Daño")]
    public Renderer playerRenderer; // Renderer para cambiar el color del jugador
    public Color damageColor = Color.red; // Color que tomará al recibir daño
    public float flashDuration = 0.2f; // Duración del parpadeo

    private Color originalColor; // Color original del jugador

    void Start()
    {
        animator = GetComponent<Animator>(); // Obtener la referencia al Animator
        uiManager = FindObjectOfType<UIManager>(); // Obtener la referencia al UIManager

        // Guardar el color original del jugador
        if (playerRenderer == null)
        {
            playerRenderer = GetComponentInChildren<Renderer>();
        }
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
        else
        {
            Debug.LogError("PlayerHealth: No se encontró un Renderer asignado.");
        }
    }

    // Método para recibir daño
    public void TakeDamage(int damage)
    {
        if (isDead) return; // No recibir más daño si ya está muerto

        health -= damage;
        Debug.Log("Jugador recibió daño. Salud restante: " + health);

        // Activar el efecto de parpadeo
        StartCoroutine(FlashDamageEffect());

        if (health <= 0)
        {
            Die();
        }
    }

    // Método para manejar la muerte del jugador
    private void Die()
    {
        if (isDead) return; // Evitar múltiples llamadas a Die()

        isDead = true; // Establecer estado de muerte

        Debug.Log("El jugador ha muerto");

        // Activar la animación de muerte
        animator.SetTrigger("Die");

        // Desactivar el movimiento
        GetComponent<PlayerMovement>().enabled = false;

        // Llamar al UIManager para mostrar la pantalla de Game Over
        uiManager.ShowGameOver();
    }

    // Efecto de parpadeo rojo al recibir daño
    private IEnumerator FlashDamageEffect()
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.color = damageColor; // Cambiar al color de daño
            yield return new WaitForSeconds(flashDuration); // Esperar
            playerRenderer.material.color = originalColor; // Restaurar el color original
        }
    }
}
