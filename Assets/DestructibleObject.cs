using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public int health = 2;  // Vida del objeto (2 impactos para destruirse)
    public GameObject destructionEffect;  // Prefab del efecto de destrucción

    // Método para recibir daño
    public void TakeDamage(int damage)
    {
        // Reducir la vida del objeto
        health -= damage;

        // Si la vida llega a 0, destruir el objeto
        if (health <= 0)
        {
            // Instanciar el efecto de destrucción
            if (destructionEffect != null)
            {
                Instantiate(destructionEffect, transform.position, Quaternion.identity);

                // Destruir el efecto de partículas después de que termine
                Destroy(destructionEffect, destructionEffect.GetComponent<ParticleSystem>().main.duration);
            }

            // Destruir el objeto
            Destroy(gameObject);
        }
    }
}
