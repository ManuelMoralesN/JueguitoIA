using System.Collections;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public int health = 100;  // Vida del enemigo
    public int damageToPlayer = 10;  // Daño que hace al jugador al tocarlo
    public LayerMask playerLayer;  // Capa asignada al jugador
    protected UIManager uiManager;  // Referencia al UIManager

    [Header("Parpadeo al recibir daño")]
    public Renderer enemyRenderer;          // Renderer del enemigo
    public Color damageColor = Color.red;   // Color de parpadeo al recibir daño
    public float flashDuration = 0.1f;      // Duración del parpadeo en segundos

    [Header("Audio Settings")]
    public AudioClip damageSound; // Sonido al recibir daño
    public AudioSource audioSource;       // Referencia al AudioSource

    [Header("Animación")]
    public Animator animator;       // Referencia al Animator del enemigo
    public string deathAnimation = "Death"; // Nombre de la animación de muerte


    private Color originalColor;            // Color original del material

    public virtual void Awake()
    {
    }

    void Start()
        {
        // Buscar automáticamente el Animator si no está asignado
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("BaseEnemy: No se encontró un Animator en el enemigo.");
        }
            else
        {
            Debug.Log("BaseEnemy: Animator asignado automáticamente.");
        }

        // Asignar automáticamente el AudioSource si no está configurado
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Verificar si el AudioSource está configurado correctamente
        if (audioSource == null)
        {
            Debug.LogError("BaseEnemy: No se encontró un AudioSource en el enemigo.");
        }
        uiManager = FindObjectOfType<UIManager>();  // Obtener la referencia al UIManager

        // Guardar el color original del material
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
        else
        {
            Debug.LogError("BaseEnemy: El Renderer del enemigo no está asignado.");
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip); // Reproduce el sonido sin interrumpir otros
        }
    }

    // Método para recibir daño
    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        PlaySound(damageSound); // Sonido al recibir daño

        Debug.Log("Enemigo ha recibido " + damage + " puntos de daño. Vida restante: " + health);

        if (health <= 0)
        {
            Die();  // Llamar al método de muerte solo si la vida llega a 0
        }
        else
        {
            StartCoroutine(FlashRed()); // Iniciar parpadeo rojo
        }
    }

    // Método para manejar el parpadeo en rojo
    private IEnumerator FlashRed()
    {
        if (enemyRenderer != null)
        {
            // Cambiar al color de daño
            enemyRenderer.material.color = damageColor;

            // Esperar la duración del parpadeo
            yield return new WaitForSeconds(flashDuration);

            // Restaurar el color original
            enemyRenderer.material.color = originalColor;
        }
    }

    public virtual void Die()
{
    Debug.Log("El enemigo ha muerto.");

    // Detener interacciones
    DisableEnemyBehaviors();

    // Reproducir la animación de muerte
    if (animator != null)
    {
        animator.SetTrigger("Death");
        StartCoroutine(HandleDeathAnimation());
    }
    else
    {
        Debug.LogWarning("BaseEnemy: No se encontró Animator. Desactivando el enemigo inmediatamente.");
        gameObject.SetActive(false);
    }
}

// Método para desactivar comportamientos clave
private void DisableEnemyBehaviors()
{
    // Desactivar FSM
    var fsm = GetComponent<EnemyFSM>();
    if (fsm != null)
    {
        fsm.enabled = false;
    }

    // Desactivar NavMeshAgent para detener movimiento
    var navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    if (navMeshAgent != null)
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;
    }

    // Desactivar scripts de ataques
    foreach (var script in GetComponents<MonoBehaviour>())
    {
        if (script != this) // No desactivar BaseEnemy
        {
            script.enabled = false;
        }
    }

    // Desactivar colisiones para evitar interacciones
    var collider = GetComponent<Collider>();
    if (collider != null)
    {
        collider.enabled = false;
    }
}

    // Manejar la animación de muerte y la pantalla de victoria
private IEnumerator HandleDeathAnimation()
{
    // Obtener la duración de la animación de muerte
    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    float deathAnimationDuration = stateInfo.length;

    // Esperar la duración de la animación
    yield return new WaitForSeconds(deathAnimationDuration);

    // Mostrar pantalla de victoria después de la animación
    uiManager.ShowVictory();

    // Destruir el enemigo tras la pantalla de victoria
    Destroy(gameObject, 3f);
    
}

    public virtual void ExecuteUltimateAttack(bool isRanged)
    {
        Debug.Log("Ejecutando ataque ultimate (BaseEnemy).");
    }

    private IEnumerator DestroyAfterVictory()
    {
        yield return new WaitForSeconds(uiManager.delayBeforeVictory + 1.5f);  // Esperar el tiempo de la pantalla de victoria + un segundo y medio extra
        Destroy(gameObject);
    }
     
}
