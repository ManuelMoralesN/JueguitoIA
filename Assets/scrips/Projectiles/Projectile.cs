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
        // Verificar si el objeto colisionado está en la capa destructible o es un enemigo
        if (((1 << collision.gameObject.layer) & destructibleLayer) != 0)
        {
            // Verificar si el objeto colisionado es un enemigo
            BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                // Infligir daño al enemigo
                enemy.TakeDamage(damage);
            }
            else
            {
                // Si no es un enemigo, crear una explosión
                Instantiate(Explosion, collision.transform.position, Quaternion.identity);
            }
        }

        // Desactiva el proyectil en cualquier caso
        gameObject.SetActive(false);
    }
}
