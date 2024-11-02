using UnityEngine;
using UnityEngine.AI;

public class MovingAgentController : MonoBehaviour
{
    [SerializeField] private Transform movePositionTransform;
    private 
    NavMeshAgent anvMeshAgent;

    private 
    NavMeshAgent navMeshAgent;

    private 
    void 
    Awake(){
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private 
    void 
    Update(){
            navMeshAgent.destination=movePositionTransform.position;
    }
}