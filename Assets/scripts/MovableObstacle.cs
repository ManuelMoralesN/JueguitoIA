using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MovableObstacle : MonoBehaviour
{
    public enum MovementType { Rotate, Linear }
    public MovementType movementType = MovementType.Rotate;

    public float interval = 2.0f; // Tiempo entre movimientos
    public float moveDuration = 1.0f; // Duración del movimiento suave
    public float rotationAngle = 90.0f; // Ángulo de rotación
    public float moveDistance = 5.0f; // Distancia de movimiento lineal
    public float blinkDuration = 0.5f; // Duración total del parpadeo antes del movimiento
    public int blinkCount = 3; // Número de veces que parpadea antes de moverse

    private NavMeshObstacle navMeshObstacle;
    private bool movingForward = true;
    private bool isMoving = false;
    private Renderer objectRenderer;
    private Color originalColor;

    private void Start()
    {
        // Obtén el componente NavMeshObstacle del objeto
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        if (navMeshObstacle == null)
        {
            Debug.LogError("No se encontró un NavMeshObstacle en el objeto. Por favor, asegúrate de agregar uno.");
            return;
        }

        // Activa la opción Carve para que el obstáculo actualice el NavMesh al moverse
        navMeshObstacle.carving = true;

        // Obtén el Renderer para cambiar el color
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }

        // Inicia la rutina de movimiento
        StartCoroutine(MoveObstacle());
    }

    private IEnumerator MoveObstacle()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (!isMoving)
            {
                // Inicia el parpadeo antes de moverse
                yield return StartCoroutine(BlinkBeforeMove());

                if (movementType == MovementType.Rotate)
                {
                    // Realiza la rotación suave
                    StartCoroutine(RotateOverTime(rotationAngle, moveDuration));
                }
                else if (movementType == MovementType.Linear)
                {
                    // Realiza el movimiento lineal suave
                    Vector3 direction = movingForward ? Vector3.forward : -Vector3.forward;
                    Vector3 targetPosition = transform.position + direction * moveDistance;
                    StartCoroutine(MoveOverTime(targetPosition, moveDuration));
                    movingForward = !movingForward; // Cambia la dirección para el próximo movimiento
                }
            }
        }
    }

    private IEnumerator BlinkBeforeMove()
    {
        if (objectRenderer == null)
            yield break;

        Color blinkColor = Color.red; // Color de parpadeo, puedes cambiarlo
        float blinkInterval = blinkDuration / (blinkCount * 2); // Tiempo entre cada cambio de color

        for (int i = 0; i < blinkCount; i++)
        {
            objectRenderer.material.color = blinkColor;
            yield return new WaitForSeconds(blinkInterval);
            objectRenderer.material.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private IEnumerator RotateOverTime(float angle, float duration)
    {
        isMoving = true;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);
        float elapsed = 0;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isMoving = false;
    }

    private IEnumerator MoveOverTime(Vector3 targetPosition, float duration)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
