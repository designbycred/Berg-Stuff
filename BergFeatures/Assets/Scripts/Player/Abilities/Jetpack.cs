using UnityEngine;
using UnityEngine.InputSystem;

public class Jetpack : MonoBehaviour
{
    [Header("Rules")]
    [SerializeField] private bool requireAirborne = true;

    [Header("Tap Boost (Impulse)")]
    [Tooltip("Vertical impulse added on tap (airborne).")]
    [SerializeField] private float tapUpImpulse = 4f;

    [Tooltip("Horizontal impulse added on tap when WASD held (airborne).")]
    [SerializeField] private float tapPlanarImpulse = 5f;

    [Header("Hold Lift (Sustain)")]
    [Tooltip("Upward acceleration while holding Space (airborne).")]
    [SerializeField] private float holdUpAcceleration = 18f;

    [Tooltip("Max upward velocity while holding.")]
    [SerializeField] private float maxUpwardVelocity = 8f;

    [Header("Steering While Holding")]
    [Tooltip("Extra planar speed while holding Space (airborne) based on WASD.")]
    [SerializeField] private float holdPlanarSpeed = 4f;

    private PlayerMain player;
    private PlayerMotor motor;
    private MoveInput moveInput;

    private Vector3 planarAdd; // cached per-frame contribution

    private void Awake()
    {
        player = GetComponentInParent<PlayerMain>();
        motor = GetComponentInParent<PlayerMotor>();
        moveInput = GetComponentInParent<MoveInput>();

        if (player == null) Debug.LogError("Jetpack needs PlayerMain in parent.");
        if (motor == null) Debug.LogError("Jetpack needs PlayerMotor in parent.");
        if (moveInput == null) Debug.LogError("Jetpack needs MoveInput in parent.");
    }

    private void OnEnable()
    {
        // Subscribe once input exists
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

        // Tap = performed
        player.Controls.Player.Jump.performed += OnJumpTap;
    }

    private void OnDisable()
    {
        if (player == null || player.Controls == null) return;
        player.Controls.Player.Jump.performed -= OnJumpTap;
    }

    /// <summary>
    /// Called by PlayerMain each frame to get extra planar velocity from jetpack (hold steering).
    /// </summary>
    public Vector3 GetPlanarVelocity(float dt)
    {
        planarAdd = Vector3.zero;

        if (player == null || motor == null || moveInput == null || player.Controls == null)
            return Vector3.zero;

        if (requireAirborne && motor.IsGrounded)
            return Vector3.zero;

        bool holding = player.Controls.Player.Jump.IsPressed();

        if (holding)
        {
            // Hold = lift straight up
            motor.VerticalVelocity += holdUpAcceleration * dt;
            motor.VerticalVelocity = Mathf.Min(motor.VerticalVelocity, maxUpwardVelocity);

            // Optional: WASD steering while holding
            Vector2 input = moveInput.Value;
            Vector3 forward = player.transform.forward;
            Vector3 right = player.transform.right;
            forward.y = 0f; right.y = 0f;
            forward.Normalize(); right.Normalize();

            Vector3 dir = right * input.x + forward * input.y;
            if (dir.sqrMagnitude > 1f) dir.Normalize();

            planarAdd = dir * holdPlanarSpeed;
        }

        return planarAdd;
    }

    private void OnJumpTap(InputAction.CallbackContext ctx)
    {
        if (player == null || motor == null || moveInput == null) return;
        if (player.Controls == null) return;

        // Only jetpack-tap while airborne (so jump on ground still works)
        if (requireAirborne && motor.IsGrounded)
            return;

        // Tap upward impulse
        motor.VerticalVelocity += tapUpImpulse;

        // Tap directional impulse based on WASD held at tap moment
        Vector2 input = moveInput.Value;

        Vector3 forward = player.transform.forward;
        Vector3 right = player.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 dir = right * input.x + forward * input.y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        // We can't directly "impulse" CharacterController, so we add it as extra planar speed for a moment.
        // Easiest: add to a small one-shot burst variable (below).
        StartCoroutine(PlanarBurst(dir * tapPlanarImpulse, 0.12f));
    }

    private System.Collections.IEnumerator PlanarBurst(Vector3 burstVel, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            planarAdd += burstVel;
            t += Time.deltaTime;
            yield return null;
        }
    }
}
