using UnityEngine;

/// <summary>
/// Converts MoveInput (WASD) into planar world-space velocity.
/// Does NOT call CharacterController.Move() directly.
/// </summary>
public class Walk : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private MoveInput moveInput;
    private PlayerMain playerMain;

    private void Awake()
    {
        moveInput = GetComponent<MoveInput>();
        playerMain = GetComponent<PlayerMain>();

        if (moveInput == null) Debug.LogError("Walk requires MoveInput on the same GameObject.");
        if (playerMain == null) Debug.LogError("Walk requires PlayerMain on the same GameObject.");
    }

    /// <summary>
    /// Returns the planar (XZ) velocity to apply this frame.
    /// </summary>
    public Vector3 GetPlanarVelocity()
    {
        Vector2 input = moveInput != null ? moveInput.Value : Vector2.zero;

        // Forward/right from player root (first-person)
        Vector3 forward = playerMain.transform.forward;
        Vector3 right = playerMain.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 dir = right * input.x + forward * input.y;

        // Prevent faster diagonal movement
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        return dir * moveSpeed;
    }
}
