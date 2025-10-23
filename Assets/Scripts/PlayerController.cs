using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    public float speed = 4f;

    [Header("Movement System")]
    public float runSpeed = 8f;
    public float walkSpeed = 5f;

    [Header("Camera")]
    public Transform cameraTransform; 

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

        if (Input.GetKeyDown(KeyCode.U))
        {
            for (int i = 0; i < 43200; i++)
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

    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        /*
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 velocity = speed * Time.deltaTime * direction; // units per second*/

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
        Vector3 velocity = speed * Time.deltaTime * direction; // units per second

        //shift
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

        //movement and rotation
        if (direction.magnitude >= 0.1f)
        {            
            transform.rotation = Quaternion.LookRotation(direction);

            // Move expects displacement for this frame (units), so multiply by deltaTime here
            controller.Move(velocity);
        }

           animator.SetFloat("Speed", velocity.magnitude);

        
    }
    /*
    public void TriggerWateringAnimation()
    {
        animator.SetBool("Watering", true);
    }
    */


}