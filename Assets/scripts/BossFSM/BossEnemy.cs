using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    [Header("Rangos")]
    public float meleeRange = 5f;
    public float rangedRange = 10f;

    [Header("Cooldowns de Ataque")]
    public float basicAttackCooldown = 1f;
    public float areaAttackCooldown = 3f;
    public float dashAttackCooldown = 2f;
    public float ultimateAttackCooldown = 10f;

    [Header("Sonidos del Jefe")]
    public AudioClip basicAttackSound; // Sonido del ataque básico
    public AudioClip areaAttackSound; // Sonido del ataque de área
    public AudioClip dashAttackSound; // Sonido del ataque de dash
    public AudioClip ultimateAttackSound; // Sonido del ataque ultimate

    [Header("NavMesh Settings")]
    private UnityEngine.AI.NavMeshAgent agent; // Agente de movimiento

    [Header("Movimiento")]
    public float movementSpeed = 3.5f;
    public float stoppingDistance = 2f; // Distancia a la cual se detiene cerca del jugador

    [Header("Daño del Ataque Básico")]
    public float basicAttackDamage = 10f; // Cantidad de daño infligido por el ataque básico

    [Header("Ataque de Área")]
    public float areaAttackDamage = 20f;      // Daño del ataque de área
    public float areaAttackRange = 5f;        // Rango del ataque de área

    [Header("Ataque de Dash")]
    public float dashAttackDamage = 15f;  // Daño infligido por el dash
    public float dashDistance = 5f; // Distancia que recorrerá el dash
    public float dashSpeed = 10f;   // Velocidad del movimiento

    [Header("Ultimate Attack")]
    public float ultimateAttackDamage = 50f;   // Daño infligido por el ultimate
    public float ultimateRange = 10f;
    
    [Header("Ataque a Distancia")]
    public GameObject projectilePrefab;   // Prefab del proyectil
    public Transform projectileSpawnPoint; // Punto de origen para los proyectiles

    [Header("Ataque de Área Aéreo")]
    public GameObject projectilePrefab1;         // Prefab del proyectil
    public Transform[] projectileFirePoints;      // Punto de origen de los proyectiles

    [Header("Ataque Combinado: Dash + Ráfaga")]
    public GameObject projectilePrefab2;          // Prefab del proyectil
    public Transform projectileSpawnPoint2;       // Punto de disparo de los proyectiles
    public float timeBetweenProjectiles = 0.2f;  // Tiempo entre cada proyectil en la ráfaga
    public float rangedDashDistance = 10f; // Distancia del dash en estado rango
    public float rangedDashSpeed = 15f;   // Velocidad del dash en estado rango

    [Header("Ataque Ultimate Aereo")]
    public GameObject projectilePrefab3;          // Prefab del proyectil
    public Transform projectileSpawnPoint3;       // Punto de disparo

    private float nextAttackTime;
    private List<string> recentSpecialAttacks = new List<string>();

    private EnemyFSM fsm;
    private Transform player;
    private bool isGameStarted = false;
    private bool ultimateUsed = false;
    public Transform Player => player;
    private UnityEngine.AI.NavMeshAgent navMeshAgent; // Referencia al NavMeshAgent
    private Animator animator;
    public Animator Animator => animator; // Propiedad para acceder al Animator
    public UnityEngine.AI.NavMeshAgent NavMeshAgent => navMeshAgent; // Propiedad para acceso desde estados

    public override void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); // Obtener NavMeshAgent
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (agent == null)
        {
            Debug.LogError("BossEnemy requiere un NavMeshAgent.");
        }
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("BossEnemy: No se encontró Animator.");

        agent.speed = movementSpeed;
        agent.stoppingDistance = stoppingDistance;

        base.Awake();
        fsm = GetComponent<EnemyFSM>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(StartDelay());
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(3f); // Retraso para evitar ejecuciones prematuras
        isGameStarted = true;
    }

    void Update()
    {
        if (!isGameStarted || player == null) return;

        HandleStateTransitions();
        FollowPlayer();
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        // Determina si el enemigo está en movimiento
        bool isRunning = agent.velocity.sqrMagnitude > 0.1f;
        animator.SetBool("IsRunning", isRunning);
    }


    private void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Girar hacia el jugador
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0; // Evitar inclinación

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Suavizar el giro
        }

        // Mover hacia el jugador si está fuera del rango de parada
        if (distanceToPlayer > stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void HandleStateTransitions()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isInMeleeRange = distanceToPlayer <= meleeRange;
        bool isInRangedRange = distanceToPlayer > meleeRange && distanceToPlayer <= rangedRange;

        if (isInMeleeRange && !(fsm.CurrentState is MeleeState))
        {
            fsm.ChangeState(fsm.MeleeState);
        }

        else if (isInRangedRange && !(fsm.CurrentState is RangeState))
        {
            fsm.ChangeState(fsm.RangeState);
        }
    }

    public void DoSphereDamage(Vector3 spherePosition, float sphereRadius, float attackDamage, LayerMask damageLayer)
    {
        // Detectar colisiones en el rango del ataque
        Collider[] playerColliders = Physics.OverlapSphere(spherePosition, sphereRadius, damageLayer);

        foreach (Collider playerCollider in playerColliders)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($"Jugador impactado por ataque. Daño: {attackDamage}");
                playerHealth.TakeDamage((int)attackDamage); // Aplicar daño
            }
            else
            {
                Debug.LogWarning("El Collider detectado no tiene el componente PlayerHealth.");
            }
        }
    }

    //public void ExecuteNextAttack()
    //{
    //    if (Time.time < nextAttackTime) return;

    //    if (!ultimateUsed && CanExecuteUltimateMeleeAttack())
    //    {
    //        ExecuteUltimateMeleeAttack();
    //    }
    //    else if (!ultimateUsed && CanExecuteUltimateRangedAttack())
    //    {
    //        ExecuteUltimateRangedAttack();
    //    }
    //    else
    //    {
    //        ExecuteAttack(DetermineNextAttack());
    //    }
    //}

    //private string DetermineNextAttack()
    //{
    //    if (recentSpecialAttacks.Count == 0 || recentSpecialAttacks[^1] == "Dash")
    //    {
    //        return "Basic";
    //    }
    //    else if (recentSpecialAttacks[^1] == "Basic")
    //    {
    //        return "Area";
    //    }
    //    return "Dash";
    //}

    //public void ExecuteAttack(string attackType)
    //{
    //    switch (attackType)
    //    {
    //        case "Basic":
    //            StartCoroutine(ExecuteBasicAttack());
    //            break;
    //        case "Area":
    //            StartCoroutine(ExecuteAreaAttack());
    //            break;
    //        case "Dash":
    //            StartCoroutine(ExecuteDashAttack());
    //            break;
    //    }
    //}

    public IEnumerator ExecuteBasicAttack()
    {
        Debug.Log("Ejecutando ataque básico.");
        PlaySound(basicAttackSound); // Reproducir sonido de ataque básico
        yield return new WaitForSeconds(basicAttackCooldown);
        nextAttackTime = Time.time + basicAttackCooldown;
    }

    public IEnumerator ExecuteAreaAttack()
    {
        Debug.Log("Ejecutando ataque de área.");
        PlaySound(areaAttackSound); // Reproducir sonido de ataque de área
        yield return new WaitForSeconds(areaAttackCooldown);
        recentSpecialAttacks.Add("Area");
        nextAttackTime = Time.time + areaAttackCooldown;
    }

    public IEnumerator ExecuteDashAttack()
    {
        Debug.Log("Ejecutando ataque de dash.");
        PlaySound(dashAttackSound); // Reproducir sonido de ataque de dash
        yield return new WaitForSeconds(dashAttackCooldown);
        recentSpecialAttacks.Add("Dash");
        nextAttackTime = Time.time + dashAttackCooldown;
    }

    //public override void ExecuteUltimateAttack(bool isRanged)
    //{
    //    Debug.Log(isRanged ? "Ejecutando ataque ultimate a distancia." : "Ejecutando ataque ultimate melee.");
    //    PlaySound(ultimateAttackSound); // Reproducir sonido de ataque ultimate
    //    nextAttackTime = Time.time + ultimateAttackCooldown;
    //    recentSpecialAttacks.Clear();
    //    ultimateUsed = true;
    //}


    public void ExecuteUltimateMeleeAttack()
    {
        Debug.Log("Ejecutando ataque ultimate melee.");
        ExecuteUltimateAttack(false);
    }

    public void ExecuteUltimateRangedAttack()
    {
        Debug.Log("Ejecutando ataque ultimate a distancia.");
        ExecuteUltimateAttack(true);
    }

    public bool CanExecuteUltimateMeleeAttack()
    {
        return isGameStarted && !ultimateUsed && health < 50 && recentSpecialAttacks.Count >= 2 &&
               (recentSpecialAttacks[^1] == "Dash" || recentSpecialAttacks[^1] == "Area");
    }

    public bool CanExecuteUltimateRangedAttack()
    {
        return isGameStarted && !ultimateUsed && health < 50 && recentSpecialAttacks.Count >= 3;
    }

    public bool IsPlayerInMeleeRange()
    {
        return Vector3.Distance(transform.position, player.position) <= meleeRange;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }

}
