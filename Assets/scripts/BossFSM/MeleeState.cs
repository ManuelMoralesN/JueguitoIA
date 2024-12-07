// MeleeState.cs - Mejoras y optimización
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

    public MeleeState()
    {
        Name = "Melee State";
    }

    public override void OnEnter()
    {
        base.OnEnter();
        InitializeReferences();
        Debug.Log("Entrando al estado MeleeState.");
    }

    private void InitializeReferences()
    {
        if (_enemyFSMRef == null)
            _enemyFSMRef = (EnemyFSM)FSMRef;

        if (_owner == null)
            _owner = (BossEnemy)_enemyFSMRef.Owner;

        Debug.Log("MeleeState: Referencias inicializadas.");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Verificar si el jugador está fuera del rango melee
        if (!_owner.IsPlayerInMeleeRange())
        {
            Debug.Log("Jugador fuera de rango melee. Cambiando a estado Ranged.");
            _enemyFSMRef.ChangeState(_enemyFSMRef.RangeState);
            return;
        }

        HandleSubstateLogic();
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
        Debug.Log("Ejecutando ataque básico.");
        _owner.ExecuteBasicAttack();
        yield break;
    }

    private IEnumerator AreaAttack()
    {
        Debug.Log("Ejecutando ataque de área.");
        _owner.ExecuteAreaAttack();
        yield break;
    }

    private IEnumerator DashAttack()
    {
        Debug.Log("Ejecutando ataque de dash.");
        _owner.ExecuteDashAttack();
        yield break;
    }

    private IEnumerator UltimateAttack()
    {
        Debug.Log("Ejecutando ataque ultimate melee.");
        _owner.ExecuteUltimateAttack(false);
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