// EnemyFSM.cs - Mejoras y optimización
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : BaseFSM
{
    private MeleeState _meleeState;
    private RangeState _rangeState;

    public MeleeState MeleeState => _meleeState;
    public RangeState RangeState => _rangeState;

    [Header("Contexto del Enemigo")]
    public BossEnemy Owner;

    // Inicialización de la FSM
    public override void Start()
    {
        // Crear y asignar estados
        _meleeState = CreateState<MeleeState>("MeleeState");
        _rangeState = CreateState<RangeState>("RangeState");

        base.Start(); // Llamar al Start del padre
    }

    private T CreateState<T>(string stateName) where T : BaseState
    {
        T state = gameObject.AddComponent<T>();
        state.InitializeState(this);
        Debug.Log($"FSM: Estado '{stateName}' inicializado.");
        return state;
    }

    public override BaseState GetInitialState()
    {
        // Retornar el estado inicial
        return _meleeState;
    }
}