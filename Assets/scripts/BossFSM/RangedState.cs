using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeState : BaseState
{
    public enum RangeSubstate
    {
        SubstateSelection,
        BasicAttack,
        AreaAttack,
        Dash,
        Ultimate
    }

    private RangeSubstate _currentSubstate = RangeSubstate.SubstateSelection;
    private readonly List<RangeSubstate> _substateHistory = new();
    private EnemyFSM _enemyFSMRef;
    private BossEnemy _owner;
    private bool _substateEntered = false;
    private UnityEngine.AI.NavMeshAgent agent;

    public AudioClip basicAttackSound;
    public AudioClip areaAttackSound;
    public AudioClip dashAttackSound;
    public AudioClip ultimateAttackSound;
    private AudioSource audioSource; // Referencia al AudioSource del enemigo


    public RangeState()
    {
        Name = "Range State";
    }

    public override void OnEnter()
    {
        InitializeReferences();

        _currentSubstate = RangeSubstate.SubstateSelection; // Reiniciar subestado
        _substateEntered = false; // Reiniciar flag de entrada
        _substateHistory.Clear(); // Limpiar historial de subestados

        Debug.Log("Entrando al estado MeleeState. Subestado reiniciado a SubstateSelection.");

        base.OnEnter();

        agent = _owner.GetComponent<UnityEngine.AI.NavMeshAgent>();
        Debug.Log("Entrando al estado MeleeState.");
    }

    private void InitializeReferences()
    {
        if (_enemyFSMRef == null)
            _enemyFSMRef = (EnemyFSM)FSMRef;

        if (_owner == null)
            _owner = (BossEnemy)_enemyFSMRef.Owner;

        Debug.Log($"RangeState: Referencias inicializadas. Owner: {_owner}, FSMRef: {_enemyFSMRef}");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip); // Reproduce el sonido sin interrumpir otros
        }
    }

    public override void OnUpdate()
    {
         base.OnUpdate();

    if (_owner.IsPlayerInMeleeRange())
    {
        Debug.Log("Jugador dentro de rango melee. Cambiando a estado Melee.");
        agent.isStopped = true;
        _enemyFSMRef.ChangeState(_enemyFSMRef.MeleeState);
        return;
    }

    // Mantener distancia del jugador
    if (Vector3.Distance(_owner.transform.position, _owner.Player.position) > _owner.rangedRange)
    {
        agent.isStopped = false;
        agent.SetDestination(_owner.Player.position);
    }
    else
    {
        agent.isStopped = true;
    }

    HandleSubstateLogic();
    }

    public override void OnExit()
    {
        base.OnExit();
        agent.isStopped = true;
        Debug.Log("Saliendo del estado RangeState.");
    }

    private void HandleSubstateLogic()
    {
        Debug.Log($"RangeState: Subestado actual: {_currentSubstate}");

        switch (_currentSubstate)
        {
            case RangeSubstate.SubstateSelection:
                SelectNextSubstate();
                break;

            case RangeSubstate.BasicAttack:
                ExecuteSubstateOnce(() => StartCoroutine(PerformAttack(_owner.basicAttackCooldown, BasicAttack)));
                break;

            case RangeSubstate.AreaAttack:
                ExecuteSubstateOnce(() => StartCoroutine(PerformAttack(_owner.areaAttackCooldown, AreaAttack)));
                break;

            case RangeSubstate.Dash:
                ExecuteSubstateOnce(() => StartCoroutine(PerformAttack(_owner.dashAttackCooldown, DashAttack)));
                break;

            case RangeSubstate.Ultimate:
                ExecuteSubstateOnce(() => StartCoroutine(PerformAttack(_owner.ultimateAttackCooldown, UltimateAttack)));
                break;
        }
    }


    private void ExecuteSubstateOnce(System.Action action)
    {
        if (_substateEntered) return;

        _substateEntered = true;
        action.Invoke();
    }

    private IEnumerator PerformAttack(float cooldownTime, System.Func<IEnumerator> attackMethod)
    {
        yield return StartCoroutine(attackMethod.Invoke());
        yield return StartCoroutine(Cooldown(cooldownTime));
    }

    private IEnumerator BasicAttack()
    {
            Debug.Log("RangedState: Ejecutando ataque básico aéreo.");

        PlaySound(basicAttackSound);


        // Activar la animación del ataque
        _owner.Animator.SetTrigger("RangedAttackTrigger");

    // Sincronizar con la animación (ajusta el tiempo al momento del disparo)
    yield return new WaitForSeconds(0.5f);

    // Calcular la dirección hacia el jugador
    Vector3 directionToPlayer = (_owner.Player.position - _owner.projectileSpawnPoint.position).normalized;

    // Generar el proyectil
    GameObject projectile = GameObject.Instantiate(
        _owner.projectilePrefab,
        _owner.projectileSpawnPoint.position,  // Posición del origen del proyectil
        Quaternion.identity  // Rotación inicial
    );

    // Configurar el proyectil
    EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
    if (projScript != null)
    {
        projScript.Initialize(directionToPlayer); // Pasar dirección al proyectil
        projScript.playerLayer = LayerMask.GetMask("Player"); // Asignar capa del jugador
    }
    else
    {
        Debug.LogError("El prefab del proyectil no tiene el componente EnemyProjectile.");
    }


        _owner.ExecuteBasicAttack();
        yield break;
    }

    private IEnumerator AreaAttack()
    {
         Debug.Log("RangedState: Ejecutando ataque aéreo de área con múltiples puntos de disparo.");
        PlaySound(areaAttackSound);

    // Activar la animación del ataque
    _owner.Animator.SetTrigger("AreaAttackTriggerA");

    // Sincronizar con la animación
    yield return new WaitForSeconds(1.0f);

    // Lista de puntos de disparo
    Transform[] firePoints = _owner.projectileFirePoints; // Array de puntos de disparo en el BossEnemy

    // Generar un proyectil por cada punto de disparo
    foreach (Transform firePoint in firePoints)
    {
        // Instanciar el proyectil
        GameObject projectile = GameObject.Instantiate(
            _owner.projectilePrefab,
            firePoint.position,      // Posición del punto de disparo
            firePoint.rotation       // Rotación del punto de disparo
        );

        // Configurar el proyectil
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(firePoint.forward); // Usar la dirección del punto de disparo
            projScript.playerLayer = LayerMask.GetMask("Player"); // Asignar capa del jugador
        }
        else
        {
            Debug.LogError("El prefab del proyectil no tiene el componente EnemyProjectile.");
        }
    }

        _owner.ExecuteAreaAttack();
        yield break;
    }

    private IEnumerator DashAttack()
    {
        Debug.Log("Ejecutando ataque combinado: Dash + Ráfaga de proyectiles con una animación.");
        PlaySound(dashAttackSound);

    // Activar la animación del ataque
    _owner.Animator.SetTrigger("DashProjectileTrigger");

    // Dirección del dash
    Vector3 directionToPlayer = (_owner.Player.position - _owner.transform.position).normalized;

    // Distancia y velocidad del dash
    float dashDistance = 10f;
    float dashSpeed = 20f;

    // Punto final del dash
    Vector3 dashTarget = _owner.transform.position + directionToPlayer * dashDistance;

    // Evitar atravesar obstáculos
    if (Physics.Raycast(_owner.transform.position, directionToPlayer, out RaycastHit hit, dashDistance))
    {
        dashTarget = hit.point;
    }

    // Movimiento del dash
    float elapsedTime = 0f;
    float dashDuration = dashDistance / dashSpeed;
    Vector3 startPosition = _owner.transform.position;

    while (elapsedTime < dashDuration)
    {
        elapsedTime += Time.deltaTime;
        _owner.transform.position = Vector3.Lerp(startPosition, dashTarget, elapsedTime / dashDuration);

        yield return null;
    }

    // Pausa breve después del Dash para sincronizar con los disparos
    yield return new WaitForSeconds(0.20f);

    // Disparar la ráfaga de proyectiles
    int projectilesToShoot = 3;
    float timeBetweenShots = 0.2f;

    for (int i = 0; i < projectilesToShoot; i++)
    {
        // Calcular dirección hacia el jugador
        Vector3 directionToFire = (_owner.Player.position - _owner.projectileSpawnPoint.position).normalized;

        // Instanciar el proyectil
        GameObject projectile = GameObject.Instantiate(
            _owner.projectilePrefab,
            _owner.projectileSpawnPoint.position,
            Quaternion.identity
        );

        // Configurar el proyectil
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(directionToFire); // Configurar dirección del proyectil
            projScript.playerLayer = LayerMask.GetMask("Player"); // Asignar capa del jugador
        }
        else
        {
            Debug.LogError("El prefab del proyectil no tiene el componente EnemyProjectile.");
        }

        // Esperar antes de disparar el siguiente proyectil
        yield return new WaitForSeconds(timeBetweenShots);
    }


        _owner.ExecuteDashAttack();
        yield break;
    }

    private IEnumerator UltimateAttack()
    {
        Debug.Log("RangedState: Ejecutando ataque ultimate de bolas de fuego.");
        PlaySound(ultimateAttackSound);

    // Activar la animación del ataque ultimate
    _owner.Animator.SetTrigger("UltimateAttackTrigger");

    // Tiempo total del ataque
    float attackDuration = 1f;
    float timeBetweenProjectiles = 0.1f; // Intervalo entre cada proyectil
    float elapsedTime = 0f;

    // Número de direcciones (mayor número = más densidad de proyectiles)
    int numDirections = 12; // Cambia según la cantidad de proyectiles deseada por ciclo

    while (elapsedTime < attackDuration)
    {
        elapsedTime += timeBetweenProjectiles;

        // Generar proyectiles en todas las direcciones
        for (int i = 0; i < numDirections; i++)
        {
            float angle = (360f / numDirections) * i; // Ángulo entre cada proyectil
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            // Instanciar el proyectil
            GameObject projectile = GameObject.Instantiate(
                _owner.projectilePrefab,
                _owner.projectileSpawnPoint.position,
                Quaternion.identity
            );

            // Configurar el proyectil
            EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
            if (projScript != null)
            {
                projScript.Initialize(direction); // Configurar dirección del proyectil
                projScript.playerLayer = LayerMask.GetMask("Player"); // Asignar capa del jugador
            }
            else
            {
                Debug.LogError("El prefab del proyectil no tiene el componente EnemyProjectile.");
            }
        }

        // Esperar antes de lanzar la siguiente ráfaga
        yield return new WaitForSeconds(timeBetweenProjectiles);
    }
        _owner.ExecuteUltimateAttack(true);
        yield break;
    }

    private IEnumerator Cooldown(float cooldownTime)
    {
        Debug.Log($"Iniciando cooldown por {cooldownTime} segundos.");
        yield return new WaitForSeconds(cooldownTime);
        TransitionToSelectionState();
    }


    private void TransitionToSelectionState()
    {
        Debug.Log($"RangeState: Transici n al subestado {RangeSubstate.SubstateSelection}");
        _substateHistory.Add(_currentSubstate);
        _substateEntered = false;
        _currentSubstate = RangeSubstate.SubstateSelection;
    }


    private void SelectNextSubstate()
    {
        Debug.Log("RangeState: Selecci n de siguiente subestado.");

        if (_owner.CanExecuteUltimateRangedAttack())
        {
            Debug.Log("RangeState: Cambiando a Ultimate.");
            _currentSubstate = RangeSubstate.Ultimate;
        }
        else if (_substateHistory.Count == 0 || _substateHistory[^1] == RangeSubstate.Dash)
        {
            Debug.Log("RangeState: Cambiando a BasicAttack.");
            _currentSubstate = RangeSubstate.BasicAttack;
        }
        else if (_substateHistory[^1] == RangeSubstate.BasicAttack)
        {
            Debug.Log("RangeState: Cambiando a AreaAttack.");
            _currentSubstate = RangeSubstate.AreaAttack;
        }
        else if (_substateHistory[^1] == RangeSubstate.AreaAttack)
        {
            Debug.Log("RangeState: Cambiando a Dash.");
            _currentSubstate = RangeSubstate.Dash;
        }
    }

}