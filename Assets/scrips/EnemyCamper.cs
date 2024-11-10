using System.Collections;
using UnityEngine;

public class EnemyCamper : BaseEnemy
{
    public Transform player;  // Referencia al jugador
    public GameObject projectilePrefab;  // Prefab del proyectil
    public float projectileSpeed = 10f;  // Velocidad del proyectil
    public float fleeTime = 3f;  // Tiempo que huye antes de "cansarse"
    public float restTime = 2f;  // Tiempo que descansa antes de repetir el ciclo
    public float fleeSpeed = 5f;  // Velocidad máxima al huir
    public float acceleration = 2f;  // Qué tan rápido acelera
    public Transform shootPoint;  // Punto desde donde se disparan los proyectiles
    private float currentSpeed = 0f;  // Velocidad actual
    private bool isResting = false;  // Controla si está descansando
    private float nextFireTime;  // Controla el tiempo entre disparos
    public float fireRate = 2f;  // Tiempo entre disparos

    private Rigidbody rb;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Inicia el ciclo de huir y descansar
        StartCoroutine(FleeCycle());
    }

    void Update()
    {
        if (isResting || player == null) return;  // No se mueve ni dispara si está descansando o si no encuentra al jugador

        FleeFromPlayer();  // Movimiento de huida

        if (Time.time > nextFireTime)
        {
            ShootAtPredictedPosition();  // Dispara a la posición predicha del jugador
            nextFireTime = Time.time + fireRate;  // Actualiza el tiempo del próximo disparo
        }
    }

    // Método que controla la huida del jugador
    void FleeFromPlayer()
    {
        // Calcular la dirección opuesta al jugador
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        directionAwayFromPlayer.y = 0;  // Mantener el movimiento en el plano XZ

        // Aumentar la velocidad hasta la velocidad máxima
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, fleeSpeed);

        // Mover al enemigo en la dirección opuesta al jugador
        rb.MovePosition(transform.position + directionAwayFromPlayer * currentSpeed * Time.deltaTime);

        // Girar al enemigo para que mire en la dirección opuesta al jugador
        Quaternion targetRotation = Quaternion.LookRotation(-directionAwayFromPlayer);  // Invertimos para mirar hacia la dirección de huida
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    // Método para disparar hacia la posición predicha del jugador
    void ShootAtPredictedPosition()
    {
        // Obtén la posición inicial del jugador y su velocidad
        Vector3 playerPos = player.position;
        Vector3 playerVelocity = player.GetComponent<Rigidbody>().velocity;  // Asumiendo que el jugador tiene un Rigidbody

        // Calcula el tiempo de predicción basado en la distancia y la velocidad del proyectil
        float predictedTime = CalculatePredictedTime(projectileSpeed, shootPoint.position, playerPos);

        // Calcula la posición futura del jugador
        Vector3 predictedPos = PredictPos(playerPos, playerVelocity, predictedTime);

        // Dispara el proyectil desde el shootPoint hacia la posición predicha
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);  // Usamos shootPoint.position
        Rigidbody rbProjectile = projectile.GetComponent<Rigidbody>();

        // Apunta el proyectil hacia la posición predicha
        Vector3 direction = (predictedPos - shootPoint.position).normalized;
        rbProjectile.velocity = direction * projectileSpeed;
    }

    // Predicción de la posición futura del jugador
    Vector3 PredictPos(Vector3 initialPos, Vector3 velocity, float timePrediction)
    {
        return initialPos + velocity * timePrediction;  // Calcula la posición futura
    }

    // Calcula el tiempo estimado para que el proyectil alcance al jugador
    float CalculatePredictedTime(float maxSpeed, Vector3 initialPos, Vector3 targetPos)
    {
        float distance = (targetPos - initialPos).magnitude;  // Distancia entre el shootPoint y el jugador
        return distance / maxSpeed;  // Tiempo necesario para que el proyectil alcance al jugador
    }

    // Corutina que controla el ciclo de huir y descansar
    IEnumerator FleeCycle()
    {
        while (true)
        {
            // Huir durante un tiempo
            isResting = false;
            animator.SetBool("isResting", false);  // Desactivar la animación de descanso
            animator.SetBool("isRunning", true);  // Activar la animación de correr
            currentSpeed = 0f;  // Reinicia la velocidad al iniciar la huida
            yield return new WaitForSeconds(fleeTime);

            // Detenerse y descansar
            isResting = true;
            currentSpeed = 0f;  // Velocidad 0 mientras descansa
            animator.SetBool("isRunning", false);  // Desactivar la animación de correr
            animator.SetBool("isResting", true);  // Activar la animación de descanso
            yield return new WaitForSeconds(restTime);
        }
    }
            // Sobrescribimos el método Die de BaseEnemy para implementar la animación de muerte
    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        // Desactivar el movimiento
        rb.velocity = Vector3.zero;

        Debug.Log("EnemyCamper ha muerto.");

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
}
