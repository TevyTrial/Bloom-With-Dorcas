using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerPos;      // assign in Inspector or it will auto-find
    [Tooltip("Camera offset relative to the player (local): X=right, Y=up, Z=back)")]
    public Vector3 offset = new Vector3(0f, 5f, -6f);

    [Tooltip("Smoothing time in seconds (lower = tighter follow)")]
    public float smoothTime = 0.08f;

    [Tooltip("Don't allow the camera Y to go below this world height")]
    public float minY = 0.5f;

    private Vector3 velocity = Vector3.zero;

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
    }

    void LateUpdate()
    {
if (playerPos == null) return;

        Vector3 desiredPos = playerPos.position + offset;
        // Optionally clamp the camera's world Y so it never goes below the floor
        if (desiredPos.y < minY) desiredPos.y = minY;

        // Smoothly move camera to target position
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);

        // Optional: look at player head a bit higher than root
        transform.LookAt(playerPos.position + Vector3.up * 1.5f);
    }
}