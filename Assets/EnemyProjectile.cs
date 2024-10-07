using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 10;  // Daño que inflige el proyectil
    public LayerMask playerLayer;  // Layer para el jugador
    public GameObject explosionEffect;  // Prefab de la explosión

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
            Instantiate(explosionEffect, collision.transform.position, Quaternion.identity);
        }

        // Independientemente del objeto con el que colisione, desactivar el proyectil
        gameObject.SetActive(false);
    }
}
