using System.Collections;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public int health = 100;  // Vida del enemigo
    public int damageToPlayer = 10;  // Daño que hace al jugador al tocarlo
    public LayerMask playerLayer;  // Capa asignada al jugador
    protected UIManager uiManager;  // Referencia al UIManager

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();  // Obtener la referencia al UIManager
    }

    // Método para recibir daño
    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemigo ha recibido " + damage + " puntos de daño. Vida restante: " + health);

        if (health <= 0)
        {
            Die();  // Llamar al método de muerte solo si la vida llega a 0
        }
    }
    public virtual void Die()
    {
        Debug.Log("El enemigo ha muerto.");
        uiManager.ShowVictory();  // Mostrar pantalla de victoria antes de destruir el enemigo

        // Agregar un retraso antes de destruir el objeto enemigo para asegurar que la pantalla de victoria se vea
        StartCoroutine(DestroyAfterVictory());
    }

    private IEnumerator DestroyAfterVictory()
    {
        yield return new WaitForSeconds(uiManager.delayBeforeVictory + 1.5f);  // Esperar el tiempo de la pantalla de victoria + un segundo y medio extra
        Destroy(gameObject);
    }

}
