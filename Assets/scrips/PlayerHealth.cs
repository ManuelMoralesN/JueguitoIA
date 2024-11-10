using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;
    private Animator animator;
    private bool isDead = false;  // Para evitar múltiples llamadas a Die()

    public GameObject gameOverUI;  // Asigna el Canvas de Game Over desde el Inspector
    public float delayBeforeGameOver = 2f;  // Tiempo en segundos antes de mostrar el Game Over

    void Start()
    {
        animator = GetComponent<Animator>();  // Obtener la referencia al Animator
        gameOverUI.SetActive(false);  // Asegurarse de que la UI esté desactivada al iniciar
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

        // Iniciar la corutina para mostrar la pantalla de Game Over después del retraso
        StartCoroutine(ShowGameOverUI());
    }

    // Corutina para esperar antes de mostrar el Game Over
    IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);  // Esperar 2 segundos
        gameOverUI.SetActive(true);  // Mostrar la pantalla de Game Over
    }

    // Método para reiniciar el nivel cuando se presiona el botón de reinicio
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // Reiniciar la escena actual
    }
}
