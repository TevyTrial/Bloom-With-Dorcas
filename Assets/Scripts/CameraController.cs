using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerPos; // assign in Inspector or it will auto-find

    [Tooltip("Smoothing time in seconds (lower = tighter follow)")]
    public float smoothTime = 0.08f;

    [Tooltip("Don't allow the camera Y to go below this world height")]
    public float minY = 0.5f;

    [Header("Orbit Controls")]
    public float distance = 19f; // Current zoom distance
    public float minDistance = 3f;
    public float maxDistance = 15f;
    public float zoomSpeed = 5f;
    public float orbitSpeed = 180f; // degrees/sec
    public float minVerticalAngle = 10f;
    public float maxVerticalAngle = 80f;
    public float initialVerticalAngle = 40f;
    public float initialHorizontalAngle = 180f;

    private float yaw = 0f; // horizontal angle (degrees)
    private float pitch = 40f; // vertical angle (degrees)
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;

    void Awake()
    {
        if (playerPos == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
                playerPos = player.transform;
            else
                Debug.LogWarning("CameraController: PlayerController not found in scene. Assign playerPos in Inspector.");
        }
        pitch = initialVerticalAngle;
        yaw = initialHorizontalAngle;
    }

    void Update()
    {
        if (playerPos == null) return;

        // Reset camera position (R key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            yaw = initialHorizontalAngle;
            pitch = initialVerticalAngle;
            distance = distance; // Reset to default distance
        }

        // Orbit controls (right mouse drag)
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            yaw += mouseX * orbitSpeed * Time.deltaTime;
            pitch -= mouseY * orbitSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }

        // Zoom controls (scroll wheel)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void LateUpdate()
    {
        if (playerPos == null) return;

        // Spherical coordinates to cartesian
        float yawRad = Mathf.Deg2Rad * yaw;
        float pitchRad = Mathf.Deg2Rad * pitch;
        Vector3 offset = new Vector3(
            distance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad),
            distance * Mathf.Sin(pitchRad),
            distance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad)
        );

        Vector3 desiredPos = playerPos.position + offset;
        if (desiredPos.y < minY) desiredPos.y = minY;

        // Smooth follow
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);

        // Look at player head
        transform.LookAt(playerPos.position + Vector3.up * 1.5f);
    }
}