// RangeState.cs - Mejoras y optimización
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

    public RangeState()
    {
        Name = "Range State";
    }

    public override void OnEnter()
    {
        base.OnEnter();
        InitializeReferences();
    }

    private void InitializeReferences()
    {
        if (_enemyFSMRef == null)
            _enemyFSMRef = (EnemyFSM)FSMRef;

        if (_owner == null)
            _owner = (BossEnemy)_enemyFSMRef.Owner;

        Debug.Log($"RangeState: Referencias inicializadas. Owner: {_owner}, FSMRef: {_enemyFSMRef}");
    }


    public override void OnUpdate()
    {
        base.OnUpdate();

        HandleSubstateLogic();
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
        Debug.Log("Ejecutando ataque básico a distancia.");
        _owner.ExecuteBasicAttack();
        yield break;
    }

    private IEnumerator AreaAttack()
    {
        Debug.Log("Ejecutando ataque de área a distancia.");
        _owner.ExecuteAreaAttack();
        yield break;
    }

    private IEnumerator DashAttack()
    {
        Debug.Log("Ejecutando ataque de dash a distancia.");
        _owner.ExecuteDashAttack();
        yield break;
    }

    private IEnumerator UltimateAttack()
    {
        Debug.Log("Ejecutando ataque ultimate a distancia.");
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
        Debug.Log($"RangeState: Transición al subestado {RangeSubstate.SubstateSelection}");
        _substateHistory.Add(_currentSubstate);
        _substateEntered = false;
        _currentSubstate = RangeSubstate.SubstateSelection;
    }


    private void SelectNextSubstate()
    {
        Debug.Log("RangeState: Selección de siguiente subestado.");

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