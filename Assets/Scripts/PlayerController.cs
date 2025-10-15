using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    public float speed = 4f;

    [Header("Movement System")]
    public float runSpeed = 8f;
    public float walkSpeed = 5f;

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
            for (int i = 0; i < 30; i++)
            {
                TimeManager.Instance.Tick();
            }
        }
        

    }

    public void Interact() {
        // Prevent input when Alt keys are held (to avoid conflicts)
        if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
            return;
        }

        // Check cooldown to prevent rapid interactions
        if(Time.time - lastInteractionTime < interactionCooldown) {
            return;
        }

        //left mouse click
        if(Input.GetButtonDown("Fire1")) {
            PlayerInteraction.Interact();
            lastInteractionTime = Time.time;
        }

        //press Q key for item interaction 
        if(Input.GetKeyDown(KeyCode.Q)) {
            PlayerInteraction.ItemInteract();
            lastInteractionTime = Time.time;
        }

    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
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

}