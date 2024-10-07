using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public int health = 100;  // Vida del enemigo
    public int damageToPlayer = 10;  // Daño que hace al jugador al tocarlo
    public LayerMask playerLayer;  // Capa asignada al jugador

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

    // Método para hacer daño al jugador cuando lo toca
    private void OnCollisionEnter(Collision collision)
    {
        // Verificar si el objeto que tocó pertenece a la capa del jugador
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Asumimos que el jugador tiene un componente de salud llamado "PlayerHealth"
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                Debug.Log("El jugador ha recibido daño: " + damageToPlayer);
            }
        }
    }

   public virtual void Die()
    {
        Debug.Log("El enemigo ha muerto.");
        Destroy(gameObject);  // Destruye el objeto enemigo
    }
}
