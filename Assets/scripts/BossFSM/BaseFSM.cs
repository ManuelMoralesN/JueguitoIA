// BaseFSM.cs - Mejoras y protecciones
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFSM : MonoBehaviour
{
    // Referencia al estado actual
    private BaseState currentState;

    // Propiedad p blica para acceder al estado actual
    public BaseState CurrentState
    {
        get { return currentState; }
    }

    // M todo Start inicializa la FSM
    public virtual void Start()
    {
        // Obtener el estado inicial definido por clases derivadas
        currentState = GetInitialState();

        if (currentState == null)
        {
            Debug.LogWarning("FSM: El estado inicial no es v lido.");
            return;
        }

        // Llamar al m todo OnEnter del estado inicial
        currentState.OnEnter();
    }

    // M todo Update llama al m todo OnUpdate del estado actual
    void Update()
    {
        if (currentState != null)
        {
            currentState.OnUpdate();
        }
    }

    // Cambia al nuevo estado especificado
    public void ChangeState(BaseState newState)
    {
        if (newState == null)
        {
            Debug.LogWarning("FSM: No se puede cambiar a un estado nulo.");
            return;
        }

        if (currentState != null)
        {
            Debug.Log($"FSM: Saliendo del estado {currentState.Name}");
            currentState.OnExit();
        }

        currentState = newState;

        Debug.Log($"FSM: Entrando al estado {currentState.Name}");
        currentState.OnEnter();
    }



    // M todo virtual para definir el estado inicial (debe ser sobrescrito)
    public virtual BaseState GetInitialState()
    {
        Debug.LogError("FSM: GetInitialState no ha sido sobrescrito en la clase derivada.");
        return null;
    }
}
