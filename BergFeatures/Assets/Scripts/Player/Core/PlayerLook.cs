using UnityEngine;

/// <summary>
/// Consumes LookInput and applies:
/// - Yaw (left/right) to PlayerRoot
/// - Pitch (up/down) to CameraPivot
/// </summary>
public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private LookInput lookInput;
    private float pitch;

    private void Awake()
    {
        lookInput = GetComponent<LookInput>();

        if (lookInput == null)
            Debug.LogError("PlayerLook requires LookInput on the same GameObject.");

        if (cameraPivot == null)
            Debug.LogError("PlayerLook requires a Camera Pivot reference.");
    }

    private void Update()
    {
        if (lookInput == null || cameraPivot == null)
            return;

        Vector2 look = lookInput.Value;

        // ----- Yaw (rotate player root) -----
        float yaw = look.x * sensitivity;
        transform.Rotate(Vector3.up * yaw);

        // ----- Pitch (rotate camera pivot) -----
        pitch -= look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
