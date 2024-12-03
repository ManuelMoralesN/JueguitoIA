using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;
    private Animator animator;
    private bool isDead = false;  // Para evitar múltiples llamadas a Die()

    private UIManager uiManager;  // Referencia al UIManager

    void Start()
    {
        animator = GetComponent<Animator>();  // Obtener la referencia al Animator
        uiManager = FindObjectOfType<UIManager>();  // Obtener la referencia al UIManager
    }

    // Método para recibir daño
    public void TakeDamage(int damage)
    {
        if (isDead) return;  // No recibir más daño si ya está muerto

        health -= damage;
        Debug.Log("Jugador recibió daño. Salud restante: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    // Método para manejar la muerte del jugador
    private void Die()
    {
        if (isDead) return;  // Evitar múltiples llamadas a Die()

        isDead = true;  // Establecer estado de muerte

        Debug.Log("El jugador ha muerto");

        // Activar la animación de muerte
        animator.SetTrigger("Die");

        // Desactivar el movimiento
        GetComponent<PlayerMovement>().enabled = false;

        // Llamar al UIManager para mostrar la pantalla de Game Over
        uiManager.ShowGameOver();
    }
}
