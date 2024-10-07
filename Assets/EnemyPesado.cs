using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyPesado : BaseEnemy  // EnemyPesado hereda de BaseEnemy
{
    public Transform ObjectFollow;  // Referencia al jugador
    public float maxSpeed = 5f;  // Velocidad máxima del enemigo
    public float acceleration = 0.5f;  // Aceleración del enemigo
    private float currentSpeed = 0f;  // Velocidad actual del enemigo
    public float stopDistance = 0.5f;  // Distancia mínima para detenerse antes del jugador
    public float rotationSpeed = 5f;  // Velocidad de rotación del enemigo

    private Rigidbody rb;
    private Animator animator;  // Referencia al Animator
    private bool isDead = false;

    void Awake()
    {
        // Buscar el objeto con la etiqueta "Player" y obtener su Transform
        ObjectFollow = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();  // Asignar el Animator al enemigo

        // Asegurarse de que la gravedad esté activada y las rotaciones restringidas
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;  // Evitar giros extraños en los ejes X y Z
    }

    void Update()
    {
        // Si no encuentra al jugador, salir
        if (ObjectFollow == null)
            return;

        MoveTowardsPlayer();  // Mueve al enemigo hacia el jugador
    }

    // Método que controla el movimiento hacia el jugador
    void MoveTowardsPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, ObjectFollow.position);

        // Si está dentro de la distancia, activa la animación de correr
        if (distanceToPlayer > stopDistance)
        {
            // Activar la animación de correr
            animator.SetBool("isRunning", true);

            // Aumentar la velocidad gradualmente hasta la velocidad máxima
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            // Vector que apunta hacia el jugador, ignorando el eje Y
            Vector3 directionToPlayer = (ObjectFollow.position - transform.position).normalized;
            directionToPlayer.y = 0;  // Mantener el movimiento en el plano XZ

            // Mover al enemigo hacia el jugador usando la velocidad actual
            rb.MovePosition(transform.position + directionToPlayer * currentSpeed * Time.deltaTime);

            // Hacer que el enemigo mire hacia el jugador gradualmente
            Quaternion targetRotation = Quaternion.LookRotation(-directionToPlayer);  // Cambiamos la dirección a -directionToPlayer
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Desactivar la animación de correr cuando el enemigo se detiene
            animator.SetBool("isRunning", false);

            // Reiniciar la velocidad actual cuando se detiene
            currentSpeed = 0f;
        }
    }

        // Sobrescribimos el método Die de BaseEnemy para implementar la animación de muerte
    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        // Desactivar el movimiento
        rb.velocity = Vector3.zero;

        Debug.Log("EnemyPesado ha muerto.");

        // Activar la animación de muerte
        animator.SetTrigger("DieE");

        // Iniciar la corutina para destruir el objeto después de la animación
        StartCoroutine(DestroyAfterDeath());
    }

    // Corutina para esperar a que la animación de muerte termine antes de destruir al enemigo
    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f);  // Ajusta esto al tiempo de duración de la animación de muerte
        Destroy(gameObject);  // Destruye el objeto enemigo
    }
}
