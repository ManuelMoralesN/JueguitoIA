using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public Transform shootPoint;  // El punto desde donde disparará el proyectil
    public float shootForce = 40f;  // Fuerza con la que se dispara el proyectil
    private ProjectilePool projectilePool;  // Referencia al sistema de pool de proyectiles

    void Start()
    {
        // Obtener la referencia al pool de proyectiles
        projectilePool = FindObjectOfType<ProjectilePool>();
    }

    void Update()
    {
        // Disparar el proyectil cuando se presiona la tecla F
        if (Input.GetKeyDown(KeyCode.F))
        {
            ShootProjectile();
        }
    }

void ShootProjectile()
{
    // Obtener un proyectil del pool
    GameObject projectile = projectilePool.GetPooledProjectile();

    // Colocar el proyectil en el punto de disparo
    projectile.transform.position = shootPoint.position;
    projectile.transform.rotation = shootPoint.rotation;
    
    // Activar el proyectil
    projectile.SetActive(true);

    // Obtener el Collider del proyectil
    Collider projectileCollider = projectile.GetComponent<Collider>();

    // Obtener el Collider del jugador (suponiendo que este script está en el jugador)
    Collider playerCollider = GetComponent<Collider>();

    // Ignorar colisiones entre el proyectil y el jugador
    Physics.IgnoreCollision(projectileCollider, playerCollider);

    // Aplicar fuerza en la dirección hacia la que el personaje está mirando
    Rigidbody rb = projectile.GetComponent<Rigidbody>();
    rb.useGravity = false;
    rb.velocity = shootPoint.forward * shootForce;

    // Desactivar el proyectil después de un tiempo si no impacta
    StartCoroutine(DisableProjectileAfterTime(projectile, 5f));
}

IEnumerator DisableProjectileAfterTime(GameObject projectile, float time)
{
    yield return new WaitForSeconds(time);
    projectile.SetActive(false);  // Desactivar el proyectil después de que pase el tiempo
}

}