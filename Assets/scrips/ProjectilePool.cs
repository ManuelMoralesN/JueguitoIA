using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public GameObject projectilePrefab;  // Prefab del proyectil
    public int poolSize = 10;            // Tamaño del pool de proyectiles
    private List<GameObject> pool;

    void Start()
    {
        pool = new List<GameObject>();

        // Crear los proyectiles al iniciar el juego y desactivarlos
        for (int i = 0; i < poolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab);
            projectile.SetActive(false);
            pool.Add(projectile);
        }
    }

    // Método para obtener un proyectil disponible
    public GameObject GetPooledProjectile()
    {
        foreach (GameObject projectile in pool)
        {
            if (!projectile.activeInHierarchy)
            {
                return projectile;
            }
        }

        // Si no hay proyectiles disponibles, instanciamos uno nuevo
        GameObject newProjectile = Instantiate(projectilePrefab);
        newProjectile.SetActive(false);
        pool.Add(newProjectile);
        return newProjectile;
    }
}