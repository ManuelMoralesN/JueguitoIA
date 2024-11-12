using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SteeringEnemy : BaseEnemy
{
    public float fleeDuration = 3.0f;
    public float restDuration = 2.0f;
    public float detectionRadius = 10.0f;
    public float fleeDistance = 5.0f;
    public float shootingInterval = 2.0f;
    public float tiredShootingInterval = 4.0f;
    public float inaccuracyAngle = 15.0f;
    public float bulletSpeed = 10f;
    public GameObject fireball;
    public Transform bulletSpawnPoint;

    [SerializeField] private float stoppingDistance = 2f;
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

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

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
        bool canSeePlayer = HasLineOfSight();
        bool isInRange = IsPlayerInRange();

        if (canSeePlayer && isInRange)
        {
            if (!isFleeRoutineActive && !isTired)
            {
                StartCoroutine(FleeRoutine());
            }
        }
        else if (!isFleeRoutineActive && !isDead && !isTired)
        {
            PursuePlayer();
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
                bool isInRange = IsPlayerInRange();

                if (canSeePlayer && isInRange)
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

    private void FireAtPlayer(bool isTired)
    {
        if (fireball == null || bulletSpawnPoint == null || player == null || isDead) return;

        if (!HasLineOfSight() || !IsPlayerInRange()) return;

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

        Vector3 rayOrigin = bulletSpawnPoint.position;
        Vector3 directionToPlayer = (player.transform.position - rayOrigin).normalized;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, detectionRadius))
        {
            Debug.DrawRay(rayOrigin, directionToPlayer * detectionRadius, Color.green);
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    private bool IsPlayerInRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= detectionRadius;
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

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (player != null)
        {
            bool canSee = HasLineOfSight();
            bool inRange = IsPlayerInRange();
            Gizmos.color = (canSee && inRange) ? Color.green : Color.red;
            Gizmos.DrawLine(bulletSpawnPoint.position, player.transform.position);
        }
    }
}
