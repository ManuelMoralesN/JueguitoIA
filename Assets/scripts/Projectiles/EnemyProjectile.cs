using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 10;            // Daño que inflige el proyectil
    public float speed = 20;         // Velocidad del proyectil
    public LayerMask playerLayer;     // Layer para el jugador
    public GameObject explosionEffect; // Prefab de la explosión

    private Vector3 direction;        // Dirección del proyectil

    // Método para inicializar el proyectil con dirección
    public void Initialize(Vector3 dir)
    {
        direction = dir.normalized; // Asegurarse de que la dirección esté normalizada
    }

    void Update()
    {
        // Mover el proyectil en la dirección configurada
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Verificar si el objeto colisionado está en la capa del jugador
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Infligir daño al jugador si colisiona con él
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Instanciar el efecto de explosión al impactar contra el jugador
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, collision.transform.position, Quaternion.identity);
            }
        }

        // Independientemente del objeto con el que colisione, desactivar el proyectil
        Destroy(gameObject);
    }
}
