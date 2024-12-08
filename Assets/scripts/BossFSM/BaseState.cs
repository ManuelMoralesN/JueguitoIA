// BaseState.cs - Mejoras y protecciones
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState : MonoBehaviour
{
    // Nombre del estado para identificaci n
    public string Name { get; protected set; } = "BaseState";

    // Referencia a la FSM que contiene este estado
    protected BaseFSM FSMRef;

    // Constructor por defecto
    public BaseState() { }

    // Constructor con par metros
    public BaseState(string name, BaseFSM fsmRef)
    {
        Name = name;
        FSMRef = fsmRef;
    }

    // M todo para inicializar el estado
    public virtual void InitializeState(BaseFSM fsmRef)
    {
        FSMRef = fsmRef;
    }

    // Llamado al entrar al estado
    public virtual void OnEnter()
    {
        Debug.Log($"[BaseState] Entrando al estado: {Name}");
    }

    // Llamado en cada frame mientras el estado est  activo
    public virtual void OnUpdate()
    {
        Debug.Log($"[BaseState] Actualizando estado: {Name}");
    }

    // Llamado al salir del estado
    public virtual void OnExit()
    {
        Debug.Log($"[BaseState] Saliendo del estado: {Name}");
    }
}