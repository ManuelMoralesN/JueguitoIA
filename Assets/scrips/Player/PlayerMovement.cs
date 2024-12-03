using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;  // Velocidad de caminar
    private Vector3 movement;
    private Animator animator;  // Referencia al Animator
    private Rigidbody rb;

    void Start()
    {
        // Obtener la referencia al Rigidbody y al Animator del personaje
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Capturar los inputs de movimiento
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Crear un vector de movimiento
        movement = new Vector3(horizontalInput, 0f, verticalInput);

        // Normalizar para mantener la velocidad consistente en las diagonales
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // Determinar si el personaje está caminando (si hay input de movimiento)
        bool Running = movement.magnitude > 0;

        // Actualizar el Animator para cambiar la animación a caminar o idle
        animator.SetBool("Running", Running);

        // Rotar el personaje hacia la dirección del movimiento si hay input
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Activar la animación de disparo cuando se presiona la tecla F
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Shoot");  // Disparar la animación de Shoot
        }

        // Activar la animación de bailar cuando se presiona la tecla B
        if (Input.GetKeyDown(KeyCode.B))
        {
            animator.SetTrigger("Dance");  // Activar la animación de bailar
        }
    }

    void FixedUpdate()
    {
        // Mover el personaje usando el Rigidbody
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}
