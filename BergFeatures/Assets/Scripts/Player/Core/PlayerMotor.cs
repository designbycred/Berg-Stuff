using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Gravity")]
    [Tooltip("Downward acceleration (negative). Typical: -15 to -30")]
    [SerializeField] private float gravity = -20f;

    [Tooltip("Small downward value to keep the controller grounded on slopes.")]
    [SerializeField] private float groundedStick = -2f;

    private CharacterController cc;

    // We keep vertical velocity here so abilities can change it (jump, jetpack, swimming).
    public float VerticalVelocity { get; set; }

    public bool IsGrounded => cc.isGrounded;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Applies planar movement + gravity + vertical velocity once per frame.
    /// Call this exactly once per frame from PlayerMain.
    /// </summary>
    public void Move(Vector3 planarWorldVelocity, float dt)
    {
        // Stick to ground when grounded and moving downward
        if (cc.isGrounded && VerticalVelocity < 0f)
            VerticalVelocity = groundedStick;

        // Apply gravity
        VerticalVelocity += gravity * dt;

        // Move in two passes (keeps things predictable)
        Vector3 planarDelta = planarWorldVelocity * dt;
        Vector3 verticalDelta = Vector3.up * (VerticalVelocity * dt);

        cc.Move(planarDelta);
        cc.Move(verticalDelta);
    }
}