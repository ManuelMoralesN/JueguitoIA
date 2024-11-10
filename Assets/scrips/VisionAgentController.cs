using UnityEngine;
using UnityEngine.AI;

public class VisionAgentController : MonoBehaviour
{
    public Transform target;
    public float viewAngle = 45f;
    public float viewDistance = 10f;
    public float chaseTime = 3f;

    private NavMeshAgent agent;
    private bool isChasing = false;
    private float chaseTimer = 0f;
    private bool playerDetected = false;
    private Vector3 initialPosition; // Guarda la posición inicial del agente

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position; // Asigna la posición inicial

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent no encontrado en " + gameObject.name);
        }
    }

    private void Update()
    {
        if (isChasing)
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTime)
            {
                isChasing = false;
                chaseTimer = 0f;
                playerDetected = false;
                
                Debug.Log("Persecución terminada. Regresando a posición inicial.");

                // Regresa a la posición inicial
                if (agent != null)
                {
                    agent.SetDestination(initialPosition);
                }
            }
        }
        else
        {
            DetectTarget();
        }
    }

    private void DetectTarget()
    {
        Vector3 directionToTarget = target.position - transform.position;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        if (angleToTarget < viewAngle / 2 && directionToTarget.magnitude <= viewDistance)
        {
            Ray ray = new Ray(transform.position, directionToTarget);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, viewDistance))
            {
                if (hit.transform == target)
                {
                    StartChase();
                    playerDetected = true;
                }
            }
        }
    }

    private void StartChase()
    {
        if (agent != null)
        {
            isChasing = true;
            agent.SetDestination(target.position);
            Debug.Log("Comenzando persecución hacia el objetivo.");
        }
        else
        {
            Debug.LogWarning("NavMeshAgent no asignado en " + gameObject.name);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = playerDetected ? Color.red : Color.green;

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;

        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);

        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}
