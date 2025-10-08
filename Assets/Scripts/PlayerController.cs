using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    public float speed = 4f;

    [Header("Movement System")]
    public float runSpeed = 8f;
    public float walkSpeed = 4f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
      
    }

    void Update()
    {
        Move();
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