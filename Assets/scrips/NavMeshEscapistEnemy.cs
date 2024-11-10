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
    public float lineOfSightTimeout = 3.0f;
    public float bulletSpeed = 10f;
    public GameObject fireball;
    public Transform bulletSpawnPoint;

    private GameObject player;
    private NavMeshAgent agent;
    private bool isTired = false;
    private bool isFleeRoutineActive = false;
    private bool isDead = false;
    private Animator animator;
    private float lastLineOfSightTime;
    private Vector3 lastFleePosition;
    private float lastShootTime;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        lastLineOfSightTime = -lineOfSightTimeout;

        agent.stoppingDistance = 2f;
        agent.speed = 5f;
        agent.acceleration = 12f;
        agent.angularSpeed = 360f;

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
        if (IsPlayerInRange() && HasLineOfSight())
        {
            lastLineOfSightTime = Time.time;
            if (!isFleeRoutineActive)
            {
                agent.ResetPath();
                StartCoroutine(FleeRoutine());
            }
        }
        else if (!isFleeRoutineActive && Time.time - lastLineOfSightTime >= lineOfSightTimeout)
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
        if (!isTired && !isFleeRoutineActive && player != null && !isDead)
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
            Vector3 fleeDirection = (transform.position - player.transform.position).normalized;
            Vector3 targetPosition = transform.position + fleeDirection * fleeDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, fleeDistance, NavMesh.AllAreas))
            {
                lastFleePosition = hit.position;
                agent.SetDestination(hit.position);
            }

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
                if (IsPlayerInRange())
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

        RaycastHit hit;
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRadius))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    private bool IsPlayerInRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= detectionRadius;
    }

    // Método para gestionar la muerte del enemigo
    public override void Die()
    {
        if (isDead) return;

        isDead = true;
        agent.ResetPath();
        agent.isStopped = true;

        animator.SetBool("Dead", true);  // Activa la animación de muerte
        animator.SetBool("isRunning", false);
        animator.SetBool("isResting", false);

        // Inicia la rutina para destruir al enemigo después de la animación
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
            Gizmos.color = HasLineOfSight() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }

        if (isFleeRoutineActive)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(lastFleePosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastFleePosition);
        }
    }
}
