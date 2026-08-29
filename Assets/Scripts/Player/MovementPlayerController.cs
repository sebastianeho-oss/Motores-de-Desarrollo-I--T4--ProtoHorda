using UnityEngine;

public class MovementPlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 8.5f;
    public float crouchSpeed = 2.5f;
    public float gravity = -15.0f;
    public float jumpHeight = 1.2f;

    [Header("Agacharse (Crouch)")]
    [Tooltip("Tecla para agacharse.")]
    public KeyCode crouchKey = KeyCode.C;
    [Tooltip("Altura del CharacterController de pie.")]
    public float standingHeight = 2.0f;
    [Tooltip("Altura del CharacterController agachado (reducir la hitbox).")]
    public float crouchingHeight = 1.0f;
    [Tooltip("Velocidad de transición entre estar de pie y agachado.")]
    public float crouchTransitionSpeed = 10.0f;

    [Header("Permisos Locales")]
    public bool canMove = true;
    public bool canJump = true;

    [Header("Referencias")]
    public Transform cameraTransform;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    public PlayerHealth playerHealth;
    public AimCameraPlayerController cameraController;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    
    // Variables para el control de salto y tierra
    private bool isGrounded;
    private bool wasGrounded; // Detectar el impacto exacto con el suelo

    public bool IsCrouching { get; private set; }
    private Vector3 standingCenter;
    private Vector3 crouchingCenter;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (cameraController == null && cameraTransform != null)
        {
            cameraController = cameraTransform.GetComponent<AimCameraPlayerController>();
        }

        if (animator == null)
        {
            Debug.LogError("No se encontró un Animator en los hijos de este objeto.");
        }

        // Configuración inicial de la Hitbox
        if (controller != null)
        {
            standingHeight = controller.height;
            standingCenter = controller.center;
            crouchingCenter = new Vector3(standingCenter.x, standingCenter.y - (standingHeight - crouchingHeight) / 2f, standingCenter.z);
        }
    }

    void Update()
    {
        // 1. Detección de Suelo y Evento de Aterrizaje
        wasGrounded = isGrounded; // Guardamos el estado del frame anterior
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Detectar si impactó contra el suelo en este frame
        bool justLanded = !wasGrounded && isGrounded;

        // Deshabilitar apuntado si el personaje está en el aire
        if (cameraController != null)
        {
            cameraController.canAim = isGrounded;
        }

        // Evaluar permisos globales (Requiere estar vivo, permiso local y NO estar en pausa)
        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        bool isNotPaused = !GameManager.IsPaused;

        bool allowedToMove = canMove && isAlive && isNotPaused;
        bool allowedToJump = canJump && isAlive && isNotPaused;

        // 2. Control de Agachado
        if (allowedToMove && isGrounded)
        {
            if (Input.GetKeyDown(crouchKey))
            {
                IsCrouching = !IsCrouching;
            }
        }
        else if (!isGrounded)
        {
            IsCrouching = false;
        }

        // Transición suave de la Hitbox (CharacterController)
        float targetHeight = IsCrouching ? crouchingHeight : standingHeight;
        Vector3 targetCenter = IsCrouching ? crouchingCenter : standingCenter;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * crouchTransitionSpeed);

        // 3. Girar al personaje con la cámara
        if (cameraTransform != null && allowedToMove)
        {
            transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
        }

        // 4. Movimiento WASD
        float moveX = 0f;
        float moveZ = 0f;

        if (allowedToMove)
        {
            bool isAiming = (cameraController != null && cameraController.IsAiming);

            // Si presiona Sprint, se levanta de estar agachado
            if (Input.GetKey(KeyCode.LeftShift) && !isAiming)
            {
                IsCrouching = false;
            }

            bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && !isAiming && !IsCrouching;

            // Determinar velocidad base (Caminar vs Sprint vs Agachado)
            float baseSpeed = walkSpeed;
            if (wantsToSprint)
            {
                baseSpeed = sprintSpeed;
            }
            else if (IsCrouching)
            {
                baseSpeed = crouchSpeed;
            }

            float currentSpeed = baseSpeed;

            // Ralentización adicional al apuntar
            if (isAiming)
            {
                WeaponAim activeAim = cameraController.ActiveWeaponAim;
                float speedMultiplier = (activeAim != null) 
                    ? activeAim.aimMovementSpeedMultiplier 
                    : cameraController.defaultAimMovementSpeedMultiplier;

                currentSpeed *= speedMultiplier;
            }

            moveX = Input.GetAxisRaw("Horizontal");
            moveZ = Input.GetAxisRaw("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            if (move.magnitude > 1f) move.Normalize();

            controller.Move(move * currentSpeed * Time.deltaTime);
        }

        // 5. Salto
        bool jumpedThisFrame = false;
        if (Input.GetButtonDown("Jump") && isGrounded && allowedToJump)
        {
            IsCrouching = false; // Levantarse al saltar
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpedThisFrame = true;
        }

        // 6. Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); 

        // 7. Sincronización con el Animator
        if (animator != null)
        {
            animator.SetFloat("MoveX", moveX);
            animator.SetFloat("MoveZ", moveZ);
            animator.SetBool("IsCrouching", IsCrouching);
            animator.SetBool("IsGrounded", isGrounded);

            // Trigger para iniciar el salto
            if (jumpedThisFrame)
            {
                animator.SetTrigger("Jump");
            }

            // Trigger para reproducir el aterrizaje justo al tocar tierra
            if (justLanded)
            {
                animator.SetTrigger("Land");
            }
        }     
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}