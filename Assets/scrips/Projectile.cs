using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask destructibleLayer;  // Layer para objetos destructibles
    public int damage = 10;  // Daño que inflige el proyectil
    public GameObject Explosion;  // Prefab de la explosión

    void OnCollisionEnter(Collision collision)
    {
        // Verificar si el objeto colisionado está en la capa destructible
        if (((1 << collision.gameObject.layer) & destructibleLayer) != 0)
        {
            // Verificar si el objeto colisionado es un enemigo antes de destruir
            BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                // Infligir daño al enemigo en lugar de destruirlo
                enemy.TakeDamage(damage);
            }
            else
            {
                // Si no es un enemigo, destruir el objeto
                Instantiate(Explosion, collision.transform.position, Quaternion.identity);
                Destroy(collision.gameObject);
            }
        }
        else
        {
            // Si colisiona con cualquier otra cosa, desactiva la bola de fuego
            gameObject.SetActive(false);
        }
    }
}
