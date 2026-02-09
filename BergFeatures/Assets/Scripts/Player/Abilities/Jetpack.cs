using UnityEngine;
using UnityEngine.InputSystem;

public class Jetpack : MonoBehaviour
{
    [Header("Rules")]
    [SerializeField] private bool requireAirborne = true;

    [Header("Tap Boost (Impulse)")]
    [SerializeField] private float tapUpImpulse = 4f;
    [SerializeField] private float tapPlanarImpulse = 5f;

    [Header("Hold Lift (Sustain)")]
    [SerializeField] private float holdUpAcceleration = 18f;
    [SerializeField] private float maxUpwardVelocity = 8f;

    [Header("Steering While Holding")]
    [SerializeField] private float holdPlanarSpeed = 4f;

    private PlayerMain player;
    private PlayerMotor motor;
    private MoveInput moveInput;
    private Walk walk;

    private void Awake()
    {
        player = GetComponentInParent<PlayerMain>();
        motor = GetComponentInParent<PlayerMotor>();
        moveInput = GetComponentInParent<MoveInput>();
        walk = GetComponentInParent<Walk>();

        if (player == null) Debug.LogError("Jetpack needs PlayerMain in parent.");
        if (motor == null) Debug.LogError("Jetpack needs PlayerMotor in parent.");
        if (moveInput == null) Debug.LogError("Jetpack needs MoveInput in parent.");
        if (walk == null) Debug.LogError("Jetpack needs Walk in parent (for impulses).");
    }

    private void OnEnable()
    {
        StartCoroutine(BindNextFrame());
    }

    private System.Collections.IEnumerator BindNextFrame()
    {
        yield return null;

        if (player == null || player.Controls == null)
        {
            Debug.LogError("Jetpack: Controls not ready. Check PlayerMain.");
            yield break;
        }

        player.Controls.Player.Jump.performed += OnJumpTap;
    }

    private void OnDisable()
    {
        if (player == null || player.Controls == null) return;
        player.Controls.Player.Jump.performed -= OnJumpTap;
    }

    /// <summary>
    /// Called by PlayerMain each frame to contribute planar velocity while holding (steering).
    /// </summary>
    public Vector3 GetPlanarVelocity(float dt)
    {
        if (player == null || motor == null || moveInput == null) return Vector3.zero;
        if (player.Controls == null) return Vector3.zero;

        if (requireAirborne && motor.IsGrounded)
            return Vector3.zero;

        bool holding = player.Controls.Player.Jump.IsPressed();
        if (!holding) return Vector3.zero;

        // Hold = lift up
        motor.VerticalVelocity += holdUpAcceleration * dt;
        motor.VerticalVelocity = Mathf.Min(motor.VerticalVelocity, maxUpwardVelocity);

        // WASD steering while holding
        Vector2 input = moveInput.Value;

        Vector3 forward = player.transform.forward;
        Vector3 right = player.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 dir = right * input.x + forward * input.y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        return dir * holdPlanarSpeed;
    }

    private void OnJumpTap(InputAction.CallbackContext ctx)
    {
        if (player == null || motor == null || moveInput == null || walk == null) return;

        // Only jetpack-tap while airborne (so ground space = jump)
        if (requireAirborne && motor.IsGrounded)
            return;

        // Upward impulse
        motor.VerticalVelocity += tapUpImpulse;

        // Planar impulse in direction of WASD (relative to player yaw)
        Vector2 input = moveInput.Value;

        Vector3 forward = player.transform.forward;
        Vector3 right = player.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 dir = right * input.x + forward * input.y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        if (dir.sqrMagnitude > 0.0001f)
            walk.AddImpulse(dir * tapPlanarImpulse);
    }
}
