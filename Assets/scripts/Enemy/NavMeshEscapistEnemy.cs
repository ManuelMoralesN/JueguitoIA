using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshEscapistEnemy : BaseEnemy
{
    public float fleeDuration = 3.0f;
    public float restDuration = 2.0f;
    public float detectionRadius = 10.0f;
    public float fleeDistance = 2.5f;  // Distancia para iniciar flee
    public float shootingInterval = 2.0f;
    public float tiredShootingInterval = 4.0f;
    public float inaccuracyAngle = 15.0f;
    public float bulletSpeed = 10f;
    public GameObject fireball;
    public Transform bulletSpawnPoint;

    [SerializeField] private float stoppingDistance = 4f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float angularSpeed = 360f;

    private GameObject player;
    private NavMeshAgent agent;
    private bool isTired = false;
    private bool isFleeRoutineActive = false;
    private bool isDead = false;
    private Animator animator;
    private float lastShootTime;

    protected UIManager uiManager;  // Asegurarse de que sea protected para heredar la referencia

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Obtener la referencia al UIManager
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager no encontrado en la escena.");
        }

        agent.stoppingDistance = stoppingDistance;
        agent.speed = speed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;

        EnterActiveState();
    }

    private void Update()
    {
        if (!isTired && !isDead)
        {
            CheckPlayerDetection();
        }
    }

    private void CheckPlayerDetection()
    {
        bool canSeePlayer = HasLineOfSight(); // Línea de visión sin límites de distancia
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Camper: enemigo se queda quieto y dispara cuando puede ver al jugador y está fuera del rango de escape
        if (canSeePlayer && distanceToPlayer > fleeDistance && !isFleeRoutineActive)
        {
            agent.isStopped = true; // Detener al enemigo
            StartCoroutine(ShootAsCamper()); // Disparo como "camper"
        }
        // Flee: enemigo huye cuando el jugador está muy cerca
        else if (canSeePlayer && distanceToPlayer <= fleeDistance && !isFleeRoutineActive)
        {
            StopCoroutine(ShootAsCamper()); // Detiene el comportamiento de camper si está en marcha
            StartCoroutine(FleeRoutine()); // Iniciar el escape
        }
        // Persecución: enemigo sigue al jugador cuando no lo ve y no está cansado o escapando
        else if (!isFleeRoutineActive && !isDead && !isTired)
        {
            PursuePlayer(); // Perseguir al jugador
        }
    }

    private void EnterActiveState()
    {
        if (isDead) return;

        isTired = false;
        isFleeRoutineActive = false;
        animator.SetBool("isRunning", true);
        animator.SetBool("isResting", false);
        lastShootTime = 0f;
    }

    private void PursuePlayer()
    {
        if (player != null && !isDead)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
    }

    private IEnumerator FleeRoutine()
    {
        isFleeRoutineActive = true;
        agent.isStopped = false;

        float fleeTimer = 0f;
        while (fleeTimer < fleeDuration)
        {
            // Dirección de escape (opuesta al jugador)
            Vector3 fleeDirection = (transform.position - player.transform.position).normalized;
            Vector3 targetPosition = transform.position + fleeDirection * fleeDistance;

            // Gira el enemigo hacia el jugador
            if (player != null)
            {
                Vector3 lookDirection = (player.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed);
            }

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            // Dispara mientras huye
            if (Time.time >= lastShootTime + shootingInterval)
            {
                FireAtPlayer(false);
                lastShootTime = Time.time;
            }

            fleeTimer += Time.deltaTime;
            yield return null;
        }

        EnterTiredState();
    }

    private void EnterTiredState()
    {
        if (isDead) return;

        isTired = true;
        isFleeRoutineActive = false;
        agent.ResetPath();
        agent.isStopped = true;
        animator.SetBool("isResting", true);
        animator.SetBool("isRunning", false);
        StartCoroutine(TiredRoutine());
    }

    private IEnumerator TiredRoutine()
    {
        float timeSpentInTiredState = 0f;
        lastShootTime = Time.time;

        while (timeSpentInTiredState < restDuration)
        {
            if (Time.time >= lastShootTime + tiredShootingInterval)
            {
                bool canSeePlayer = HasLineOfSight();
                if (canSeePlayer)
                {
                    FireAtPlayer(true);
                    lastShootTime = Time.time;
                }
            }
            timeSpentInTiredState += Time.deltaTime;
            yield return null;
        }

        EnterActiveState();
    }

    private IEnumerator ShootAsCamper()
    {
        while (true)
        {
            if (Time.time >= lastShootTime + shootingInterval && HasLineOfSight())
            {
                FireAtPlayer(false); // Dispara sin inexactitud
                lastShootTime = Time.time;
            }
            yield return new WaitForSeconds(shootingInterval);
        }
    }

    private void FireAtPlayer(bool isTired)
    {
        if (fireball == null || bulletSpawnPoint == null || player == null || isDead) return;

        GameObject bullet = Instantiate(fireball, bulletSpawnPoint.position, Quaternion.identity);
        Vector3 directionToPlayer = (player.transform.position - bulletSpawnPoint.position).normalized;

        if (isTired)
        {
            directionToPlayer = ApplyInaccuracy(directionToPlayer);
        }

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.velocity = directionToPlayer * bulletSpeed;
        }
    }

    private Vector3 ApplyInaccuracy(Vector3 direction)
    {
        float inaccuracy = Random.Range(-inaccuracyAngle, inaccuracyAngle);
        Quaternion rotation = Quaternion.Euler(0, inaccuracy, 0);
        return rotation * direction;
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector3 rayOrigin = bulletSpawnPoint.position; // Origen del rayo
        Vector3 directionToPlayer = (player.transform.position - rayOrigin).normalized;
        RaycastHit hit;

        // Raycast sin límite de distancia
        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit))
        {
            Debug.DrawRay(rayOrigin, directionToPlayer * 100, Color.green); // Rayo de depuración
            return hit.collider.CompareTag("Player"); // Verifica si el rayo impacta al jugador
        }

        return false;
    }

    public override void Die()
    {
        if (isDead) return;

        isDead = true;
        agent.ResetPath();
        agent.isStopped = true;

        animator.SetBool("Dead", true);
        animator.SetBool("isRunning", false);
        animator.SetBool("isResting", false);

        Debug.Log("El enemigo ha muerto.");

        // Verificar si uiManager no es null antes de llamar a ShowVictory
        if (uiManager != null)
        {
            StartCoroutine(HandleVictoryAndDeath());
        }
        else
        {
            Debug.LogError("UIManager es null. No se puede mostrar la pantalla de victoria.");
            StartCoroutine(DestroyAfterDeath());  // Continuar con la destrucción para evitar bloqueo
        }
    }
    private IEnumerator HandleVictoryAndDeath()
    {
        uiManager.ShowVictory();  // Mostrar la pantalla de victoria

        // Esperar un poco más de tiempo antes de destruir el enemigo para asegurarse de que la pantalla se muestre
        yield return new WaitForSeconds(4f);  // Asegúrate de que sea suficiente tiempo para que la pantalla de victoria se vea

        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f);  // Esperar 2 segundos antes de destruir el enemigo
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (player != null)
        {
            bool canSee = HasLineOfSight();
            Gizmos.color = canSee ? Color.green : Color.red;
            Gizmos.DrawLine(bulletSpawnPoint.position, player.transform.position);
        }
    }
}

    
