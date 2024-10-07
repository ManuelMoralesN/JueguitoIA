using UnityEngine;
using System.Collections;

public class EnemyOp : BaseEnemy  
{
    public float detectionAngle = 45f;  // Ángulo del cono de visión
    public float detectionDistance = 10f;  // Distancia del cono de visión
    public float rotationSpeed = 30f;  // Velocidad de rotación del cono de visión
    public float detectionCooldown = 3f;  // Tiempo antes de volver a rotar si no ve al jugador
    public Transform shootPoint;  // Punto desde donde se dispara el proyectil
    public GameObject projectilePrefab;  // Prefab del proyectil que dispara

    private bool isPlayerDetected = false;  // Indica si el jugador está en el cono de visión
    private Transform player;
    private Coroutine rotationCoroutine;
    private Animator animator;
    private bool isDead = false;
    private Rigidbody rb;

void Start()
{
    rb = GetComponent<Rigidbody>();  // Obtener la referencia al Rigidbody
    animator = GetComponent<Animator>();  // Obtener la referencia al Animator

    if (rb == null)
    {
        Debug.LogError("Rigidbody no encontrado en el EnemyOp");
    }

    if (animator == null)
    {
        Debug.LogError("Animator no encontrado en el EnemyOp");
    }

    player = GameObject.FindGameObjectWithTag("Player").transform;
    rotationCoroutine = StartCoroutine(RotateVision());
}
    void Update()
    {
        if (player != null)
        {
            DetectPlayer();
        }
    }

    void DetectPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Verifica si el jugador está dentro del ángulo y distancia del cono de visión
        if (angleToPlayer < detectionAngle / 2 && Vector3.Distance(transform.position, player.position) <= detectionDistance)
        {
            if (!isPlayerDetected)
            {
                isPlayerDetected = true;
                if (rotationCoroutine != null)
                {
                    StopCoroutine(rotationCoroutine);  // Detener la rotación cuando el jugador es detectado
                }
                StartCoroutine(ShootAtPlayer());
            }
        }
        else
        {
            if (isPlayerDetected)
            {
                isPlayerDetected = false;
                StartCoroutine(ResumeRotationAfterDelay());
            }
        }
    }

    IEnumerator RotateVision()
    {
        while (!isPlayerDetected)  // Solo rota si el jugador no ha sido detectado
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator ShootAtPlayer()
    {
        while (isPlayerDetected)
        {
            // Dispara hacia la posición actual del jugador
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Vector3 directionToPlayer = (player.position - shootPoint.position).normalized;
            projectile.GetComponent<Rigidbody>().velocity = directionToPlayer * 30f;  // Ajusta la velocidad según sea necesario

            yield return new WaitForSeconds(1f);  // Dispara cada segundo (ajusta este valor según sea necesario)
        }
    }

    IEnumerator ResumeRotationAfterDelay()
    {
        yield return new WaitForSeconds(detectionCooldown);  // Esperar antes de reanudar la rotación
        if (!isPlayerDetected)  // Solo reanuda la rotación si el jugador ya no está detectado
        {
            rotationCoroutine = StartCoroutine(RotateVision());  // Reanudar la rotación del cono de visión
        }
    }

    protected void OnCollisionEnter(Collision collision)
{
    // Verificar si el objeto que tocó pertenece a la capa del jugador
    if (((1 << collision.gameObject.layer) & playerLayer) != 0)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageToPlayer);
            Debug.Log("El jugador ha recibido daño: " + damageToPlayer);
        }
    }
}

  // Sobrescribimos el método Die de BaseEnemy para implementar la animación de muerte
    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        // Desactivar el movimiento
        rb.velocity = Vector3.zero;

        Debug.Log("EnemyOp ha muerto.");

        // Activar la animación de muerte
        animator.SetTrigger("Dead");

        // Iniciar la corutina para destruir el objeto después de la animación
        StartCoroutine(DestroyAfterDeath());
    }

    // Corutina para esperar a que la animación de muerte termine antes de destruir al enemigo
    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(3f);  // Ajusta esto al tiempo de duración de la animación de muerte
        Destroy(gameObject);  // Destruye el objeto enemigo
    }
    void OnDrawGizmos()
    {
        Gizmos.color = isPlayerDetected ? Color.red : Color.green;  // Cambia el color del Gizmo si el jugador es detectado
        Vector3 forward = transform.forward * detectionDistance;
        Quaternion leftRayRotation = Quaternion.Euler(0, -detectionAngle / 2, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, detectionAngle / 2, 0);
        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        // Dibujar el cono de visión con la corrección de dirección
        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position, leftRayDirection);
        Gizmos.DrawRay(transform.position, rightRayDirection);

        // Dibujar una línea circular para visualizar el rango del cono de visión
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}
