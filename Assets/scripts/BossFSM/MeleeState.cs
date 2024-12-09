using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeState : BaseState
{
    public enum MeleeSubstate
    {
        SubstateSelection,
        BasicAttack,
        AreaAttack,
        Dash,
        Ultimate
    }

    private MeleeSubstate _currentSubstate = MeleeSubstate.SubstateSelection;
    private readonly List<MeleeSubstate> _substateHistory = new();
    private EnemyFSM _enemyFSMRef;
    private BossEnemy _owner;
    private bool _substateEntered = false;
    private NavMeshAgent agent;


    public AudioClip basicAttackSound;
    public AudioClip areaAttackSound;
    public AudioClip dashAttackSound;
    public AudioClip ultimateAttackSound;
    private AudioSource audioSource; // Referencia al AudioSource del enemigo

    public MeleeState()
    {
        Name = "Melee State";
    }

    public override void OnEnter()
    {

        InitializeReferences();
        audioSource = _owner.GetComponent<AudioSource>();


        _currentSubstate = MeleeSubstate.SubstateSelection; // Reiniciar subestado
        _substateEntered = false; // Reiniciar flag de entrada
        _substateHistory.Clear(); // Limpiar historial de subestados

        base.OnEnter();

        agent = _owner.GetComponent<NavMeshAgent>();
        Debug.Log("Entrando al estado MeleeState.");
    }

    private void InitializeReferences()
    {
        if (_enemyFSMRef == null)
            _enemyFSMRef = (EnemyFSM)FSMRef;

        if (_owner == null)
            _owner = (BossEnemy)_enemyFSMRef.Owner;

        Debug.Log("MeleeState: Referencias inicializadas.");
        
        if (_owner == null || _owner.Animator == null)
            Debug.LogError("MeleeState: _owner o su Animator no están asignados correctamente.");

        if (_owner == null)
        {
            Debug.LogError("MeleeState: _owner no está inicializado.");
        }
        else
        {
            Debug.Log("MeleeState: _owner inicializado correctamente.");
        }
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

        // Verificar si el jugador est  fuera del rango melee
        if (!_owner.IsPlayerInMeleeRange())
        {
            Debug.Log("Jugador fuera de rango melee. Cambiando a estado Ranged.");
            agent.isStopped = true;
            _enemyFSMRef.ChangeState(_enemyFSMRef.RangeState);
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(_owner.Player.position);
        HandleSubstateLogic();
    }

    public override void OnExit()
    {
        _currentSubstate = MeleeSubstate.SubstateSelection; // Reiniciar subestado
        _substateEntered = false; // Reiniciar flag de entrada
        _substateHistory.Clear(); // Limpiar historial de subestados

        base.OnExit();
        agent.isStopped = true;

        Debug.Log("Saliendo del estado RangeState. Subestado reiniciado a SubstateSelection.");
    }


    private void HandleSubstateLogic()
    {
        Debug.Log($"MeleeState: Subestado actual: {_currentSubstate}");
        switch (_currentSubstate)
        {
            case MeleeSubstate.SubstateSelection:
                SelectNextSubstate();
                break;

            case MeleeSubstate.BasicAttack:
                ExecuteSubstateOnce(() => StartCoroutine(PerformAttack(_owner.basicAttackCooldown, BasicAttack)));
                break;

            case MeleeSubstate.AreaAttack:
                ExecuteSubstateOnce(() => StartCoroutine(PerformAttack(_owner.areaAttackCooldown, AreaAttack)));
                break;

            case MeleeSubstate.Dash:
                ExecuteSubstateOnce(() => StartCoroutine(PerformAttack(_owner.dashAttackCooldown, DashAttack)));
                break;

            case MeleeSubstate.Ultimate:
                ExecuteSubstateOnce(() => StartCoroutine(PerformAttack(_owner.ultimateAttackCooldown, UltimateAttack)));
                break;
        }
    }



    private void ExecuteSubstateOnce(System.Action action)
    {
        if (_substateEntered) return;

        _substateEntered = true;
        Debug.Log("Ejecutando subestado por primera vez.");
        action.Invoke();
    }

    private IEnumerator PerformAttack(float cooldownTime, System.Func<IEnumerator> attackMethod)
    {
        Debug.Log("Iniciando ataque.");
        yield return StartCoroutine(attackMethod.Invoke());
        yield return StartCoroutine(Cooldown(cooldownTime));
    }

    private IEnumerator BasicAttack()
    {
        Debug.Log("MeleeState: BasicAttack ejecutado.");
        PlaySound(basicAttackSound);
        _owner.ExecuteBasicAttack();

        _owner.Animator.SetTrigger("BasicAttack");
        yield return new WaitForSeconds(_owner.basicAttackCooldown);

        // Activar la animación del ataque básico
        _owner.Animator.SetTrigger("BasicAttackTrigger");

        // Sincronizar la aplicación de daño con la animación
        yield return new WaitForSeconds(0.19f); // Ajusta este valor al tiempo del impacto en la animación

        _owner.DoSphereDamage(_owner.transform.position, _owner.meleeRange, _owner.basicAttackDamage, _owner.playerLayer);


        // Cooldown del ataque
        yield return new WaitForSeconds(_owner.basicAttackCooldown);
    TransitionToSelectionState(); // Volver al subestado de selección
    }

    private IEnumerator AreaAttack()
    {
    Debug.Log("MeleeState: Ejecutando ataque de área.");

        PlaySound(areaAttackSound);


        // Activar la animación del ataque de área
        _owner.Animator.SetTrigger("AreaAttackTrigger");

    // Sincronizar el daño con la animación
    yield return new WaitForSeconds(1.27f); // Ajusta este valor al momento del impacto en la animación

        // Detectar jugadores en el rango del ataque
        _owner.DoSphereDamage(_owner.transform.position, _owner.areaAttackRange, _owner.areaAttackDamage, _owner.playerLayer);


        // Cooldown antes de permitir otro ataque
        yield return new WaitForSeconds(_owner.areaAttackCooldown);
    TransitionToSelectionState(); // Cambiar al subestado de selección

    }



    public IEnumerator DashAttack()
    {
        Debug.Log("MeleeState: Ejecutando ataque de dash.");
        PlaySound(dashAttackSound);

        // Activar la animación del ataque de dash
        _owner.Animator.SetTrigger("DashAttackTrigger");

        // Sincronizar con la animación (puedes ajustar este valor)
        yield return new WaitForSeconds(0.16f);

        // Realizar el movimiento rápido hacia el jugador
        Vector3 dashDirection = (_owner.Player.position - _owner.transform.position).normalized;
        float dashDistance = _owner.dashDistance; // Obtener del BossEnemy
        float dashSpeed = _owner.dashSpeed;       // Obtener del BossEnemy

        float traveledDistance = 0f;
        while (traveledDistance < dashDistance)
    {
        Vector3 dashStep = dashDirection * dashSpeed * Time.deltaTime;
        _owner.transform.position += dashStep;
        traveledDistance += dashStep.magnitude;

        yield return null; // Esperar al siguiente frame
    }

        _owner.DoSphereDamage(
        _owner.transform.position + _owner.transform.forward * 2,
        _owner.meleeRange,
        _owner.dashAttackDamage,
        _owner.playerLayer
    );

        // Cooldown antes de permitir otro ataque
        yield return new WaitForSeconds(_owner.dashAttackCooldown);
        TransitionToSelectionState(); // Cambiar al subestado de selección
    }

    public IEnumerator UltimateAttack()
    {
        Debug.Log("MeleeState: UltimateAttack ejecutado.");
        PlaySound(ultimateAttackSound);
        _owner.Animator.SetTrigger("UltimateAttackTrigger");

        // Sincronizar con la animación del ataque
        yield return new WaitForSeconds(2.5f);

        // Llamar a la función genérica para aplicar daño
        _owner.DoSphereDamage(_owner.transform.position, _owner.ultimateRange, _owner.ultimateAttackDamage, _owner.playerLayer);

        // Cooldown antes de volver al estado de selección
        yield return new WaitForSeconds(_owner.ultimateAttackCooldown);
        TransitionToSelectionState();
    }


    private IEnumerator Cooldown(float cooldownTime)
    {
        Debug.Log($"Iniciando cooldown por {cooldownTime} segundos.");
        yield return new WaitForSeconds(cooldownTime);
        TransitionToSelectionState();
    }



    private void TransitionToSelectionState()
    {
        Debug.Log("Transición al subestado de selección.");
        _substateHistory.Add(_currentSubstate);
        _substateEntered = false;
        _currentSubstate = MeleeSubstate.SubstateSelection;
    }


    private void SelectNextSubstate()
    {
        Debug.Log("Seleccionando siguiente subestado.");
        if (_owner.CanExecuteUltimateMeleeAttack())
        {
            Debug.Log("MeleeState: Cambiando a subestado Ultimate.");
            _currentSubstate = MeleeSubstate.Ultimate;
        }
        else if (_substateHistory.Count == 0 || _substateHistory[^1] == MeleeSubstate.Dash)
        {
            Debug.Log("MeleeState: Cambiando a subestado BasicAttack.");
            _currentSubstate = MeleeSubstate.BasicAttack;
        }
        else if (_substateHistory[^1] == MeleeSubstate.BasicAttack)
        {
            Debug.Log("MeleeState: Cambiando a subestado AreaAttack.");
            _currentSubstate = MeleeSubstate.AreaAttack;
        }
        else if (_substateHistory[^1] == MeleeSubstate.AreaAttack)
        {
            Debug.Log("MeleeState: Cambiando a subestado Dash.");
            _currentSubstate = MeleeSubstate.Dash;
        }
    }

}