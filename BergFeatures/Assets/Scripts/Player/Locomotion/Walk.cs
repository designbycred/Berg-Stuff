using UnityEngine;

public class Walk : MonoBehaviour
{
    [Header("Speed / Accel")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 25f;

    [Header("Damping (Grounded = snappy, Airborne = floaty)")]
    [SerializeField] private float groundedDampingNoInput = 10f; // big = stops fast
    [SerializeField] private float airborneDampingNoInput = 1.2f; // small = drift

    [Tooltip("Optional always-on damping (space drag). Usually 0 for grounded games.")]
    [SerializeField] private float dampingAlways = 0f;

    private MoveInput moveInput;
    private PlayerMain playerMain;
    private PlayerMotor motor;

    private Vector3 planarVelocity;

    private void Awake()
    {
        moveInput = GetComponent<MoveInput>();
        playerMain = GetComponent<PlayerMain>();
        motor = GetComponent<PlayerMotor>();

        if (moveInput == null) Debug.LogError("Walk requires MoveInput on the same GameObject.");
        if (playerMain == null) Debug.LogError("Walk requires PlayerMain on the same GameObject.");
        if (motor == null) Debug.LogError("Walk requires PlayerMotor on the same GameObject.");
    }

    public Vector3 GetPlanarVelocity(float dt)
    {
        Vector2 input = moveInput != null ? moveInput.Value : Vector2.zero;

        Vector3 forward = playerMain.transform.forward;
        Vector3 right = playerMain.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 wishDir = right * input.x + forward * input.y;
        if (wishDir.sqrMagnitude > 1f) wishDir.Normalize();

        bool hasInput = wishDir.sqrMagnitude > 0.0001f;

        // Accelerate when input exists
        if (hasInput)
        {
            planarVelocity += wishDir * (acceleration * dt);

            float spd = planarVelocity.magnitude;
            if (spd > maxSpeed)
                planarVelocity = planarVelocity.normalized * maxSpeed;
        }

        // Choose damping depending on grounded state
        float noInputDamping = motor.IsGrounded ? groundedDampingNoInput : airborneDampingNoInput;

        float totalDamping = dampingAlways + (hasInput ? 0f : noInputDamping);

        if (totalDamping > 0f)
        {
            float k = Mathf.Exp(-totalDamping * dt);
            planarVelocity *= k;
        }

        if (planarVelocity.sqrMagnitude < 0.0001f)
            planarVelocity = Vector3.zero;

        return planarVelocity;
    }

    public void AddImpulse(Vector3 worldPlanarImpulse)
    {
        worldPlanarImpulse.y = 0f;
        planarVelocity += worldPlanarImpulse;
    }

    public void StopPlanar() => planarVelocity = Vector3.zero;
}
