using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    public float speed = 4f;

    [Header("Movement System")]
    public float runSpeed = 8f;
    public float walkSpeed = 5f;

    private float gravity = 9.81f;

    [Header("Camera")]
    public Transform cameraTransform; 

    [Header("Footstep Sounds")]
    private float footstepTimer = 0f;
    private float footstepInterval = 0.5f; // Time between footsteps

    //Interact components
    PlayerInteraction PlayerInteraction;
    
    //Input cooldown to prevent rapid interactions
    private float lastInteractionTime = 0f;
    private float interactionCooldown = 0.1f; // 100ms cooldown


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        //get interaction component
        PlayerInteraction = GetComponentInChildren<PlayerInteraction>();

        //auto-find camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        //runs the movement
        Move();

        //runs the interaction
        Interact();

        //TimeFastForward-Debug
        //Skip the time when pressing the Y key
        if (Input.GetKeyDown(KeyCode.Y))
        {
            for (int i = 0; i < 60; i++)
            {
                TimeManager.Instance.Tick();
            }
        }
    }

    public void Interact() {
        // Check cooldown to prevent rapid interactions
        if(Time.time - lastInteractionTime < interactionCooldown) {
            return;
        }

        //left mouse click
        if(Input.GetButtonDown("Fire1")) {
            PlayerInteraction.Interact();
            lastInteractionTime = Time.time;
        }

        //press F key for item interaction 
        if(Input.GetKeyDown(KeyCode.F)) {
            PlayerInteraction.ItemInteract();
            lastInteractionTime = Time.time;
        }

        //Keep items
        if(Input.GetButtonDown("Fire3")) {
            PlayerInteraction.ItemKeep();
            lastInteractionTime = Time.time;
        }
    }

    void Move()
    {
        // Don't move if controller is disabled
        if(!controller.enabled) return;
        
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Get camera's forward and right directions (flatten to horizontal plane)
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        // Remove vertical component to keep movement on ground plane
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        Vector3 direction = (cameraForward * vertical + cameraRight * horizontal).normalized;
        Vector3 velocity = speed * Time.deltaTime * direction;

        // Apply gravity
        if (controller.isGrounded)
        {
            velocity.y = 0f; // Reset vertical velocity when grounded
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        // Handle sprint
        if (Input.GetButton("Sprint"))
        {
            speed = runSpeed;
            animator.SetBool("Running", true);
        }
        else
        {
            speed = walkSpeed;
            animator.SetBool("Running", false);
        }

        // Handle movement and rotation
        if (direction.magnitude >= 0.1f)
        {            
            // Rotate player to face movement direction
            transform.rotation = Quaternion.LookRotation(direction);

            // Move the character
            controller.Move(velocity);
            
            // Update animator
            animator.SetFloat("Speed", direction.magnitude * speed * Time.deltaTime);

            // Play footstep sounds
            if (controller.isGrounded)
            {

                AudioManager.Instance.PlayWalkingSFX();

            }
        }
        else
        {
            // Player is not moving
            animator.SetFloat("Speed", 0f);
            AudioManager.Instance.StopWalkingSFX();
        }
    }
}